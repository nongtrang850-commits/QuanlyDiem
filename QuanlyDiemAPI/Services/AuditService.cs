using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Services;

public class AuditService(AppDbContext db)
{
    public async Task GhiAsync(string email, string vaiTro, string hanhDong, string chiTiet = "")
    {
        db.AuditLogs.Add(new AuditLog
        {
            Email    = email,
            VaiTro   = vaiTro,
            HanhDong = hanhDong,
            ChiTiet  = chiTiet
        });
        await db.SaveChangesAsync();
    }
}
