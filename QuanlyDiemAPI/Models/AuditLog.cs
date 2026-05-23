namespace QuanlyDiemAPI.Models;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime ThoiGian { get; set; } = DateTime.Now;
    public string Email { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public string ChiTiet { get; set; } = string.Empty;
}
