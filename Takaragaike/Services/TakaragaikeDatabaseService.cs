using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takaragaike.Contexts;

namespace Takaragaike.Services
{
    public class TakaragaikeDatabaseService : IDatabaseService<TakaragaikeContext>
    {
        private IDbContextFactory<TakaragaikeContext> contextFactory;

        public TakaragaikeDatabaseService(IDbContextFactory<TakaragaikeContext> contextFactory)
        {
            this.contextFactory = contextFactory;
            using TakaragaikeContext context = this.contextFactory.CreateDbContext();
            context.Database.Migrate();
        }

        public async Task InitializeAsync()
        {
            using var context = await contextFactory.CreateDbContextAsync();
            await context.Database.MigrateAsync();
        }

        /// <inheritdoc/>
        public async Task AddAsync(object entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            using var context = await contextFactory.CreateDbContextAsync();
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            using var context = await contextFactory.CreateDbContextAsync();
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task RemoveAsync(object entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            using var context = await contextFactory.CreateDbContextAsync();
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task RemoveAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            using var context = await contextFactory.CreateDbContextAsync();
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(object entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            using var context = await contextFactory.CreateDbContextAsync();
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            using var context = await contextFactory.CreateDbContextAsync();
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task<IList<TEntity>> GetEntitiesAsync<TEntity>(Func<TakaragaikeContext, DbSet<TEntity>> factory)
            where TEntity : class
        {
            using var context = await contextFactory.CreateDbContextAsync();
            return await factory(context).AsNoTracking().ToListAsync();
        }

        public async Task<IList<TEntity>> GetEntitiesAsync<TEntity>()
            where TEntity : class
        {
            using var context = await contextFactory.CreateDbContextAsync();
            return await context.Set<TEntity>().AsNoTracking().ToListAsync();
        }
    }
}
