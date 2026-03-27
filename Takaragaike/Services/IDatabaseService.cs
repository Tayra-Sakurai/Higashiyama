using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takaragaike.Services
{
    public interface IDatabaseService<TContext>
        where TContext : DbContext, new()
    {
        /// <summary>
        /// Adds <paramref name="entity"/> to the database referred by <typeparamref name="TContext"/>.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <returns>The task to control the operation.</returns>
        Task AddAsync(object entity);

        /// <inheritdoc cref="AddAsync(object)"/>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class;

        /// <summary>
        /// Removes <paramref name="entity"/> from the database asynchronously.
        /// </summary>
        /// <param name="entity">The database entity to be removed.</param>
        /// <returns>A <see cref="Task"/> instance to control the removal operation.</returns>
        Task RemoveAsync(object entity);

        /// <inheritdoc cref="RemoveAsync(object)"/>
        /// <typeparam name="TEntity">The type of the entity to be removed.</typeparam>
        Task RemoveAsync<TEntity>(TEntity entity)
            where TEntity : class;
    }
}
