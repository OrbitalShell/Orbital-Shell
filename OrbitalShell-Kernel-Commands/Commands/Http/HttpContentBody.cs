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
        public static readonly HttpContentBody EmptyHttpContentBody = new HttpContentBody();

        public object Content;

        public HttpContentBody() { }

        public HttpContentBody(object content)
        {
            Content = content;
        }

        public HttpContentBody(byte[] data)
        {
            Content = data;
        }
    }
}
