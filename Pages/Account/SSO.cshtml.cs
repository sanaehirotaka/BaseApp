using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BaseApp.Data;

namespace BaseApp.Pages.Account
{
    public class SSOModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;

        public SSOModel(SignInManager<User> signInManager, AppDbContext context)
        {
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("./Login");
            }

            var accessToken = await _context.AccessTokens
                .Include(at => at.User)
                .FirstOrDefaultAsync(at => at.Token == token && at.ExpiresAt > DateTime.UtcNow);

            if (accessToken == null)
            {
                // トークンが無効または期限切れ
                return RedirectToPage("./Login");
            }

            // ユーザーをサインインさせる
            await _signInManager.SignInAsync(accessToken.User, isPersistent: false);

            return LocalRedirect("~/");
        }
    }
}
