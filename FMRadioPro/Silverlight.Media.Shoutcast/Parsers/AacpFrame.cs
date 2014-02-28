//-----------------------------------------------------------------------
// <copyright file="AacpFrame.cs" company="Andrew Oakley">
//     Copyright (c) 2010 Andrew Oakley
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public License
//     along with this program.  If not, see http://www.gnu.org/licenses.
// </copyright>
//-----------------------------------------------------------------------

namespace Silverlight.Media.Parsers
{
    using System;

    /// <summary>
    /// A partial implementation of an AAC audio frame
    /// </summary>
    /// <remarks>
    /// <para>
    /// The primary purpose of this class is to represent an AAC audio file.
    /// Many of the features not explicitly needed for audio rendering are omitted from the implementation.
    /// </para>
    /// <para>
    /// Data on this format is readily discoverable in many books as well as by
    /// searching for "AAC Frame" in your favorite search engine. As always,
    /// Wikipedia is well stocked in all of these areas as well.
    /// </para>
    /// </remarks>
    public class AacpFrame : AudioFrame
    {
        /// <summary>
        /// AAC headers are 7 bytes long.
        /// </summary>
        public static readonly int FrameHeaderSize = 7;

        /// <summary>
        /// AAC frame synchronization bytes.
        /// </summary>
        public static readonly byte[] SyncBytes = new byte[] { 0xFF, 0xF0 };

        /// <summary>
        /// Frame Sync is 12 1s
        /// </summary>
        private static readonly int syncValue = 4095;

        /// <summary>
        /// A table of all of the possible sampling rates of AAC audio. 
        /// </summary>
        private static int[] sampleRateTable = new int[] { 96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11025, 8000, 7350, 0, 0, -1 };

        /// <summary>
        /// A table of all of the possible number of channels for AAC audio.
        /// </summary>
        private static int[] numberOfChannelsTable = new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Number of bits per block for AAC audio.
        /// </summary>
        private static int bitsPerBlock = 6144;

        /// <summary>
        /// Number of samples per block for AAC audio.
        /// </summary>
        private static int samplesPerBlock = 1024;

        /// <summary>
        /// Initializes a new instance of the AacpFrame class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This class is a partial implementation of an AAC audio frame.  The primary purpose of this class is to represent an AAC
        /// file. Many of the features not explicitly needed for audio rendering are omitted from the implementation.
        /// </para>
        /// <para>
        /// Data on this format is readily discoverable in many books as well as by
        /// searching for "AAC Frame" in your favorite search engine. As always,
        /// Wikipedia is well stocked in all of these areas as well.
        /// </para>
        /// </remarks>
        /// <param name="frameHeader">Byte array containing 4 bytes representing an AAC header.</param>
        public AacpFrame(byte[] frameHeader)
            : base()
        {
            if (frameHeader == null)
            {
                throw new ArgumentNullException("frameHeader");
            }

            // Sync
            int value = BitTools.MaskBits(frameHeader, 0, 12);
            if (value != syncValue)
            {
                throw new ArgumentException("Invalid sync value.");
            }

            this.NumberOfChannels = AacpFrame.ParseChannel(frameHeader);
            this.SamplingRate = AacpFrame.ParseSampleRate(frameHeader);
            this.BitRate = AacpFrame.CalculateBitRate(this.SamplingRate, this.NumberOfChannels);
            this.FrameSize = AacpFrame.ParseFrameSize(frameHeader);

            int objectTypeId = BitTools.MaskBits(frameHeader, 16, 2);
        }

        /// <summary>
        /// Quickly checks an array of bytes to see if it represents a valid AAC frame header.
        /// </summary>
        /// <param name="frameHeader">Bytes representing an AAC frame header.</param>
        /// <returns>true if the supplied bytes are a valid frame header, otherwise, false.</returns>
        public static bool IsValidFrame(byte[] frameHeader)
        {
            if (frameHeader == null)
            {
                throw new ArgumentNullException("frameHeader");
            }

            int value = BitTools.MaskBits(frameHeader, 0, 12);
            if (value != AacpFrame.syncValue)
            {
                return false;
            }

            int sampleRate = AacpFrame.ParseSampleRate(frameHeader);

            if (sampleRate == -1)
            {
                return false;
            }

            int numberOfChannels = AacpFrame.ParseChannel(frameHeader);
            if (numberOfChannels == -1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified Object is equal to the current Object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            AacpFrame other = obj as AacpFrame;

            if (other == null)
            {
                return false;
            }

            return (this.NumberOfChannels == other.NumberOfChannels) &&
                (this.SamplingRate == other.SamplingRate) &&
                (this.BitRate == other.BitRate);
        }

        /// <summary>
        /// Generates a hash code for the current Object.
        /// </summary>
        /// <returns>A hash code for the current Object.</returns>
        public override int GetHashCode()
        {
            // Pick a Prime!
            const int Prime = 17;
            int hash = Prime;
            hash = (hash * Prime) + this.NumberOfChannels;
            hash = (hash * Prime) + this.SamplingRate;
            hash = (hash * Prime) + this.BitRate;
            return hash;
        }

        /// <summary>
        /// Calculates the bit rate for an AAC frame.
        /// </summary>
        /// <param name="sampleRate">Sample rate of the AAC frame.</param>
        /// <param name="numberOfChannels">Number of channels of the AAC frame.</param>
        /// <returns>Bit rate of an AAC frame with the given sample rate and number of channels.</returns>
        private static int CalculateBitRate(int sampleRate, int numberOfChannels)
        {
            return AacpFrame.bitsPerBlock / AacpFrame.samplesPerBlock * sampleRate * numberOfChannels;
        }

        /// <summary>
        /// Parses the AAC frame header to find the actual size of the header.
        /// </summary>
        /// <param name="frameHeader">Byte array containing the AAC frame header.</param>
        /// <returns>Actual size of the supplied AAC frame header.</returns>
        private static int ParseFrameSize(byte[] frameHeader)
        {
            int value = BitTools.MaskBits(frameHeader, 30, 13);
            return value;
        }

        /// <summary>
        /// Parses the sample rate from the supplied AAC frame header.
        /// </summary>
        /// <param name="frameHeader">Byte array containing the AAC frame header.</param>
        /// <returns>The sample rate of the supplied AAC frame header.</returns>
        private static int ParseSampleRate(byte[] frameHeader)
        {
            int sampleRateValue = BitTools.MaskBits(frameHeader, 18, 4);

            if ((sampleRateValue < 0) || (sampleRateValue > 15))
            {
                return -1;
            }

            return AacpFrame.sampleRateTable[sampleRateValue];
        }

        /// <summary>
        /// Parses the number of channels from the supplied AAC frame header.
        /// </summary>
        /// <param name="frameHeader">Byte array containing the AAC frame header.</param>
        /// <returns>The number of channels of the supplied AAC frame header.</returns>
        private static int ParseChannel(byte[] frameHeader)
        {
            int channelValue = BitTools.MaskBits(frameHeader, 23, 3);

            if ((channelValue < 1) || (channelValue > 7))
            {
                // Invalid or reserved channel value
                return -1;
            }

            return AacpFrame.numberOfChannelsTable[channelValue];
        }
    }
}
