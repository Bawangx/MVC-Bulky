using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Kelas backend untuk halaman pengaturan 2FA dengan aplikasi authenticator.
    /// </summary>
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly UrlEncoder _urlEncoder;

        // Format URL standar untuk QR Code 2FA (skema otpauth)
        private const string AuthenticatorUriFormat =
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public EnableAuthenticatorModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        // Kunci rahasia dalam bentuk format yang mudah dibaca user
        public string SharedKey { get; set; }

        // URL QR code untuk aplikasi authenticator
        public string AuthenticatorUri { get; set; }

        // Recovery codes yang disiapkan setelah 2FA aktif
        [TempData]
        public string[] RecoveryCodes { get; set; }

        // Status pesan untuk ditampilkan di UI
        [TempData]
        public string StatusMessage { get; set; }

        // Data yang akan di-bind dari input form (kode verifikasi)
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Model input untuk menerima kode verifikasi dari user.
        /// </summary>
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; }
        }

        /// <summary>
        /// GET: Tampilkan halaman dan generate QR code.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadSharedKeyAndQrCodeUriAsync(user);

            return Page();
        }

        /// <summary>
        /// POST: Verifikasi kode OTP yang diinput user.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Membersihkan input dari spasi dan strip
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            // Validasi kode OTP
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Jika valid, aktifkan 2FA untuk user ini
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            // Buat recovery codes jika belum ada
            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }

        /// <summary>
        /// Generate kunci rahasia dan URL QR code untuk ditampilkan di form.
        /// </summary>
        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        /// <summary>
        /// Format kunci rahasia agar mudah dibaca user.
        /// Contoh: abcd efgh ijkl
        /// </summary>
        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Generate URL QR code yang akan dipindai aplikasi authenticator.
        /// </summary>
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                AuthenticatorUriFormat,
                _urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"), // Nama aplikasi (issuer)
                _urlEncoder.Encode(email),                               // Identitas pengguna
                unformattedKey);                                         // Kunci rahasia
        }
    }
}