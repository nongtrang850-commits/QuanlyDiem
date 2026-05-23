namespace QuanlyDiemAPI.Models;

public class LopHoc
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Room { get; set; }
    public int? AdvisorId { get; set; }

    public GiangVien? Advisor { get; set; }
    public ICollection<SinhVien> SinhViens { get; set; } = [];
}
