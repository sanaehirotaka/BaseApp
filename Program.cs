using BaseApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVCサービスを設定し、セッション状態の一時データプロバイダーを追加します。
builder.Services
    .AddMvc()
    .AddSessionStateTempDataProvider();
// 依存性注入コンテナにセッションサービスを追加します。
builder.Services
    .AddSession();

// コンテナにIdentityサービスを追加します。
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.AllowedUserNameCharacters = "";
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

if (Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_ID") != null && Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_SECRET") != null)
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_ID");
            options.ClientSecret = Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_SECRET");
        });
}

// すべてのRazor Pagesにグローバルな承認フィルターを追加します。
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Account/SSO");
    options.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// HTTPリクエストパイプラインを設定します。
if (!app.Environment.IsDevelopment())
{
    // 非開発環境でカスタムエラーハンドラページを使用します。
    app.UseExceptionHandler("/Error");
    // MITM攻撃を防ぐため、本番環境ではHTTPSを強制します。
    // デフォルトのHSTS値は30日です。本番環境に合わせて調整してください。
    app.UseHsts();
}

// 安全な接続を確保するためにHTTPSリダイレクトを有効にします。
app.UseHttpsRedirection();
// wwwrootからCSS、JavaScript、画像などの静的ファイルを提供します。
app.UseStaticFiles();
// 定義されたエンドポイントに一致するようにルーティングを有効にします。
app.UseRouting();
// ユーザーセッションを管理するためにセッションミドルウェアを有効にします。
app.UseSession();
// ユーザーを識別するために認証ミドルウェアを有効にします。
app.UseAuthentication();
// ユーザーがリソースにアクセスする権限を持っているかを判断するために承認ミドルウェアを有効にします。
app.UseAuthorization();
// Razor Pagesのエンドポイントをマッピングします。
app.MapRazorPages();
// MVCパターン用のデフォルトコントローラールートをマッピングします。
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

// アプリケーション起動時に保留中のデータベースマイグレーションを適用します。
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // DbContextの保留中のマイグレーションを適用します。
    }
    catch (Exception ex)
    {
        // データベースマイグレーション中に発生したエラーをログに記録します。
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// アプリケーションを実行し、PORT環境変数で指定されたポートをリッスンします。指定がない場合はデフォルトを使用します。
app.Run(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")) ? null : $"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT")}");
