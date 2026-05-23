namespace QuanlyDiemAPI.DTOs;

public record DangKyRequest(int MonHocId);

public record DangKyResponse(
    int Id,
    int SinhVienId,
    string SinhVienName,
    int MonHocId,
    string MonHocName,
    int Credits,
    int HocKy,
    string TrangThai,
    DateTime NgayDangKy);
