namespace QuanlyDiemAPI.DTOs;

public record DiemRequest(
    int SinhVienId,
    int MonHocId,
    float? CC,
    float? KT1,
    float? KT2,
    float? KT3,
    float? Exam);

public record DiemResponse(
    int Id,
    int SinhVienId,
    string SinhVienName,
    int MonHocId,
    string MonHocName,
    int Credits,
    int HocKy,
    float? CC,
    float? KT1,
    float? KT2,
    float? KT3,
    float? Exam,
    float? DiemTongKet);

public record DiemHocKyResponse(
    int HocKy,
    float GpaHocKy,
    string XepLoai,
    int TongTinChi,
    List<DiemResponse> Diems);

public record BangDiemTichLuyResponse(
    float GpaTichLuy,
    string XepLoaiTichLuy,
    int TongTinChiTichLuy,
    List<DiemHocKyResponse> HocKys);

public record DanhgiaAIResponse(
    int StudentId,
    string StudentName,
    float Gpa,
    string Rank,
    string? AiComment);
