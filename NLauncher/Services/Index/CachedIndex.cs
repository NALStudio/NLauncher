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

        private static (string Hash, string Content) ToBase64(byte[] bytes)
        {
            byte[] hash = SHA256.HashData(bytes);

            string hash64 = Convert.ToBase64String(hash);
            string content64 = Convert.ToBase64String(bytes);

            return (hash64, content64);
        }

        private static byte[]? FromBase64(string hash, string content)
        {
            byte[] hashBytes = Convert.FromBase64String(hash);
            byte[] contentBytes = Convert.FromBase64String(content);

            byte[] newHash = SHA256.HashData(contentBytes);

            if (hashBytes.AsSpan().SequenceEqual(newHash.AsSpan()))
                return contentBytes;
            else
                return null;
        }

        public static string Serialize(CachedIndex value)
        {
            byte[] serializedIndex = IndexJsonSerializer.SerializeToUtf8Bytes(value.Index);
            (string hash64, string content64) = ToBase64(serializedIndex);

            string[] headerValues = new string[]
            {
                value.ExpiresTimestamp.ToString(CultureInfo.InvariantCulture),
                hash64
            };

            Debug.Assert(!headerValues.Any(static hv => hv.Contains(HeaderValuesDelimiter)));
            string header = string.Join(HeaderValuesDelimiter, headerValues);

            Debug.Assert(!header.Contains(ContentDelimiter));
            Debug.Assert(!content64.Contains(ContentDelimiter));
            return header + ContentDelimiter + content64;
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
            // FIXME: Verify headerValues array length

            string expiresTimestampValue = headerValues[0];
            string hash64 = headerValues[1];

            if (!long.TryParse(expiresTimestampValue, out long expiresTimestamp))
                return null;

            byte[]? contentBytes = FromBase64(hash64, content64);
            if (contentBytes is null)
                return null;

            IndexManifest? index = IndexJsonSerializer.Deserialize<IndexManifest>(contentBytes);
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
