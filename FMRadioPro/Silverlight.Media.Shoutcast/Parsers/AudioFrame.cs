//-----------------------------------------------------------------------
// <copyright file="AudioFrame.cs" company="Andrew Oakley">
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
    /// <summary>
    /// Base class used to represent an audio frame.
    /// </summary>
    public class AudioFrame
    {
        /// <summary>
        /// Initializes a new instance of the AudioFrame class.
        /// </summary>
        protected AudioFrame()
        {
        }

        /// <summary>
        /// Gets or sets the number of channels of the audio frame.
        /// </summary>
        public int NumberOfChannels { get; protected set; }

        /// <summary>
        /// Gets or sets the bit rate of the audio frame.
        /// </summary>
        public int BitRate { get; protected set; }

        /// <summary>
        /// Gets or sets the sampling rate of the audio frame.
        /// </summary>
        public int SamplingRate { get; protected set; }

        /// <summary>
        /// Gets or sets the frame size of the audio frame.
        /// </summary>
        public int FrameSize { get; protected set; }
    }
}
