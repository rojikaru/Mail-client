using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary.DAL
{
    public class CredentialsContext : DbContext
    {
        public CredentialsContext(bool async = true)
        {
            EnsureCreated(async);
        }
        public CredentialsContext(DbContextOptions<DbContext> options, bool async = true)
            : base(options)
        {
            EnsureCreated(async);
        }

        private void EnsureCreated(bool async)
        {
            if (async) Database.EnsureCreatedAsync().ContinueWith(task => Initialized = true);
            else
            {
                Database.EnsureCreated();
                Initialized = true;
            }
        }
        public bool Initialized { get; protected set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(ConStr);

        private const string ConStr = @"Data Source=(LocalDB)\MSSQLLocalDB;Database=CredentialsDB;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True";

        public virtual DbSet<ServerCredentials> Servers { get; set; }
        public virtual DbSet<ServerData> Datas { get; set; }
        public virtual DbSet<UserCredentials> Users { get; set; }
    }
}
