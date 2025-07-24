using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        // Service untuk mengelola user ASP.NET Identity
        private readonly UserManager<ApplicationUser> _userManager;

        // Service untuk menangani proses sign in (login)
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Service untuk mengelola role (peran)
        private readonly RoleManager<IdentityRole> _roleManager;

        // Store khusus untuk email user (agar bisa simpan email)
        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        // Logger untuk menulis log aktivitas aplikasi
        private readonly ILogger<RegisterModel> _logger;

        // Service untuk mengirim email (konfirmasi, dll)
        private readonly IEmailSender _emailSender;

        // UnitOfWork untuk akses data (repository pattern)
        private readonly IUnitOfWork _unitOfWork;

        // Konstruktor: Inject semua dependency service yang dibutuhkan
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailStore = GetEmailStore(userStore);
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        // Property untuk binding form input pendaftaran
        [BindProperty]
        public InputModel Input { get; set; }

        // URL untuk redirect setelah proses selesai
        public string ReturnUrl { get; set; }

        // List external login provider (Google, Facebook, dll)
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Model input untuk form pendaftaran
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal 6 karakter.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Password dan konfirmasi tidak cocok.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }

            [ValidateNever] // Jangan divalidasi oleh model validator (karena diisi manual)
            public IEnumerable<SelectListItem> RoleList { get; set; }

            [Required]
            public string Name { get; set; }

            public string? StreetAddress { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? PostalCode { get; set; }
            public string? PhoneNumber { get; set; }

            public int? CompanyId { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> CompanyList { get; set; }
        }

        /// <summary>
        /// Handle GET request untuk menampilkan halaman register.
        /// Inisialisasi daftar Role dan Company untuk dropdown di form.
        /// Jika role belum ada, buat role-role dasar terlebih dahulu.
        /// </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Cek apakah role sudah dibuat, jika belum buat role dasar
            if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
            }

            // Siapkan data untuk dropdown role dan company
            Input = new InputModel
            {
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }),

                CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            ReturnUrl = returnUrl;

            // Ambil daftar external login provider (Google, dll)
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// Handle POST request ketika user submit form pendaftaran.
        /// Validasi input, buat user, assign role, kirim email konfirmasi.
        /// Jika sukses, redirect sesuai pengaturan aplikasi.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Buat objek user baru dari data input
                var user = new ApplicationUser
                {
                    Name = Input.Name,
                    Email = Input.Email,
                    UserName = Input.Email,
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    State = Input.State,
                    PostalCode = Input.PostalCode,
                    PhoneNumber = Input.PhoneNumber,

                    // Jika role Company, set CompanyId, jika tidak null
                    CompanyId = Input.Role == SD.Role_Company ? Input.CompanyId : null
                };

                // Set email ke user store (penting untuk UserManager)
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Buat user baru di database dengan password
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Tentukan role user, default Customer jika kosong
                    var roleToAssign = string.IsNullOrEmpty(Input.Role) ? SD.Role_Customer : Input.Role;

                    // Tambahkan user ke role yang dipilih
                    await _userManager.AddToRoleAsync(user, roleToAssign);

                    // Generate token email confirmation
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // Buat URL callback untuk konfirmasi email
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    // Kirim email konfirmasi ke user
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // Jika aplikasi mengharuskan konfirmasi email sebelum login
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // Redirect ke halaman konfirmasi registrasi
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        // Jika yang register adalah admin (dari UI admin portal)
                        if (User.IsInRole(SD.Role_Admin))
                        {
                            TempData["Success"] = "New User Created Successfully";
                            // Tetap di halaman ini (biasanya untuk admin tambah user lain)
                        }
                        else
                        {
                            // Langsung login user baru
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            // Redirect ke halaman tujuan
                            return LocalRedirect(returnUrl);
                        }
                    }
                }

                // Jika ada error ketika membuat user, tampilkan di validation summary
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Jika validasi gagal, reload halaman dengan pesan error
            return Page();
        }

        /// <summary>
        /// Mengambil email store dari user store.
        /// Pastikan UserStore mendukung operasi email.
        /// </summary>
        /// <param name="userStore">UserStore umum</param>
        /// <returns>IUserEmailStore yang support email</returns>
        private IUserEmailStore<ApplicationUser> GetEmailStore(IUserStore<ApplicationUser> userStore)
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)userStore;
        }
    }
}