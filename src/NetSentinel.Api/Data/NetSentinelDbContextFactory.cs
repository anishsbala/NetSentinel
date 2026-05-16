using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetSentinel.Api.Data;

public sealed class NetSentinelDbContextFactory : IDesignTimeDbContextFactory<NetSentinelDbContext>
{
    public NetSentinelDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NetSentinelDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=NetSentinel;User Id=sa;" +
                "Password=NetSentinel_Strong!2026;TrustServerCertificate=True;Encrypt=True")
            .Options;

        return new NetSentinelDbContext(options);
    }
}
