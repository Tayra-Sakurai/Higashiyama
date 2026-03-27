using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takaragaike.Extensions
{
    public static class ConverterExtensions
    {
        private const string CONVERT_TABLE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary>
        /// Converts the specified byte array to its equivalent Base32-encoded string representation.
        /// </summary>
        /// <remarks>Base32 encoding is useful for representing binary data in a text format that is safe
        /// for URLs and file names. The output string uses the standard Base32 alphabet as defined in RFC
        /// 4648.</remarks>
        /// <param name="bytes">The byte array to encode. Cannot be null.</param>
        /// <returns>A string containing the Base32-encoded representation of the input byte array.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="bytes"/> is null.</exception>
        public static string ToBase32String(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            string result = string.Empty;

            foreach (byte[] chunk in bytes.Chunk(5))
            {
                List<byte> chunkList = [.. chunk];
                while (chunkList.Count < 8)
                    chunkList.Insert(0, 0);
                byte[] chunkLong = chunkList.ToArray();
                ulong longValue = BitConverter.ToUInt64(chunkLong, 0);
                longValue <<= chunk.Length switch
                {
                    1 => 2,
                    2 => 4,
                    3 => 1,
                    4 => 3,
                    5 => 0,
                    _ => throw new InvalidOperationException("Invalid length was retrieved.")
                };

                string chunkStr = string.Empty;
                
                for (; longValue > 0; longValue >>= 5)
                    chunkStr = (string)chunkStr.Prepend(CONVERT_TABLE[(int)(longValue % (1UL << 5))]);

                while (chunkStr.Length < 8)
                    chunkStr += "=";

                result += chunkStr;
            }

            return result;
        }

        /// <summary>
        /// Converts the BASE32-encoded string to a raw byte array.
        /// </summary>
        /// <param name="base32string">The BASE32-encoded byte array.</param>
        /// <returns>The decoded byte array.</returns>
        /// <exception cref="ArgumentException">When <paramref name="base32string"/> is not a BASE-32-encoded string.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="base32string"/> is null.</exception>
        public static byte[] FromBase32String (string base32string)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(base32string);

            if (base32string.ToUpper().Length % 8 != 0)
                throw new ArgumentException("Invalid BASE32 string.", nameof(base32string));

            if (!base32string.ToUpper().All(CONVERT_TABLE.Append('=').Contains))
                throw new ArgumentException("Invalid BASE32 string.", nameof(base32string));

            List<byte> byteList = [];

            foreach (char[] base32bytes in base32string.Chunk(8))
            {
                if (base32bytes.Length < 8)
                    throw new ArgumentException("Invalid BASE32 string.", nameof(base32string));

                ulong bytenum = 0;

                int i = 0;

                for (; i < base32bytes.Length; i++)
                {
                    if (base32bytes[i] == '=')
                    {
                        bytenum >>= i switch
                        {
                            2 => 2,
                            4 => 4,
                            5 => 1,
                            7 => 3,
                            _ => throw new ArgumentException("Invalid BASE32 string.", nameof(base32string)),
                        };
                        break;
                    }

                    bytenum <<= 5;

                    if (!CONVERT_TABLE.Contains(base32bytes[i]))
                        throw new ArgumentException("Invalid BASE32 string.", nameof(base32string));

                    bytenum += (ulong)CONVERT_TABLE.IndexOf(base32bytes[i]);
                }

                int offset = i switch
                {
                    2 => 7,
                    4 => 6,
                    5 => 5,
                    7 => 4,
                    8 => 3,
                    _ => throw new ArgumentException("Invalid BASE32 string.", nameof(base32string))
                };

                byte[] bytes = BitConverter.GetBytes(bytenum)[offset..];

                foreach (byte b in bytes)
                    byteList.Add(b);
            }

            return [.. byteList];
        }
    }
}
