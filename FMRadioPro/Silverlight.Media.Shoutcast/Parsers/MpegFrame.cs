//-----------------------------------------------------------------------
// <copyright file="MpegFrame.cs" company="Larry Olson">
// (c) Copyright Larry Olson.
// This source is subject to the Microsoft Public License (Ms-PL)
// See http://code.msdn.microsoft.com/ManagedMediaHelpers/Project/License.aspx
// All other rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Supressing Code Analysis rule(s)

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional",
    Scope = "member",
    Target = "Silverlight.Media.Parsers.MpegFrame.#bitrateTable",
    MessageId = "Member",
    Justification = "Array is not Jagged and does not waste space.")]

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional",
    Scope = "member",
    Target = "Silverlight.Media.Parsers.MpegFrame.#samplingRateTable",
    MessageId = "Member",
    Justification = "Array is not Jagged and does not waste space.")]

namespace Silverlight.Media.Parsers
{
    using System;
    using System.IO;

    /// <summary>
    /// Reproduction mode of given audio data. Typically maps to the number of
    /// output devices used to reproduce it.
    /// </summary>
    public enum Channel
    {
        /// <summary>
        /// Stereo: independent audio typically output to 2 speakers and is intended
        /// to create a more realistic or pleasing representation of audio by
        /// representing sound coming from multiple directons.
        /// </summary>
        Stereo = 0,

        /// <summary>
        /// Joint Stereo: The joining of multiple channels of audio to create another separate
        /// one, to reduce the size of the file, or to increase the quality.
        /// </summary>
        JointStereo,

        /// <summary>
        /// Dual Channel: Two independent Mono channels. May overlap with stereo or may 
        /// be completely independent as in the case of foreign language audio dubbing.
        /// </summary>
        DualChannel,

        /// <summary>
        /// Single Channel: Also known as Mono. Typically the reproduction of a single
        /// independent audio stream in one device or of the same independent audio stream
        /// in multiple devices.
        /// </summary>
        SingleChannel,
    }

    /// <summary>
    /// A partial implementation of an MPEG audio frame
    /// </summary>
    /// <remarks>
    /// <para>
    /// The primary purpose of this class is to represent an Mpeg 1 Layer 3
    /// file or MP3 file for short. Many of the features not explicitly needed
    /// for audio rendering are omitted from the implementation.
    /// </para>
    /// <para>
    /// Data on this format is readily discoverable in many books as well as by
    /// searching for "MP3 Frame" in your favorite search engine. As always,
    /// Wikipedia is well stocked in all of these areas as well.
    /// </para>
    /// </remarks>
    public class MpegFrame : AudioFrame
    {
        /// <summary>
        /// MP3 Headers are 4 Bytes long
        /// </summary>
        public const int FrameHeaderSize = 4;

        /// <summary>
        /// MP3 frame synchronization bytes.
        /// </summary>
        public static readonly byte[] SyncBytes = new byte[] { 0xFF, 0xE0 };

        /// <summary>
        /// Frame Sync is 11 1s
        /// </summary>
        private const int SyncValue = 2047;

        /// <summary>
        /// A table of bitrates / 1000. These are all of the possible bitrates for Mpeg 1 - 2.5 audio. -1 encodes an error lookup.
        /// </summary>
        private static int[,] bitrateTable = new int[,]
            {   
                { 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448 },
                { 0, 32, 48, 56, 64,  80,  96,  112, 128, 160, 192, 224, 256, 320, 384 },
                { 0, 32, 40, 48, 56,  64,  80,  96,  112, 128, 160, 192, 224, 256, 320 },
                { 0, 32, 48, 56, 64,  80,  96,  112, 128, 144, 160, 176, 192, 224, 256 },
                { 0, 8,  16, 24, 32,  40,  48,  56,  64,  80,  96,  112, 128, 144, 160 }
            };

        /// <summary>
        /// A table of all of the possible sampling rates of Mpeg 1 - 2.5 audio. 
        /// </summary>
        private static int[,] samplingRateTable = new int[,]
            {   
                { 44100, 48000, 32000 },
                { 22050, 24000, 16000 },
                { 11025, 12000, 8000 }
            };

