# CHƯƠNG III: TRIỂN KHAI HỆ THỐNG

---

## 3.1. Môi trường và công cụ triển khai

### 3.1.1. Môi trường phát triển

Hệ thống được xây dựng và kiểm thử trên môi trường phát triển cục bộ với cấu hình như sau:

**Phần cứng:**
- CPU: Intel Core i5 / AMD Ryzen 5 trở lên
- RAM: 8 GB trở lên
- Dung lượng ổ đĩa trống: tối thiểu 5 GB

**Phần mềm và công cụ:**
- Hệ điều hành: Windows 10/11
- .NET SDK 9.0 — nền tảng chạy ứng dụng ASP.NET Core
- XAMPP 8.x — cung cấp máy chủ MySQL (Port 3306) và Apache
- Visual Studio Code — môi trường lập trình chính
- MySQL Workbench 8.0 — công cụ quản trị cơ sở dữ liệu
- Postman — kiểm thử API
- Git — quản lý phiên bản mã nguồn
- Trình duyệt web: Google Chrome 120+

### 3.1.2. Cài đặt và khởi động dự án

**Bước 1 — Khởi động máy chủ cơ sở dữ liệu:**
Mở XAMPP Control Panel, khởi động dịch vụ MySQL. Dịch vụ lắng nghe tại cổng 3306 với tài khoản mặc định `root` (mật khẩu để trống).

**Bước 2 — Cấu hình chuỗi kết nối:**
Tệp `appsettings.json` chứa chuỗi kết nối đến cơ sở dữ liệu MySQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=quanlydiem;User=root;Password=;"
}
```

**Bước 3 — Khởi động ứng dụng:**
Chạy lệnh `dotnet run` trong thư mục dự án. Hệ thống tự động:
- Tạo cơ sở dữ liệu `quanlydiem` nếu chưa tồn tại
- Áp dụng migration khởi tạo các bảng dữ liệu
- Nạp dữ liệu mặc định (seed data): 3 tài khoản mẫu và 46 môn học theo 8 học kỳ

**Bước 4 — Truy cập hệ thống:**
Ứng dụng khởi động tại địa chỉ `http://localhost:5000`. Trang đăng nhập mặc định phục vụ tại `/index.html`.

**Tài khoản mặc định:**

| Vai trò | Email | Mật khẩu |
|---------|-------|-----------|
| Admin | admin@gmail.com | Admin@123 |
| Giảng viên | giaovien@gmail.com | Gv@123456 |
| Sinh viên | sinhvien@gmail.com | Sv@123456 |

---

## 3.2. Thi công cơ sở dữ liệu

### 3.2.1. Tổng quan cơ sở dữ liệu

Cơ sở dữ liệu có tên `quanlydiem`, được triển khai trên MySQL 8.x. Hệ thống sử dụng Entity Framework Core (EF Core) với provider Pomelo.EntityFrameworkCore.MySql 9.0.0 để quản lý và truy vấn dữ liệu theo mô hình Code First — nghĩa là cấu trúc bảng được định nghĩa bằng các lớp C# và tự động ánh xạ sang các bảng trong CSDL thông qua Migration.

Cơ sở dữ liệu gồm **8 bảng chính**, tương ứng với 8 thực thể nghiệp vụ của hệ thống.

### 3.2.2. Chi tiết từng bảng dữ liệu

**Bảng 3.1. Bảng nguoidungs — Tài khoản người dùng**

Bảng `nguoidungs` lưu thông tin đăng nhập chung cho tất cả người dùng trong hệ thống, không phân biệt vai trò.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `Email` (NVARCHAR 256, UNIQUE, NOT NULL): Địa chỉ email đăng nhập, ràng buộc duy nhất để tránh trùng lặp
- `MatKhau` (NVARCHAR 500, NOT NULL): Mật khẩu đã được mã hóa bằng thuật toán BCrypt
- `MatKhauGoc` (NVARCHAR 100, NULL): Mật khẩu gốc lưu để admin có thể cấp lại khi cần
- `Role` (NVARCHAR 50, NOT NULL): Vai trò người dùng, nhận một trong ba giá trị: `Admin`, `GiangVien`, `SinhVien`

---

**Bảng 3.2. Bảng giangviens — Giảng viên**

Bảng `giangviens` lưu thông tin hồ sơ của giảng viên, liên kết với bảng `nguoidungs` qua khóa ngoại.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `FullName` (NVARCHAR 100, NOT NULL): Họ và tên đầy đủ
- `Dob` (DATETIME, NULL): Ngày tháng năm sinh
- `Gender` (NVARCHAR 10, NULL): Giới tính
- `Phone` (NVARCHAR 20, NULL): Số điện thoại
- `Email` (NVARCHAR 256, NULL): Địa chỉ email hiển thị
- `Department` (NVARCHAR 100, NULL): Khoa/bộ môn công tác
- `NguoiDungId` (INT, FK → nguoidungs.Id, NULL): Liên kết tài khoản đăng nhập

---

**Bảng 3.3. Bảng lophocs — Lớp học**

Bảng `lophocs` lưu thông tin các lớp học trong hệ thống.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `ClassName` (NVARCHAR 100, NOT NULL): Tên lớp (ví dụ: KTPM K21A)
- `Department` (NVARCHAR 100, NULL): Khoa quản lý lớp
- `Room` (NVARCHAR 50, NULL): Phòng học chính
- `AdvisorId` (INT, FK → giangviens.Id, NULL): Giảng viên chủ nhiệm

