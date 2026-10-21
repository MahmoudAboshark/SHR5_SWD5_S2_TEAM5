using Microsoft.EntityFrameworkCore;
using ContactBook.ConsoleApp.Models;

namespace ContactBook.ConsoleApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }
    }
}