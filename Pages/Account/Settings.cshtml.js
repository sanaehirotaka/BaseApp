// アクセストークン破棄フォームの確認ダイアログ
document.querySelectorAll('.revoke-token-form').forEach(form => {
    form.addEventListener('submit', function (event) {
        if (!confirm('このアクセストークンを破棄してもよろしいですか？')) {
            event.preventDefault(); // フォームの送信をキャンセル
        }
    });
});

// アクセストークンコピー機能
document.querySelectorAll('.copy-token-button').forEach(button => {
    button.addEventListener('click', function () {
        const token = this.dataset.token;
        const successMessage = this.closest('td').querySelector('.copy-success-message');

        navigator.clipboard.writeText(token).then(() => {
            // コピー成功メッセージを表示
            if (successMessage) {
                successMessage.style.display = 'inline';
                setTimeout(() => {
                    successMessage.style.display = 'none';
                }, 2000); // 2秒後に非表示
            }
        }).catch(err => {
            console.error('トークンのコピーに失敗しました: ', err);
            alert('トークンのコピーに失敗しました。手動でコピーしてください。');
        });
    });
});

// SSO URLコピー機能
document.querySelectorAll('.copy-sso-url-button').forEach(button => {
    button.addEventListener('click', function () {
        const ssoUrl = this.dataset.url;
        const successMessage = this.closest('div').querySelector('.copy-sso-success-message');

        navigator.clipboard.writeText(ssoUrl).then(() => {
            // コピー成功メッセージを表示
            if (successMessage) {
                successMessage.style.display = 'inline';
                setTimeout(() => {
                    successMessage.style.display = 'none';
                }, 2000); // 2秒後に非表示
            }
        }).catch(err => {
            console.error('SSO URLのコピーに失敗しました: ', err);
            alert('SSO URLのコピーに失敗しました。手動でコピーしてください。');
        });
    });
});