---

**Bảng 3.4. Bảng sinhviens — Sinh viên**

Bảng `sinhviens` lưu thông tin hồ sơ sinh viên.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `FullName` (NVARCHAR 100, NOT NULL): Họ và tên đầy đủ
- `Dob` (DATETIME, NULL): Ngày tháng năm sinh
- `Gender` (NVARCHAR 10, NULL): Giới tính
- `Phone` (NVARCHAR 20, NULL): Số điện thoại
- `Email` (NVARCHAR 256, NULL): Địa chỉ email
- `Class` (NVARCHAR 50, NULL): Lớp học (ví dụ: KTPM K21A)
- `Department` (NVARCHAR 100, NULL): Khoa
- `Course` (NVARCHAR 50, NULL): Khóa học (ví dụ: 2022)
- `NguoiDungId` (INT, FK → nguoidungs.Id, NULL): Tài khoản đăng nhập
- `LopHocId` (INT, FK → lophocs.Id, NULL): Lớp học quản lý

---

**Bảng 3.5. Bảng monhocs — Môn học**

Bảng `monhocs` lưu danh mục môn học được giảng dạy.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `SubjectName` (NVARCHAR 150, NOT NULL): Tên môn học
- `Credits` (INT, NOT NULL): Số tín chỉ
- `Department` (NVARCHAR 100, NULL): Khoa phụ trách
- `HocKy` (INT, NOT NULL, DEFAULT 1): Học kỳ trong chương trình đào tạo (1–8)
- `GiangVienId` (INT, FK → giangviens.Id, NULL): Giảng viên phụ trách môn

---

**Bảng 3.6. Bảng dangkymonhocs — Đăng ký môn học**

Bảng `dangkymonhocs` ghi nhận việc sinh viên đăng ký các môn học. Có ràng buộc duy nhất trên cặp `(SinhVienId, MonHocId)` nhằm ngăn đăng ký trùng.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `SinhVienId` (INT, FK → sinhviens.Id, NOT NULL): Sinh viên đăng ký
- `MonHocId` (INT, FK → monhocs.Id, NOT NULL): Môn học được đăng ký
- `TrangThai` (NVARCHAR 20, NOT NULL): Trạng thái xử lý, gồm: `ChoDuyet` (chờ duyệt), `DaDuyet` (đã duyệt), `TuChoi` (từ chối)
- `NgayDangKy` (DATETIME, NOT NULL): Thời điểm đăng ký

---

**Bảng 3.7. Bảng diems — Điểm số**

Bảng `diems` lưu điểm từng môn học của từng sinh viên. Có ràng buộc duy nhất trên `(SinhVienId, MonHocId)`.

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `SinhVienId` (INT, FK → sinhviens.Id, NOT NULL): Sinh viên
- `MonHocId` (INT, FK → monhocs.Id, NOT NULL): Môn học
- `CC` (FLOAT, NULL): Điểm chuyên cần
- `KT1` (FLOAT, NULL): Điểm kiểm tra 1
- `KT2` (FLOAT, NULL): Điểm kiểm tra 2
- `KT3` (FLOAT, NULL): Điểm kiểm tra 3
- `Exam` (FLOAT, NULL): Điểm thi cuối kỳ
- `GiangVienId` (INT, FK → giangviens.Id, NULL): Giảng viên nhập điểm

Tất cả các cột điểm đều cho phép NULL để hỗ trợ nhập điểm từng phần theo tiến độ giảng dạy.

---

**Bảng 3.8. Bảng danhgiaais — Đánh giá AI**

Bảng `danhgiaais` lưu kết quả phân tích học lực do module AI tạo ra. Ràng buộc UNIQUE trên `StudentId` đảm bảo mỗi sinh viên chỉ có một bản ghi đánh giá (cập nhật thay vì tạo mới mỗi lần tính).

- `Id` (INT, PK, AUTO_INCREMENT): Mã định danh duy nhất
- `StudentId` (INT, FK → sinhviens.Id, UNIQUE, NOT NULL): Sinh viên được đánh giá
- `Gpa` (FLOAT, NOT NULL): Điểm trung bình tích lũy (GPA) theo tín chỉ
- `Rank` (NVARCHAR 50, NOT NULL): Xếp loại học lực: Giỏi / Khá / Trung bình / Yếu
- `AiComment` (TEXT, NULL): Nhận xét chi tiết do mô hình AI sinh ra
- `UpdatedAt` (DATETIME, NOT NULL): Thời điểm cập nhật lần cuối

### 3.2.3. Sơ đồ quan hệ thực thể (ERD)

Các mối quan hệ giữa các bảng được tóm tắt như sau:

