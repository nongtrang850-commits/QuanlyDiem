using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.DTOs;
using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GiangVien")]
public class SinhVienController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? lopHocId)
    {
        var query = db.SinhViens
            .Include(s => s.LopHoc)
            .Where(s => s.NguoiDungId != null);
        if (lopHocId.HasValue) query = query.Where(s => s.LopHocId == lopHocId.Value);
        var raw = await query.ToListAsync();

        return Ok(raw.Select(s => new SinhVienResponse(
            s.Id, s.FullName, s.Dob?.ToString(), s.Gender, s.Phone, s.Email,
            s.Class, s.Department, s.Course,
            s.LopHocId, s.LopHoc?.ClassName)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await db.SinhViens.Include(x => x.LopHoc).FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound();
        return Ok(new SinhVienResponse(
            s.Id, s.FullName, s.Dob?.ToString(), s.Gender, s.Phone, s.Email,
            s.Class, s.Department, s.Course,
            s.LopHocId, s.LopHoc?.ClassName));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(SinhVienRequest req)
    {
        var s = new SinhVien
        {
            FullName   = req.FullName,
            Dob        = req.Dob is not null ? DateOnly.Parse(req.Dob) : null,
            Gender     = req.Gender,
            Phone      = req.Phone,
            Email      = req.Email,
            Department = req.Department,
            Course     = req.Course,
            LopHocId   = req.LopHocId
        };
        await SyncClass(s, req.LopHocId, req.Class);
        db.SinhViens.Add(s);
        await db.SaveChangesAsync();
        return Ok(new { message = "Thêm sinh viên thành công", id = s.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, SinhVienRequest req)
    {
        var s = await db.SinhViens.FindAsync(id);
        if (s is null) return NotFound();

        s.FullName   = req.FullName;
        s.Dob        = req.Dob is not null ? DateOnly.Parse(req.Dob) : null;
        s.Gender     = req.Gender;
        s.Phone      = req.Phone;
        s.Email      = req.Email;
        s.Department = req.Department;
        s.Course     = req.Course;
        s.LopHocId   = req.LopHocId;
        await SyncClass(s, req.LopHocId, req.Class);

        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await db.SinhViens.FindAsync(id);
        if (s is null) return NotFound();
        db.SinhViens.Remove(s);
        await db.SaveChangesAsync();
        return Ok(new { message = "Xóa thành công" });
    }

    // Khi có LopHocId thì đồng bộ Class string từ LopHoc.ClassName
    private async Task SyncClass(SinhVien s, int? lopHocId, string? fallback)
    {
        if (lopHocId.HasValue)
        {
            var lh = await db.LopHocs.FindAsync(lopHocId.Value);
            s.Class = lh?.ClassName ?? fallback;
        }
        else
        {
            s.Class = fallback;
        }
    }
}
