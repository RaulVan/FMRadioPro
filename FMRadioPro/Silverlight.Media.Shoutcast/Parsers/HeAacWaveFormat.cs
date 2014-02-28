//-----------------------------------------------------------------------
// <copyright file="HeAacWaveFormat.cs" company="Andrew Oakley">
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
    using System.Globalization;

    /// <summary>
    /// A managed representation of the multimedia HEAACWAVEINFO 
    /// structure declared in mmreg.h.
    /// </summary>
    /// <remarks>
    /// This was designed for usage in an environment where PInvokes are not
    /// allowed.
    /// </remarks>
    public class HeAacWaveFormat : WaveFormat
    {
        /// <summary>
        /// Initializes a new instance of the HeAacWaveFormat class.
        /// </summary>
        /// <param name="waveFormatExtensible">WaveFormatExtensible instance representing this audio format.</param>
        public HeAacWaveFormat(WaveFormatExtensible waveFormatExtensible)
            : base(waveFormatExtensible)
        {
        }

        /// <summary>
        /// Gets or sets the the AAC payload type.
        /// </summary>
        public short PayloadType { get; set; }

        /// <summary>
        /// Gets or sets the audio profile indication (as defined in the MPEG-4 audio specification) required to process the audio.
        /// </summary>
        public short AudioProfileLevelIndication { get; set; }

        /// <summary>
        /// Gets or sets the structure type that describes the data that follows this structure (per MPEG-4 audio specification).
        /// </summary>
        public short StructType { get; set; }

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
        public override string ToHexString()
        {
            string s = this.WaveFormatExtensible.ToHexString();
            s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.PayloadType).ToLittleEndian();
            s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.AudioProfileLevelIndication).ToLittleEndian();
            s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.StructType).ToLittleEndian();
            return s;
        }
    }
}