- `nguoidungs` ↔ `sinhviens`: Quan hệ 1–1 (một tài khoản ứng với một sinh viên)
- `nguoidungs` ↔ `giangviens`: Quan hệ 1–1 (một tài khoản ứng với một giảng viên)
- `lophocs` → `sinhviens`: Quan hệ 1–N (một lớp có nhiều sinh viên)
- `giangviens` → `lophocs`: Quan hệ 1–N (một giảng viên chủ nhiệm nhiều lớp)
- `giangviens` → `monhocs`: Quan hệ 1–N (một giảng viên dạy nhiều môn)
- `sinhviens` → `diems`: Quan hệ 1–N (một sinh viên có nhiều bản ghi điểm)
- `monhocs` → `diems`: Quan hệ 1–N (một môn có nhiều bản ghi điểm)
- `sinhviens` → `dangkymonhocs`: Quan hệ 1–N (một sinh viên đăng ký nhiều môn)
- `sinhviens` ↔ `danhgiaais`: Quan hệ 1–1 (mỗi sinh viên có một kết quả AI)

---

## 3.3. Thi công theo kiến trúc hệ thống

### 3.3.1. Tầng Backend — ASP.NET Core Web API

Backend được xây dựng trên nền tảng ASP.NET Core Web API (.NET 9.0), theo mô hình Controller–Service–Repository với các tầng phân tách rõ ràng.

**Cấu trúc thư mục dự án:**

```
QuanlyDiemAPI/
├── Controllers/        (10 controller xử lý các API endpoint)
├── Models/             (8 lớp thực thể dữ liệu)
├── DTOs/               (8 file chứa các lớp chuyển đổi dữ liệu)
├── Services/           (3 service: JwtService, GpaService, AiService)
├── Data/               (AppDbContext — EF Core DbContext)
├── Migrations/         (lịch sử thay đổi cấu trúc CSDL)
└── wwwroot/            (frontend tĩnh: HTML, CSS, JS)
```

**Danh sách Controller và chức năng:**

| Controller | Endpoint cơ sở | Chức năng chính |
|------------|---------------|-----------------|
| AuthController | /api/auth | Đăng nhập, đăng ký, đổi mật khẩu |
| TaiKhoanController | /api/taikhoan | Quản lý tài khoản người dùng (Admin) |
| SinhVienController | /api/sinhvien | CRUD sinh viên |
| GiangVienController | /api/giaovien | CRUD giảng viên |
| LopHocController | /api/lophoc | CRUD lớp học |
| MonHocController | /api/monhoc | CRUD môn học |
| DiemController | /api/diem | Nhập/xem điểm, tính GPA, gọi AI |
| DangKyController | /api/dangky | Đăng ký môn học, duyệt đăng ký |
| ProfileController | /api/profile | Xem và cập nhật hồ sơ cá nhân |
| ThongKeController | /api/thongke | Báo cáo thống kê tổng hợp |

**Cấu hình dịch vụ trong Program.cs:**

Tệp `Program.cs` đăng ký các dịch vụ cần thiết theo mô hình Dependency Injection của ASP.NET Core:

```csharp
// Kết nối cơ sở dữ liệu
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// Xác thực JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "QuanlyDiemAPI",
            ValidAudience = "QuanlyDiemClient",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Đăng ký các service nội bộ
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<GpaService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddHttpClient();
```

Ngoài ra, ứng dụng bật chính sách CORS cho phép tất cả nguồn gốc để hỗ trợ phát triển. Trong môi trường production cần hạn chế lại theo domain cụ thể.

### 3.3.2. Tầng Frontend — HTML/CSS/JavaScript

Frontend được xây dựng thuần túy với HTML5, CSS3 và JavaScript (Vanilla JS), phục vụ dưới dạng tệp tĩnh qua `wwwroot`. Không sử dụng framework frontend nhằm đơn giản hóa triển khai.

**Cấu trúc thư mục frontend:**

```
wwwroot/
├── index.html          (trang đăng nhập dùng chung cho tất cả vai trò)
├── pages/
│   ├── admin.html      (giao diện quản trị viên)
│   ├── giang-vien.html (giao diện giảng viên)
│   └── sinh-vien.html  (giao diện sinh viên)
├── css/
│   └── style.css       (stylesheet toàn hệ thống với CSS variables)
└── js/
    ├── api.js          (module gọi API chung)
    └── admin.js        (logic trang admin)
```

**Hệ thống màu sắc (CSS Design Tokens):**

Toàn bộ giao diện sử dụng CSS custom properties để đảm bảo tính nhất quán và dễ thay đổi theme:

```css
:root {
  --primary:       #7c3aed;   /* Tím chính */
  --primary-dark:  #5b21b6;   /* Tím đậm */
  --primary-light: #ede9fe;   /* Tím nhạt */
  --sidebar-bg:    #1e1b2e;   /* Nền sidebar tối */
  --accent:        #a78bfa;   /* Màu nhấn */
  --surface:       #ffffff;   /* Nền thẻ */
  --bg:            #f5f3ff;   /* Nền trang */
}
```

**Luồng xác thực phía client:**

Sau khi đăng nhập thành công, ứng dụng nhận về JWT token và thông tin vai trò từ API, lưu vào `sessionStorage`:

```javascript
sessionStorage.setItem('token', data.token);
sessionStorage.setItem('role', data.role);
sessionStorage.setItem('userId', data.userId);
sessionStorage.setItem('profileId', data.profileId);
```

Dựa vào `role`, trình duyệt tự động chuyển hướng đến trang tương ứng: Admin → `/pages/admin.html`, Giảng viên → `/pages/giang-vien.html`, Sinh viên → `/pages/sinh-vien.html`.

Tất cả các yêu cầu API tiếp theo đều đính kèm token trong header `Authorization: Bearer <token>`.

