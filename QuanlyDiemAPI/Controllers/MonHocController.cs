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
[Authorize(Roles = "Admin,GiangVien")]
public class MonHocController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int? hocKy)
    {
        var query = db.MonHocs.Include(m => m.GiangVien).AsQueryable();
        if (hocKy.HasValue) query = query.Where(m => m.HocKy == hocKy.Value);
        var list = await query
            .OrderBy(m => m.HocKy).ThenBy(m => m.SubjectName)
            .ToListAsync();
        return Ok(list.Select(m => new MonHocResponse(
            m.Id, m.SubjectName, m.Credits, m.Department, m.HocKy,
            m.GiangVienId, m.GiangVien?.FullName)));
    }

    [HttpPost]
    public async Task<IActionResult> Create(MonHocRequest req)
    {
        var exists = await db.MonHocs.AnyAsync(m =>
            m.SubjectName.ToLower() == req.SubjectName.ToLower());
        if (exists) return Conflict(new { message = "Môn học này đã tồn tại" });

        var m = new MonHoc
        {
            SubjectName = req.SubjectName,
            Credits     = req.Credits,
            Department  = req.Department,
            HocKy       = req.HocKy,
            GiangVienId = req.GiangVienId
        };
        db.MonHocs.Add(m);
        await db.SaveChangesAsync();
        return Ok(new { message = "Thêm môn học thành công", id = m.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MonHocRequest req)
    {
        var m = await db.MonHocs.FindAsync(id);
        if (m is null) return NotFound();

        var exists = await db.MonHocs.AnyAsync(x =>
            x.SubjectName.ToLower() == req.SubjectName.ToLower() && x.Id != id);
        if (exists) return Conflict(new { message = "Môn học này đã tồn tại" });

        m.SubjectName = req.SubjectName;
        m.Credits     = req.Credits;
        m.Department  = req.Department;
        m.HocKy       = req.HocKy;
        m.GiangVienId = req.GiangVienId;
        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await db.MonHocs.FindAsync(id);
        if (m is null) return NotFound();
        db.MonHocs.Remove(m);
        await db.SaveChangesAsync();
        return Ok(new { message = "Xóa thành công" });
    }

    // Giảng viên xem danh sách môn được phân công cho mình
    [HttpGet("cua-toi")]
    [Authorize(Roles = "GiangVien")]
    public async Task<IActionResult> GetCuaToi()
    {
        var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
        if (gv is null) return Ok(Array.Empty<MonHocResponse>());

        var list = await db.MonHocs
            .Include(m => m.GiangVien)
            .Where(m => m.GiangVienId == gv.Id)
            .OrderBy(m => m.HocKy).ThenBy(m => m.SubjectName)
            .ToListAsync();

        return Ok(list.Select(m => new MonHocResponse(
            m.Id, m.SubjectName, m.Credits, m.Department, m.HocKy,
            m.GiangVienId, m.GiangVien?.FullName)));
    }

    // Danh sách sinh viên đã được duyệt đăng ký môn học này (kèm điểm nếu có)
    [HttpGet("{id:int}/sinh-vien")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> GetSinhVienCuaMon(int id)
    {
        var mon = await db.MonHocs.FindAsync(id);
        if (mon is null) return NotFound();

        if (User.IsInRole("GiangVien"))
        {
            var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
            if (gv is null || mon.GiangVienId != gv.Id)
                return Forbid();
        }

        var enrollments = await db.DangKyMonHocs
            .Include(dk => dk.SinhVien).ThenInclude(sv => sv!.LopHoc)
            .Where(dk => dk.MonHocId == id && dk.TrangThai == "DaDuyet")
            .ToListAsync();

        var diemMap = await db.Diems
            .Where(d => d.MonHocId == id)
            .ToDictionaryAsync(d => d.SinhVienId);

        var result = enrollments
            .OrderBy(dk => dk.SinhVien!.LopHoc?.ClassName ?? dk.SinhVien.Class ?? "")
            .ThenBy(dk => dk.SinhVien!.FullName)
            .Select(dk =>
            {
                diemMap.TryGetValue(dk.SinhVienId, out var d);
                return new
                {
                    diemId     = d?.Id,
                    sinhVienId = dk.SinhVienId,
                    tenSV      = dk.SinhVien!.FullName,
                    lop        = dk.SinhVien.LopHoc?.ClassName ?? dk.SinhVien.Class ?? "—",
                    cc         = d?.CC,
                    kT1        = d?.KT1,
                    kT2        = d?.KT2,
                    kT3        = d?.KT3,
                    exam       = d?.Exam,
                    diemTongKet = d is not null ? GpaService.TinhDiemTongKet(d) : null
                };
            });

        return Ok(result);
    }
}
