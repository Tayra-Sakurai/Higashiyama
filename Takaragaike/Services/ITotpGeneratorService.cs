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
        uint GenerateTotp(byte[] key);
    }
}
