using Microsoft.EntityFrameworkCore;
using TechChallenge.Data;

namespace TechChallenge.API.Infra;

public static class DBContextConfiguration
{
    public static void DbContextConfiguration(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddDbContext<DBContext>(options => options.UseSqlServer(configuration.GetConnectionString("ConnString")));
    }
}
