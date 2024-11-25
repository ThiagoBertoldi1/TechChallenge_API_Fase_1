using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Data;
public class DBContext(DbContextOptions<DBContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts { get; set; }
}
