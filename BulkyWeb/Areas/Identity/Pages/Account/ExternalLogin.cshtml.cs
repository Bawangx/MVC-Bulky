// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Bulky.Models;
using Bulky.Utility;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous] // Mengizinkan akses tanpa login
    public class ExternalLoginModel : PageModel
    {
        // Dependency services yang dibutuhkan
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        // Model data yang di-bind ke form input
        [BindProperty]
        public InputModel Input { get; set; }

        // Nama provider login eksternal (misal: Google, Facebook)
        public string ProviderDisplayName { get; set; }

        // URL untuk redirect setelah login berhasil
        public string ReturnUrl { get; set; }

        // Pesan error disimpan sementara menggunakan TempData
        [TempData]
        public string ErrorMessage { get; set; }

        // Model input form untuk data user tambahan
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Name { get; set; }

            public string? StreetAddress { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? PostalCode { get; set; }
            public string? PhoneNumber { get; set; }
        }

        /// <summary>
        /// Menangani request GET ke halaman ini.
        /// Redirect ke halaman login biasa, karena login eksternal harus dimulai dari halaman login.
        /// </summary>
        public IActionResult OnGet() => RedirectToPage("./Login");

        /// <summary>
        /// Menangani permintaan POST saat user memilih provider eksternal untuk login.
        /// Menginisiasi challenge ke provider (misal Google).
        /// </summary>
        /// <param name="provider">Nama provider eksternal</param>
        /// <param name="returnUrl">URL tujuan setelah login</param>
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Buat URL callback setelah login eksternal selesai
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });

            // Konfigurasi properti challenge external login
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Redirect ke penyedia login eksternal
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// Callback dari penyedia eksternal setelah proses login selesai.
        /// Melakukan proses autentikasi dan penanganan user terkait.
        /// </summary>
        /// <param name="returnUrl">URL tujuan setelah login</param>
        /// <param name="remoteError">Error dari provider eksternal jika ada</param>
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/"); // Default ke homepage jika returnUrl null

            if (remoteError != null)
            {
                // Tangani error dari provider eksternal
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Dapatkan info login eksternal
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Coba login user berdasarkan info login eksternal yang sudah ada
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                // Login berhasil
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                // User terkunci
                return RedirectToPage("./Lockout");
            }
            else
            {
                // User belum punya akun, tampilkan halaman registrasi untuk melengkapi data
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;

                // Ambil email & nama dari klaim provider jika ada
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Name = info.Principal.FindFirstValue(ClaimTypes.Name)
                    };
                }
                return Page();
            }
        }

        /// <summary>
        /// Menangani form registrasi setelah user mengisi data tambahan
        /// dan mengonfirmasi pembuatan akun baru yang terhubung ke login eksternal.
        /// </summary>
        /// <param name="returnUrl">URL tujuan setelah registrasi</param>
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Ambil info login eksternal lagi
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                // Buat instance user baru
                var user = CreateUser();

                // Set username dan email user
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Set data tambahan user
                user.StreetAddress = Input.StreetAddress;
                user.City = Input.City;
                user.State = Input.State;
                user.PostalCode = Input.PostalCode;
                user.Name = Input.Name;
                user.PhoneNumber = Input.PhoneNumber;

                // Simpan user ke database
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Tambahkan role Customer ke user baru
                    await _userManager.AddToRoleAsync(user, SD.Role_Customer);

                    // Kaitkan login eksternal dengan user baru
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // Kirim email konfirmasi akun
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // Jika harus konfirmasi akun, redirect ke halaman konfirmasi
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        // Login user dan redirect ke returnUrl
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }

                // Jika ada error, tampilkan pesan error
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        /// <summary>
        /// Membuat instance ApplicationUser baru.
        /// Pastikan ApplicationUser memiliki constructor parameterless dan bukan abstract class.
        /// </summary>
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or override this method.");
            }
        }

        /// <summary>
        /// Mendapatkan IUserEmailStore dari IUserStore.
        /// Pastikan user store mendukung email.
        /// </summary>
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}