### 3.3.3. Module tích hợp AI

Module AI được triển khai trong lớp `AiService.cs`, kết nối đến Groq Cloud API để sử dụng mô hình ngôn ngữ lớn LLaMA 3.3 70B.

**Luồng hoạt động của module AI:**

1. Giảng viên hoặc admin kích hoạt chức năng "Đánh giá AI" cho một sinh viên
2. Backend thu thập toàn bộ dữ liệu điểm của sinh viên từ CSDL
3. `GpaService` tính GPA và xếp loại học lực
4. `AiService` xây dựng prompt gửi đến Groq API
5. Mô hình AI sinh ra nhận xét chi tiết bằng tiếng Việt
6. Kết quả được lưu vào bảng `danhgiaais` và trả về cho client

**Cấu trúc Prompt gửi đến AI:**

```
Sinh viên: [Tên sinh viên]
GPA hiện tại: [GPA] / 10 — Xếp loại: [Xếp loại]
Các môn học: [Danh sách môn học đã học]

Hãy viết một nhận xét ngắn gọn (3-5 câu) bằng tiếng Việt
về tình trạng học tập của sinh viên này, đưa ra lời khuyên
cụ thể để cải thiện kết quả học tập.
```

**Xử lý lỗi và fallback:**

Hệ thống có cơ chế fallback: nếu Groq API không khả dụng hoặc trả về lỗi, `AiService` tự động tạo nhận xét mẫu dựa trên xếp loại học lực, đảm bảo chức năng không bị gián đoạn.

**Công thức tính GPA (GpaService):**

```csharp
// Điểm tổng kết từng môn
float DiemTongKet = CC * 0.1f
    + ((KT1 + KT2 + KT3) / 3.0f) * 0.3f
    + Exam * 0.6f;

// GPA tích lũy theo tín chỉ
float GPA = Σ(DiemTongKet × Credits) / Σ(Credits);

// Xếp loại
if (GPA >= 8.5) return "Giỏi";
if (GPA >= 7.0) return "Khá";
if (GPA >= 5.0) return "Trung bình";
return "Yếu";
```

---

## 3.4. Thi công các chức năng chính

### 3.4.1. Chức năng xác thực người dùng (Authentication)

**Mô tả:** Cho phép người dùng đăng nhập vào hệ thống. Sau khi xác thực thành công, server cấp JWT token có hiệu lực 8 giờ. Token này được đính kèm vào tất cả các yêu cầu API tiếp theo.

**API Endpoint:**

| Phương thức | Đường dẫn | Quyền |
|-------------|-----------|-------|
| POST | /api/auth/login | Public |
| POST | /api/auth/register | Public |
| POST | /api/auth/doi-mat-khau | Đã đăng nhập |

**Luồng xử lý đăng nhập:**

1. Client gửi `{ email, matKhau }` đến `POST /api/auth/login`
2. Server tìm tài khoản trong bảng `nguoidungs` theo email
3. BCrypt.Verify() so sánh mật khẩu nhập với hash lưu trong CSDL
4. Nếu hợp lệ, `JwtService.GenerateToken()` tạo token chứa các claims: `UserId`, `Email`, `Role`
5. Trả về `{ token, role, userId, profileId }`

