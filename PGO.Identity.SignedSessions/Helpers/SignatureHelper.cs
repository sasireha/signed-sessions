    using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using PGO.Identity.SignedSessions.Session;

namespace PGO.Identity.SignedSessions.Signature
{
    public class SignatureHelper : ISignatureHelper
    {
        public Encoding Encoding => Encoding.UTF8;

        private readonly SessionConfig _sessionConfig;

        public SignatureHelper(IOptions<SessionConfig> sessionConfig)
        {
            _sessionConfig = sessionConfig.Value;
        }

        public string CreateSignature(byte[] message)
        {
            if (message == null)
                return string.Empty;

            byte[] keyByte = Encoding.GetBytes(_sessionConfig.HmacSecretKey);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(message);
                return Convert.ToBase64String(hashmessage);
            }
        }

        public bool VerifySignature(string unsignedValue, string signature)
        {
            string computedSignature;
            if (!string.IsNullOrEmpty(unsignedValue) && !string.IsNullOrEmpty(signature))
            {
                computedSignature = CreateSignature(Convert.FromBase64String(unsignedValue));
                if (computedSignature.Equals(signature))
                    return true;
            }
            return false;
        }
    }
}
