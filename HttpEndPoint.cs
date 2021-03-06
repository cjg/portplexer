﻿/* HttpEndPoint.cs */

/* Portplexer -- TCP port multiplexer
*
* Copyright (C) 2009
*     Giuseppe Coviello <cjg@cruxppc.org>
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Portplexer
{
    class HttpEndPoint : EndPoint
    {
        private string hostname;
        private int port;

        public HttpEndPoint(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }

        public override bool IsValid(Stream stream)
        {
            byte[] buffer = new byte[1024];
            stream.Peek(buffer, 0, 1024);
            Encoding enc = Encoding.ASCII;
            string s = enc.GetString(buffer);
            if (s.StartsWith("GET") || s.StartsWith("POST"))
                return true;
            return false;
        }

        public override TcpClient Connect()
        {
            TcpClient client = new TcpClient();
            client.Connect(hostname, port);
            return client;
        }
    }
}
