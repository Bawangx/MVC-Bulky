using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface generic untuk operasi dasar CRUD (Create, Read, Update, Delete)
    // T adalah tipe class yang akan di-manage, misal Category, Product, dll.
    public interface IRepository<T> where T : class
    {
        // Ambil semua data dengan opsi filter dan relasi yang ingin dimasukkan (includeProperties)
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        // Ambil 1 data berdasarkan filter, dengan opsi include properti relasi dan tracking
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);

        // Tambahkan data baru ke database
        void Add(T entity);

        // Hapus data tertentu
        void Remove(T entity);

        // Hapus banyak data sekaligus
        void RemoveRange(IEnumerable<T> entity);
    }
}