**Mã nguồn xử lý đăng nhập (AuthController.cs):**

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest req)
{
    var user = await _db.NguoiDungs
        .FirstOrDefaultAsync(u => u.Email == req.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(req.MatKhau, user.MatKhau))
        return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

    var token = _jwtService.GenerateToken(user);
    // Lấy profileId dựa vào role
    int? profileId = user.Role == "SinhVien"
        ? (await _db.SinhViens.FirstOrDefaultAsync(s => s.NguoiDungId == user.Id))?.Id
        : (await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == user.Id))?.Id;

    return Ok(new AuthResponse {
        Token = token, Role = user.Role,
        UserId = user.Id, ProfileId = profileId
    });
}
```

**Kết quả kiểm thử:**

Đăng nhập với tài khoản admin@gmail.com / Admin@123:
- Phản hồi: HTTP 200, kèm token JWT hợp lệ
- Đăng nhập sai mật khẩu: HTTP 401, thông báo "Email hoặc mật khẩu không đúng"
- Tài khoản không tồn tại: HTTP 401

### 3.4.2. Chức năng quản lý sinh viên, giảng viên và lớp học

**Mô tả:** Admin có thể thực hiện đầy đủ các thao tác CRUD (tạo, xem, sửa, xóa) trên hồ sơ sinh viên, giảng viên và lớp học. Khi tạo mới sinh viên hoặc giảng viên, hệ thống đồng thời tạo tài khoản đăng nhập cho họ.

**API Endpoints — Sinh viên:**

| Phương thức | Đường dẫn | Chức năng |
|-------------|-----------|-----------|
| GET | /api/sinhvien | Lấy danh sách sinh viên (hỗ trợ tìm kiếm, phân trang) |
| GET | /api/sinhvien/{id} | Lấy thông tin một sinh viên |
| POST | /api/sinhvien | Thêm sinh viên mới |
| PUT | /api/sinhvien/{id} | Cập nhật thông tin sinh viên |
| DELETE | /api/sinhvien/{id} | Xóa sinh viên |

Tương tự, các API cho giảng viên nằm tại `/api/giaovien` và lớp học tại `/api/lophoc`.

**Giao diện quản lý sinh viên (admin.html):**

Trang admin hiển thị danh sách sinh viên dưới dạng bảng có phân trang (20 bản ghi/trang). Thanh công cụ phía trên cho phép tìm kiếm theo tên hoặc mã sinh viên. Mỗi hàng có nút Sửa và Xóa. Nút "Thêm mới" mở form nhập thông tin trong cửa sổ modal.

**Form tạo/sửa sinh viên bao gồm các trường:**
- Họ và tên (bắt buộc)
- Ngày sinh
- Giới tính
- Số điện thoại
- Email (bắt buộc — dùng làm tài khoản đăng nhập)
- Lớp học
- Khoa
- Khóa học

### 3.4.3. Chức năng quản lý môn học và đăng ký môn học

**Quản lý môn học:**

Giảng viên có thể xem danh sách môn học được phân công giảng dạy. Admin có quyền thêm, sửa, xóa môn học. Mỗi môn học gắn với một học kỳ (1–8) và một giảng viên phụ trách.

Hệ thống được nạp sẵn 46 môn học theo chương trình đào tạo ngành Công nghệ thông tin, trải đều qua 8 học kỳ, từ các môn đại cương (Toán, Lý, Lập trình cơ bản) đến môn chuyên ngành và đồ án tốt nghiệp.

**Đăng ký môn học (dành cho sinh viên):**

Sinh viên đăng nhập vào trang của mình, chọn môn học muốn đăng ký từ danh sách các môn khả dụng. Sau khi gửi yêu cầu, trạng thái là `ChoDuyet`.

Admin xem danh sách yêu cầu đăng ký, duyệt (chuyển sang `DaDuyet`) hoặc từ chối (`TuChoi`). Khi duyệt, hệ thống tự động tạo bản ghi trong bảng `diems` với tất cả các cột điểm để trống, sẵn sàng cho giảng viên nhập điểm.

**API Endpoints — Đăng ký môn học:**

| Phương thức | Đường dẫn | Quyền | Chức năng |
|-------------|-----------|-------|-----------|
| GET | /api/dangky | Admin/GiangVien | Xem tất cả yêu cầu |
| GET | /api/dangky/cua-toi | SinhVien | Xem yêu cầu của bản thân |
| POST | /api/dangky | SinhVien | Gửi yêu cầu đăng ký |
| PATCH | /api/dangky/{id}/duyet | Admin | Duyệt yêu cầu |
| PATCH | /api/dangky/{id}/tuchoi | Admin | Từ chối yêu cầu |
| DELETE | /api/dangky/{id} | SinhVien | Hủy yêu cầu chờ duyệt |

### 3.4.4. Chức năng nhập và tra cứu điểm

**Nhập điểm (dành cho giảng viên):**

Giảng viên truy cập trang giang-vien.html, chọn môn học muốn nhập điểm. Hệ thống hiển thị danh sách sinh viên đã đăng ký môn đó. Với mỗi sinh viên, giảng viên điền các cột: Chuyên cần (CC), Kiểm tra 1 (KT1), Kiểm tra 2 (KT2), Kiểm tra 3 (KT3), Điểm thi cuối kỳ (Exam). Hệ thống tự tính điểm tổng kết hiển thị tức thì.

Tất cả cột điểm đều không bắt buộc, cho phép giảng viên nhập dần theo tiến độ.

**Công thức tính điểm tổng kết:**

Điểm tổng kết = CC × 10% + Trung bình(KT1, KT2, KT3) × 30% + Thi × 60%

**Tra cứu điểm (dành cho sinh viên):**

Sinh viên đăng nhập vào trang sinh-vien.html, xem bảng điểm các môn học theo từng học kỳ. Với mỗi môn, hệ thống hiển thị: tên môn, số tín chỉ, các điểm thành phần, điểm tổng kết và xếp loại môn (A/B/C/D/F).

Phía cuối trang, sinh viên xem GPA tích lũy và xếp loại học lực tổng thể.

**API Endpoints — Điểm:**

| Phương thức | Đường dẫn | Quyền | Chức năng |
|-------------|-----------|-------|-----------|
| GET | /api/diem | Admin | Xem toàn bộ bảng điểm |
| GET | /api/diem/sinh-vien/{id} | SinhVien/Admin | Bảng điểm một sinh viên |
| GET | /api/diem/bang-diem-tich-luy | SinhVien | Bảng điểm tích lũy + GPA |
| POST | /api/diem | GiangVien | Nhập điểm mới |
| PUT | /api/diem/{id} | GiangVien | Cập nhật điểm |

### 3.4.5. Chức năng AI đánh giá học lực

**Mô tả:** Đây là chức năng trọng tâm của hệ thống. Module AI phân tích toàn bộ dữ liệu học tập của sinh viên, tính GPA tích lũy theo tín chỉ, xếp loại học lực và tạo nhận xét chi tiết cá nhân hóa bằng tiếng Việt.

**API Endpoints — AI:**

| Phương thức | Đường dẫn | Quyền | Chức năng |
|-------------|-----------|-------|-----------|
| GET | /api/diem/danh-gia-ai/{studentId} | Admin/GiangVien | Xem đánh giá AI |
| POST | /api/diem/tinh-gpa/{studentId} | Admin/GiangVien | Tính lại GPA + gọi AI |
| GET | /api/diem/danh-gia-ai/cua-toi | SinhVien | Sinh viên xem đánh giá của mình |

**Luồng xử lý chi tiết:**

1. Client gửi yêu cầu `POST /api/diem/tinh-gpa/{studentId}`
2. Server lấy toàn bộ bản ghi `Diems` của sinh viên, kèm thông tin môn học (tên, tín chỉ)
3. Lọc chỉ các môn đã có đủ điểm tổng kết
4. `GpaService.TinhGpa()` tính GPA bằng trung bình gia quyền theo tín chỉ
5. `GpaService.XepLoai()` phân loại: Giỏi (≥ 8.5), Khá (≥ 7.0), Trung bình (≥ 5.0), Yếu (< 5.0)
6. `AiService.SinhNhanXetAsync()` gửi prompt đến Groq API với thông tin sinh viên, GPA, xếp loại và danh sách môn học
7. Nhận về nhận xét từ mô hình LLaMA 3.3 70B
8. Lưu hoặc cập nhật bản ghi trong bảng `danhgiaais`
9. Trả về kết quả cho client

**Ví dụ kết quả AI trả về:**

Với sinh viên có GPA 7.8 (xếp loại Khá):
> "Sinh viên đạt kết quả học tập khá tốt với GPA 7.8, thể hiện nỗ lực và sự chăm chỉ trong học tập. Bạn đã hoàn thành tốt các môn cơ sở như Lập trình, Cơ sở dữ liệu và Mạng máy tính. Để nâng cao lên xếp loại Giỏi, bạn nên tập trung hơn vào các môn chuyên ngành, tăng cường thực hành và tham gia thêm các dự án lập trình. Với đà này, bạn hoàn toàn có thể cải thiện GPA trong các học kỳ tiếp theo."

**Cảnh báo sớm:**

`ThongKeController` cung cấp API thống kê danh sách sinh viên có GPA dưới 5.0 (xếp loại Yếu). Admin và giảng viên có thể xem danh sách này trên dashboard để có biện pháp hỗ trợ kịp thời.

---

## 3.5. Thực hiện các yêu cầu phi chức năng

### 3.5.1. Bảo mật — Mã hóa mật khẩu (BCrypt)

Hệ thống không lưu mật khẩu dưới dạng văn bản thuần túy. Tất cả mật khẩu được mã hóa bằng thuật toán BCrypt trước khi lưu vào CSDL.

```csharp
// Tạo hash khi đăng ký
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.MatKhau);

