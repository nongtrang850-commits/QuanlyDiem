namespace QuanlyDiemAPI.DTOs;

public record MonHocRequest(string SubjectName, int Credits, string? Department, int HocKy = 1, int? GiangVienId = null);
public record MonHocResponse(int Id, string SubjectName, int Credits, string? Department, int HocKy, int? GiangVienId, string? GiangVienName);
