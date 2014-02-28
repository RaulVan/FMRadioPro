//-----------------------------------------------------------------------
// <copyright file="PlaylistFactory.cs" company="Andrew Oakley">
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

namespace Silverlight.Media.Playlist
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Factory to parse different playlist types.
    /// </summary>
    public static class PlaylistFactory
    {
        /// <summary>
        /// Factory method that parses a given Stream with the appropriate playlist type, based on the supplied content type.
        /// </summary>
        /// <param name="contentType">Internet content type representing the playlist type of the Stream.</param>
        /// <param name="stream">Stream representing the playlist data.</param>
        /// <returns>Successfully parsed playlist.</returns>
        public static IPlaylist Parse(string contentType, Stream stream)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("contentType cannot be null or empty.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            IPlaylistParser result;
            switch (contentType)
            {
                case M3uParser.M3uContentType:
                    result = new M3uParser();
                    break;
                case PlsParser.PlsContentType:
                    result = new PlsParser();
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid content type: {0}", contentType));
            }

            return result.Parse(stream);
        }
    }
}
