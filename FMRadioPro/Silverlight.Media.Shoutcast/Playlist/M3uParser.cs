//-----------------------------------------------------------------------
// <copyright file="M3uParser.cs" company="Andrew Oakley">
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
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses M3U playlist.
    /// </summary>
    public class M3uParser : IPlaylistParser
    {
        /// <summary>
        /// Content type of the m3u playlist format.
        /// </summary>
        internal const string M3uContentType = "audio/x-mpegurl";

        /// <summary>
        /// M3U Extended Header tag.
        /// </summary>
        private const string M3uExtendedHeader = "#EXTM3U";

        /// <summary>
        /// M3U Extended Detail tag.
        /// </summary>
        private const string M3uExtendedDetail = "#EXTINF";

        /// <summary>
        /// Regex to parse M3U Extended Detail.
        /// </summary>
        private static Regex extendedDetailRegex = new Regex(M3uParser.M3uExtendedDetail + @":(?<seconds>[\+-]?\d+),(?<file>.*)");

        /// <summary>
        /// Initializes a new instance of the M3uParser class.
        /// </summary>
        public M3uParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the M3uParser class.
        /// </summary>
        /// <param name="textReader">TextReader representing an M3U file.</param>
        public M3uParser(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            this.Parse(textReader);
        }

        /// <summary>
        /// Gets the supported content type of the M3U playlist format.
        /// </summary>
        public string ContentType
        {
            get { return M3uParser.M3uContentType; }
        }

        /// <summary>
        /// Parses the M3U file.
        /// </summary>
        /// <param name="stream">Stream representing a M3U file.</param>
        /// <returns>Parsed M3U playlist.</returns>
        public IPlaylist Parse(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return this.Parse(new StreamReader(stream));
        }

        /// <summary>
        /// Parses the M3U playlist.
        /// </summary>
        /// <param name="textReader">TextReader representing the M3U playlist.</param>
        /// <returns>Parsed M3U playlist.</returns>
        private IPlaylist Parse(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            string line = textReader.ReadLine();

            if (line == null)
            {
                throw new ArgumentException("Invalid M3U playlist.");
            }

            M3uPlaylist playlist = new M3uPlaylist();
            ICollection<IPlaylistItem> items = playlist.Items;

            bool isExtended = line.Equals(M3uParser.M3uExtendedHeader);

            if (isExtended)
            {
                while ((line = textReader.ReadLine()) != null)
                {
                    string extendedDetail = line;
                    string detail = textReader.ReadLine();

                    if ((extendedDetail == null) || (detail == null))
                    {
                        throw new Exception("File is malformed");
                    }

                    Match match = M3uParser.extendedDetailRegex.Match(extendedDetail);
                    if (!match.Success)
                    {
                        throw new Exception("Invalid m3u extended detail line");
                    }

                    items.Add(new M3uPlaylistItem()
                    {
                        DisplayName = match.Groups["file"].Value,
                        Path = detail,
                        Length = new TimeSpan(0, 0, int.Parse(match.Groups["seconds"].Value))
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(line))
                {
                    items.Add(new M3uPlaylistItem()
                    {
                        DisplayName = line,
                        Path = line,
                        Length = new TimeSpan(0, 0, -1)
                    });
                }

                while ((line = textReader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    items.Add(new M3uPlaylistItem()
                    {
                        DisplayName = line,
                        Path = line,
                        Length = new TimeSpan(0, 0, -1)
                    });
                }
            }

            return playlist;
        }
    }
}
