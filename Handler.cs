/* Handler.cs */

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
using System.Threading;

namespace Portplexer
{
    class Handler
    {
        private TcpClient client;
        private TcpClient server;
        private Stream client_stream;
        private Stream server_stream;

        public Handler(TcpClient client)
        {
            this.client = client;
            client_stream = new Stream(client.GetStream());
        }

        public void Start()
        {
            foreach(EndPoint e in EndPoint.EndPoints)
            {
                if (e.IsValid(client_stream))
                {
                    server = e.Connect();
                    server_stream = new Stream(server.GetStream());
                    break;
                }
            }
            if(server == null)
                throw new Exception("No valid EndPoint found!");
            server_stream.DataReceived += ServerDataReceivedEventHandler;
            client_stream.DataReceived += ClientDataReceivedEventHandler;
            server_stream.BeginAsyncRead();
            client_stream.BeginAsyncRead();
        }

        private void ServerDataReceivedEventHandler(object sender, byte[] buffer, int size)
        {
            client_stream.Write(buffer, 0, size);
        }

        private void ClientDataReceivedEventHandler(object sender, byte[] buffer, int size)
        {
            server_stream.Write(buffer, 0, size);
        }
    }
}
