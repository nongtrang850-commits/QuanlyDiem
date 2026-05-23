namespace QuanlyDiemAPI.DTOs;

public record GiangVienRequest(
    string FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string Email,
    string? Department);

public record GiangVienResponse(
    int Id,
    string FullName,
    string? Dob,
    string? Gender,
    string? Phone,
    string Email,
    string? Department);
