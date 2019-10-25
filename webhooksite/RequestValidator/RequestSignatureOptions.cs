using System.Collections.Generic;

namespace webhooksite.RequestValidator
{
    public class RequestSignatureOptions
    {
        public string Host { get; set; }
        public Dictionary<string, byte[]> Keys { get; set; }
        public int MaxAllowedTimeSeconds { get; set; }
    }
}