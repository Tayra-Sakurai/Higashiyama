using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takaragaike.Models;

namespace Takaragaike.Contexts
{
    public class TakaragaikeContext : DbContext
    {
        public DbSet<OtpData> OtpData { get; set; }

        public TakaragaikeContext(DbContextOptions<TakaragaikeContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OtpData>();
        }
    }
}
