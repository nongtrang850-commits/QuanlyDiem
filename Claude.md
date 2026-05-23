# Hệ thống Quản lý Học tập tích hợp AI

## Tổng quan dự án

Đề tài: **Xây dựng hệ thống quản lý học tập, tích hợp AI đánh giá tình trạng học tập của sinh viên khoa CNTT**

Mục tiêu: Xây dựng hệ thống quản lý sinh viên, môn học, điểm số, tích hợp AI phân tích và xếp loại học lực (GPA) để hỗ trợ giảng viên và nhà trường.

---

## Kiến trúc hệ thống

- **Mô hình**: MVC (Model – View – Controller) + Client-Server
- **API**: RESTful API (GET, POST, PUT, PATCH, DELETE)
- **Bảo mật**: JWT authentication, bcrypt mã hóa mật khẩu, phân quyền theo vai trò

---

## Công nghệ đã chọn trong đồ án

| Lớp | Công nghệ |
|-----|-----------|
| Backend | ASP.NET Core Web API (C#) |
| Frontend | HTML, CSS, JavaScript (vanilla) |
| Database | MySQL |
| AI/LLM | Tích hợp mô hình ngôn ngữ lớn (LLM) |
| Tools | Visual Studio / VS Code, Git, Postman, XAMPP, MySQL Workbench |

---

## Gợi ý ngôn ngữ / stack nên dùng

### Lựa chọn 1 — Giữ nguyên theo đồ án (ASP.NET Core + Vanilla JS)
**Phù hợp nếu:** Bạn quen C# và muốn bám sát báo cáo.

```
Backend : C# / ASP.NET Core 8 Web API
Frontend: HTML + CSS + JavaScript (fetch API)
Database: MySQL (qua Entity Framework Core hoặc Dapper)
AI     : Gọi API Gemini / OpenAI / Claude từ backend
```

**Ưu điểm:** Hiệu năng cao, bảo mật tốt, tài liệu chính thức phong phú.  
**Nhược điểm:** Cần cài .NET SDK, cấu hình ban đầu phức tạp hơn PHP.

---

### Lựa chọn 2 — PHP + MySQL (XAMPP sẵn có)
**Phù hợp nếu:** Bạn đang dùng XAMPP và muốn khởi động nhanh.

```
Backend : PHP 8 (Laravel hoặc thuần PHP)
Frontend: HTML + CSS + JavaScript (hoặc Bootstrap)
Database: MySQL
AI     : Gọi API Gemini / OpenAI từ PHP (curl / Guzzle)
```

**Ưu điểm:** Triển khai ngay trên XAMPP (htdocs), không cần cài thêm.  
**Nhược điểm:** Không khớp với báo cáo đồ án (ASP.NET Core).

---

### Lựa chọn 3 — Python + FastAPI (đề xuất nếu muốn AI mạnh)
**Phù hợp nếu:** Bạn muốn tích hợp AI/ML sâu hơn.

```
Backend : Python / FastAPI
Frontend: HTML + CSS + JavaScript hoặc React
Database: MySQL (qua SQLAlchemy)
AI     : scikit-learn / transformers / LangChain
```

**Ưu điểm:** Hệ sinh thái AI/ML mạnh nhất, tích hợp trực tiếp với thư viện ML.  
**Nhược điểm:** Khác hoàn toàn với báo cáo; cần cài Python environment.

---

## Khuyến nghị

> **Nếu đây là dự án thực tế để nộp đồ án:** dùng **ASP.NET Core (C#)** đúng như báo cáo.  
> **Nếu chỉ cần demo nhanh trên XAMPP đang có:** dùng **PHP + MySQL** để tiết kiệm thời gian cài đặt.

---

## Các vai trò người dùng

| Vai trò | Quyền chính |
|---------|-------------|
| **Admin** | Quản lý sinh viên, giảng viên, lớp học; xem đánh giá AI |
| **Giảng viên** | Quản lý môn học; nhập và sửa điểm |
| **Sinh viên** | Tra cứu điểm, xem GPA và xếp loại, đăng ký môn học |

---

## Cấu trúc cơ sở dữ liệu (MySQL)

```sql
-- Người dùng (đăng nhập chung)
NguoiDung (id, email, matkhau, role)

-- Admin
Admin (id, email, password, type, role)

-- Giảng viên
GiangVien (id, fullName, dob, gender, phone, email, department)

-- Sinh viên
SinhVien (id, fullName, dob, gender, phone, email, class, department, course)

-- Lớp học
LopHoc (id, className, department, room, quantity, advisorId → GiangVien)

-- Môn học
MonHoc (id, subjectName, credits, department)

-- Điểm
Diem (id, fullName, class, cc, kt1, kt2, kt3, exam)

-- Đánh giá AI
DanhgiaAI (id, studentId → SinhVien, gpa, rank, aiComment)
```

---

## Chức năng chính

### Admin
- Đăng nhập / Đăng xuất
- CRUD sinh viên, giảng viên, lớp học
- Xem đánh giá học lực AI (GPA + xếp loại + nhận xét)

### Giảng viên
- Đăng nhập / Đăng xuất
- CRUD môn học
- Nhập điểm (chuyên cần, KT1, KT2, KT3, thi); cập nhật điểm

### Sinh viên
- Đăng ký tài khoản / Đăng nhập / Đăng xuất
- Tra cứu điểm từng môn
- Xem GPA và xếp loại học lực
- Đăng ký / hủy đăng ký môn học

---

## Yêu cầu phi chức năng

- Thời gian phản hồi API < 2–3 giây
- Bảo mật: JWT + bcrypt + phân quyền role-based
- Hoạt động 24/7, hỗ trợ nhiều người dùng đồng thời
- Responsive trên web và mobile

---

## Tích hợp AI

AI được dùng để:
1. Tính GPA (trung bình có trọng số theo tín chỉ)
2. Phân loại học lực: Giỏi / Khá / Trung bình / Yếu
3. Sinh nhận xét tự động (`aiComment`) cho từng sinh viên
4. Cảnh báo sớm sinh viên có nguy cơ học yếu

Cách tích hợp: gọi REST API của LLM (Gemini / OpenAI / Claude) từ backend, truyền dữ liệu điểm, nhận về nhận xét và xếp loại.

---

## Định hướng phát triển

- Chatbot AI hỗ trợ sinh viên tra cứu thông tin
- Ứng dụng mobile (Android / iOS)
- Machine Learning dự đoán kết quả học tập
- Mở rộng thành nền tảng LMS hoàn chỉnh
- Thông báo tự động qua email / push notification
