using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.DTOs;
using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(AppDbContext db) : ControllerBase
{
    private string UserEmail => User.FindFirstValue(ClaimTypes.Email)!;
    private string UserRole  => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        if (UserRole == "SinhVien")
        {
            var sv = await db.SinhViens.Include(x => x.LopHoc).FirstOrDefaultAsync(s => s.Email == UserEmail);
            if (sv is null) return NotFound(new { message = "Chưa có hồ sơ" });
            return Ok(new SinhVienResponse(sv.Id, sv.FullName, sv.Dob?.ToString(),
                sv.Gender, sv.Phone, sv.Email, sv.Class, sv.Department, sv.Course,
                sv.LopHocId, sv.LopHoc?.ClassName));
        }
        if (UserRole == "GiangVien")
        {
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.Email == UserEmail);
            if (gv is null) return NotFound(new { message = "Chưa có hồ sơ" });
            return Ok(new GiangVienResponse(gv.Id, gv.FullName, gv.Dob?.ToString(),
                gv.Gender, gv.Phone, gv.Email, gv.Department));
        }
        return Ok(new { email = UserEmail, role = UserRole });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] System.Text.Json.JsonElement body)
    {
        if (UserRole == "SinhVien")
        {
            var sv = await db.SinhViens.FirstOrDefaultAsync(s => s.Email == UserEmail);
            if (sv is null) return NotFound();
            if (body.TryGetProperty("fullName", out var fn))  sv.FullName   = fn.GetString()!;
            if (body.TryGetProperty("dob",      out var dob)) sv.Dob        = string.IsNullOrEmpty(dob.GetString()) ? null : DateOnly.Parse(dob.GetString()!);
            if (body.TryGetProperty("gender",   out var g))   sv.Gender     = g.GetString();
            if (body.TryGetProperty("phone",    out var ph))  sv.Phone      = ph.GetString();
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ thành công" });
        }
        if (UserRole == "GiangVien")
        {
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.Email == UserEmail);
            if (gv is null) return NotFound();
            if (body.TryGetProperty("fullName", out var fn))  gv.FullName   = fn.GetString()!;
            if (body.TryGetProperty("dob",      out var dob)) gv.Dob        = string.IsNullOrEmpty(dob.GetString()) ? null : DateOnly.Parse(dob.GetString()!);
            if (body.TryGetProperty("gender",   out var g))   gv.Gender     = g.GetString();
            if (body.TryGetProperty("phone",    out var ph))  gv.Phone      = ph.GetString();
            if (body.TryGetProperty("department", out var d)) gv.Department = d.GetString();
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ thành công" });
        }
        return BadRequest(new { message = "Không hỗ trợ cập nhật cho vai trò này" });
    }
}
