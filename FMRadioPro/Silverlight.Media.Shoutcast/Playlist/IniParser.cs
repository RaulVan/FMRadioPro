//-----------------------------------------------------------------------
// <copyright file="IniParser.cs" company="Andrew Oakley">
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
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses INI file format.
    /// </summary>
    public class IniParser
    {
        /// <summary>
        /// Regex to parse comment lines.
        /// </summary>
        private static Regex iniComment = new Regex(@"^\s*;.");

        /// <summary>
        /// Regex to parse section names.
        /// </summary>
        private static Regex iniSectionName = new Regex(@"^\[(?<name>[\w\s]*)\]$");

        /// <summary>
        /// Regex to parse key/value pairs.
        /// </summary>
        private static Regex iniKeyValue = new Regex(@"^(?<key>[\w\s]*)=(?<value>.*)");

        /// <summary>
        /// Field to store duplicate name handling enumeration.
        /// </summary>
        private DuplicateNameHandling duplicateNameHandling;

        /// <summary>
        /// Dictionary to store ini file sections and their associated key/value pairs.
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the IniParser class.
        /// </summary>
        /// <param name="textReader">TextReader representing an ini file.</param>
        public IniParser(TextReader textReader)
            : this(textReader, DuplicateNameHandling.Abort)
        {
        }

        /// <summary>
        /// Initializes a new instance of the IniParser class.
        /// </summary>
        /// <param name="textReader">TextReader representing an ini file.</param>
        /// <param name="duplicateNameHandling">Specifies how IniParser will handle duplicate names.</param>
        public IniParser(TextReader textReader, DuplicateNameHandling duplicateNameHandling)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            this.duplicateNameHandling = duplicateNameHandling;
            this.Parse(textReader);
        }

        /// <summary>
        /// Gets the sections from the ini file containing name/value pairs.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Sections
        {
            get { return this.sections; }
        }

        /// <summary>
        /// Parses the ini file.
        /// </summary>
        /// <param name="textReader">TextReader representing an ini file.</param>
        private void Parse(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            int lineNumber = 0;
            string line = null;
            Dictionary<string, string> currentSection = null;

            while ((line = textReader.ReadLine()) != null)
            {
                lineNumber++;

                // Skip blank lines and comments
                if (string.IsNullOrEmpty(line) || IniParser.iniComment.IsMatch(line))
                {
                    continue;
                }

                Match match = IniParser.iniSectionName.Match(line);
                if (match.Success)
                {
                    if (this.sections.ContainsKey(match.Groups["name"].Value))
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Section name already exists: {0}", match.Groups["name"].Value));
                    }

                    currentSection = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                    this.sections.Add(match.Groups["name"].Value, currentSection);
                }
                else
                {
                    // Not a section, so maybe a key/value
                    match = IniParser.iniKeyValue.Match(line);
                    if (match.Success)
                    {
                        // If we have a null current section, the file format is invalid
                        if (currentSection == null)
                        {
                            throw new InvalidOperationException("No current section");
                        }

                        if (currentSection.ContainsKey(match.Groups["key"].Value))
                        {
                            if (this.duplicateNameHandling == DuplicateNameHandling.Abort)
                            {
                                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Duplicate key: {0}", match.Groups["key"].Value));
                            }
                            else if (this.duplicateNameHandling == DuplicateNameHandling.Overwrite)
                            {
                                currentSection[match.Groups["key"].Value] = match.Groups["value"].Value;
                            }
                            else if (this.duplicateNameHandling == DuplicateNameHandling.Discard)
                            {
                                // Just in case we need to add something in this case.
                                continue;
                            }
                        }

                        currentSection.Add(match.Groups["key"].Value, match.Groups["value"].Value);
                    }
                    else
                    {
                        // Invalid format
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid line #: {0}", lineNumber));
                    }
                }
            }
        }
    }
}
