using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HC.TechnicalCalculators.Src.Security
{
    /// <summary>
    /// Interface for string data protection operations
    /// </summary>
    public interface IStringDataProtector
    {
        /// <summary>
        /// Protects a string value using encryption
        /// </summary>
        /// <param name="data">The string to protect</param>
        /// <returns>Protected string data</returns>
        string ProtectString(string data);

        /// <summary>
        /// Unprotects a previously protected string
        /// </summary>
        /// <param name="protectedData">The protected string data</param>
        /// <returns>Original unprotected string</returns>
        string UnprotectString(string protectedData);
    }

    /// <summary>
    /// Interface for binary data protection operations
    /// </summary>
    public interface IBinaryDataProtector
    {
        /// <summary>
        /// Protects binary data using encryption
        /// </summary>
        /// <param name="data">The byte array to protect</param>
        /// <returns>Protected byte array</returns>
        byte[] ProtectBytes(byte[] data);

        /// <summary>
        /// Unprotects previously protected binary data
        /// </summary>
        /// <param name="protectedData">The protected byte array</param>
        /// <returns>Original unprotected byte array</returns>
        byte[] UnprotectBytes(byte[] protectedData);
    }

    /// <summary>
    /// Composite interface for complete data protection functionality
    /// Inherits from segregated interfaces to follow ISP while maintaining backward compatibility
    /// </summary>
    public interface ISecureDataService : IStringDataProtector, IBinaryDataProtector
    {
        // Interface now inherits from segregated interfaces
        // All methods are defined in the base interfaces
    }

    public class SecureDataService : ISecureDataService
    {
        private readonly ILogger<SecureDataService> _logger;
        private readonly byte[] _key;

        public SecureDataService(ILogger<SecureDataService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException("Logger can not be null");
            // In production, this should come from a secure key management system
            _key = Encoding.UTF8.GetBytes("HC-TechnicalCalculators-Key-32B");
            if (_key.Length != 32)
            {
                Array.Resize(ref _key, 32);
            }
        }

        public string ProtectString(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            try
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var protectedBytes = ProtectBytes(dataBytes);
                return Convert.ToBase64String(protectedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to protect string data");
                throw new InvalidOperationException("Data protection failed", ex);
            }
        }

        public string UnprotectString(string protectedData)
        {
            if (string.IsNullOrEmpty(protectedData))
                return string.Empty;

            try
            {
                var protectedBytes = Convert.FromBase64String(protectedData);
                var dataBytes = UnprotectBytes(protectedBytes);
                return Encoding.UTF8.GetString(dataBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unprotect string data");
                throw new InvalidOperationException("Data unprotection failed", ex);
            }
        }

        public byte[] ProtectBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            
            // Write IV first
            ms.Write(aes.IV, 0, aes.IV.Length);
            
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }

        public byte[] UnprotectBytes(byte[] protectedData)
        {
            if (protectedData == null || protectedData.Length == 0)
                return Array.Empty<byte>();

            using var aes = Aes.Create();
            aes.Key = _key;

            // Extract IV
            var iv = new byte[aes.IV.Length];
            Array.Copy(protectedData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(protectedData, iv.Length, protectedData.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var result = new MemoryStream();
            
            cs.CopyTo(result);
            return result.ToArray();
        }
    }
}
