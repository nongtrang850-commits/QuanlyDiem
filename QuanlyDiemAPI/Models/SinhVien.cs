namespace QuanlyDiemAPI.Models;

public class SinhVien
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Department { get; set; }
    public string? Course { get; set; }

    public int? NguoiDungId { get; set; }
    public NguoiDung? NguoiDung { get; set; }

    public int? LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }

    public ICollection<Diem> Diems { get; set; } = [];
    public DanhgiaAI? DanhgiaAI { get; set; }
}
