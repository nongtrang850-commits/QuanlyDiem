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
public class AuthController(AppDbContext db, JwtService jwt, AuditService audit) : ControllerBase
{
    // Chỉ Admin mới tạo được tài khoản qua endpoint này (frontend dùng TaiKhoanController)
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await db.NguoiDungs.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email đã tồn tại" });

        var user = new NguoiDung
        {
            Email = req.Email,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(req.MatKhau),
            Role = req.Role
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return Ok(new { message = "Đăng ký thành công" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await db.NguoiDungs
            .Include(u => u.SinhVien)
            .Include(u => u.GiangVien)
            .FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.MatKhau, user.MatKhau))
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

        int? profileId = user.Role switch
        {
            "SinhVien"  => user.SinhVien?.Id,
            "GiangVien" => user.GiangVien?.Id,
            _           => null
        };

        var token = jwt.GenerateToken(user);
        await audit.GhiAsync(user.Email, user.Role, "Đăng nhập", "Đăng nhập thành công");
        return Ok(new AuthResponse(token, user.Role, user.Id, profileId));
    }

    [HttpPut("doi-mat-khau")]
    [Authorize]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.NguoiDungs.FindAsync(userId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.MatKhauCu, user.MatKhau))
            return BadRequest(new { message = "Mật khẩu cũ không đúng" });

        if (req.MatKhauMoi.Length < 6)
            return BadRequest(new { message = "Mật khẩu mới tối thiểu 6 ký tự" });

        user.MatKhau = BCrypt.Net.BCrypt.HashPassword(req.MatKhauMoi);
        await db.SaveChangesAsync();
        return Ok(new { message = "Đổi mật khẩu thành công" });
    }
}
