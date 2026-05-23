using QuanlyDiemAPI.Models;

namespace QuanlyDiemAPI.Services;

public class GpaService
{
    // Trả về null nếu chưa đủ điểm để tính tổng kết
    public static float? TinhDiemTongKet(Diem d)
    {
        if (d.CC is null || d.KT1 is null || d.KT2 is null || d.KT3 is null || d.Exam is null)
            return null;
        float ktTb = (d.KT1.Value + d.KT2.Value + d.KT3.Value) / 3f;
        return MathF.Round(d.CC.Value * 0.1f + ktTb * 0.3f + d.Exam.Value * 0.6f, 2);
    }

    // GPA chỉ tính trên những môn đã có điểm tổng kết đầy đủ
    public static float TinhGpa(IEnumerable<Diem> diems)
    {
        float tongDiemTinChi = 0;
        int tongTinChi = 0;

        foreach (var d in diems)
        {
            if (d.MonHoc is null) continue;
            var dtk = TinhDiemTongKet(d);
            if (dtk is null) continue;
            tongDiemTinChi += dtk.Value * d.MonHoc.Credits;
            tongTinChi += d.MonHoc.Credits;
        }

        return tongTinChi == 0 ? 0 : MathF.Round(tongDiemTinChi / tongTinChi, 2);
    }

    public static string XepLoai(float gpa) => gpa switch
    {
        >= 8.5f => "Giỏi",
        >= 7.0f => "Khá",
        >= 5.0f => "Trung bình",
        _ => "Yếu"
    };
}
