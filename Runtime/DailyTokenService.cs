using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Nistec.Runtime
{
    /*
    // same master key on both sides (store securely)
    byte[] masterKey = Encoding.UTF8.GetBytes("your-32-byte-secret-key-here-123456"); // use real random 32 bytes

    var service = new DailyTokenService(masterKey, TimeSpan.FromMinutes(2));

    string token = service.CreateToken("userId=12345");
    Console.WriteLine("Send this token: " + token);
    */

    /*
    byte[] masterKey = Encoding.UTF8.GetBytes("your-32-byte-secret-key-here-123456"); // same as sender

    var service = new DailyTokenService(masterKey, TimeSpan.FromMinutes(2));

    string receivedToken = tokenFromSender;
    string data;

    if (service.TryValidateToken(receivedToken, out data))
    {
        Console.WriteLine("Valid token, data: " + data);
    }
    else
    {
        Console.WriteLine("Invalid or expired token");
    }
    */
    public class DailyTokenService
    {
        private readonly byte[] _masterKey;
        private readonly TimeSpan _ttl;

        public DailyTokenService(string masterKey, TimeSpan? ttl = null)
        {
            byte[] master_Key = Encoding.UTF8.GetBytes(masterKey); // same as sender

            _masterKey = master_Key ?? throw new ArgumentNullException(nameof(masterKey));
            _ttl = ttl ?? TimeSpan.FromMinutes(2);
        }

        public DailyTokenService(byte[] masterKey, TimeSpan? ttl = null)
        {
            _masterKey = masterKey ?? throw new ArgumentNullException(nameof(masterKey));
            _ttl = ttl ?? TimeSpan.FromMinutes(2);
        }

        public static string GenerateToken(string data,string masterKey, int minutes=2)
        {
            var service = new DailyTokenService(masterKey, TimeSpan.FromMinutes(minutes));
            string token = service.CreateToken(data);
            return token;
        }

        public static string ValidateToken(string receivedToken, string masterKey, int minutes = 2)
        {
            var service = new DailyTokenService(masterKey, TimeSpan.FromMinutes(minutes));

            string data;

            if (service.TryValidateToken(receivedToken, out data))
            {
                return data;
            }
            else
            {
                Console.WriteLine("Invalid or expired token");
                throw new Exception("Invalid or expired token");
            }
        }

        public static string RandomBase64Key()
        {
            byte[] masterKey = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(masterKey);

            string base64 = Convert.ToBase64String(masterKey);
            Console.WriteLine(base64);
            return base64;
        }

        // Sender calls this
        public string CreateToken(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var expires = DateTime.UtcNow.Add(_ttl);
            string payload = data + "|" + expires.Ticks;

            var key = GetDailyKey();
            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(iv);

            byte[] plainBytes = Encoding.UTF8.GetBytes(payload);
            byte[] cipherBytes;

            using (var aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var enc = aes.CreateEncryptor())
                {
                    cipherBytes = enc.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            byte[] hmacBytes;
            using (var hmac = new HMACSHA256(key))
            {
                byte[] combined = cipherBytes.Concat(iv).ToArray();
                hmacBytes = hmac.ComputeHash(combined);
            }

            string cipherB64 = Convert.ToBase64String(cipherBytes);
            string ivB64 = Convert.ToBase64String(iv);
            string hmacB64 = Convert.ToBase64String(hmacBytes);

            return cipherB64 + "." + ivB64 + "." + hmacB64;
        }

        // Receiver calls this
        public bool TryValidateToken(string token, out string data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            string cipherB64 = parts[0];
            string ivB64 = parts[1];
            string hmacB64 = parts[2];

            var key = GetDailyKey();

            byte[] cipherBytes, iv, hmacBytes;
            try
            {
                cipherBytes = Convert.FromBase64String(cipherB64);
                iv = Convert.FromBase64String(ivB64);
                hmacBytes = Convert.FromBase64String(hmacB64);
            }
            catch
            {
                return false;
            }

            // verify HMAC
            using (var hmac = new HMACSHA256(key))
            {
                byte[] combined = cipherBytes.Concat(iv).ToArray();
                byte[] expected = hmac.ComputeHash(combined);

                if (!TimeConstantEquals(expected, hmacBytes))
                    return false;
            }

            string payload;
            using (var aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var dec = aes.CreateDecryptor())
                {
                    byte[] plainBytes;
                    try
                    {
                        plainBytes = dec.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    }
                    catch
                    {
                        return false;
                    }

                    payload = Encoding.UTF8.GetString(plainBytes);
                }
            }

            var payloadParts = payload.Split('|');
            if (payloadParts.Length != 2)
                return false;

            string originalData = payloadParts[0];
            long ticks;
            if (!long.TryParse(payloadParts[1], out ticks))
                return false;

            var expires = new DateTime(ticks, DateTimeKind.Utc);
            if (DateTime.UtcNow > expires)
                return false;

            data = originalData;
            return true;
        }

        private byte[] GetDailyKey()
        {
            using (var hmac = new HMACSHA256(_masterKey))
            {
                string day = DateTime.UtcNow.ToString("yyyyMMdd");
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(day)); // 32 bytes
            }
        }

        private static bool TimeConstantEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }

}
