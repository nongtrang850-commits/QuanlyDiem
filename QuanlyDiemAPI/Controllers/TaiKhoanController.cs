using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.DTOs;
using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TaiKhoanController(AppDbContext db) : ControllerBase
{
    static TaiKhoanResponse ToResponse(NguoiDung acc)
    {
        string? fullName = null, dob = null, gender = null, phone = null,
                dept = null, cls = null, course = null;
        int? lopHocId = null;

        if (acc.Role == "SinhVien" && acc.SinhVien is { } sv)
        {
            fullName = sv.FullName; dob = sv.Dob?.ToString();
            gender = sv.Gender; phone = sv.Phone;
            dept = sv.Department; cls = sv.Class; course = sv.Course;
            lopHocId = sv.LopHocId;
        }
        else if (acc.Role == "GiangVien" && acc.GiangVien is { } gv)
        {
            fullName = gv.FullName; dob = gv.Dob?.ToString();
            gender = gv.Gender; phone = gv.Phone; dept = gv.Department;
        }
        else if (acc.Role == "Admin")
        {
            fullName = "Quản trị viên";
        }

        return new TaiKhoanResponse(acc.Id, acc.Email, acc.Role,
            fullName, dob, gender, phone, dept, cls, course, lopHocId, acc.MatKhauGoc);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await db.NguoiDungs
            .Include(u => u.SinhVien)
            .Include(u => u.GiangVien)
            .ToListAsync();
        return Ok(accounts.Select(ToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var acc = await db.NguoiDungs
            .Include(u => u.SinhVien)
            .Include(u => u.GiangVien)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (acc is null) return NotFound();
        return Ok(ToResponse(acc));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaiKhoanCreateRequest req)
    {
        if (await db.NguoiDungs.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email đã tồn tại" });

        var acc = new NguoiDung
        {
            Email    = req.Email,
            MatKhau  = BCrypt.Net.BCrypt.HashPassword(req.MatKhau),
            MatKhauGoc = req.MatKhau,
            Role     = req.Role
        };

        if (req.Role == "SinhVien")
        {
            string? className = req.Class;
            if (req.LopHocId.HasValue)
            {
                var lh = await db.LopHocs.FindAsync(req.LopHocId.Value);
                className = lh?.ClassName ?? req.Class;
            }
            acc.SinhVien = new SinhVien
            {
                FullName   = req.FullName,
                Email      = req.Email,
                Dob        = req.Dob is not null ? DateOnly.Parse(req.Dob) : null,
                Gender     = req.Gender,
                Phone      = req.Phone,
                Department = req.Department,
                Class      = className,
                Course     = req.Course,
                LopHocId   = req.LopHocId
            };
        }
        else if (req.Role == "GiangVien")
        {
            acc.GiangVien = new GiangVien
            {
                FullName = req.FullName,
                Email = req.Email,
                Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null,
                Gender = req.Gender,
                Phone = req.Phone,
                Department = req.Department
            };
        }

        db.NguoiDungs.Add(acc);
        await db.SaveChangesAsync();
        return Ok(new { message = "Tạo tài khoản thành công", id = acc.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaiKhoanUpdateRequest req)
    {
        var acc = await db.NguoiDungs
            .Include(u => u.SinhVien)
            .Include(u => u.GiangVien)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (acc is null) return NotFound();

        acc.Email = req.Email;
        acc.Role = req.Role;
        if (!string.IsNullOrEmpty(req.MatKhau))
        {
            acc.MatKhau    = BCrypt.Net.BCrypt.HashPassword(req.MatKhau);
            acc.MatKhauGoc = req.MatKhau;
        }

        if (req.Role == "SinhVien")
        {
            string? className = req.Class;
            if (req.LopHocId.HasValue)
            {
                var lh = await db.LopHocs.FindAsync(req.LopHocId.Value);
                className = lh?.ClassName ?? req.Class;
            }
            if (acc.SinhVien is not null)
            {
                acc.SinhVien.Email = req.Email; acc.SinhVien.FullName = req.FullName;
                acc.SinhVien.Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null;
                acc.SinhVien.Gender = req.Gender; acc.SinhVien.Phone = req.Phone;
                acc.SinhVien.Department = req.Department;
                acc.SinhVien.Class = className; acc.SinhVien.Course = req.Course;
                acc.SinhVien.LopHocId = req.LopHocId;
            }
            else
            {
                acc.SinhVien = new SinhVien
                {
                    FullName = req.FullName, Email = req.Email,
                    Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null,
                    Gender = req.Gender, Phone = req.Phone,
                    Department = req.Department, Class = className,
                    Course = req.Course, LopHocId = req.LopHocId
                };
            }
        }
        else if (req.Role == "GiangVien")
        {
            if (acc.GiangVien is not null)
            {
                acc.GiangVien.Email = req.Email; acc.GiangVien.FullName = req.FullName;
                acc.GiangVien.Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null;
                acc.GiangVien.Gender = req.Gender; acc.GiangVien.Phone = req.Phone;
                acc.GiangVien.Department = req.Department;
            }
            else
            {
                acc.GiangVien = new GiangVien
                {
                    FullName = req.FullName, Email = req.Email,
                    Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null,
                    Gender = req.Gender, Phone = req.Phone, Department = req.Department
                };
            }
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var acc = await db.NguoiDungs
            .Include(u => u.SinhVien)
            .Include(u => u.GiangVien)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (acc is null) return NotFound();

        if (acc.SinhVien is { } sv)
        {
            db.Diems.RemoveRange(db.Diems.Where(d => d.SinhVienId == sv.Id));
            var dg = await db.DanhgiaAIs.FirstOrDefaultAsync(x => x.StudentId == sv.Id);
            if (dg != null) db.DanhgiaAIs.Remove(dg);
            db.SinhViens.Remove(sv);
        }
        else if (acc.GiangVien is { } gv)
        {
            db.GiangViens.Remove(gv);
        }

        db.NguoiDungs.Remove(acc);
        await db.SaveChangesAsync();
        return Ok(new { message = "Xóa tài khoản thành công" });
    }
}
