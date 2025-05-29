using BaseApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BaseApp.Pages.Account;

/// <summary>
/// ユーザー登録ページのモデルを表します。
/// 新規ユーザーの作成と検証を処理します。
/// </summary>
public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<RegisterModel> _logger;

    /// <summary>
    /// <see cref="RegisterModel"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="userManager">ユーザーを管理するためのマネージャー。</param>
    /// <param name="signInManager">ユーザーのサインインを管理するためのマネージャー。</param>
    /// <param name="logger">ロギング機能を提供します。</param>
    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// 登録詳細の入力モデルを取得または設定します。
    /// このプロパティはHTTPリクエストからバインドされます。
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// 登録フォームの入力フィールドを表します。
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// ユーザーのメールアドレスを取得または設定します。
        /// このフィールドは必須であり、有効なメール形式である必要があり、「Email」として表示されます。
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = default!;

        /// <summary>
        /// ユーザーの表示名を取得または設定します。
        /// このフィールドは必須であり、最大長は50文字で、「表示名」として表示されます。
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "表示名")]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// ユーザーのパスワードを取得または設定します。
        /// このフィールドは必須であり、10文字以上255文字以下である必要があり、入力タイプはパスワードです。
        /// </summary>
        [Required]
        [StringLength(255, ErrorMessage = "{0} は {2} 文字以上 {1} 文字以下である必要があります。", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = default!;

        /// <summary>
        /// 確認用パスワードを取得または設定します。
        /// このフィールドはパスワードフィールドと一致する必要があります。
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "パスワードの確認")]
        [Compare("Password", ErrorMessage = "パスワードと確認用パスワードが一致しません。")]
        public string ConfirmPassword { get; set; } = default!;
    }

    /// <summary>
    /// 登録ページのGETリクエストを処理します。
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// 登録ページのPOSTリクエストを処理します。
    /// 入力を検証し、既存のユーザーをチェックし、パスワードをハッシュ化し、新しいユーザーをデータベースに保存します。
    /// </summary>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = new User { UserName = Input.Email, Email = Input.Email, DisplayName = Input.DisplayName, CreatedAt = DateTime.UtcNow };
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // ユーザー作成後、自動的にサインインさせます。
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect("~/");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // モデルの状態が無効な場合、検証エラーとともにフォームを再表示します。
        return Page();
    }
}
