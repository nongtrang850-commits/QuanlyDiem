using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyDiemAPI.Data;

namespace QuanlyDiemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var total = await db.AuditLogs.CountAsync();
        var logs = await db.AuditLogs
            .OrderByDescending(x => x.ThoiGian)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new {
                x.Id,
                thoiGian = x.ThoiGian.ToString("dd/MM/yyyy HH:mm:ss"),
                x.Email,
                x.VaiTro,
                x.HanhDong,
                x.ChiTiet
            })
            .ToListAsync();
        return Ok(new { total, page, pageSize, logs });
    }
}
