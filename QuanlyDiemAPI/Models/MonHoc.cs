namespace QuanlyDiemAPI.Models;

public class MonHoc
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Department { get; set; }
    public int HocKy { get; set; } = 1;

    public int? GiangVienId { get; set; }
    public GiangVien? GiangVien { get; set; }

    public ICollection<Diem> Diems { get; set; } = [];
}