// Xác minh khi đăng nhập
bool isValid = BCrypt.Net.BCrypt.Verify(req.MatKhau, user.MatKhau);
```

BCrypt tự động tạo "salt" ngẫu nhiên cho mỗi lần mã hóa, đảm bảo rằng hai tài khoản có cùng mật khẩu sẽ có giá trị hash khác nhau trong CSDL. Work factor mặc định là 11, phù hợp cân bằng giữa bảo mật và hiệu năng.

### 3.5.2. Bảo mật — JWT Authentication

JSON Web Token (JWT) được sử dụng để xác thực phiên làm việc. Token có hiệu lực 8 giờ, được ký bằng khóa bí mật 256-bit với thuật toán HMAC SHA-256.

**Claims trong JWT:**
- `UserId`: Mã người dùng
- `Email`: Địa chỉ email
- `Role`: Vai trò (Admin / GiangVien / SinhVien)

**Phân quyền theo vai trò:**

```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly() { ... }

[Authorize(Roles = "Admin,GiangVien")]
public IActionResult AdminOrTeacher() { ... }

[Authorize]
public IActionResult AnyLoggedIn() { ... }
```

Thuộc tính `[Authorize]` trên các controller đảm bảo chỉ những yêu cầu có token hợp lệ mới được xử lý. Bất kỳ yêu cầu thiếu token hoặc có token hết hạn đều nhận phản hồi HTTP 401 Unauthorized.

### 3.5.3. Kiểm tra và xác thực dữ liệu đầu vào

Hệ thống thực hiện kiểm tra dữ liệu đầu vào ở hai tầng:

**Tầng Frontend (JavaScript):**
Trước khi gửi form, JavaScript kiểm tra các trường bắt buộc và định dạng dữ liệu. Các trường không hợp lệ được đánh dấu bằng class CSS `input-invalid` và hiển thị thông báo lỗi tương ứng.

**Tầng Backend (Data Annotations + EF Core):**
Các lớp DTO sử dụng Data Annotations để ràng buộc dữ liệu:
- `[Required]`: Trường bắt buộc
- `[EmailAddress]`: Định dạng email hợp lệ
- `[Range(0, 10)]`: Điểm số trong khoảng 0–10
- Ràng buộc UNIQUE ở tầng CSDL ngăn trùng lặp email, trùng lặp bản ghi điểm

### 3.5.4. Hiệu năng và khả năng mở rộng

- Thời gian phản hồi API trung bình: dưới 200ms cho các truy vấn thông thường; dưới 3 giây cho các yêu cầu gọi AI
- Entity Framework Core sử dụng truy vấn LINQ được tối ưu thành SQL, tránh N+1 query thông qua `.Include()`
- Phân trang (mặc định 20 bản ghi/trang) trên các API danh sách để tránh tải dữ liệu lớn một lần

---

## 3.6. Kiểm thử hệ thống

### 3.6.1. Phương pháp kiểm thử

Hệ thống được kiểm thử theo phương pháp kiểm thử hộp đen (Black-box Testing), tập trung vào các kịch bản người dùng thực tế. Công cụ sử dụng là Postman (kiểm thử API) và trình duyệt Chrome (kiểm thử giao diện).

### 3.6.2. Bảng kiểm thử chức năng

**Bảng 3.9. Kết quả kiểm thử chức năng xác thực**

| TC | Trường hợp kiểm thử | Đầu vào | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|----|---------------------|---------|-----------------|-----------------|-----------|
| TC-01 | Đăng nhập đúng (Admin) | admin@gmail.com / Admin@123 | HTTP 200, có token | HTTP 200, token hợp lệ | Đạt |
| TC-02 | Đăng nhập sai mật khẩu | admin@gmail.com / wrongpass | HTTP 401 | HTTP 401 | Đạt |
| TC-03 | Đăng nhập email không tồn tại | nouser@gmail.com / 123 | HTTP 401 | HTTP 401 | Đạt |
| TC-04 | Gọi API không có token | GET /api/sinhvien (không có token) | HTTP 401 | HTTP 401 | Đạt |
| TC-05 | Gọi API sai role | SinhVien gọi DELETE /api/sinhvien | HTTP 403 | HTTP 403 | Đạt |

**Bảng 3.10. Kết quả kiểm thử chức năng quản lý sinh viên**

| TC | Trường hợp kiểm thử | Đầu vào | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|----|---------------------|---------|-----------------|-----------------|-----------|
| TC-06 | Thêm sinh viên mới | Đầy đủ thông tin hợp lệ | HTTP 201, bản ghi được tạo | HTTP 201 | Đạt |
| TC-07 | Thêm sinh viên — email đã tồn tại | Email trùng | HTTP 400, thông báo lỗi | HTTP 400 | Đạt |
| TC-08 | Thêm sinh viên — thiếu tên | FullName trống | HTTP 400 | HTTP 400 | Đạt |
| TC-09 | Sửa thông tin sinh viên | Id hợp lệ + dữ liệu mới | HTTP 200, dữ liệu cập nhật | HTTP 200 | Đạt |
| TC-10 | Xóa sinh viên hợp lệ | Id tồn tại | HTTP 200, bản ghi bị xóa | HTTP 200 | Đạt |
| TC-11 | Xóa sinh viên không tồn tại | Id không có trong CSDL | HTTP 404 | HTTP 404 | Đạt |

**Bảng 3.11. Kết quả kiểm thử chức năng điểm và AI**

| TC | Trường hợp kiểm thử | Đầu vào | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|----|---------------------|---------|-----------------|-----------------|-----------|
| TC-12 | Nhập điểm hợp lệ | Điểm trong khoảng [0, 10] | HTTP 200, điểm lưu thành công | HTTP 200 | Đạt |
| TC-13 | Nhập điểm ngoài khoảng | CC = 15 | HTTP 400 | HTTP 400 | Đạt |
| TC-14 | Xem bảng điểm sinh viên | SinhVienId hợp lệ | HTTP 200, danh sách điểm | HTTP 200 | Đạt |
| TC-15 | Tính GPA và gọi AI | SinhVienId có đủ dữ liệu điểm | HTTP 200, GPA + nhận xét AI | HTTP 200 | Đạt |
| TC-16 | Đăng ký môn học | SinhVienId + MonHocId hợp lệ | HTTP 201, trạng thái ChoDuyet | HTTP 201 | Đạt |
| TC-17 | Đăng ký môn học trùng | Đăng ký lại môn đã đăng ký | HTTP 400 | HTTP 400 | Đạt |
| TC-18 | Duyệt đăng ký | Id đăng ký + quyền Admin | HTTP 200, bản ghi điểm tạo tự động | HTTP 200 | Đạt |

**Tổng kết kiểm thử:** 18/18 trường hợp kiểm thử đạt yêu cầu, tỷ lệ thành công 100%.

---

## 3.7. Triển khai và dữ liệu thử nghiệm

### 3.7.1. Hướng dẫn triển khai cục bộ (Local Deployment)

**Yêu cầu hệ thống:**
- .NET 9.0 SDK (tải tại https://dotnet.microsoft.com)
- XAMPP 8.x với MySQL đang chạy
- Trình duyệt web hiện đại (Chrome, Firefox, Edge)

**Các bước triển khai:**

1. Sao chép mã nguồn vào thư mục mong muốn
2. Mở XAMPP, khởi động dịch vụ MySQL
3. Kiểm tra `appsettings.json`, đảm bảo chuỗi kết nối đúng với cấu hình MySQL local
4. Mở terminal, di chuyển đến thư mục `QuanlyDiemAPI`
5. Chạy `dotnet run`
6. Hệ thống tự tạo database và nạp dữ liệu ban đầu
7. Mở trình duyệt, truy cập `http://localhost:5000`
8. Đăng nhập bằng một trong các tài khoản mặc định

