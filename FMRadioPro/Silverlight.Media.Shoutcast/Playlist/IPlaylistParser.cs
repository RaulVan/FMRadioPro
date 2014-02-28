//-----------------------------------------------------------------------
// <copyright file="IPlaylistParser.cs" company="Andrew Oakley">
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
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Interface representing a audio stream playlist parser.
    /// </summary>
    public interface IPlaylistParser
    {
        /// <summary>
        /// Gets the Internet content type supported by this playlist parser.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Parses the supplied Stream into an IPlaylist instance.
        /// </summary>
        /// <param name="stream">Stream representing the playlist data.</param>
        /// <returns>Successfully parsed IPlaylist instance.</returns>
        IPlaylist Parse(Stream stream);
    }
}
