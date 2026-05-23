namespace QuanlyDiemAPI.Models;

public class DanhgiaAI
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public float Gpa { get; set; }
    public string Rank { get; set; } = string.Empty; // Giỏi | Khá | Trung bình | Yếu
    public string? AiComment { get; set; }

    public SinhVien? SinhVien { get; set; }
}
