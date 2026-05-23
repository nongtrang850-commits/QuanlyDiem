using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.DTOs;
using QuanlyDiemAPI.Models;
using QuanlyDiemAPI.Services;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiemController(AppDbContext db, AiService ai, AuditService audit) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> GetAll([FromQuery] int? sinhVienId, [FromQuery] int? monHocId)
    {
        var activeEmails = db.NguoiDungs.Where(u => u.Role == "SinhVien").Select(u => u.Email);
        var query = db.Diems
            .Include(d => d.SinhVien).Include(d => d.MonHoc)
            .Where(d => activeEmails.Contains(d.SinhVien!.Email))
            .AsQueryable();

        if (User.IsInRole("GiangVien"))
        {
            var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
            if (gv is not null)
                query = query.Where(d => d.MonHoc!.GiangVienId == gv.Id);
        }

        if (sinhVienId.HasValue) query = query.Where(d => d.SinhVienId == sinhVienId);
        if (monHocId.HasValue) query = query.Where(d => d.MonHocId == monHocId);

        var list = await query.Select(d => new DiemResponse(
            d.Id, d.SinhVienId, d.SinhVien!.FullName,
            d.MonHocId, d.MonHoc!.SubjectName, d.MonHoc.Credits, d.MonHoc.HocKy,
            d.CC, d.KT1, d.KT2, d.KT3, d.Exam,
            GpaService.TinhDiemTongKet(d)
        )).ToListAsync();
        return Ok(list);
    }

    [HttpGet("cua-toi/{sinhVienId}")]
    public async Task<IActionResult> GetCuaToi(int sinhVienId)
    {
        var diems = await db.Diems
            .Include(d => d.MonHoc)
            .Where(d => d.SinhVienId == sinhVienId)
            .ToListAsync();

        var result = diems.Select(d => new DiemResponse(
            d.Id, d.SinhVienId, string.Empty,
            d.MonHocId, d.MonHoc!.SubjectName, d.MonHoc.Credits, d.MonHoc.HocKy,
            d.CC, d.KT1, d.KT2, d.KT3, d.Exam,
            GpaService.TinhDiemTongKet(d)
        )).ToList();
        return Ok(result);
    }

    [HttpGet("bang-diem-tich-luy/{sinhVienId}")]
    public async Task<IActionResult> GetBangDiemTichLuy(int sinhVienId)
    {
        var diems = await db.Diems
            .Include(d => d.MonHoc)
            .Where(d => d.SinhVienId == sinhVienId)
            .ToListAsync();

        if (!diems.Any())
            return Ok(new BangDiemTichLuyResponse(0, "Yếu", 0, []));

        var grouped = diems
            .GroupBy(d => d.MonHoc!.HocKy)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var gpaHK = GpaService.TinhGpa(g);
                var diemsHK = g.Select(d => new DiemResponse(
                    d.Id, d.SinhVienId, string.Empty,
                    d.MonHocId, d.MonHoc!.SubjectName, d.MonHoc.Credits, d.MonHoc.HocKy,
                    d.CC, d.KT1, d.KT2, d.KT3, d.Exam,
                    GpaService.TinhDiemTongKet(d)
                )).ToList();
                return new DiemHocKyResponse(
                    g.Key, gpaHK, GpaService.XepLoai(gpaHK),
                    g.Sum(d => d.MonHoc!.Credits), diemsHK);
            }).ToList();

        var gpaTichLuy = GpaService.TinhGpa(diems);
        var tongTC = diems.Where(d => GpaService.TinhDiemTongKet(d) is not null).Sum(d => d.MonHoc!.Credits);
        return Ok(new BangDiemTichLuyResponse(gpaTichLuy, GpaService.XepLoai(gpaTichLuy), tongTC, grouped));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> Create(DiemRequest req)
    {
        if (!ValidateScores(req, out var err)) return BadRequest(new { message = err });

        if (User.IsInRole("GiangVien"))
        {
            var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
            var mon = await db.MonHocs.FindAsync(req.MonHocId);
            if (gv is null || mon?.GiangVienId != gv.Id)
                return BadRequest(new { message = "Bạn không được phân công dạy môn học này" });

            // Giảng viên chỉ được nhập điểm cho sinh viên đã được duyệt đăng ký
            var enrolled = await db.DangKyMonHocs.AnyAsync(dk =>
                dk.SinhVienId == req.SinhVienId && dk.MonHocId == req.MonHocId && dk.TrangThai == "DaDuyet");
            if (!enrolled)
                return BadRequest(new { message = "Sinh viên chưa đăng ký hoặc chưa được Admin duyệt môn học này" });
        }

        if (await db.Diems.AnyAsync(d => d.SinhVienId == req.SinhVienId && d.MonHocId == req.MonHocId))
            return BadRequest(new { message = "Sinh viên này đã có điểm môn học này rồi" });

        var d = new Diem
        {
            SinhVienId = req.SinhVienId,
            MonHocId   = req.MonHocId,
            CC = req.CC, KT1 = req.KT1, KT2 = req.KT2, KT3 = req.KT3, Exam = req.Exam,
            GiangVienId = await GetGiangVienId()
        };
        db.Diems.Add(d);
        await db.SaveChangesAsync();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await audit.GhiAsync(email, User.IsInRole("Admin") ? "Admin" : "GiangVien",
            "Nhập điểm", $"SinhVienId={req.SinhVienId}, MonHocId={req.MonHocId}");
        return Ok(new { message = "Nhập điểm thành công", id = d.Id });
    }

    // PUT: cập nhật điểm — null = không thay đổi giá trị cũ
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> Update(int id, DiemRequest req)
    {
        var d = await db.Diems.FindAsync(id);
        if (d is null) return NotFound();

        if (!ValidateScores(req, out var err)) return BadRequest(new { message = err });

        if (User.IsInRole("GiangVien"))
        {
            var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
            var mon = await db.MonHocs.FindAsync(d.MonHocId);
            if (gv is null || mon?.GiangVienId != gv.Id)
                return BadRequest(new { message = "Bạn không được phân công dạy môn học này" });
        }

        // Chỉ cập nhật những field được gửi (không null)
        if (req.CC   is not null) d.CC   = req.CC;
        if (req.KT1  is not null) d.KT1  = req.KT1;
        if (req.KT2  is not null) d.KT2  = req.KT2;
        if (req.KT3  is not null) d.KT3  = req.KT3;
        if (req.Exam is not null) d.Exam = req.Exam;
        d.GiangVienId = await GetGiangVienId() ?? d.GiangVienId;

        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật điểm thành công" });
    }

    private static bool ValidateScores(DiemRequest req, out string error)
    {
        error = string.Empty;
        if (req.CC   is not null && (req.CC   < 0 || req.CC   > 10)) { error = "Điểm CC phải trong khoảng 0-10";   return false; }
        if (req.KT1  is not null && (req.KT1  < 0 || req.KT1  > 10)) { error = "Điểm KT1 phải trong khoảng 0-10";  return false; }
        if (req.KT2  is not null && (req.KT2  < 0 || req.KT2  > 10)) { error = "Điểm KT2 phải trong khoảng 0-10";  return false; }
        if (req.KT3  is not null && (req.KT3  < 0 || req.KT3  > 10)) { error = "Điểm KT3 phải trong khoảng 0-10";  return false; }
        if (req.Exam is not null && (req.Exam < 0 || req.Exam > 10)) { error = "Điểm thi phải trong khoảng 0-10"; return false; }
        return true;
    }

    private async Task<int?> GetGiangVienId()
    {
        if (!User.IsInRole("GiangVien")) return null;
        var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
        return gv?.Id;
    }

    [HttpPost("danh-gia-ai/{sinhVienId}")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> DanhGiaAI(int sinhVienId)
    {
        var sv = await db.SinhViens.FindAsync(sinhVienId);
        if (sv is null) return NotFound(new { message = "Không tìm thấy sinh viên" });

        var diems = await db.Diems
            .Include(d => d.MonHoc)
            .Where(d => d.SinhVienId == sinhVienId)
            .ToListAsync();

        var diemsDayDu = diems.Where(d => GpaService.TinhDiemTongKet(d) is not null).ToList();
        if (!diemsDayDu.Any())
            return BadRequest(new { message = "Sinh viên chưa có môn học nào đủ điểm để đánh giá" });

        var gpa     = GpaService.TinhGpa(diemsDayDu);
        var rank    = GpaService.XepLoai(gpa);
        var monHocs = diemsDayDu.Select(d => d.MonHoc!.SubjectName);
        var monYeu  = diemsDayDu
            .Where(d => GpaService.TinhDiemTongKet(d) < 5.0f)
            .Select(d => d.MonHoc!.SubjectName);
        var gpaTheoHocKy = diemsDayDu
            .GroupBy(d => d.MonHoc!.HocKy)
            .Select(g => (HocKy: g.Key, Gpa: GpaService.TinhGpa(g)));

        var comment = await ai.SinhNhanXetAsync(sv.FullName, gpa, rank, monHocs, monYeu, gpaTheoHocKy);

        var existing = await db.DanhgiaAIs.FirstOrDefaultAsync(x => x.StudentId == sinhVienId);
        if (existing is null)
            db.DanhgiaAIs.Add(new DanhgiaAI { StudentId = sinhVienId, Gpa = gpa, Rank = rank, AiComment = comment });
        else
        {
            existing.Gpa = gpa; existing.Rank = rank; existing.AiComment = comment;
        }
        await db.SaveChangesAsync();

        var actorEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await audit.GhiAsync(actorEmail, User.IsInRole("Admin") ? "Admin" : "GiangVien",
            "Đánh giá AI", $"SinhVienId={sinhVienId} | GPA={gpa:F2} | Xếp loại={rank}");
        return Ok(new DanhgiaAIResponse(sinhVienId, sv.FullName, gpa, rank, comment));
    }

    [HttpPost("danh-gia-ai/tat-ca")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DanhGiaAITatCa()
    {
        var activeEmails = db.NguoiDungs.Where(u => u.Role == "SinhVien").Select(u => u.Email);
        var svs = await db.SinhViens
            .Where(s => activeEmails.Contains(s.Email))
            .ToListAsync();

        int success = 0, skip = 0;
        var results = new List<DanhgiaAIResponse>();

        foreach (var sv in svs)
        {
            var diems = await db.Diems
                .Include(d => d.MonHoc)
                .Where(d => d.SinhVienId == sv.Id)
                .ToListAsync();

            var diemsDayDu = diems.Where(d => GpaService.TinhDiemTongKet(d) is not null).ToList();
            if (!diemsDayDu.Any()) { skip++; continue; }

            var gpa     = GpaService.TinhGpa(diemsDayDu);
            var rank    = GpaService.XepLoai(gpa);
            var monHocs = diemsDayDu.Select(d => d.MonHoc!.SubjectName);
            var monYeu  = diemsDayDu
                .Where(d => GpaService.TinhDiemTongKet(d) < 5.0f)
                .Select(d => d.MonHoc!.SubjectName);
            var gpaTheoHocKy = diemsDayDu
                .GroupBy(d => d.MonHoc!.HocKy)
                .Select(g => (HocKy: g.Key, Gpa: GpaService.TinhGpa(g)));

            var comment = await ai.SinhNhanXetAsync(sv.FullName, gpa, rank, monHocs, monYeu, gpaTheoHocKy);

            var existing = await db.DanhgiaAIs.FirstOrDefaultAsync(x => x.StudentId == sv.Id);
            if (existing is null)
                db.DanhgiaAIs.Add(new DanhgiaAI { StudentId = sv.Id, Gpa = gpa, Rank = rank, AiComment = comment });
            else
            {
                existing.Gpa = gpa; existing.Rank = rank; existing.AiComment = comment;
            }
            success++;
            results.Add(new DanhgiaAIResponse(sv.Id, sv.FullName, gpa, rank, comment));
        }
        await db.SaveChangesAsync();

        var actorEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await audit.GhiAsync(actorEmail, "Admin", "Đánh giá AI hàng loạt",
            $"Đã đánh giá {success} sinh viên, bỏ qua {skip} sinh viên chưa có đủ điểm");

        return Ok(new { success, skip, results });
    }

    [HttpGet("danh-gia-ai")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> GetAllDanhGiaAI()
    {
        var list = await db.DanhgiaAIs
            .Include(x => x.SinhVien)
            .OrderByDescending(x => x.Gpa)
            .Select(x => new DanhgiaAIResponse(x.StudentId, x.SinhVien!.FullName, x.Gpa, x.Rank, x.AiComment))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("danh-gia-ai/{sinhVienId}")]
    public async Task<IActionResult> GetDanhGiaAI(int sinhVienId)
    {
        var dg = await db.DanhgiaAIs
            .Include(x => x.SinhVien)
            .FirstOrDefaultAsync(x => x.StudentId == sinhVienId);
        if (dg is null) return NotFound(new { message = "Chưa có đánh giá" });
        return Ok(new DanhgiaAIResponse(sinhVienId, dg.SinhVien!.FullName, dg.Gpa, dg.Rank, dg.AiComment));
    }
}
