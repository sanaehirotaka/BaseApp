using System;
using Microsoft.AspNetCore.Identity;

namespace BaseApp.Data
{
    /// <summary>
    /// アプリケーションのユーザーを表します。
    /// このクラスは、データベースに保存されるユーザーデータのスキーマを定義します。
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// ユーザーアカウントが作成されたときのタイムスタンプを取得または設定します。
        /// 作成時には現在のUTC時刻がデフォルトとなります。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ユーザーの表示名を取得または設定します。
        /// </summary>
        public string? DisplayName { get; set; }
    }
}
