using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takaragaike.Services
{
    public interface ITotpGeneratorService
    {
        /// <summary>
        /// Generates the OATH-TOTP based on the shared secret key.
        /// </summary>
        /// <param name="key">The shared secret key.</param>
        /// <returns>The time-based one time password.</returns>
        /// <exception cref="ArgumentNullException">When the key is null.</exception>
        /// <exception cref="ArgumentException">When the key is not valid.</exception>
        static abstract uint GenerateTotp(byte[] key);

        /// <summary>
        /// Generates an OATH-HOTP based on the shared key and the counter.
        /// </summary>
        /// <param name="key">The secret shared key.</param>
        /// <param name="counter">The secret counter.</param>
        /// <returns>The HMAC-based one-time password.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="key"/> and/or <paramref name="counter"/> are/is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is invalid and/or <paramref name="counter"/> is invalid.</exception>
        static abstract uint GenerateHotp(byte[] key, byte[] counter);
    }
}
