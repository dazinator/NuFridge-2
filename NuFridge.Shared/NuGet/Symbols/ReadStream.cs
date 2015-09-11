﻿/* 
* @author themotleyfool https://github.com/themotleyfool/NuGet.Lucene
* Apache License
* Version 2.0, January 2004
* http://www.apache.org/licenses/
*
* Copyright 2008-2012 by themotleyfool
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NuFridge.Shared.NuGet.Symbols
{
    public abstract class ReadStream : Stream
    {
        private readonly Stream stream;

        protected ReadStream(Stream stream)
        {
            this.stream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return stream.EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return stream.ReadAsync(buffer, offset, count, cancellationToken);
        }
        
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int ReadByte()
        {
            return stream.ReadByte();
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        public override long Length
        {
            get { return stream.Length; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }
        }

        #region NotSupported
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }
        #endregion

    }
}
