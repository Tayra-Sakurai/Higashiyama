using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takaragaike.Models
{
    public class OtpData
    {
        public int Id { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public byte[] SecretKey { get; set; } = null!;
        public OTPType OtpType { get; set; }
        public byte[] Counter { get; set; } = null;
        public int? Duration { get; set; } = null;
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;
        public HashAlgorithm Algorithm { get; set; } = HashAlgorithm.SHA1;
        public int Digits { get; set; } = 6;
    }
}
