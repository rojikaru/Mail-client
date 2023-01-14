using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;

namespace HelperLibrary.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        #region Create
        public void Add(TEntity item)
        {
            _dbSet.Add(item);
            _context.SaveChanges();
        }
        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public void AddRange(params TEntity[] entities)
            => AddRange(entities.AsEnumerable());
        public void AddRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
            _context.SaveChanges();
        }
        public async Task AddRangeAsync(params TEntity[] entities)
            => await AddRangeAsync(entities.AsEnumerable());
        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Read
        public IEnumerable<TEntity> GetAll()
            => _dbSet.ToList();
        public IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
            => _dbSet.Where(predicate).ToList();

        public TEntity? FindById(params object?[]? id)
            => _dbSet.Find(id);
        public IEnumerable<TEntity> GetWithInclude(params Expression<Func<TEntity, object>>[] includeProperties)
            => Include(includeProperties).ToList();

        public IEnumerable<TEntity> GetWithInclude(
            Func<TEntity, bool> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
            => Include(includeProperties).Where(predicate).ToList();

        private IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includeProperties)
            => includeProperties.Aggregate(_dbSet.AsQueryable(), (current, includeProperty) => current.Include(includeProperty));
        #endregion

        #region Update
        public void Update(TEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.SaveChanges();
        }
        #endregion

        #region Delete
        public void Remove(TEntity item)
        {
            _dbSet.Remove(item);
            _context.SaveChanges();
        }
        #endregion

        public int Count => _dbSet.Count();
        public IEnumerator<TEntity> GetEnumerator() => _dbSet.AsQueryable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dbSet.AsQueryable().GetEnumerator();
    }
}
