using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Takaragaike.Extensions;
using Takaragaike.Models;

namespace Takaragaike.Services
{
    /// <summary>
    /// The OTP URI parsing service class.
    /// </summary>
    public class OtpauthUriParserService : IUriParserService<OtpData>
    {
        /// <summary>
        /// Judges if <paramref name="uri"/> is a valid otpauth:// URI.
        /// </summary>
        /// <param name="uri">The URI to be checked.</param>
        /// <returns>The value indicating whether <paramref name="uri"/> is valid.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="uri"/> is null.</exception>
        private static bool IsOtpauthUri(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (!uri.IsAbsoluteUri)
                return false;

            if (uri.Scheme != "otpauth")
                return false;

            if (uri.Host != "totp" && uri.Host != "hotp")
                return false;

            if (string.IsNullOrEmpty(uri.Query))
                return false;

            return true;
        }

        private static NameValueCollection ParseQuery(Uri uri)
        {
            // The parameters
            string query = uri.Query[1..];

            return HttpUtility.ParseQueryString(query);
        }

        private static string ToQueryString(NameValueCollection query)
        {
            string result = "?";

            for (int i = 0; i < query.Count; i++)
            {
                string key = query.GetKey(i);
                string value = query[i];

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    result += key + "=" + HttpUtility.UrlEncode(value);
                    continue;
                }
            }

            return result;
        }

        public static byte[] GetSecretKey(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (!IsOtpauthUri(uri))
                throw new ArgumentException("Invalid URI scheme.", nameof(uri));

            NameValueCollection queryCollection = ParseQuery(uri);

            return ConverterExtensions.FromBase32String(queryCollection["secret"]);
        }

        public static byte[] GetSecretKey(string uri)
        {
            return GetSecretKey(new Uri(uri));
        }

        public static byte[] GetInitialCounter(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (!IsOtpauthUri(uri))
                throw new ArgumentException("Can't decode the URI.", nameof(uri));

            if (uri.Host != "hotp")
                throw new ArgumentException("Initial counter value is only used for HOTP.", nameof(uri));

            NameValueCollection queryCollection = ParseQuery(uri);

            bool parsingResult = ulong.TryParse(queryCollection["counter"], out ulong counter);

            if (parsingResult)
                return BitConverter.GetBytes(counter);

            throw new ArgumentException("Invalid URI.", nameof(uri));
        }

        public static byte[] GetInitialCounter(string uri)
        {
            return GetInitialCounter(new Uri(uri));
        }

        public static string GetIssuer(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
            {
                // The label component.
                string label = uri.AbsolutePath;

                if (string.IsNullOrEmpty(label))
                {
                    // The query which may include the issuer domain.
                    NameValueCollection query = ParseQuery(uri);

                    // Extracts the issuer URI.

                    string issuerDomain = query["issuer"];

                    if (string.IsNullOrEmpty(issuerDomain))
                        return null;

                    return issuerDomain;
                }

                label = HttpUtility.UrlDecode(label);

                string isser = label.Split(':')[0];

                return isser;
            }

            throw new ArgumentException("Invalid URI scheme.", nameof(uri));
        }

        public static string GetIssuer(string uri)
        {
            return GetIssuer(new Uri(uri));
        }

        public static OTPType GetOTPType(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
                return uri.Host switch
                {
                    "totp" => OTPType.TOTP,
                    "hotp" => OTPType.HOTP,
                    _ => throw new ArgumentException("Invalid URI scheme", nameof(uri))
                };

            throw new ArgumentException("Invalid URI scheme.", nameof(uri));
        }

        public static OTPType GetOTPType(string uri)
        {
            return GetOTPType(new Uri(uri));
        }

        public static string GetAccountName(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
            {
                // The label data.
                string label = HttpUtility.UrlDecode(uri.AbsolutePath);

                if (string.IsNullOrEmpty(label))
                    return null;

                string[] labels = label.Split(':');

                if (labels.Length == 2)
                    return labels[1];

                return null;
            }

            throw new ArgumentException("Invalid URI scheme.", nameof(uri));
        }

