using BaseApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BaseApp.Pages.Account;

/// <summary>
/// ログインページのモデルを表します。
/// ユーザー認証と、ログイン成功後のリダイレクトを処理します。
/// </summary>
public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager; // Add UserManager
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<LoginModel> logger) // Update constructor
    {
        _signInManager = signInManager;
        _userManager = userManager; // Assign UserManager
        _logger = logger;
    }

    /// <summary>
    /// ログイン資格情報のための入力モデルを取得または設定します。
    /// このプロパティはHTTPリクエストからバインドされます。
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// ページに表示するエラーメッセージを取得または設定します。
    /// このデータは一時的なものであり、読み取られた後にクリアされます。
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; } = default!;

    /// <summary>
    /// 外部ログインプロバイダーのリストを取得または設定します。
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = default!;

    /// <summary>
    /// ログインフォームの入力フィールドを表します。
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// ユーザーのメールアドレスを取得または設定します。
        /// このフィールドは必須であり、有効なメール形式である必要があります。
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = default!;

        /// <summary>
        /// ユーザーのパスワードを取得または設定します。
        /// このフィールドは必須であり、入力タイプはパスワードです。
        /// </summary>
        [Required]
        [StringLength(255, ErrorMessage = "{0} は {2} 文字以上 {1} 文字以下である必要があります。", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = default!;

        /// <summary>
        /// ユーザーがログイン状態を保持するかどうかを示す値を取得または設定します。
        /// </summary>
        [Display(Name = "ログイン状態を保持しますか？")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// ログインページのGETリクエストを処理します。
    /// 戻るURLを初期化し、既存の外部クッキーをクリアします。
    /// </summary>
    public async Task OnGetAsync()
    {
        // 外部ログインプロバイダーをクリアして、新しいログインスキームを確保します。
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }
    }

    /// <summary>
    /// ログインページのPOSTリクエストを処理します。
    /// ユーザーの資格情報を検証し、有効な場合はユーザーをサインインさせます。
    /// </summary>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        // 外部ログインプロバイダーをクリアして、新しいログインスキームを確保します。
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            // これはアカウントロックアウトに対するログイン失敗をカウントしません
            // パスワード失敗でアカウントロックアウトをトリガーするには、lockoutOnFailure: trueを設定します
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return LocalRedirect("~/");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "無効なログイン試行です。");
                return Page();
            }
        }

        // ここまで来た場合、何かが失敗しました。フォームを再表示します
        return Page();
    }

    /// <summary>
    /// 外部ログインプロバイダーからのPOSTリクエストを処理します。
    /// </summary>
    /// <param name="provider">外部ログインプロバイダーの名前。</param>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public IActionResult OnPostExternalLogin(string provider)
    {
        var redirectUrl = Url.Page("./Login", pageHandler: "Callback");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    /// <summary>
    /// 外部ログインプロバイダーからのコールバックを処理します。
    /// </summary>
    /// <param name="remoteError">外部ログインプロバイダーからのエラーメッセージ。</param>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnGetCallbackAsync(string? remoteError = null)
    {
        if (remoteError != null)
        {
            ErrorMessage = $"外部プロバイダーからのエラー: {remoteError}";
            return RedirectToPage("./Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "外部ログイン情報の読み込み中にエラーが発生しました。";
            return RedirectToPage("./Login");
        }

        // ユーザーがこのサイトに既にログインしている場合、外部ログイン情報でサインインします。
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} が {LoginProvider} ログインでログインしました。", info.Principal.Identity?.Name, info.LoginProvider);
            return LocalRedirect("~/");
        }
        else
        {
            // ユーザーがアカウントを持っていない場合、自動的に登録を試みます。
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;

            if (string.IsNullOrEmpty(email))
            {
                ErrorMessage = "ユーザー情報を取得できませんでした。";
                return RedirectToPage("./Login");
            }

            var user = new User { UserName = userName, Email = email, CreatedAt = DateTime.UtcNow };
            var createResult = await _userManager.CreateAsync(user);

            if (createResult.Succeeded)
            {
                createResult = await _userManager.AddLoginAsync(user, info);
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    _logger.LogInformation("ユーザー {Email} が {LoginProvider} でアカウントを作成しました。", user.Email, info.LoginProvider);
                    return LocalRedirect("~/");
                }
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ErrorMessage = "外部ログインでユーザーを登録できませんでした。";
            return RedirectToPage("./Login");
        }
    }
}
