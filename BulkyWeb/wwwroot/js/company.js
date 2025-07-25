// Variabel global untuk menyimpan instance DataTable,
// agar bisa dipakai ulang, misal untuk reload data
var dataTable;

// Ketika halaman selesai dimuat
$(document).ready(function () {
    loadDataTable(); // Inisialisasi DataTable dan muat data
});

/**
 * Fungsi untuk memuat dan menampilkan data perusahaan
 * dari endpoint API dan render di tabel #tbData
 */
function loadDataTable() {
    dataTable = $('#tbData').DataTable({
        "ajax": {
            url: '/admin/company/getall' // Endpoint API untuk ambil semua data company
        },
        "columns": [
            { data: 'name', "width": "15%" },          // Nama Perusahaan
            { data: 'streetAddress', "width": "15%" }, // Alamat jalan
            { data: 'city', "width": "15%" },          // Kota
            { data: 'state', "width": "15%" },         // Provinsi/State
            { data: 'phoneNumber', "width": "15%" },   // No telepon
            {
                data: 'id',
                "render": function (data) {
                    // Tombol aksi Edit dan Delete dengan link dinamis berdasarkan id company
                    return `
                <div class="w-75 btn-group" role="group">
                    <a href="/admin/company/upsert?id=${data}" class="btn btn-primary mx-2">
                        <i class="bi bi-pencil-square"></i> Edit
                    </a>
                    <a onClick="Delete('/admin/company/delete/${data}')" class="btn btn-danger mx-2">
                        <i class="bi bi-trash-fill"></i> Delete
                    </a>
                </div>`;
                },
                "width": "25%"
            }
        ]
    });
}

/**
 * Fungsi untuk menghapus data company berdasarkan URL API
 * @param {string} url - URL endpoint untuk menghapus data
 */
function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            // Gunakan AJAX untuk request penghapusan data
            // Di sini gunakan method POST karena seringkali method DELETE tidak diterima oleh server default
            $.ajax({
                url: url,
                type: 'POST', // Ganti dari 'DELETE' ke 'POST' agar kompatibel
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload(); // Reload tabel agar data terbaru muncul
                        toastr.success(data.message); // Tampilkan notifikasi sukses
                    } else {
                        toastr.error(data.message); // Tampilkan notifikasi error dari server
                    }
                },
                error: function (xhr, status, error) {
                    toastr.error("Error deleting item."); // Notifikasi error umum
                    console.error(error); // Log error ke console untuk debugging
                }
            });
        }
    });
}