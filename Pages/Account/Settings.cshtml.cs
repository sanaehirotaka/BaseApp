using BaseApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace BaseApp.Pages.Account;

public class SettingsModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<SettingsModel> _logger;
    private readonly AppDbContext _context;

    public SettingsModel(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<SettingsModel> logger, AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _context = context;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [StringLength(50)]
        [Required(ErrorMessage = "表示名は必須です。")]
        [Display(Name = "表示名")]
        public string DisplayName { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "現在のパスワード")]
        public string? OldPassword { get; set; }

        [StringLength(512, ErrorMessage = "{0}は{2}文字以上{1}文字以下である必要があります。", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Display(Name = "新しいパスワード")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "新しいパスワードの確認")]
        [Compare("NewPassword", ErrorMessage = "新しいパスワードと確認用パスワードが一致しません。")]
        public string? ConfirmPassword { get; set; }
    }

    [Display(Name = "メールアドレス")]
    public string? Email { get; set; }

    public IList<AccessToken> AccessTokens { get; set; } = [];

    private async Task LoadAsync(User user)
    {
        Input.DisplayName ??= user.DisplayName!;
        Email ??= await _userManager.GetEmailAsync(user);
        AccessTokens = await _context.AccessTokens
            .Where(at => at.UserId == user.Id)
            .OrderByDescending(at => at.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // ユーザーを取得できない場合はログアウト
            await _signInManager.SignOutAsync();
            return LocalRedirect("~/");
        }
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateAccessTokenAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("~/");
        }

        var randomNumber = new byte[48];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        var newToken = Convert.ToBase64String(randomNumber)
                            .Replace('+', '-')
                            .Replace('/', '_')
                            .Replace("=", ""); // URLセーフなBase64に変換

        var accessToken = new AccessToken
        {
            Token = newToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        _context.AccessTokens.Add(accessToken);
        await _context.SaveChangesAsync();

        StatusMessage = "新しいアクセストークンが発行されました。";
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostRevokeAccessTokenAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }

        var accessToken = await _context.AccessTokens.FirstOrDefaultAsync(at => at.Id == id && at.UserId == user.Id);

        if (accessToken == null)
        {
            StatusMessage = "エラー: 指定されたアクセストークンが見つからないか、権限がありません。";
            await LoadAsync(user);
            return Page();
        }

        _context.AccessTokens.Remove(accessToken);
        await _context.SaveChangesAsync();

        StatusMessage = "アクセストークンが破棄されました。";
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // ユーザーを取得できない場合はログアウト
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
        await LoadAsync(user);

        // パスワードが提供されていない場合はUserNameのみを検証
        if (string.IsNullOrEmpty(Input.NewPassword) && string.IsNullOrEmpty(Input.OldPassword) && string.IsNullOrEmpty(Input.ConfirmPassword))
        {
            ModelState.Remove("Input.OldPassword");
            ModelState.Remove("Input.NewPassword");
            ModelState.Remove("Input.ConfirmPassword");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.DisplayName = Input.DisplayName;

        // NewPasswordが提供されている場合のみパスワード変更を試行
        if (!string.IsNullOrEmpty(Input.NewPassword))
        {
            var hasPassword = await _userManager.HasPasswordAsync(user);
            IdentityResult changePasswordResult;

            if (hasPassword)
            {
                if (string.IsNullOrEmpty(Input.OldPassword))
                {
                    ModelState.AddModelError(string.Empty, "パスワードを変更するには現在のパスワードが必要です。");
                    return Page();
                }
                changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            }
            else
            {
                // ユーザーがパスワードを持っていない場合、初めて設定する。
                // 古いパスワードは不要。
                changePasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            }

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
        }

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "プロフィールが更新されました。";
        return Page();
    }
}
