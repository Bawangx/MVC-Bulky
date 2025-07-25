using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repository
{
    // Repository umum yang bisa digunakan untuk berbagai tipe entitas (generic T)
    // T harus kelas (class)
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db; // DbContext untuk akses database
        internal DbSet<T> dbSet; // DbSet untuk entitas tipe T

        // Konstruktor menerima ApplicationDbContext dan set DbSet<T>
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();

            // Baris ini kurang tepat fungsinya di sini, bisa dihapus atau dipindah:
            // _db.Products.Include(u => u.Category).Include(u => u.CategoryId);
            // Karena ini tidak menyimpan hasil dan tidak berpengaruh.
        }

        // Menambah entitas baru ke DbSet
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        // Mengambil satu entitas berdasarkan filter (kondisi)
        // includeProperties memungkinkan untuk eager load properti navigation (relasi)
        // tracked menentukan apakah entitas akan dilacak oleh EF (untuk update) atau tidak (lebih ringan jika hanya baca)
        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;

            if (tracked)
            {
                query = dbSet; // entitas dilacak oleh EF
            }
            else
            {
                query = dbSet.AsNoTracking(); // entitas tidak dilacak (untuk performa baca saja)
            }

            query = query.Where(filter); // filter kondisi

            // Jika ada properti relasi yang ingin dimuat, lakukan Include
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return query.FirstOrDefault(); // ambil entitas pertama atau null jika tidak ada
        }

        // Mengambil semua entitas (atau yang sesuai filter)
        // includeProperties untuk eager loading relasi
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return query.ToList(); // eksekusi query dan kembalikan list hasil
        }

        // Menghapus entitas dari DbSet
        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        // Menghapus banyak entitas sekaligus
        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}