        /// <summary>
        /// Initializes a new instance of the MpegFrame class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This class is a partial implementation of an MPEG audio frame.  The primary purpose of this class is to represent an Mpeg 1 Layer 3
        /// file or MP3 file for short. Many of the features not explicitly needed
        /// for audio rendering are omitted from the implementation.
        /// </para>
        /// <para>
        /// Data on this format is readily discoverable in many books as well as by
        /// searching for "MP3 Frame" in your favorite search engine. As always,
        /// Wikipedia is well stocked in all of these areas as well.
        /// </para>
        /// </remarks>
        /// <param name="frameHeader">Byte array containing 4 bytes representing an MPEG Layer 3 header.</param>
        public MpegFrame(byte[] frameHeader)
        {
            if (frameHeader == null)
            {
                throw new ArgumentNullException("frameHeader");
            }

            if (frameHeader.Length != 4)
            {
                throw new ArgumentException("Invalid frame header length.");
            }

            // Sync
            int value = BitTools.MaskBits(frameHeader, 0, 11);
            if (value != SyncValue)
            {
                throw new ArgumentException("Invalid sync value.");
            }

            this.Version = ParseVersion(frameHeader);
            this.Layer = ParseLayer(frameHeader);
            this.IsProtected = BitTools.MaskBits(frameHeader, 15, 1) == 1 ? false : true;
            this.BitrateIndex = BitTools.MaskBits(frameHeader, 16, 4);
            this.SamplingRateIndex = BitTools.MaskBits(frameHeader, 20, 2);
            this.Padding = BitTools.MaskBits(frameHeader, 22, 1);
            //// Private Bit = BitTools.MaskBits(_mp3FrameHeader,8,1); //USELESS
            this.Channels = ParseChannel(frameHeader);
            //// Joint Mode = ParseJoitMode(_mp3FrameHeader); //Not used by  Mp3MSS
            //// CopyRight = BitTools.MaskBits(_mp3FrameHeader,3,1); //Not used by Mp3MSS
            //// Original = BitTools.MaskBits(_mp3FrameHeader,2,1); //Not used by Mp3MSS
            //// Emphasis = ParseEmphasis(_mp3FrameHeader); //Not used by Mp3MSS

            this.BitRate = MpegFrame.CalculateBitRate(this.Version, this.Layer, this.BitrateIndex);
            this.SamplingRate = MpegFrame.LookupSamplingRate(this.Version, this.SamplingRateIndex);
            this.FrameSize = MpegFrame.CalculateFrameSize(this.Version, this.Layer, this.BitRate, this.SamplingRate, this.Padding);
            this.NumberOfChannels = (this.Channels == Channel.SingleChannel) ? 1 : 2;

            if ((this.Version == -1) || (this.Layer == -1) ||
                (this.BitrateIndex < 0) || (this.BitrateIndex >= 15) ||
                (this.SamplingRateIndex < 0) || (this.SamplingRateIndex >= 3))
            {
                throw new ArgumentException("Invalid header values");
            }

            // Add in the bytes we already read
            if (this.FrameSize <= 0)
            {
                throw new InvalidOperationException("MpegFrame's FrameSize must be greater than zero.");
            }
        }

        /**********************************************************************
         * FILE DATA- data which comes directly from the MP3 header.
         *********************************************************************/
        #region File Data

