using Microsoft.EntityFrameworkCore;

namespace HelperLibrary.Repository
{
    public class GenericUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DbContext context;
        private Dictionary<Type, object> Repositories { get; set; }

        public GenericUnitOfWork(DbContext context)
        {
            this.context = context;
            Repositories = new();
        }

        public int SaveChanges() => context.SaveChanges();
        public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (Repositories.ContainsKey(typeof(T)))
                return (IGenericRepository<T>)Repositories[typeof(T)];

            IGenericRepository<T> repo = new GenericRepository<T>(context);
            Repositories.Add(typeof(T), repo);

            return repo;
        }

        public void Dispose()
        {
            context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
