//-----------------------------------------------------------------------
// <copyright file="WaveFormat.cs" company="Andrew Oakley">
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
    /// Base class for wave format support.
    /// </summary>
    public abstract class WaveFormat
    {
        /// <summary>
        /// Initializes a new instance of the WaveFormat class.
        /// </summary>
        /// <param name="waveFormatExtensible">WaveFormatExtensible instance representing an audio format.</param>
        public WaveFormat(WaveFormatExtensible waveFormatExtensible)
        {
            if (waveFormatExtensible == null)
            {
                throw new ArgumentNullException("waveFormatExtensible");
            }

            this.WaveFormatExtensible = waveFormatExtensible;
        }

        /// <summary>
        /// Gets the core WaveFormatExtensible strucutre representing the Mp3 audio data's
        /// core attributes. 
        /// </summary>
        /// <remarks>
        /// wfx.FormatTag must be WAVE_FORMAT_MPEGLAYER3 = 0x0055 = (85)
        /// wfx.Size must be >= 12
        /// </remarks>
        public WaveFormatExtensible WaveFormatExtensible { get; private set; }

        /// <summary>
        /// Returns a string representing the structure in little-endian 
        /// hexadecimal format.
        /// </summary>
        /// <remarks>
        /// The string generated here is intended to be passed as 
        /// CodecPrivateData for Silverlight's MediaStreamSource
        /// </remarks>
        /// <returns>
        /// A string representing the structure in little-endia hexadecimal
        /// format.
        /// </returns>
        public abstract string ToHexString();

        /// <summary>          
        /// Calculate the duration of audio based on the size of the buffer          
        /// </summary>          
        /// <param name="audioDataSize">the buffer size in bytes</param>          
        /// <returns>The duration of that buffer</returns>          
        public long AudioDurationFromBufferSize(uint audioDataSize)
        {
            if (this.WaveFormatExtensible.AverageBytesPerSecond == 0)
            {
                return 0;
            }

            return (long)audioDataSize * 10000000 / this.WaveFormatExtensible.AverageBytesPerSecond;
        }  
    }
}
