using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Bulky.Utility
{
    // Kelas ini bertugas mengirim email menggunakan layanan SendGrid
    public class EmailSender : IEmailSender
    {
        // Menyimpan API key rahasia SendGrid yang diambil dari konfigurasi aplikasi
        public string SendGridSecret { get; set; }

        // Konstruktor mengambil konfigurasi aplikasi untuk mengambil API key SendGrid
        public EmailSender(IConfiguration _config)
        {
            // Ambil nilai SecretKey dari bagian konfigurasi "SendGrid:SecretKey"
            SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        }

        // Metode utama untuk mengirim email secara asynchronous
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Membuat client SendGrid menggunakan API key
            var client = new SendGridClient(SendGridSecret);

            // Alamat pengirim email, bisa disesuaikan sesuai domain Anda
            var from = new EmailAddress("hello@moakt.cc", "Bulky");

            // Alamat penerima email (diterima dari parameter method)
            var to = new EmailAddress(email);

            // Membuat objek email dengan format plain text kosong ("") dan isi htmlMessage
            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: "", htmlContent: htmlMessage);

            // Kirim email secara asynchronous dan kembalikan Task-nya
            return client.SendEmailAsync(message);
        }
    }
}