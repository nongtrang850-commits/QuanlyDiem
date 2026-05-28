namespace QuanlyDiemAPI.Models;

public class NguoiDung
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string Role { get; set; } = "SinhVien"; // Admin | GiangVien | SinhVien

    public SinhVien? SinhVien { get; set; }
    public GiangVien? GiangVien { get; set; }
}
