namespace HelperLibrary.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();

        IGenericRepository<T> Repository<T>() where T : class;
    }
}
