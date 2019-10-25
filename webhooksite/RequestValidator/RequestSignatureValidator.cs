using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace webhooksite.RequestValidator
{
    public class RequestSignatureValidator
    {
        private readonly RequestDelegate _next;
        private readonly RequestSignatureOptions _options;
        private readonly ILogger _logger;

        public RequestSignatureValidator(RequestDelegate next, RequestSignatureOptions options, ILogger<RequestSignatureValidator> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        private string GetDigest(string digest, MemoryStream body)
        {
            using (var sha = new SHA256Managed())
            {
                var match = Regex.Match(digest, "^SHA-256=(.+)");
                if (!match.Success)
                    throw new SignatureException("Cannot find SHA-256 digest header.");

                var requesthash = match.Groups[1].Value;
                body.Position = 0;
                var datahash = Convert.ToBase64String(sha.ComputeHash(body));
                body.Position = 0;

                if (requesthash != datahash)
                    throw new SignatureException($"Message SHA-256 digest hash '{requesthash}' doesn't match with data hash '{datahash}'.");
                return "SHA-256=" + datahash;
            }
        }

        private Dictionary<string, string> ParseSignatureHeader(string signature_header)
        {
            var matches = Regex.Matches(signature_header, @"(keyId|algorithm|headers|signature)=""([^""]*)"",?");
            return matches.ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                var req = httpContext.Request;

                var host = _options.Host ?? req.Host.Host;

                var pathandquery = req.Path.Value;

                var sig_header = req.Headers["Signature"].FirstOrDefault();
                if (sig_header == null)
                {
                    await _next(httpContext);
                    return;
                }

                var buffer = new MemoryStream();
                using (req.Body)
                    await req.Body.CopyToAsync(buffer);

                req.Body = buffer;

                var sig_params = ParseSignatureHeader(sig_header);

                var date = DateTime.Parse(req.Headers["Date"].First(), null, DateTimeStyles.AdjustToUniversal);

                var now = DateTime.UtcNow;
                double ageSeconds = (date - now).TotalSeconds;

                if (ageSeconds > _options.MaxAllowedTimeSeconds)
                {
                    throw new SignatureException(
                        $"Query too old: '{date:r}', older than {ageSeconds} seconds from '{now:r}', " +
                        $"(maximum allowed is {_options.MaxAllowedTimeSeconds}).");
                }

                var sign_string =
                    $"(request-target): {req.Method.ToLowerInvariant()} {pathandquery}\n" +
                    $"host: {host}\n" +
                    $"date: {req.Headers["Date"]}\n" +
                    $"content-type: {req.Headers["Content-Type"]}\n" +
                    $"content-length: {req.Headers["Content-Length"]}\n" +
                    $"digest: {GetDigest(req.Headers["Digest"], buffer)}";


                string keyId = sig_params["keyId"];

                if (!_options.Keys.TryGetValue(keyId, out var key))
                    throw new SignatureException($"Key with ID '{keyId}' does not exist.");
                if (sig_params["algorithm"] != "hmac-sha256")
                    throw new SignatureException($"Unsupported algorithm: '{sig_params["algorithm"]}'.");

                using (var hmac = new HMACSHA256(key))
                {
                    var hmac_sha256 = hmac.ComputeHash(Encoding.UTF8.GetBytes(sign_string));
                    var sig_hash = Convert.ToBase64String(hmac_sha256);

                    if (sig_hash != sig_params["signature"])
                        throw new SignatureException(
                            $"Signature mismatch for key ID '{keyId}', computed '{sig_hash}', posted '{sig_params["signature"]}'");
                }
            }
            catch (SignatureException se)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync(se.Message);
                _logger.LogWarning($"Request validation failed: {se.Message}");
                return;
            }

            _logger.LogInformation("Request validation succeeded.");
            await _next(httpContext);
        }
    }
}