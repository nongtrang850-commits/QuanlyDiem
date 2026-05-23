namespace QuanlyDiemAPI.DTOs;

public record TaiKhoanCreateRequest(
    string Email,
    string MatKhau,
    string Role,
    string FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string? Department,
    string? Class,
    string? Course,
    int? LopHocId = null
);

public record TaiKhoanUpdateRequest(
    string Email,
    string? MatKhau,
    string Role,
    string FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string? Department,
    string? Class,
    string? Course,
    int? LopHocId = null
);

public record TaiKhoanResponse(
    int Id,
    string Email,
    string Role,
    string? FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string? Department,
    string? Class,
    string? Course,
    int? LopHocId = null,
    string? MatKhauGoc = null
);
