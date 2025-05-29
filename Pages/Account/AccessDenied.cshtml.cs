using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BaseApp.Pages.Account;

/// <summary>
/// アクセス拒否ページのモデルを表します。
/// このページは、ユーザーが十分な認証なしにリソースにアクセスしようとしたときに表示されます。
/// </summary>
public class AccessDeniedModel : PageModel
{
    /// <summary>
    /// アクセス拒否ページのGETリクエストを処理します。
    /// </summary>
    public void OnGet()
    {
    }
}
