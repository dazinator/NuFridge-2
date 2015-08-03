﻿using System;

namespace NuFridge.Shared.Server.Storage
{
    public class DelegateReaderWriter<TTarget, TProperty> : IPropertyReaderWriter<object>
    {
        private readonly Func<TTarget, TProperty> _reader;
        private readonly Action<TTarget, TProperty> _writer;

        public DelegateReaderWriter(Func<TTarget, TProperty> reader)
            : this(reader, null)
        {
        }

        public DelegateReaderWriter(Func<TTarget, TProperty> reader, Action<TTarget, TProperty> writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public bool Read(object target, out object value)
        {
            if (_reader == null)
            {
                value = null;
                return false;
            }

            value = _reader((TTarget)target);
            return true;
        }

        public void Write(object target, object value)
        {
            if (_writer == null)
                return;
            if (value != DBNull.Value)
            {
                _writer((TTarget) target, (TProperty) value);
            }
        }
    }
}
