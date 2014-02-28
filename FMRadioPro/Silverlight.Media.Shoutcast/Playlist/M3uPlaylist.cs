//-----------------------------------------------------------------------
// <copyright file="M3uPlaylist.cs" company="Andrew Oakley">
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
    using System.Collections.Generic;

    /// <summary>
    /// Represents an M3U playlist.
    /// </summary>
    public class M3uPlaylist : IPlaylist
    {
        /// <summary>
        /// M3U playlist items.
        /// </summary>
        private List<IPlaylistItem> items = new List<IPlaylistItem>();

        /// <summary>
        /// Gets a collection of the M3U playlist items.
        /// </summary>
        public ICollection<IPlaylistItem> Items
        {
            get { return this.items; }
        }
    }
}
