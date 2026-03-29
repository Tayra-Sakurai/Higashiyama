using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takaragaike.Services
{
    public interface IDatabaseService<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Adds <paramref name="entity"/> to the database referred by <typeparamref name="TContext"/>.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <returns>The task to control the operation.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="entity"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="entity"/> is not acceptable.</exception>
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
        /// <inheritdoc cref="AddAsync(object)"/>
        Task RemoveAsync(object entity);

        /// <inheritdoc cref="RemoveAsync(object)"/>
        /// <typeparam name="TEntity">The type of the entity to be removed.</typeparam>
        Task RemoveAsync<TEntity>(TEntity entity)
            where TEntity : class;

        /// <summary>
        /// Initializes the database and make the initialization data.
        /// </summary>
        /// <returns>The <see cref="Task"/> instance to manage the operation.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Asynchronously retrieves a list of entities from the data context using the specified factory function.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities to retrieve. Must be a reference type.</typeparam>
        /// <param name="factory">A function that, given the data context, returns the <see cref="DbSet{TEntity}"/> representing the collection of entities to query.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities of type <typeparamref name="TEntity"/>.</returns>
        Task<IList<TEntity>> GetEntitiesAsync<TEntity>(Func<TContext, DbSet<TEntity>> factory)
            where TEntity : class;

        /// <inheritdoc cref="GetEntitiesAsync{TEntity}(Func{TContext, DbSet{TEntity}})"/>
        Task<IList<TEntity>> GetEntitiesAsync<TEntity>()
            where TEntity : class;

        /// <summary>
        /// Updates the entity to the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity including the updated data.</param>
        /// <returns>The task to manage this operation.</returns>
        Task UpdateAsync(object entity);

        /// <inheritdoc cref="UpdateAsync(object)"/>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        Task UpdateAsync<TEntity>(TEntity entity)
            where TEntity : class;
    }
}
