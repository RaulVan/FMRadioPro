//-----------------------------------------------------------------------
// <copyright file="PlsParser.cs" company="Andrew Oakley">
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
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Parses the PLS playlist format.
    /// </summary>
    public class PlsParser : IPlaylistParser
    {
        /// <summary>
        /// Content type of the PLS playlist format.
        /// </summary>
        internal const string PlsContentType = "audio/x-scpls";

        /// <summary>
        /// Initializes a new instance of the PlsParser class.
        /// </summary>
        public PlsParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PlsParser class.
        /// </summary>
        /// <param name="textReader">TextReader representing a PLS playlist file.</param>
        public PlsParser(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            this.Parse(textReader);
        }

        /// <summary>
        /// Gets the supported content type of the PLS playlist format.
        /// </summary>
        public string ContentType
        {
            get { return PlsParser.PlsContentType; }
        }

        /// <summary>
        /// Parses the PLS file.
        /// </summary>
        /// <param name="stream">Stream representing a PLS file.</param>
        /// <returns>A successfully parsed playlist.</returns>
        public IPlaylist Parse(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return this.Parse(new StreamReader(stream));
        }

        /// <summary>
        /// Parses the PLS file.
        /// </summary>
        /// <param name="textReader">TextReader representing a PLS playlist file.</param>
        /// <returns>A successfully parsed playlist.</returns>
        private IPlaylist Parse(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            // Shoutcast.com PLS files are messed up.  The LengthX values are all Length1=-1 instead of LengthX=-1.
            IniParser iniFile = new IniParser(textReader, DuplicateNameHandling.Discard);
            Dictionary<string, Dictionary<string, string>> sections = iniFile.Sections;

            if (!sections.ContainsKey("playlist"))
            {
                throw new InvalidOperationException("playlist section not found");
            }

            PlsPlaylist playlist = new PlsPlaylist();
            ICollection<IPlaylistItem> items = playlist.Items;

            Dictionary<string, string> playlistEntries = sections["playlist"];

            int numberOfEntries;
            if ((!playlistEntries.ContainsKey("NumberOfEntries")) || (!int.TryParse(playlistEntries["NumberOfEntries"], out numberOfEntries)))
            {
                throw new InvalidOperationException("NumberOfEntries key missing or not a valid integer.");
            }

            for (int i = 1; i <= numberOfEntries; i++)
            {
                string fileKey = string.Format(CultureInfo.InvariantCulture, "File{0}", i);
                string titleKey = string.Format(CultureInfo.InvariantCulture, "Title{0}", i);
                string lengthKey = string.Format(CultureInfo.InvariantCulture, "Length{0}", i);

                if (!playlistEntries.ContainsKey(fileKey))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Missing file key: {0}", fileKey));
                }

                int lengthInSeconds = -1;

                if (playlistEntries.ContainsKey(lengthKey))
                {
                    // We don't really care if this works or not
                    int.TryParse(playlistEntries[lengthKey], out lengthInSeconds);
                }

                items.Add(new M3uPlaylistItem()
                {
                    DisplayName = playlistEntries.ContainsKey(titleKey) ? playlistEntries[titleKey] : string.Empty,
                    Length = new TimeSpan(0, 0, lengthInSeconds),
                    Path = playlistEntries[fileKey]
                });
            }

            return playlist;
        }
    }
}
