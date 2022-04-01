#nullable enable
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Traducir.Core.TransifexV3
{
    public sealed class TransifexException : Exception
    {
        public TransifexException()
            : this("Unrecognised failure", ImmutableList<TransifexClientError>.Empty)
        {
        }

        public TransifexException(string message)
            : this(message, ImmutableList<TransifexClientError>.Empty)
        {
        }

        public TransifexException(string message, Exception innerException)
            : this(message, ImmutableList<TransifexClientError>.Empty, innerException)
        {
        }

        public TransifexException(string message, ImmutableList<TransifexClientError> errors, Exception? innerException = null)
            : base($"{message}. Errors = [{string.Join(',', errors.Select(error => error.ToString()))}]", innerException)
                => Errors = errors;

        private TransifexException(SerializationInfo info, StreamingContext context)
            : base(info, context) =>
                Errors = ((TransifexClientError[])info.GetValue(nameof(Errors), typeof(TransifexClientError[]))!).ToImmutableList();

        public ImmutableList<TransifexClientError> Errors { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Errors), Errors.ToArray());
            base.GetObjectData(info, context);
        }
    }
}
