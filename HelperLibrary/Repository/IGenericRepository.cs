using System.Linq.Expressions;

namespace HelperLibrary.Repository
{
    /// <summary>
    /// CRUD interface
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IGenericRepository<TEntity> : IEnumerable<TEntity> where TEntity : class 
    {
        // Create
        void Add(TEntity entity);
        Task AddAsync(TEntity entity);
        void AddRange(params TEntity[] entities);
        void AddRange(IEnumerable<TEntity> entities);
        Task AddRangeAsync(params TEntity[] entities);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // Read
        TEntity? FindById(params object?[]? id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> _pred);

        // Update
        void Update(TEntity item);

        // Delete
        void Remove(TEntity item);

        int Count { get; }
    }
}
