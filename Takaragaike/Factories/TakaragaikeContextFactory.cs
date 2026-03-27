using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takaragaike.Contexts;

namespace Takaragaike.Factories
{
    internal class TakaragaikeContextFactory : IDesignTimeDbContextFactory<TakaragaikeContext>
    {
        public TakaragaikeContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<TakaragaikeContext> optionsBuilder = new();

            optionsBuilder.UseSqlite("Data Source=Takaragaike_Design.db");

            return new(optionsBuilder.Options);
        }
    }
}
