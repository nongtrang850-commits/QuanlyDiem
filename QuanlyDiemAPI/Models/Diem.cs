namespace QuanlyDiemAPI.Models;

public class Diem
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public int MonHocId { get; set; }
    public float? CC { get; set; }   // Chuyên cần
    public float? KT1 { get; set; }
    public float? KT2 { get; set; }
    public float? KT3 { get; set; }
    public float? Exam { get; set; } // Thi cuối kỳ

    public int? GiangVienId { get; set; }

    public SinhVien? SinhVien { get; set; }
    public MonHoc? MonHoc { get; set; }
    public GiangVien? GiangVien { get; set; }
}