        public static string GetAccountName(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetAccountName(new Uri(uri));
        }

        public static TimeSpan GetTotpDuration(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
            {
                if (uri.Host == "totp")
                {
                    // Parameters
                    NameValueCollection nvc = ParseQuery(uri);

                    if (uint.TryParse(nvc["period"], out uint num))
                    {
                        TimeSpan duration = TimeSpan.FromSeconds(num);

                        return duration;
                    }

                    return TimeSpan.FromSeconds(30d);
                }
            }

            throw new ArgumentException("Invalid TOTP URI scheme.", nameof(uri));
        }

        public static TimeSpan GetTotpDuration(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetTotpDuration(new Uri(uri));
        }

        public static uint GetDigits(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
            {
                // The query.
                NameValueCollection query = ParseQuery(uri);

                if (query["digits"] == null)
                    return 6;

                if (uint.TryParse(query["digits"], out uint dgt))
                    return dgt;
            }

            throw new ArgumentException("Not a valid otpauth:// URI.", nameof(uri));
        }

        public static uint GetDigits(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetDigits(new Uri(uri));
        }

        public static HashAlgorithm GetAlgorithm(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
            {
                NameValueCollection query = ParseQuery(uri);

                HashAlgorithm? algorithm = query["algorithm"] switch
                {
                    null => HashAlgorithm.SHA1,
                    "SHA1" => HashAlgorithm.SHA1,
                    "SHA256" => HashAlgorithm.SHA256,
                    "SHA512" => HashAlgorithm.SHA512,
                    _ => null
                };

                if (algorithm is HashAlgorithm hashAlgorithm)
                    return hashAlgorithm;
            }

            throw new ArgumentException("Invalid otpauth:// URI.", nameof(uri));
        }

        public static HashAlgorithm GetAlgorithm(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetAlgorithm(new Uri(uri));
        }

        public static OtpData GetDataModel(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (IsOtpauthUri(uri))
            {
                OtpData result = new()
                {
                    Algorithm = GetAlgorithm(uri),
                    CreationTime = DateTime.UtcNow,
                    Digits = (int)GetDigits(uri),
                    Issuer = GetIssuer(uri) ?? string.Empty,
                    OtpType = GetOTPType(uri),
                    SecretKey = GetSecretKey(uri),
                    UserName = GetAccountName(uri) ?? string.Empty,
                };

                if (result.OtpType == OTPType.TOTP)
                {
                    result.Duration = (int)GetTotpDuration(uri).TotalSeconds;

                    return result;
                }

                result.Counter = GetInitialCounter(uri);

                return result;
            }

            throw new ArgumentException("Invalid otpauth:// URI.", nameof(uri));
        }

        public static OtpData GetDataModel(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetDataModel(new Uri(uri));
        }

        public static Uri GetUri(OtpData dataModel)
        {
            ArgumentNullException.ThrowIfNull(dataModel);

            UriBuilder builder = new()
            {
                Scheme = "otpauth",
                Host = dataModel.OtpType.ToString().ToLower(),
                Path = "/" + HttpUtility.UrlPathEncode(dataModel.Issuer + ":" + dataModel.UserName),
            };

            NameValueCollection queryCollection = new();

            queryCollection["algorithm"] = dataModel.Algorithm.ToString();
            queryCollection["secret"] = ConverterExtensions.ToBase32String(dataModel.SecretKey);
            queryCollection["digits"] = dataModel.Digits.ToString();
            queryCollection["period"] = dataModel.Duration?.ToString() ?? string.Empty;
            queryCollection["counter"] = dataModel.Counter?.ToString() ?? string.Empty;

            builder.Query = ToQueryString(queryCollection);

            return builder.Uri;
        }
    }
}
