/* Stream.cs */

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
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Portplexer
{
    class Stream
    {
        private NetworkStream stream;
        private byte[] buffer;
        private int buffer_offset;
        private int buffer_size;
        private object mutex;

        public delegate void DataReceivedEventHandler(object sender, byte[] buffer, int size);
        public event DataReceivedEventHandler DataReceived;

        public Stream(NetworkStream stream)
        {
            this.stream = stream;
            buffer = new byte[2048];
            buffer_offset = 0;
            buffer_size = 0;
            mutex = new object();
        }

        public int Read(byte[] buffer, int offset, int size)
        {
            lock (mutex)
            {
                size = Peek(buffer, offset, size);
                buffer_size -= size;
                buffer_offset += size;
                return size;
            }
        }

        public int Peek(byte[] buffer, int offset, int size)
        {
            lock (mutex)
            {
                if (size > buffer_size)
                {
                    try
                    {
                        size = BufferRead(size - buffer_size);
                    }
                    catch (System.IO.IOException e)
                    {
                        return 0;
                    }
                }
                for (int i = 0; i < size; i++)
                    buffer[i + offset] = this.buffer[i + buffer_offset];
                return size;
            }
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            stream.Write(buffer, offset, size);
        }

        public void Close()
        {
            stream.Close();
        }

        public void BeginAsyncRead()
        {
            Thread t = new Thread(ReadContinous);
            t.Start();
        }

        private void BufferShift()
        {
            if (buffer_offset == 0)
                return;
            for (int i = 0; i < buffer_size; i++)
                buffer[i] = buffer[i + buffer_offset];
            buffer_offset = 0;
        }

        private int BufferRead(int size)
        {
            if(buffer_size + size > 1024)
                throw new Exception("Buffer overflow!");
            BufferShift();
            size = stream.Read(buffer, buffer_size, size);
            buffer_size += size;
            return buffer_size;
        }

        private void ReadContinous()
        {
            lock (mutex)
            {
                if(buffer_size != 0 && DataReceived != null) {
                    BufferShift();
                    DataReceived(this, buffer, buffer_size);
                }
                int size;
                while ((size = stream.Read(buffer, 0, 1024)) != 0)
                    if (DataReceived != null)
                        DataReceived(this, buffer, size);
            }
        }
    }
}
