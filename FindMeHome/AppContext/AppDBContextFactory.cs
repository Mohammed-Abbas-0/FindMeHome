using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FindMeHome.AppContext
{
    public class AppDBContextFactory : IDesignTimeDbContextFactory<AppDBContext>
    {
        public AppDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();

            optionsBuilder.UseSqlServer(
                //"Server=db33755.public.databaseasp.net;Database=db33755;User Id=db33755;Password=S+z25K_x%mH6;Encrypt=False;MultipleActiveResultSets=True;"
                "Server=.;Database=FindMeHomeDB;User Id=sa;Password=123456;TrustServerCertificate=True;"

             );

            return new AppDBContext(optionsBuilder.Options);
        }
    }
}