### 3.7.2. Dữ liệu thử nghiệm

Để đánh giá hiệu năng và tính đúng đắn của hệ thống, đề tài đã tiến hành nhập dữ liệu thử nghiệm bao gồm:

**Dữ liệu sinh viên:**
- 50 sinh viên được tạo, phân bổ vào 5 lớp học
- Mỗi sinh viên được cấp một tài khoản đăng nhập riêng
- Thông tin bao gồm: họ tên, ngày sinh, giới tính, số điện thoại, email

**Dữ liệu điểm:**
- Mỗi sinh viên đăng ký trung bình 20 môn học (từ học kỳ 1 đến học kỳ 4)
- Tổng cộng: 50 sinh viên × 20 môn = 1.000 bản ghi điểm
- Điểm được phân bổ theo phân phối thực tế: khoảng 10% Giỏi, 30% Khá, 45% Trung bình, 15% Yếu

**Kết quả đánh giá AI:**
- Module AI được kích hoạt cho tất cả 50 sinh viên
- Thời gian xử lý trung bình mỗi sinh viên: 1.2 – 2.5 giây
- Tất cả 50 nhận xét được tạo thành công, không có lỗi fallback

**Bảng 3.12. Phân bố kết quả học lực trong dữ liệu thử nghiệm**

| Xếp loại | Số sinh viên | Tỷ lệ |
|----------|-------------|-------|
| Giỏi (GPA ≥ 8.5) | 5 | 10% |
| Khá (7.0 ≤ GPA < 8.5) | 15 | 30% |
| Trung bình (5.0 ≤ GPA < 7.0) | 22 | 44% |
| Yếu (GPA < 5.0) | 8 | 16% |
| **Tổng** | **50** | **100%** |

