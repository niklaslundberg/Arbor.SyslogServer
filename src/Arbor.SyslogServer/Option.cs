using System;
using JetBrains.Annotations;

namespace Arbor.SyslogServer
{
    public struct Option<T> where T : class
    {
        public T Value
        {
            get
            {
                if (_value is null)
                {
                    throw new InvalidOperationException(
                        $"Missing value of type {typeof(T).FullName}, check {nameof(HasValue)} first");
                }

                return _value;
            }
        }

        public static implicit operator Option<T>(T value)
        {
            if (value is null)
            {
                return Empty;
            }

            return new Option<T>(value);
        }

        public bool HasValue => _value != null;

        private static readonly Lazy<Option<T>> _Lazy = new Lazy<Option<T>>(() => new Option<T>());
        private readonly T _value;

        public static readonly Option<T> Empty = _Lazy.Value;

        public Option([CanBeNull] T value)
        {
            _value = value;
        }

        public static Option<T> ToOption(T value)
        {
            if (value is null)
            {
                return Empty;
            }

            return new Option<T>(value);
        }
    }
}
