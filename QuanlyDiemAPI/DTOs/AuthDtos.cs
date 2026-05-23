namespace QuanlyDiemAPI.DTOs;

public record LoginRequest(string Email, string MatKhau);
public record RegisterRequest(string Email, string MatKhau, string Role = "SinhVien");
public record AuthResponse(string Token, string Role, int UserId, int? ProfileId);
public record DoiMatKhauRequest(string MatKhauCu, string MatKhauMoi);
