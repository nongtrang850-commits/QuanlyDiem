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
public class LopHocController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var list = await db.LopHocs
            .Include(l => l.Advisor)
            .Include(l => l.SinhViens)
            .ToListAsync();

        return Ok(list.Select(l => new LopHocResponse(
            l.Id, l.ClassName, l.Department, l.Room,
            l.SinhViens.Count,
            l.AdvisorId, l.Advisor?.FullName)));
    }

    [HttpPost]
    public async Task<IActionResult> Create(LopHocRequest req)
    {
        var exists = await db.LopHocs.AnyAsync(l =>
            l.ClassName.ToLower() == req.ClassName.ToLower());
        if (exists) return Conflict(new { message = "Lớp học này đã tồn tại" });

        var l = new LopHoc
        {
            ClassName  = req.ClassName,
            Department = req.Department,
            Room       = req.Room,
            AdvisorId  = req.AdvisorId
        };
        db.LopHocs.Add(l);
        await db.SaveChangesAsync();
        return Ok(new { message = "Thêm lớp học thành công", id = l.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, LopHocRequest req)
    {
        var l = await db.LopHocs.FindAsync(id);
        if (l is null) return NotFound();

        var exists = await db.LopHocs.AnyAsync(x =>
            x.ClassName.ToLower() == req.ClassName.ToLower() && x.Id != id);
        if (exists) return Conflict(new { message = "Lớp học này đã tồn tại" });

        l.ClassName  = req.ClassName;
        l.Department = req.Department;
        l.Room       = req.Room;
        l.AdvisorId  = req.AdvisorId;
        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var l = await db.LopHocs.FindAsync(id);
        if (l is null) return NotFound();
        db.LopHocs.Remove(l);
        await db.SaveChangesAsync();
        return Ok(new { message = "Xóa thành công" });
    }
}
