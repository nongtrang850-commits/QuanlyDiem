namespace QuanlyDiemAPI.DTOs;

public record SinhVienRequest(
    string FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string Email,
    string? Class,
    string? Department,
    string? Course,
    int? LopHocId = null);

public record SinhVienResponse(
    int Id,
    string FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string Email,
    string? Class,
    string? Department,
    string? Course,
    int? LopHocId,
    string? LopHocName);
