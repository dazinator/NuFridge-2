using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NuFridge.Shared.Server.Web.Batch
{
    internal static class RequestLineParser
    {
        private const int Cr = '\r';
        private const int Lf = '\n';

        public static RequestLine Parse(Stream stream)
        {
            int b = stream.ReadByte();
            while (b == Cr || b == Lf)
            {
                b = stream.ReadByte();
            }

            var bytes = new LinkedList<byte>();
            bytes.AddLast((byte)b);

            while (true)
            {
                b = stream.ReadByte();
                if (b == Cr || b < 0)
                {
                    stream.ReadByte();
                    break;
                }
                bytes.AddLast((byte)b);
            }

            var text = Encoding.Default.GetString(bytes.ToArray());
            var parts = text.Split(' ');

            if (parts.Length == 3)
            {
                return new RequestLine(parts[0], parts[1], parts[2]);
            }

            throw new InvalidOperationException("Invalid Request Line.");
        }
    }
}
