using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<GiangVien> GiangViens { get; set; }
    public DbSet<SinhVien> SinhViens { get; set; }
    public DbSet<LopHoc> LopHocs { get; set; }
    public DbSet<MonHoc> MonHocs { get; set; }
    public DbSet<Diem> Diems { get; set; }
    public DbSet<DanhgiaAI> DanhgiaAIs { get; set; }
    public DbSet<DangKyMonHoc> DangKyMonHocs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NguoiDung>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<SinhVien>(e =>
        {
            e.HasOne(x => x.NguoiDung)
             .WithOne(u => u.SinhVien)
             .HasForeignKey<SinhVien>(x => x.NguoiDungId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.LopHoc)
             .WithMany(l => l.SinhViens)
             .HasForeignKey(x => x.LopHocId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<GiangVien>(e =>
        {
            e.HasOne(x => x.NguoiDung)
             .WithOne(u => u.GiangVien)
             .HasForeignKey<GiangVien>(x => x.NguoiDungId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MonHoc>(e =>
        {
            e.Property(x => x.HocKy).HasDefaultValue(1);
            e.HasOne(x => x.GiangVien)
             .WithMany(g => g.MonHocs)
             .HasForeignKey(x => x.GiangVienId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Diem>(e =>
        {
            e.HasOne(x => x.GiangVien)
             .WithMany()
             .HasForeignKey(x => x.GiangVienId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DanhgiaAI>(e =>
        {
            e.HasIndex(x => x.StudentId).IsUnique();
            e.HasOne(x => x.SinhVien)
             .WithOne(s => s.DanhgiaAI)
             .HasForeignKey<DanhgiaAI>(x => x.StudentId);
        });

        modelBuilder.Entity<Diem>(e =>
        {
            e.HasOne(x => x.SinhVien)
             .WithMany(s => s.Diems)
             .HasForeignKey(x => x.SinhVienId);
            e.HasOne(x => x.MonHoc)
             .WithMany(m => m.Diems)
             .HasForeignKey(x => x.MonHocId);
            e.HasIndex(x => new { x.SinhVienId, x.MonHocId }).IsUnique();
        });

        modelBuilder.Entity<LopHoc>(e =>
        {
            e.HasOne(x => x.Advisor)
             .WithMany(g => g.LopHocs)
             .HasForeignKey(x => x.AdvisorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DangKyMonHoc>(e =>
        {
            e.HasOne(x => x.SinhVien)
             .WithMany()
             .HasForeignKey(x => x.SinhVienId);
            e.HasOne(x => x.MonHoc)
             .WithMany()
             .HasForeignKey(x => x.MonHocId);
            e.HasIndex(x => new { x.SinhVienId, x.MonHocId }).IsUnique();
        });
    }
}