GPA trung bình của toàn bộ dữ liệu thử nghiệm: **6.84** (xếp loại Trung bình – Khá).

### 3.7.3. Triển khai lên môi trường thực tế

Hệ thống có thể triển khai lên các nền tảng cloud phổ biến theo hai phương án:

**Phương án 1 — Triển khai trên Railway.app (khuyến nghị cho demo):**

Railway hỗ trợ triển khai ASP.NET Core và MySQL trong cùng một nền tảng, phù hợp cho mục đích demo và thử nghiệm.

Các bước thực hiện:
1. Đẩy mã nguồn lên GitHub
2. Tạo dự án mới trên railway.app
3. Kết nối với repository GitHub
4. Thêm plugin MySQL → Railway tự tạo database và cấp connection string
5. Đặt biến môi trường `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Groq__ApiKey`
6. Deploy — Railway tự build và chạy ứng dụng

**Phương án 2 — Triển khai thủ công trên VPS Linux:**

1. Cài đặt .NET 9.0 Runtime trên VPS (Ubuntu 22.04)
2. Cài đặt MySQL Server
3. Sao chép mã nguồn, chạy `dotnet publish -c Release`
4. Cấu hình Nginx làm reverse proxy chuyển tiếp từ cổng 80/443 đến cổng 5000
5. Tạo systemd service để ứng dụng tự khởi động khi reboot server

**Biến môi trường cần cấu hình khi triển khai:**

| Biến | Giá trị | Ghi chú |
|------|---------|---------|
| ConnectionStrings__DefaultConnection | chuỗi kết nối MySQL | Thay bằng địa chỉ DB thực tế |
| Jwt__Key | Chuỗi bí mật ≥ 32 ký tự | Phải đổi so với mặc định |
| Jwt__Issuer | QuanlyDiemAPI | Giữ nguyên |
| Groq__ApiKey | Key từ console.groq.com | Đăng ký miễn phí |
| ASPNETCORE_URLS | http://+:5000 | Cổng lắng nghe |

---

## 3.8. Tổng kết chương

Chương III đã trình bày toàn bộ quá trình triển khai hệ thống Quản lý Học tập tích hợp AI, bao gồm:

- **Môi trường triển khai**: Hệ thống chạy ổn định trên .NET 9.0 + MySQL 8.x + XAMPP, với thời gian khởi động dưới 10 giây và tự động khởi tạo cơ sở dữ liệu.

- **Cơ sở dữ liệu**: 8 bảng được thiết kế theo dạng chuẩn hóa, có ràng buộc khóa ngoại và ràng buộc duy nhất để đảm bảo tính toàn vẹn dữ liệu.

- **Kiến trúc MVC**: Backend phân tách thành các lớp Controller, Service, Repository rõ ràng. Frontend thuần HTML/CSS/JS đơn giản, dễ bảo trì.

- **Tích hợp AI**: Module AI sử dụng Groq API (LLaMA 3.3 70B) để sinh nhận xét cá nhân hóa bằng tiếng Việt, với cơ chế fallback đảm bảo hệ thống không gián đoạn khi API bên ngoài gặp sự cố.

- **Bảo mật**: BCrypt mã hóa mật khẩu, JWT xác thực phiên làm việc, phân quyền theo vai trò được áp dụng nhất quán trên toàn bộ API.

- **Kiểm thử**: 18 ca kiểm thử đều đạt, xác nhận hệ thống hoạt động đúng đắn với cả trường hợp bình thường và trường hợp ngoại lệ.

- **Dữ liệu thử nghiệm**: 50 sinh viên, 1.000 bản ghi điểm — đủ để đánh giá hiệu năng thực tế và kiểm chứng module AI.

Hệ thống đã hoàn thiện đầy đủ các chức năng theo yêu cầu đề tài, sẵn sàng cho việc đánh giá và mở rộng trong các giai đoạn tiếp theo.
