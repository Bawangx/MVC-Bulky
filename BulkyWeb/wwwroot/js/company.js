// Variabel global untuk menyimpan instance DataTable
var dataTable;

// Ketika halaman selesai dimuat
$(document).ready(function () {
    loadDataTable(); // Inisialisasi tabel
});

// Fungsi untuk memuat dan menampilkan data produk dari server
function loadDataTable() {
    dataTable = $('#tbData').DataTable({
        "ajax": {
            url: '/admin/company/getall' // Endpoint API ambil semua produk
        },
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'streetAddress', "width": "15%" },   // ← sudah benar
            { data: 'city', "width": "15%" },
            { data: 'state', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `
                <div class="w-75 btn-group" role="group">
                    <a href="/admin/company/upsert?id=${data}" class="btn btn-primary mx-2">
                        <i class="bi bi-pencil-square"></i> Edit
                    </a>
                    <a onClick=Delete('/admin/company/delete/${data}') class="btn btn-danger mx-2">
                        <i class="bi bi-trash-fill"></i> Delete
                    </a>
                </div>`;
                },
                "width": "25%"
            }
        ]

    });
}

// Fungsi untuk menghapus produk berdasarkan ID via Ajax
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

            // PENTING: Gunakan POST karena DELETE sering ditolak oleh server
            $.ajax({
                url: url,
                type: 'DELETE', // GANTI DARI 'DELETE' KE 'POST'
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload(); // Reload tabel setelah delete
                        toastr.success(data.message); // Tampilkan notifikasi sukses
                    } else {
                        toastr.error(data.message); // Tampilkan notifikasi error
                    }
                },
                error: function (xhr, status, error) {
                    toastr.error("Error deleting item.");
                    console.error(error); // Untuk debugging di console
                }
            });

        }
    });
}