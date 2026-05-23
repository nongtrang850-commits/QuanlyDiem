namespace QuanlyDiemAPI.DTOs;

public record LopHocRequest(
    string ClassName,
    string? Department,
    string? Room,
    int? AdvisorId);

public record LopHocResponse(
    int Id,
    string ClassName,
    string? Department,
    string? Room,
    int SinhVienCount,
    int? AdvisorId,
    string? AdvisorName);
