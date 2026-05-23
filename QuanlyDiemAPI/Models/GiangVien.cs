namespace QuanlyDiemAPI.Models;

public class GiangVien
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }

    public int? NguoiDungId { get; set; }
    public NguoiDung? NguoiDung { get; set; }

    public ICollection<LopHoc> LopHocs { get; set; } = [];
    public ICollection<MonHoc> MonHocs { get; set; } = [];
}
