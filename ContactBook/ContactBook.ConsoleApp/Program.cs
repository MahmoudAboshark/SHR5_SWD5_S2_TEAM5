using System.Text;
using ContactBook.ConsoleApp;
using ContactBook.ConsoleApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// UTF-8 so non-English names (e.g. Arabic) are read and stored correctly.
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string 'DefaultConnection' was not found in appsettings.json.");
    return 1;
}

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connectionString)
    .Options;

using var db = new AppDbContext(options);

try
{
    // Creates the database and applies any pending migrations.
    db.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine("Could not connect to the database.");
    Console.WriteLine("Check the connection string in appsettings.json and that SQLite is accessible.");
    Console.WriteLine();
    Console.WriteLine(ex.Message);
    return 1;
}

new ContactBookApp(db).Run();
return 0;