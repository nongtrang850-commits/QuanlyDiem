namespace QuanlyDiemAPI.Models;

public class DangKyMonHoc
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public int MonHocId { get; set; }
    // ChoDuyet | DaDuyet | TuChoi
    public string TrangThai { get; set; } = "ChoDuyet";
    public DateTime NgayDangKy { get; set; } = DateTime.UtcNow;

    public SinhVien? SinhVien { get; set; }
    public MonHoc? MonHoc { get; set; }
}
