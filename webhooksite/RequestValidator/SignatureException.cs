using System;

namespace webhooksite.RequestValidator
{
    [Serializable]
    public class SignatureException : Exception
    {
        public SignatureException() { }
        public SignatureException(string message) : base(message) { }
        public SignatureException(string message, Exception inner) : base(message, inner) { }
        protected SignatureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}