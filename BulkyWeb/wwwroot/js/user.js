// Variabel global untuk menyimpan instance DataTable
var dataTable;

// Ketika halaman selesai dimuat
$(document).ready(function () {
    loadDataTable(); // Memanggil fungsi untuk menampilkan tabel
});

// Fungsi untuk memuat dan menampilkan data user dari server menggunakan DataTables
function loadDataTable() {
    dataTable = $('#tbData').DataTable({
        "ajax": {
            url: '/admin/user/getall' // Memanggil endpoint API untuk mendapatkan semua user
        },
        "columns": [
            { data: 'name', "width": "15%" },            // Nama user
            { data: 'email', "width": "15%" },           // Email user
            { data: 'phoneNumber', "width": "15%" },     // Nomor telepon user
            { data: 'company.name', "width": "15%" },    // Nama perusahaan user (relasi)
            { data: 'role', "width": "15%" },            // Role user (admin/customer/etc)
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" }, // Data kombinasi id dan lockoutEnd
                "render": function (data) {
                    var today = new Date().getTime();               // Waktu saat ini
                    var lockout = new Date(data.lockoutEnd).getTime(); // Waktu lockout user

                    var lockButton = `
                        <a onclick="LockUnlock('${data.id}')" 
                           class="btn ${lockout > today ? 'btn-danger' : 'btn-success'} text-white" 
                           style="cursor:pointer; width:100px;">
                            <i class="bi bi-unlock-fill"></i> 
                            ${lockout > today ? 'Locked' : 'Unlocked'}
                        </a>`;

                    var permissionButton = `
                        <a href="/admin/user/RoleManagment?userId=${data.id}" 
                           class="btn btn-primary text-white" 
                           style="cursor:pointer; width:150px;">
                            <i class="bi bi-pencil-square"></i> Permission
                        </a>`;

                    return `
                        <div class="text-center">
                            ${lockButton}
                            ${permissionButton}
                        </div>`;
                },
                "width": "25%"
            }
        ]
    });
}

// Fungsi untuk mengunci atau membuka kunci user (lock/unlock)
function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',         // Endpoint API lock/unlock
        data: JSON.stringify(id),              // Kirim ID user dalam format JSON
        contentType: "application/json",       // Tipe konten yang dikirim
        success: function (data) {             // Callback jika berhasil
            if (data.success) {
                toastr.success(data.message);  // Tampilkan notifikasi sukses
                dataTable.ajax.reload();       // Reload tabel agar data terbaru muncul
            } else {
                toastr.error(data.message);    // Jika gagal, tampilkan pesan error
            }
        }
    });
}