using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace webhooksite.RequestValidator
{
    public static class RequestSignatureValidatorExtensions
    {
        public static IApplicationBuilder UseSignatureValidation(this IApplicationBuilder builder, Action<RequestSignatureOptions> configure)
        {
            var options = new RequestSignatureOptions
            {
                MaxAllowedTimeSeconds = 60,
                Keys = new Dictionary<string, byte[]>(),
            };

            configure(options);
            return builder.UseMiddleware<RequestSignatureValidator>(options);
        }
    }
}