using System;
using System.Collections.Generic;
using System.Text;

namespace OrbitalShell.Commands.Http
{
    /// <summary>
    /// http get result content
    /// </summary>
    public class HttpContentBody
    {
        public static readonly HttpContentBody EmptyHttpContentBody = new HttpContentBody(null);

        public string Content;

        public HttpContentBody(string content)
        {
            Content = content;
        }
    }
}
