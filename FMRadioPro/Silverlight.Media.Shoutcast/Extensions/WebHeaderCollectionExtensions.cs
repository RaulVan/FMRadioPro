//-----------------------------------------------------------------------
// <copyright file="WebHeaderCollectionExtensions.cs" company="Andrew Oakley">
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

namespace Silverlight.Media.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    /// <summary>
    /// Extension methods for the WebHeaderCollection class.
    /// </summary>
    public static class WebHeaderCollectionExtensions
    {
        /// <summary>
        /// Converts a WebHeaderCollection to an IDictionary&lt;string, string&gt;.
        /// </summary>
        /// <param name="webHeaderCollection">WebHeaderCollection to convert to an IDictionary&lt;string, string&gt;.</param>
        /// <returns>IDictionary&lt;string, string&gt; representing the provided WebHeaderCollection.</returns>
        public static IDictionary<string, string> ToDictionary(this WebHeaderCollection webHeaderCollection)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (webHeaderCollection != null)
            {
                foreach (string key in webHeaderCollection.AllKeys)
                {
                    headers.Add(key, webHeaderCollection[key]);
                }
            }

            return headers;
        }
    }
}
