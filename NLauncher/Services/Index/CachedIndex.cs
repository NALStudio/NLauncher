using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NLauncher.Services.Index;

public partial class IndexService
{
    // Can't serialize to JSON since NLauncherJsonContext can't seem to discover the Color24 struct.
    private readonly struct CachedIndex
    {
        // Format spec, names with the '64' suffix are Base64 encoded.
        // 
        // Serialized Format
        // HEADER0,HEADER1,HEADER2,...,HEADERN:CONTENT64
        //
        // Header Format
        // EXPIRES,CONTENTHASH64

        /// <summary>
        /// Delimiter to split header values.
        /// </summary>
        private const char HeaderValuesDelimiter = ',';

        /// <summary>
        /// Delimiter to split header from content.
        /// </summary>
        private const char ContentDelimiter = ':';

        /// <summary>
        /// A UNIX timestamp for when this resource expires.
        /// </summary>
        /// <remarks>
        /// Timestamp represents the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). It does not take leap seconds into account.
        /// </remarks>
        public required long ExpiresTimestamp { get; init; }
        public required IndexManifest Index { get; init; }

        private static (string Hash, string Content) ComputeHash(byte[] utf8Bytes)
        {
            byte[] hash = SHA256.HashData(utf8Bytes);

            string hash64 = Convert.ToBase64String(hash);
            string content = Encoding.UTF8.GetString(utf8Bytes);

            return (hash64, content);
        }

        private static byte[]? VerifyHash(string hash, string content)
        {
            byte[] hashBytes = Convert.FromBase64String(hash);
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            byte[] newHash = SHA256.HashData(contentBytes);

            if (hashBytes.AsSpan().SequenceEqual(newHash.AsSpan()))
                return contentBytes;
            else
                return null;
        }

        public static string Serialize(CachedIndex value)
        {
            byte[] serializedIndex = JsonSerializer.SerializeToUtf8Bytes(value.Index, IndexJsonContext.Default.IndexManifest);
            (string hash, string content) = ComputeHash(utf8Bytes: serializedIndex);

            string[] headerValues = new string[]
            {
                value.ExpiresTimestamp.ToString(CultureInfo.InvariantCulture),
                hash
            };

            Debug.Assert(!headerValues.Any(static hv => hv.Contains(HeaderValuesDelimiter)));
            string header = string.Join(HeaderValuesDelimiter, headerValues);

            Debug.Assert(!header.Contains(ContentDelimiter));
            return header + ContentDelimiter + content;
        }

        public static bool TryDeserialize(string serialized, [MaybeNullWhen(false)] out CachedIndex value)
        {
            CachedIndex? deserialized = TryDeserialize(serialized);
            if (deserialized.HasValue)
            {
                value = deserialized.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static CachedIndex? TryDeserialize(string serialized)
        {
            string[] headerContentPair = serialized.Split(ContentDelimiter, 2);
            if (headerContentPair.Length != 2)
                return null;

            string header = headerContentPair[0];
            string content64 = headerContentPair[1];

            string[] headerValues = header.Split(HeaderValuesDelimiter);
            if (headerValues.Length < 2)
                return null;
            string expiresTimestampValue = headerValues[0];
            string hash64 = headerValues[1];

            if (!long.TryParse(expiresTimestampValue, out long expiresTimestamp))
                return null;

            byte[]? contentUtf8 = VerifyHash(hash64, content64);
            if (contentUtf8 is null)
                return null;

            IndexManifest? index = JsonSerializer.Deserialize(contentUtf8, IndexJsonContext.Default.IndexManifest);
            if (index is null)
                return null;

            return new CachedIndex()
            {
                ExpiresTimestamp = expiresTimestamp,
                Index = index
            };
        }
    }
}
