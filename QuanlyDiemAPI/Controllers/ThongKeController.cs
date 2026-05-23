using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.Services;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ThongKeController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetThongKe()
    {
        var totalSV = await db.SinhViens.CountAsync();
        var totalGV = await db.GiangViens.CountAsync();
        var totalMH = await db.MonHocs.CountAsync();
        var totalLH = await db.LopHocs.CountAsync();

        // Lấy tất cả đánh giá AI đã có
        var danhGias = await db.DanhgiaAIs.Include(x => x.SinhVien).ToListAsync();

        // Phân bố học lực
        var rankDist = new Dictionary<string, int>
        {
            ["Giỏi"] = danhGias.Count(d => d.Rank == "Giỏi"),
            ["Khá"] = danhGias.Count(d => d.Rank == "Khá"),
            ["Trung bình"] = danhGias.Count(d => d.Rank == "Trung bình"),
            ["Yếu"] = danhGias.Count(d => d.Rank == "Yếu"),
        };

        var avgGpa = danhGias.Any() ? MathF.Round(danhGias.Average(d => d.Gpa), 2) : 0f;

        // Sinh viên cảnh báo (GPA < 5)
        var canhBao = danhGias
            .Where(d => d.Gpa < 5f)
            .OrderBy(d => d.Gpa)
            .Select(d => new
            {
                id = d.StudentId,
                fullName = d.SinhVien?.FullName ?? "—",
                cls = d.SinhVien?.Class ?? "—",
                gpa = d.Gpa,
                rank = d.Rank
            })
            .ToList();

        // Sinh viên chưa có đánh giá
        var evaluatedIds = danhGias.Select(d => d.StudentId).ToHashSet();
        var chuaDanhGia = await db.SinhViens
            .Where(s => !evaluatedIds.Contains(s.Id))
            .CountAsync();

        return Ok(new
        {
            totalSV,
            totalGV,
            totalMH,
            totalLH,
            avgGpa,
            rankDistribution = rankDist,
            sinhVienCanhBao = canhBao,
            chuaDanhGia
        });
    }
}
