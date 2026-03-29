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
    public static class OtpUriParser
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

        /// <inheritdoc cref="GetSecretKey(Uri)"/>
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

        /// <summary>
        /// Gets the TOTP or HOTP secret shared key as byte array.
        /// </summary>
        /// <param name="uri">The URI of the otpauth:// URI scheme.</param>
        /// <returns>The byte array which represents the shared secret key.</returns>
        /// <exception cref="ArgumentNullException">When the URI is null.</exception>
        /// <exception cref="ArgumentException">When the URI is invalid.</exception>
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

        /// <summary>
        /// Gets the initial counter of the HOTP from the URI.
        /// </summary>
        /// <param name="hotpUri">The otpauth:// URI scheme of the HOTP algorithm.</param>
        /// <returns>The 8-byte counter of the HOTP.</returns>
        /// <remarks>
        /// <para>The HOTP uses the counter as the key variable of the HMAC-SHA-1. The counter is updated every when the OTP is published.</para>
        /// <para>This is HOTP-specific function which is incompatible with the TOTP.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">When <paramref name="hotpUri"/> is not an otpauth:// URI.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="hotpUri"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="hotpUri"/> is a TOTP URI.</exception>
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

        /// <inheritdoc cref="GetInitialCounter(Uri)"/>
        public static byte[] GetInitialCounter(string uri)
        {
            return GetInitialCounter(new Uri(uri));
        }

        /// <summary>
        /// Gets the issuer of the OTP URI.
        /// </summary>
        /// <param name="uri">The otpauth:// URI scheme of the OTP generator.</param>
        /// <returns>The issuer name of the <paramref name="uri"/> if it is specified; otherwise, returns null.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="uri"/> is not a valid otpauth:// URI.</exception>
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

        /// <inheritdoc cref="GetIssuer(Uri)"/>
        public static string GetIssuer(string uri)
        {
            return GetIssuer(new Uri(uri));
        }

        /// <summary>
        /// Gets the OTP type from the URI.
        /// </summary>
        /// <param name="uri">The otpauth:// URI.</param>
        /// <returns>The type of OTP.</returns>
        /// <exception cref="ArgumentNullException">When the URI is null.</exception>
        /// <exception cref="ArgumentException">When the URI is invalid.</exception>
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

        /// <inheritdoc cref="GetOTPType(Uri)"/>
        public static OTPType GetOTPType(string uri)
        {
            return GetOTPType(new Uri(uri));
        }

        /// <summary>
        /// Gets the account name incleded by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The otpauth:// URI scheme to be parsed by this function.</param>
        /// <returns>The account name of <paramref name="uri"/>. If there is not a user name field in <paramref name="uri"/>, this returns null.</returns>
        /// <inheritdoc cref="GetIssuer(Uri)"/>
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

        /// <inheritdoc cref="GetAccountName(Uri)"/>
        public static string GetAccountName(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetAccountName(new Uri(uri));
        }

        /// <summary>
        /// Gets the duration of <paramref name="uri"/>.
        /// </summary>
        /// <returns>The TOTP duration gotten from <paramref name="uri"/>. Defaults to 30 seconds.</returns>
        /// <inheritdoc cref="GetAccountName(Uri)"/>
        /// <exception cref="InvalidOperationException">When the URI is a HOTP URI scheme.</exception>
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

        /// <inheritdoc cref="GetTotpDuration(Uri)"/>
        public static TimeSpan GetTotpDuration(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetTotpDuration(new Uri(uri));
        }

        /// <summary>
        /// Gets the digits of the OTP.
        /// </summary>
        /// <returns>The digits of the OTP.</returns>
        /// <inheritdoc cref="GetDataModel(Uri)"/>
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

        /// <inheritdoc cref="GetDigits(Uri)"/>
        public static uint GetDigits(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetDigits(new Uri(uri));
        }

        /// <summary>
        /// Gets the algorithm name of the OTP.
        /// </summary>
        /// <returns>The algorithm identifier of the OTP generator.</returns>
        /// <exception cref="InvalidOperationException">When the Uri is for HOTP.</exception>
        /// <inheritdoc cref="GetDigits(Uri)"/>
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

        /// <inheritdoc cref="GetAlgorithm(Uri)"/>
        public static HashAlgorithm GetAlgorithm(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetAlgorithm(new Uri(uri));
        }

        /// <summary>
        /// Generates a data model to be saved onto the database.
        /// </summary>
        /// <param name="uri">The otpauth:// URI.</param>
        /// <returns>The data model.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="uri"/> is not a valid otpauth:// URI.</exception>
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

        /// <inheritdoc cref="GetDataModel(Uri)"/>
        public static OtpData GetDataModel(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            return GetDataModel(new Uri(uri));
        }

        /// <summary>
        /// Generates an otpauth:// URI from the data included by <paramref name="dataModel"/>.
        /// </summary>
        /// <param name="dataModel">The data model to include the OTP authentication data.</param>
        /// <returns>The generated URI.</returns>
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
