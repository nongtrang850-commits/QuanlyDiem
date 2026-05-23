using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QuanlyDiemAPI.Services;

public class AiService(IConfiguration config, IHttpClientFactory httpFactory)
{
    public async Task<string> SinhNhanXetAsync(
        string tenSinhVien,
        float gpa,
        string xepLoai,
        IEnumerable<string> danhSachMon,
        IEnumerable<string>? monYeu = null,
        IEnumerable<(int HocKy, float Gpa)>? gpaTheoHocKy = null)
    {
        var apiKey = config["AI:GroqApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return SinhNhanXetCoBan(gpa, xepLoai);

        var monHocStr  = string.Join(", ", danhSachMon);
        var monYeuStr  = monYeu?.Any() == true
            ? $"\n- Môn học cần cải thiện (điểm dưới 5.0): {string.Join(", ", monYeu)}"
            : "";
        var trendStr   = "";
        if (gpaTheoHocKy?.Count() >= 2)
        {
            var hks = gpaTheoHocKy.OrderBy(x => x.HocKy).ToList();
            trendStr = "\n- GPA theo học kỳ: " + string.Join(" → ", hks.Select(h => $"HK{h.HocKy}: {h.Gpa:F2}"));
            var delta = hks.Last().Gpa - hks.First().Gpa;
            trendStr += delta > 0.3f ? " (xu hướng tăng)" : delta < -0.3f ? " (xu hướng giảm)" : " (ổn định)";
        }
        var canhBaoStr = xepLoai == "Yếu"
            ? "\nLưu ý: Sinh viên đang trong tình trạng học yếu, cần cảnh báo mạnh và đưa ra giải pháp cụ thể."
            : "";

        var prompt = $"""
            Bạn là trợ lý giáo dục AI của trường Đại học CNTT. Hãy viết nhận xét (3-4 câu) bằng tiếng Việt về tình trạng học tập:
            - Sinh viên: {tenSinhVien}
            - GPA tích lũy: {gpa:F2}/10
            - Xếp loại học lực: {xepLoai}
            - Các môn đã học: {monHocStr}{monYeuStr}{trendStr}{canhBaoStr}

            Yêu cầu nhận xét:
            1. Đánh giá tổng quan kết quả hiện tại (1 câu)
            2. Nhận xét điểm mạnh hoặc vấn đề cụ thể cần chú ý (1 câu)
            3. Lời khuyên thiết thực và động viên (1-2 câu)
            QUAN TRỌNG: Chỉ dùng tiếng Việt thuần túy, tuyệt đối không dùng ký tự Hán/Trung Quốc hay bất kỳ ngôn ngữ nào khác.
            Chỉ trả lời phần nhận xét, không thêm tiêu đề hay giải thích.
            """;

        try
        {
            var client = httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var body = JsonSerializer.Serialize(new
            {
                model    = "llama-3.3-70b-versatile",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 200
            });

            var response = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AiService] HTTP {(int)response.StatusCode}: {err}");
                throw new Exception($"Groq lỗi {(int)response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("[AiService] Groq gọi thành công");
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? SinhNhanXetCoBan(gpa, xepLoai);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AiService] Lỗi gọi Groq: {ex.Message}");
            return SinhNhanXetCoBan(gpa, xepLoai);
        }
    }

    private static string SinhNhanXetCoBan(float gpa, string xepLoai) => xepLoai switch
    {
        "Giỏi"       => $"Kết quả học tập xuất sắc với GPA {gpa:F2}. Tiếp tục phát huy và duy trì thành tích tốt.",
        "Khá"        => $"Kết quả học tập khá tốt với GPA {gpa:F2}. Cần cố gắng thêm để đạt loại Giỏi.",
        "Trung bình" => $"GPA {gpa:F2} ở mức trung bình. Cần nỗ lực nhiều hơn để cải thiện kết quả.",
        _            => $"GPA {gpa:F2} dưới mức trung bình. Cần có kế hoạch học tập nghiêm túc để tránh nguy cơ học yếu."
    };
}
