using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takaragaike.Models;

namespace Takaragaike.Services
{
    /// <summary>
    /// Basic OATH-HOTP and OATH-TOTP URI parsing service interface to be implemented for URI parsing.
    /// </summary>
    /// <typeparam name="TDataModel">The data type for the database which stores the data.</typeparam>
    public interface IUriParserService<TDataModel>
        where TDataModel : class, new()
    {
        /// <summary>
        /// Gets the TOTP or HOTP secret shared key as byte array.
        /// </summary>
        /// <param name="uri">The URI of the otpauth:// URI scheme.</param>
        /// <returns>The byte array which represents the shared secret key.</returns>
        /// <exception cref="ArgumentNullException">When the URI is null.</exception>
        /// <exception cref="ArgumentException">When the URI is invalid.</exception>
        static abstract byte[] GetKey(Uri uri);

        /// <inheritdoc cref="GetKey(Uri)"/>
        static abstract byte[] GetKey(string uri);

        /// <summary>
        /// Gets the OTP type from the URI.
        /// </summary>
        /// <param name="uri">The otpauth:// URI.</param>
        /// <returns>The type of OTP.</returns>
        /// <exception cref="ArgumentNullException">When the URI is null.</exception>
        /// <exception cref="ArgumentException">When the URI is invalid.</exception>
        static abstract OTPType GetOTPType(Uri uri);

        /// <inheritdoc cref="GetOTPType(Uri)"/>
        static abstract OTPType GetOTPType(string uri);

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
        static abstract byte[] GetInitialCounter(Uri hotpUri);

        /// <inheritdoc cref="GetInitialCounter(Uri)"/>
        static abstract byte[] GetInitialCounter(string hotpUri);

        /// <summary>
        /// Gets the issuer of the OTP URI.
        /// </summary>
        /// <param name="uri">The otpauth:// URI scheme of the OTP generator.</param>
        /// <returns>The issuer name of the <paramref name="uri"/> if it is specified; otherwise, returns null.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="uri"/> is not a valid otpauth:// URI.</exception>
        static abstract string GetIssuer(Uri uri);

        /// <inheritdoc cref="GetIssuer(Uri)"/>
        static abstract string GetIssuer(string uri);

        /// <summary>
        /// Gets the account name incleded by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The otpauth:// URI scheme to be parsed by this function.</param>
        /// <returns>The account name of <paramref name="uri"/>. If there is not a user name field in <paramref name="uri"/>, this returns null.</returns>
        /// <inheritdoc cref="GetIssuer(Uri)"/>
        static abstract string GetAccountName(Uri uri);

        /// <inheritdoc cref="GetAccountName(Uri)"/>
        static abstract string GetAccountName(string uri);

        /// <summary>
        /// Gets the duration of <paramref name="uri"/>.
        /// </summary>
        /// <returns>The TOTP duration gotten from <paramref name="uri"/>. Defaults to 30 seconds.</returns>
        /// <inheritdoc cref="GetAccountName(Uri)"/>
        /// <exception cref="InvalidOperationException">When the URI is a HOTP URI scheme.</exception>
        static abstract TimeSpan GetTotpDuration(Uri uri);

        /// <inheritdoc cref="GetTotpDuration(Uri)"/>
        static abstract TimeSpan GetTotpDuration(string uri);
        
        /// <summary>
        /// Generates a data model to be saved onto the database.
        /// </summary>
        /// <param name="uri">The otpauth:// URI.</param>
        /// <returns>The data model.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="uri"/> is not a valid otpauth:// URI.</exception>
        static abstract TDataModel GetDataModel(Uri uri);

        /// <inheritdoc cref="GetDataModel(Uri)"/>
        static abstract TDataModel GetDataModel(string uri);

        /// <summary>
        /// Gets the digits of the OTP.
        /// </summary>
        /// <returns>The digits of the OTP.</returns>
        /// <inheritdoc cref="GetDataModel(Uri)"/>
        static abstract uint GetDigits(Uri uri);

        /// <inheritdoc cref="GetDigits(Uri)"/>
        static abstract uint GetDigits(string uri);

        /// <summary>
        /// Gets the algorithm name of the OTP.
        /// </summary>
        /// <returns>The algorithm identifier of the OTP generator.</returns>
        /// <exception cref="InvalidOperationException">When the Uri is for HOTP.</exception>
        /// <inheritdoc cref="GetDigits(Uri)"/>
        static abstract HashAlgorithm GetAlgorithm(Uri uri);

        /// <inheritdoc cref="GetAlgorithm(Uri)"/>
        static abstract HashAlgorithm GetAlgorithm(string uri);

        /// <summary>
        /// Generates an otpauth:// URI from the data included by <paramref name="dataModel"/>.
        /// </summary>
        /// <param name="dataModel">The data model to include the OTP authentication data.</param>
        /// <returns>The generated URI.</returns>
        static abstract Uri GetUri(TDataModel dataModel);
    }
}
