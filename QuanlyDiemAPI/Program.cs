using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanlyDiemAPI.Data;
using QuanlyDiemAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Đọc PORT từ Railway (hoặc dùng 5000 khi chạy local)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Tự ghép connection string từ biến môi trường Railway MySQL
// Nếu không có biến Railway thì dùng connection string trong appsettings.json
var mysqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
var connStr = mysqlHost is not null
    ? $"Server={mysqlHost};Port={Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306"};" +
      $"Database={Environment.GetEnvironmentVariable("MYSQLDATABASE")};" +
      $"User={Environment.GetEnvironmentVariable("MYSQLUSER")};" +
      $"Password={Environment.GetEnvironmentVariable("MYSQLPASSWORD")};"
    : builder.Configuration.GetConnectionString("Default");

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddControllers();

// CORS cho frontend
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

var app = builder.Build();

// Tự động tạo database và seed tài khoản mặc định
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Tạo bảng AuditLogs nếu chưa tồn tại (DB cũ không có bảng này)
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS AuditLogs (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            ThoiGian DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            Email VARCHAR(255) NOT NULL DEFAULT '',
            VaiTro VARCHAR(50) NOT NULL DEFAULT '',
            HanhDong VARCHAR(255) NOT NULL DEFAULT '',
            ChiTiet TEXT
        );
    ");

    if (!db.NguoiDungs.Any())
    {
        var adminAcc = new QuanlyDiemAPI.Models.NguoiDung
        {
            Email = "admin@gmail.com",
            MatKhau = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            MatKhauGoc = "Admin@123",
            Role = "Admin"
        };
        var gvAcc = new QuanlyDiemAPI.Models.NguoiDung
        {
            Email = "giaovien@gmail.com",
            MatKhau = BCrypt.Net.BCrypt.HashPassword("Gv@123456"),
            MatKhauGoc = "Gv@123456",
            Role = "GiangVien",
            GiangVien = new QuanlyDiemAPI.Models.GiangVien
            {
                FullName = "Giảng Viên Demo",
                Email = "giaovien@gmail.com",
                Department = "Công nghệ thông tin"
            }
        };
        var svAcc = new QuanlyDiemAPI.Models.NguoiDung
        {
            Email = "sinhvien@gmail.com",
            MatKhau = BCrypt.Net.BCrypt.HashPassword("Sv@123456"),
            MatKhauGoc = "Sv@123456",
            Role = "SinhVien",
            SinhVien = new QuanlyDiemAPI.Models.SinhVien
            {
                FullName = "Sinh Viên Demo",
                Email = "sinhvien@gmail.com",
                Class = "CNTT01",
                Department = "Công nghệ thông tin",
                Course = "2022"
            }
        };
        db.NguoiDungs.AddRange(adminAcc, gvAcc, svAcc);
        db.SaveChanges();
    }

    // Seed môn học theo từng học kỳ (chương trình CNTT 4 năm = 8 học kỳ)
    if (!db.MonHocs.Any())
    {
        db.MonHocs.AddRange(
            // ===== HỌC KỲ 1 (Năm 1 - HK1) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Tin học đại cương",               Credits = 3, Department = "CNTT",              HocKy = 1 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Toán cao cấp",                    Credits = 4, Department = "Toán",               HocKy = 1 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Vật lý",                          Credits = 3, Department = "Khoa học tự nhiên",  HocKy = 1 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Anh văn 1",                       Credits = 3, Department = "Ngoại ngữ",          HocKy = 1 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Giáo dục thể chất 1",             Credits = 1, Department = "Thể dục",            HocKy = 1 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Triết học Mác - Lênin",           Credits = 3, Department = "Lý luận chính trị",  HocKy = 1 },
            // ===== HỌC KỲ 2 (Năm 1 - HK2) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Toán rời rạc",                    Credits = 3, Department = "Toán",               HocKy = 2 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Anh văn 2",                       Credits = 3, Department = "Ngoại ngữ",          HocKy = 2 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Kỹ thuật lập trình",              Credits = 3, Department = "CNTT",               HocKy = 2 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Giáo dục thể chất 2",             Credits = 1, Department = "Thể dục",            HocKy = 2 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Lịch sử Đảng Cộng sản Việt Nam", Credits = 2, Department = "Lý luận chính trị",  HocKy = 2 },
            // ===== HỌC KỲ 3 (Năm 2 - HK1) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Cấu trúc dữ liệu và giải thuật", Credits = 3, Department = "CNTT",               HocKy = 3 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Cơ sở dữ liệu",                  Credits = 3, Department = "CNTT",               HocKy = 3 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Phương pháp luận lập trình",      Credits = 2, Department = "CNTT",               HocKy = 3 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Xác suất thống kê",               Credits = 3, Department = "Toán",               HocKy = 3 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Kinh tế chính trị Mác - Lênin",  Credits = 3, Department = "Lý luận chính trị",  HocKy = 3 },
            // ===== HỌC KỲ 4 (Năm 2 - HK2) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Lập trình hướng đối tượng",       Credits = 3, Department = "CNTT",               HocKy = 4 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Cấu trúc máy tính và Hệ điều hành", Credits = 3, Department = "CNTT",            HocKy = 4 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Mạng máy tính",                  Credits = 3, Department = "CNTT",               HocKy = 4 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Nhập môn công nghệ phần mềm",    Credits = 3, Department = "CNTT",               HocKy = 4 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Tư tưởng Hồ Chí Minh",          Credits = 2, Department = "Lý luận chính trị",  HocKy = 4 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Chủ nghĩa xã hội khoa học",      Credits = 2, Department = "Lý luận chính trị",  HocKy = 4 },
            // ===== HỌC KỲ 5 (Năm 3 - HK1) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Phân tích thiết kế thuật toán",  Credits = 3, Department = "CNTT",               HocKy = 5 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Lập trình ứng dụng Java",        Credits = 3, Department = "CNTT",               HocKy = 5 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Thiết kế web",                   Credits = 3, Department = "CNTT",               HocKy = 5 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Phân tích và quản lý yêu cầu phần mềm", Credits = 3, Department = "CNTT",        HocKy = 5 },
            // ===== HỌC KỲ 6 (Năm 3 - HK2) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Phương pháp phát triển phần mềm hướng đối tượng", Credits = 3, Department = "CNTT", HocKy = 6 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Công nghệ .Net",                 Credits = 3, Department = "CNTT",               HocKy = 6 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Công nghệ ASP.NET",              Credits = 3, Department = "CNTT",               HocKy = 6 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Phân tích thiết kế hệ thống",    Credits = 3, Department = "CNTT",               HocKy = 6 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Quản lý dự án CNTT",             Credits = 3, Department = "CNTT",               HocKy = 6 },
            // ===== HỌC KỲ 7 (Năm 4 - HK1) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "An toàn thông tin",              Credits = 3, Department = "CNTT",               HocKy = 7 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Trí tuệ nhân tạo",               Credits = 3, Department = "CNTT",               HocKy = 7 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Kiến trúc và thiết kế phần mềm", Credits = 3, Department = "CNTT",               HocKy = 7 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Lập trình cho thiết bị di động", Credits = 3, Department = "CNTT",               HocKy = 7 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Kiểm thử và đảm bảo chất lượng phần mềm", Credits = 3, Department = "CNTT",     HocKy = 7 },
            // ===== HỌC KỲ 8 (Năm 4 - HK2) =====
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Vận hành và bảo trì phần mềm",  Credits = 3, Department = "CNTT",               HocKy = 8 },
            new QuanlyDiemAPI.Models.MonHoc { SubjectName = "Thực tập tốt nghiệp",            Credits = 4, Department = "CNTT",               HocKy = 8 }
        );
        db.SaveChanges();
    }

    // Seed 20 sinh viên mẫu
    if (!db.NguoiDungs.Any(u => u.Email == "sv001@stu.edu.vn"))
    {
        var svData = new[]
        {
            ("Nguyễn Văn An",    "sv001@stu.edu.vn", "Nam", "CNTT01", "K2022"),
            ("Trần Thị Bình",    "sv002@stu.edu.vn", "Nữ",  "CNTT01", "K2022"),
            ("Lê Văn Cường",     "sv003@stu.edu.vn", "Nam", "CNTT01", "K2022"),
            ("Phạm Thị Dung",    "sv004@stu.edu.vn", "Nữ",  "CNTT01", "K2022"),
            ("Hoàng Văn Em",     "sv005@stu.edu.vn", "Nam", "CNTT02", "K2022"),
            ("Đỗ Thị Phương",    "sv006@stu.edu.vn", "Nữ",  "CNTT02", "K2022"),
            ("Vũ Văn Giang",     "sv007@stu.edu.vn", "Nam", "CNTT02", "K2022"),
            ("Bùi Thị Hoa",      "sv008@stu.edu.vn", "Nữ",  "CNTT02", "K2022"),
            ("Đinh Văn Hùng",    "sv009@stu.edu.vn", "Nam", "CNTT03", "K2023"),
            ("Đặng Thị Lan",     "sv010@stu.edu.vn", "Nữ",  "CNTT03", "K2023"),
            ("Ngô Văn Minh",     "sv011@stu.edu.vn", "Nam", "CNTT03", "K2023"),
            ("Phan Thị Nga",     "sv012@stu.edu.vn", "Nữ",  "CNTT03", "K2023"),
            ("Lý Văn Oanh",      "sv013@stu.edu.vn", "Nam", "CNTT04", "K2023"),
            ("Dương Thị Phúc",   "sv014@stu.edu.vn", "Nữ",  "CNTT04", "K2023"),
            ("Trịnh Văn Quang",  "sv015@stu.edu.vn", "Nam", "CNTT04", "K2023"),
            ("Nguyễn Thị Hồng",  "sv016@stu.edu.vn", "Nữ",  "CNTT04", "K2023"),
            ("Phạm Văn Sơn",     "sv017@stu.edu.vn", "Nam", "CNTT05", "K2024"),
            ("Lê Thị Tâm",       "sv018@stu.edu.vn", "Nữ",  "CNTT05", "K2024"),
            ("Hoàng Văn Ước",    "sv019@stu.edu.vn", "Nam", "CNTT05", "K2024"),
            ("Vũ Thị Vân",       "sv020@stu.edu.vn", "Nữ",  "CNTT05", "K2024"),
        };
        foreach (var (name, email, gender, cls, course) in svData)
        {
            db.NguoiDungs.Add(new QuanlyDiemAPI.Models.NguoiDung
            {
                Email     = email,
                MatKhau   = BCrypt.Net.BCrypt.HashPassword("Sv@123456"),
                MatKhauGoc = "Sv@123456",
                Role      = "SinhVien",
                SinhVien  = new QuanlyDiemAPI.Models.SinhVien
                {
                    FullName   = name,
                    Email      = email,
                    Gender     = gender,
                    Class      = cls,
                    Department = "Công nghệ thông tin",
                    Course     = course
                }
            });
        }
        db.SaveChanges();
    }

    // Seed 20 bản ghi điểm cho các SV mẫu sv001-sv020
    var sv01 = db.SinhViens.FirstOrDefault(s => s.Email == "sv001@stu.edu.vn");
    if (sv01 != null && !db.Diems.Any(d => d.SinhVienId == sv01.Id))
    {
        var svList = db.SinhViens
            .Where(s => s.Email.StartsWith("sv0") && s.Email.EndsWith("@stu.edu.vn"))
            .OrderBy(s => s.Email)
            .ToList();
        var mhList = db.MonHocs.OrderBy(m => m.Id).Take(5).ToList();
        if (svList.Count >= 20 && mhList.Count >= 5)
        {
            var scores = new (float cc, float kt1, float kt2, float kt3, float exam)[]
            {
                (9,8,9,8,9),(8,7,8,7,8),(6,5,6,5,6),(4,3,4,3,4),   // sv01-04
                (10,9,9,10,9),(8,7,7,8,7),(5,5,6,5,5),(3,4,3,4,3), // sv05-08
                (9,8,8,9,8),(7,7,8,7,7),(6,6,5,6,5),(4,4,3,4,4),   // sv09-12
                (8,9,8,9,9),(7,7,8,7,8),(5,6,5,6,6),(3,3,4,3,3),   // sv13-16
                (9,8,9,8,9),(8,7,7,8,8),(6,5,6,5,6),(4,4,3,4,4),   // sv17-20
            };
            for (int i = 0; i < 20; i++)
            {
                var s = scores[i];
                db.Diems.Add(new QuanlyDiemAPI.Models.Diem
                {
                    SinhVienId = svList[i].Id,
                    MonHocId   = mhList[i % 5].Id,
                    CC = s.cc, KT1 = s.kt1, KT2 = s.kt2, KT3 = s.kt3, Exam = s.exam
                });
            }
            db.SaveChanges();
        }
    }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions {
    OnPrepareResponse = ctx => {
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
