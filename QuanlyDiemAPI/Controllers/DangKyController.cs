using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.DTOs;
using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/dangky")]
[Authorize]
public class DangKyController(AppDbContext db) : ControllerBase
{
    // Sinh viên đăng ký môn học
    [HttpPost]
    [Authorize(Roles = "SinhVien")]
    public async Task<IActionResult> DangKy(DangKyRequest req)
    {
        var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sv = await db.SinhViens.FirstOrDefaultAsync(s => s.NguoiDungId == nguoiDungId);
        if (sv is null) return BadRequest(new { message = "Tài khoản chưa liên kết với hồ sơ sinh viên" });

        var mon = await db.MonHocs.FindAsync(req.MonHocId);
        if (mon is null) return NotFound(new { message = "Không tìm thấy môn học" });

        if (await db.DangKyMonHocs.AnyAsync(dk => dk.SinhVienId == sv.Id && dk.MonHocId == req.MonHocId))
            return BadRequest(new { message = "Bạn đã đăng ký môn học này rồi" });

        var dk = new DangKyMonHoc { SinhVienId = sv.Id, MonHocId = req.MonHocId };
        db.DangKyMonHocs.Add(dk);
        await db.SaveChangesAsync();
        return Ok(new { message = "Đăng ký thành công, vui lòng chờ Admin duyệt", id = dk.Id });
    }

    // Sinh viên xem danh sách đăng ký của mình
    [HttpGet("cua-toi")]
    [Authorize(Roles = "SinhVien")]
    public async Task<IActionResult> GetCuaToi()
    {
        var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sv = await db.SinhViens.FirstOrDefaultAsync(s => s.NguoiDungId == nguoiDungId);
        if (sv is null) return Ok(Array.Empty<DangKyResponse>());

        var list = await db.DangKyMonHocs
            .Include(dk => dk.MonHoc)
            .Where(dk => dk.SinhVienId == sv.Id)
            .OrderByDescending(dk => dk.NgayDangKy)
            .ToListAsync();

        return Ok(list.Select(dk => new DangKyResponse(
            dk.Id, dk.SinhVienId, string.Empty,
            dk.MonHocId, dk.MonHoc!.SubjectName, dk.MonHoc.Credits, dk.MonHoc.HocKy,
            dk.TrangThai, dk.NgayDangKy)));
    }

    // Admin xem tất cả đăng ký (có thể lọc theo trangThai)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] string? trangThai)
    {
        var query = db.DangKyMonHocs
            .Include(dk => dk.SinhVien)
            .Include(dk => dk.MonHoc)
            .AsQueryable();

        if (!string.IsNullOrEmpty(trangThai))
            query = query.Where(dk => dk.TrangThai == trangThai);

        var list = await query.OrderByDescending(dk => dk.NgayDangKy).ToListAsync();
        return Ok(list.Select(dk => new DangKyResponse(
            dk.Id, dk.SinhVienId, dk.SinhVien!.FullName,
            dk.MonHocId, dk.MonHoc!.SubjectName, dk.MonHoc.Credits, dk.MonHoc.HocKy,
            dk.TrangThai, dk.NgayDangKy)));
    }

    // Admin duyệt → tự động tạo bản ghi điểm trống
    [HttpPut("{id}/duyet")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Duyet(int id)
    {
        var dk = await db.DangKyMonHocs.FindAsync(id);
        if (dk is null) return NotFound();
        if (dk.TrangThai != "ChoDuyet")
            return BadRequest(new { message = "Đăng ký này đã được xử lý rồi" });

        dk.TrangThai = "DaDuyet";

        // Tự động tạo bản ghi điểm trống nếu chưa có
        if (!await db.Diems.AnyAsync(d => d.SinhVienId == dk.SinhVienId && d.MonHocId == dk.MonHocId))
            db.Diems.Add(new Diem { SinhVienId = dk.SinhVienId, MonHocId = dk.MonHocId });

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã duyệt đăng ký môn học" });
    }

    // Admin từ chối
    [HttpPut("{id}/tu-choi")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TuChoi(int id)
    {
        var dk = await db.DangKyMonHocs.FindAsync(id);
        if (dk is null) return NotFound();
        if (dk.TrangThai != "ChoDuyet")
            return BadRequest(new { message = "Đăng ký này đã được xử lý rồi" });

        dk.TrangThai = "TuChoi";
        await db.SaveChangesAsync();
        return Ok(new { message = "Đã từ chối đăng ký" });
    }

    // Sinh viên hủy đăng ký (chỉ được hủy khi còn ChoDuyet)
    [HttpDelete("{id}")]
    [Authorize(Roles = "SinhVien")]
    public async Task<IActionResult> HuyDangKy(int id)
    {
        var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sv = await db.SinhViens.FirstOrDefaultAsync(s => s.NguoiDungId == nguoiDungId);

        var dk = await db.DangKyMonHocs.FindAsync(id);
        if (dk is null || dk.SinhVienId != sv?.Id) return NotFound();
        if (dk.TrangThai != "ChoDuyet")
            return BadRequest(new { message = "Không thể hủy đăng ký đã được duyệt hoặc từ chối" });

        db.DangKyMonHocs.Remove(dk);
        await db.SaveChangesAsync();
        return Ok(new { message = "Đã hủy đăng ký" });
    }
}
