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
public class GiangVienController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await db.GiangViens.Select(g => new GiangVienResponse(
            g.Id, g.FullName, g.Dob.ToString(), g.Gender, g.Phone, g.Email, g.Department
        )).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var g = await db.GiangViens.FindAsync(id);
        if (g is null) return NotFound();
        return Ok(new GiangVienResponse(g.Id, g.FullName, g.Dob.ToString(), g.Gender, g.Phone, g.Email, g.Department));
    }

    [HttpPost]
    public async Task<IActionResult> Create(GiangVienRequest req)
    {
        var g = new GiangVien
        {
            FullName = req.FullName,
            Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null,
            Gender = req.Gender,
            Phone = req.Phone,
            Email = req.Email,
            Department = req.Department
        };
        db.GiangViens.Add(g);
        await db.SaveChangesAsync();
        return Ok(new { message = "Thêm giảng viên thành công", id = g.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, GiangVienRequest req)
    {
        var g = await db.GiangViens.FindAsync(id);
        if (g is null) return NotFound();

        g.FullName = req.FullName;
        g.Dob = req.Dob is not null ? DateOnly.Parse(req.Dob) : null;
        g.Gender = req.Gender;
        g.Phone = req.Phone;
        g.Email = req.Email;
        g.Department = req.Department;

        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await db.GiangViens.FindAsync(id);
        if (g is null) return NotFound();
        db.GiangViens.Remove(g);
        await db.SaveChangesAsync();
        return Ok(new { message = "Xóa thành công" });
    }
}
