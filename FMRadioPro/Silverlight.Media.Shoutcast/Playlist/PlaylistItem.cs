﻿//-----------------------------------------------------------------------
// <copyright file="PlaylistItem.cs" company="Andrew Oakley">
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

    /// <summary>
    /// Represents a playlist entry.
    /// </summary>
    public class PlaylistItem : IPlaylistItem
    {
        /// <summary>
        /// Initializes a new instance of the PlaylistItem class.
        /// </summary>
        public PlaylistItem()
        {
        }

        /// <summary>
        /// Gets or sets the display name of the playlist entry.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the path of the playlist entry.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the length of the media represented by this playlist entry.
        /// </summary>
        public TimeSpan Length { get; set; }
    }
}
