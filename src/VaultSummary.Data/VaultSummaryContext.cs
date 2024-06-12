using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace VaultSummary.Data;

public class VaultSummaryContext : DbContext
{
    public VaultSummaryContext()
    {
        
    }
    
    public VaultSummaryContext(DbContextOptions<VaultSummaryContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer();
    }
}