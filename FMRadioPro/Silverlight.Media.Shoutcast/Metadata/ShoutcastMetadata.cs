//-----------------------------------------------------------------------
// <copyright file="ShoutcastMetadata.cs" company="Andrew Oakley">
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

namespace Silverlight.Media.Metadata
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parses MP3 stream metadata.
    /// </summary>
    public class ShoutcastMetadata
    {
        /// <summary>
        /// Key for the stream title.
        /// </summary>
        private const string StreamTitle = "StreamTitle";

        /// <summary>
        /// Key for the stream url.
        /// </summary>
        private const string StreamUrl = "StreamUrl";

        /// <summary>
        /// Dictionary&lt;string, string&gt; to store the parsed metadata key/value pairs.
        /// </summary>
        private Dictionary<string, string> metadatas = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the ShoutcastMetadata class.
        /// </summary>
        public ShoutcastMetadata()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShoutcastMetadata class.
        /// </summary>
        /// <param name="metadata">String representing the MP3 stream metadata.</param>
        public ShoutcastMetadata(string metadata)
        {
            this.Title = string.Empty;
            this.Url = string.Empty;

            // We'll parse in here for now.
            if (string.IsNullOrEmpty(metadata))
            {
                return;
            }

            this.ParseMetadata(metadata);
        }

        /// <summary>
        /// Gets a value representing the title from the audio stream metadata.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets a value representing the url from the audio stream metadata.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified Object is equal to the current Object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            // We need value-type semantics.
            ShoutcastMetadata other = obj as ShoutcastMetadata;
            if (other == null)
            {
                return false;
            }

            return this.Title.Equals(other.Title) && this.Url.Equals(other.Url);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current Object.</returns>
        public override int GetHashCode()
        {
            return this.Title.GetHashCode() ^ this.Url.GetHashCode();
        }

        /// <summary>
        /// Parses the metadata from the MP3 audio stream.
        /// </summary>
        /// <param name="metadata">String representing the MP3 stream metadata.</param>
        private void ParseMetadata(string metadata)
        {
            if (string.IsNullOrEmpty(metadata))
            {
                return;
            }

            // I'm bored, so we'll use some LINQ. :)
            this.metadatas = metadata.Replace("\0", string.Empty).Split(';').Where(s => (!string.IsNullOrEmpty(s)) && (s.IndexOf('=') > -1)).Select(s =>
            {
                int equalSignIndex = s.IndexOf('=');
                string key = s.Substring(0, equalSignIndex);
                string value = s.Length > equalSignIndex ? s.Substring(equalSignIndex + 1).Trim('\'') : string.Empty;
                return new { Key = key, Value = value };
            }).ToDictionary(a => a.Key, a => a.Value);

            // Parse out the known values
            string metadataValue;

            if (this.metadatas.TryGetValue(ShoutcastMetadata.StreamTitle, out metadataValue))
            {
                this.Title = metadataValue;
            }

            if (this.metadatas.TryGetValue(ShoutcastMetadata.StreamUrl, out metadataValue))
            {
                this.Url = metadataValue;
            }
        }
    }
}
