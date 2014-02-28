//-----------------------------------------------------------------------
// <copyright file="ShoutcastStreamInformation.cs" company="Andrew Oakley">
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

namespace Silverlight.Media
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;

    /// <summary>
    /// Represents the metadata information about an MP3 stream.
    /// </summary>
    public class ShoutcastStreamInformation
    {
        /// <summary>
        /// Base name of the ICY Notice header tag.
        /// </summary>
        private const string IcyNoticeBase = "icy-notice";

        /// <summary>
        /// ICY Name header tag.
        /// </summary>
        private const string IcyName = "icy-name";

        /// <summary>
        /// ICY Genre header tag.
        /// </summary>
        private const string IcyGenre = "icy-genre";

        /// <summary>
        /// ICY Url header tag.
        /// </summary>
        private const string IcyUrl = "icy-url";

        /// <summary>
        /// ICY Public header tag.
        /// </summary>
        private const string IcyPublic = "icy-pub";

        /// <summary>
        /// ICY Bitrate headter tag.
        /// </summary>
        private const string IcyBitrate = "icy-br";

        /// <summary>
        /// ICY Metadata Interval header tag.
        /// </summary>
        private const string IcyMetadataInterval = "icy-metaint";

        /// <summary>
        /// List of ICY Notice header tags.
        /// </summary>
        private List<string> notices = new List<string>();

        /// <summary>
        /// Initializes a new instance of the ShoutcastStreamInformation class.
        /// </summary>
        /// <param name="headers">IDictionary&lt;string, string&gt; of HTTP headers.</param>
        public ShoutcastStreamInformation(IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            this.ParseHeaders(headers);
        }

        /// <summary>
        /// Gets the name of the MP3 stream.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the genre of the MP3 stream.
        /// </summary>
        public string Genre { get; private set; }

        /// <summary>
        /// Gets the url of the MP3 stream.
        /// </summary>
        public Uri Url { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not this MP3 stream is public.
        /// </summary>
        public bool IsPublic { get; private set; }

        /// <summary>
        /// Gets the bitrate of the MP3 stream.
        /// </summary>
        public int BitRate { get; private set; }

        /// <summary>
        /// Gets the metadata interval of the MP3 stream.
        /// </summary>
        public int MetadataInterval { get; private set; }

        /// <summary>
        /// Gets the notices of the MP3 stream..
        /// </summary>
        public ReadOnlyCollection<string> Notices
        {
            get { return new ReadOnlyCollection<string>(this.notices); }
        }

        /// <summary>
        /// Gets the HTTP content type.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Parses the supplied HTTP headers.
        /// </summary>
        /// <param name="headers">IDictionary&lt;string, string&gt; of HTTP headers.</param>
        private void ParseHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            // Get the notice headers.  While the normal number is 2, we'll support more, just in case.
            int i = 1;
            string value = null;
            while (headers.TryGetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", ShoutcastStreamInformation.IcyNoticeBase, i), out value))
            {
                this.notices.Add(value);
                i++;
            }

            if (headers.TryGetValue(ShoutcastStreamInformation.IcyName, out value))
            {
                this.Name = value;
            }

            if (headers.TryGetValue(ShoutcastStreamInformation.IcyGenre, out value))
            {
                this.Genre = value;
            }

            if (headers.TryGetValue(ShoutcastStreamInformation.IcyUrl, out value))
            {
                this.Url = new Uri(value);
            }

            if (headers.TryGetValue(ShoutcastStreamInformation.IcyPublic, out value))
            {
                this.IsPublic = value == "1";
            }

            if (headers.TryGetValue(ShoutcastStreamInformation.IcyBitrate, out value))
            {
                int bitRate = -1;
                if (int.TryParse(value, out bitRate))
                {
                    // Per Mp3 specs
                    bitRate *= 1000;
                }

                this.BitRate = bitRate;
            }

            if (headers.TryGetValue(ShoutcastStreamInformation.IcyMetadataInterval, out value))
            {
                int metadataInterval = -1;
                if (!int.TryParse(value, out metadataInterval))
                {
                    // TODO - Should this be an error?
                    // throw new ArgumentException("icy-metaint must be a valid integer");
                }

                this.MetadataInterval = metadataInterval;
            }

            if (headers.TryGetValue("Content-Type", out value))
            {
                this.ContentType = value;
            }
        }
    }
}