        /// <summary>
        /// Gets the Version of the MPEG standard this frame conforms to.
        /// MPEG 1, MPEG 2, or MPEG 2.5
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Gets the layer of complexity used in this frame.
        /// Layer 1, 2, or 3.
        /// </summary>
        public int Layer { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the frame is protected by a
        /// Cyclic Redundancy Check (CRC). If true, then a 16 bit
        /// CRC follows the header.
        /// </summary>
        public bool IsProtected { get; private set; }

        /// <summary>
        /// Gets the Index into the bitrate table as defined in the MPEG spec.
        /// </summary>
        public int BitrateIndex { get; private set; }

        /// <summary>
        /// Gets the Index into the samplingrate table as defined in the MPEG spec.
        /// </summary>
        public int SamplingRateIndex { get; private set; }

        /// <summary>
        /// Gets the number of additional bytes of padding in this frame.
        /// </summary>
        public int Padding { get; private set; }
        
        /// <summary>
        /// Gets the output channel used to playback this frame.
        /// </summary>
        public Channel Channels { get; private set; }

        #endregion

        /// <summary>
        /// Quickly checks an array of bytes to see if it represents a valid MP3 frame header.
        /// </summary>
        /// <param name="frameHeader">Bytes representing an MP3 frame header.</param>
        /// <returns>true if the supplied bytes are a valid frame header, otherwise, false.</returns>
        public static bool IsValidFrame(byte[] frameHeader)
        {
            if (frameHeader == null)
            {
                throw new ArgumentNullException("frameHeader");
            }

            if (frameHeader.Length != 4)
            {
                throw new ArgumentException("frameHeader must be of length 4.");
            }

            int value = BitTools.MaskBits(frameHeader, 0, 11);
            if (value != SyncValue)
            {
                return false;
            }

            int version = ParseVersion(frameHeader);
            int layer = ParseLayer(frameHeader);
            int bitrateIndex = BitTools.MaskBits(frameHeader, 16, 4);
            int samplingRateIndex = BitTools.MaskBits(frameHeader, 20, 2);
            if ((version == -1) || (layer == -1) ||
                (bitrateIndex < 0) || (bitrateIndex >= 15) ||
                (samplingRateIndex < 0) || (samplingRateIndex >= 3))
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
            MpegFrame other = obj as MpegFrame;

            if (other == null)
            {
                return false;
            }

            return (this.Version == other.Version) &&
                (this.Layer == other.Layer) &&
                (this.SamplingRate == other.SamplingRate) &&
                (this.Channels == other.Channels);
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
            hash = (hash * Prime) + this.Version;
            hash = (hash * Prime) + this.Layer;
            hash = (hash * Prime) + this.SamplingRate;
            hash = (hash * Prime) + (int)this.Channels;
            return hash;
        }

        /// <summary>
        /// Converts the MpegFrame into a human readable form.
        /// </summary>
        /// <returns>
        /// A textual representation of the MpegFrame.
        /// </returns>
        public override string ToString()
        {
            string s = string.Empty;
            s += "FrameSize\t" + this.FrameSize + "\n";
            s += "BitRate\t" + this.BitRate + "\n";
            s += "SamplingRate" + this.SamplingRate + "\n";
            return s;
        }

        /**********************************************************************
         * DERIVED DATA - data which is calculated from data in the header.
         *********************************************************************/
        #region Derived Data

        /// <summary>
        /// Calculates the bit rate of the Mp3 audio from the data in the frame header.
        /// </summary>
        /// <param name="version">Mp3 version parsed out of the audio frame header.</param>
        /// <param name="layer">Mp3 layer parsed out of the audio frame header.</param>
        /// <param name="bitRateIndex">Mp3 Bit rate index parsed out of the audio frame header.</param>
        /// <returns>Mp3 bit rate calculated from the provided values, if valid.  Otherwise, -2 is returned.</returns>
        private static int CalculateBitRate(int version, int layer, int bitRateIndex)
        {
            switch (version)
            {
                case 1: // Version 1.0
                    switch (layer)
                    {
                        case 1: // MPEG 1 Layer 1
                            return bitrateTable[0, bitRateIndex] * 1000;
                        case 2: // MPEG 1 Layer 2
                            return bitrateTable[1, bitRateIndex] * 1000;
                        case 3: // MPEG 1 Layer 3 (MP3)
                            return bitrateTable[2, bitRateIndex] * 1000;
                        default: // MPEG 1 LAYER ERROR
                            return -2;
                    }

                case 2: // Version 2.0
                case 3: // Version 2.5 in reality
                    switch (layer)
                    {
                        case 1: // MPEG 2 or 2.5 Layer 1
                            return bitrateTable[3, bitRateIndex] * 1000;
                        case 2: // MPEG 2 or 2.5 Layer 2
                        case 3: // MPEG 2 or 2.5 Layer 3
                            return bitrateTable[4, bitRateIndex] * 1000;
                        default: // Mpeg 2 LAYER ERROR
                            return -2;
                    }

                default: // VERSION ERROR
                    return -2;
            }
        }

        /// <summary>
        /// Looks up the sampling rate of the Mp3 audio from the data in the frame header.
        /// </summary>
        /// <param name="version">Mp3 version parsed out of the audio frame header.</param>
        /// <param name="samplingRateIndex">Mp3 sampling rate index parsed out of the audio frame header.</param>
        /// <returns>Mp3 sampling rate for the provided version and sampling rate index, if valid.  Otherwise, -1 is returned.</returns>
        private static int LookupSamplingRate(int version, int samplingRateIndex)
        {
            switch (version)
            {
                case 1: // MPEG 1
                    return samplingRateTable[0, samplingRateIndex];
                case 2: // MPEG 2
                    return samplingRateTable[1, samplingRateIndex];
                case 3: // MPEG 2.5
                    return samplingRateTable[2, samplingRateIndex];
                default:
                    return -1; // RESERVED
            }
        }

        /// <summary>
        /// Calculates the frame size given the header information from the Mp3 frame.
        /// </summary>
        /// <param name="version">Mp3 version.</param>
        /// <param name="layer">Mp3 layer.</param>
        /// <param name="bitRate">Mp3 bit rate.</param>
        /// <param name="samplingRate">Mp3 sampling rate.</param>
        /// <param name="padding">Mp3 padding.</param>
        /// <returns>Mp3 frame size calculated from the provided values, if valid.  Otherwise, -1 is returned.</returns>
        private static int CalculateFrameSize(int version, int layer, int bitRate, int samplingRate, int padding)
        {
            switch (layer)
            {
                case 1:
                    return ((12 * bitRate / samplingRate) + padding) * 4;
                case 2:
                case 3:
                    // MPEG2 is a special case here.
                    switch (version)
                    {
                        case 1:
                            return (144 * bitRate / samplingRate) + padding;
                        case 2:
                        case 3:
                            return (72 * bitRate / samplingRate) + padding;
                        default:
                            return -1;
                    }

                default:
                    return -1;
            }
        }

        #endregion

        /// <summary>
        /// Parses the version of the MPEG standard this frame header conforms to from the frame header.
        /// </summary>
        /// <param name="frameHeader"> The 4 byte header for this frame. </param>
        /// <returns>
        /// The version of the MPEG standard this frame conforms to.
        /// 1 = Mpeg 1
        /// 2 = Mpeg 2
        /// 3 = Mpeg 2.5
        /// </returns>
        private static int ParseVersion(byte[] frameHeader)
        {
            int version;
            int versionValue = BitTools.MaskBits(frameHeader, 11, 2);

            switch (versionValue)
            {
                case 3:
                    version = 1;
                    break;
                case 2:
                    version = 2;
                    break;
                case 0:
                    version = 3;
                    break;
                default:
                    // This indicates an invalid version.
                    version = -1;
                    break;
            }

            return version;
        }

        /// <summary>
        /// Parses which complexity layer of the MPEG standard this frame conforms to from the frame header.
        /// </summary>
        /// <param name="frameHeader">The 4 byte header for this frame.</param>
        /// <returns>The complexity layer this frame conforms to.</returns>
        private static int ParseLayer(byte[] frameHeader)
        {
            int layer;
            int layerValue = BitTools.MaskBits(frameHeader, 13, 2);

            switch (layerValue)
            {
                case 3:
                    layer = 1;
                    break;
                case 2:
                    layer = 2;
                    break;
                case 1:
                    layer = 3;
                    break;
                default:
                    // This indicates an invalid layer.
                    layer = -1;
                    break;
            }

            return layer;
        }

        /// <summary>
        /// Parses the audio output mode of this frame's audio data.
        /// </summary>
        /// <param name="frameHeader">The 4 byte header for this frame.</param>
        /// <returns>The audio output mode of this frame's audio data.</returns>
        private static Channel ParseChannel(byte[] frameHeader)
        {
            Channel channel;
            int channelValue = BitTools.MaskBits(frameHeader, 24, 2);

            switch (channelValue)
            {
                case 3:
                    channel = Channel.SingleChannel;
                    break;
                case 2:
                    channel = Channel.DualChannel;
                    break;
                case 1:
                    channel = Channel.JointStereo;
                    break;
                case 0:
                    channel = Channel.Stereo;
                    break;
                default:
                    channel = Channel.SingleChannel; // ERROR CASE
                    break;
            }

            return channel;
        }
    }
}