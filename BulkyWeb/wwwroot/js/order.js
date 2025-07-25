// Variabel global untuk menyimpan instance DataTable agar bisa diakses ulang
var dataTable;

$(document).ready(function () {
    // Ambil query string dari URL, misal ?status=inprocess
    var url = window.location.search;

    // Cek apakah query string mengandung kata status tertentu,
    // lalu panggil fungsi loadDataTable dengan status tersebut
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    } else if (url.includes("completed")) {
        loadDataTable("completed");
    } else if (url.includes("pending")) {
        loadDataTable("pending");
    } else if (url.includes("approved")) {
        loadDataTable("approved");
    } else {
        // Jika tidak ada filter status, muat semua data
        loadDataTable("all");
    }
});

/**
 * Fungsi untuk memuat dan menampilkan data ke tabel dengan DataTables
 * @param {string} status - status filter untuk request data
 */
function loadDataTable(status) {
    // Inisialisasi DataTable pada elemen dengan id 'tbData'
    dataTable = $('#tbData').DataTable({
        // Konfigurasi ajax untuk mengambil data dari server
        "ajax": {
            url: '/admin/order/getall?status=' + status, // Kirim status sebagai query param
        },
        // Definisi kolom yang akan ditampilkan di tabel
        "columns": [
            { data: 'id', "width": "5%" },                 // Kolom ID
            { data: 'name', "width": "25%" },              // Nama customer
            { data: 'phoneNumber', "width": "20%" },       // Nomor telepon
            { data: 'applicationUser.email', "width": "20%" }, // Email user (nested object)
            { data: 'orderStatus', "width": "10%" },       // Status order
            { data: 'orderTotal', "width": "10%" },        // Total harga order
            {
                data: 'id',                                // Kolom aksi (edit/detail)
                "render": function (data) {
                    // Mengembalikan HTML tombol dengan link dinamis berdasarkan id order
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                        </div>`;
                },
                "width": "10%"
            }
        ]
    });
}