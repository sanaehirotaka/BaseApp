using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BaseApp.Data;

namespace BaseApp.Data;

/// <summary>
/// アプリケーションのデータベースコンテキストを表し、IdentityDbContextを継承します。
/// このクラスは、基になるデータベースと対話し、エンティティ（ASP.NET Core Identityに必要なものを含む）を管理するために使用されます。
/// </summary>
public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<AccessToken> AccessTokens { get; set; }

    /// <summary>
    /// <see cref="AppDbContext"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="options"><see cref="DbContext"/> で使用されるオプション。</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
