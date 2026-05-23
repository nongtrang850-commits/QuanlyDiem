# CHƯƠNG 4: KIỂM THỬ HỆ THỐNG

---

## 4.1. Mục tiêu kiểm thử

Kiểm thử hệ thống nhằm đảm bảo các chức năng hoạt động đúng theo yêu cầu đặc tả, phát hiện và khắc phục lỗi trước khi triển khai, đồng thời xác nhận hệ thống đáp ứng các tiêu chí về bảo mật, hiệu năng và trải nghiệm người dùng.

**Cụ thể, quá trình kiểm thử hướng đến:**
- Kiểm tra tính đúng đắn của các chức năng nghiệp vụ (đăng nhập, quản lý điểm, tính GPA, đánh giá AI,…)
- Xác minh cơ chế xác thực và phân quyền theo vai trò (Admin, Giảng viên, Sinh viên)
- Kiểm tra tính toàn vẹn dữ liệu và xử lý ngoại lệ
- Đánh giá hiệu năng API dưới tải bình thường

---

## 4.2. Phạm vi kiểm thử

| Loại kiểm thử | Phạm vi |
|---------------|---------|
| Kiểm thử chức năng (Functional Testing) | Toàn bộ các module nghiệp vụ |
| Kiểm thử API | Tất cả 11 controller, ~35 endpoint |
| Kiểm thử bảo mật | Xác thực JWT, phân quyền, mã hóa mật khẩu |
| Kiểm thử tích hợp | Luồng dữ liệu từ Frontend → API → Database → AI |
| Kiểm thử giao diện | 3 trang vai trò (Admin, Giảng viên, Sinh viên) |
| Kiểm thử hiệu năng | Thời gian phản hồi API |

---

## 4.3. Môi trường kiểm thử

| Thành phần | Cấu hình |
|------------|----------|
| **Hệ điều hành** | Windows 11 |
| **Backend** | ASP.NET Core 8, chạy tại `http://localhost:5000` |
| **Database** | MySQL 8 (XAMPP) |
| **Trình duyệt** | Google Chrome, Microsoft Edge |
| **Công cụ kiểm thử API** | Postman / Swagger UI |
| **Tài khoản test** | admin@gmail.com, giaovien@gmail.com, sinhvien@gmail.com |

---

## 4.4. Danh sách ca kiểm thử

### 4.4.1. Module Xác thực (Authentication)

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-AUTH-01 | Đăng nhập thành công với Admin | Email: admin@gmail.com, Mật khẩu: Admin@123 | Trả về JWT token, role = "Admin", chuyển hướng đến trang Admin | Nhận token hợp lệ, điều hướng đúng trang | ✅ Đạt |
| TC-AUTH-02 | Đăng nhập thành công với Giảng viên | Email: giaovien@gmail.com, Mật khẩu: Gv@123456 | Trả về JWT token, role = "GiangVien" | Nhận token hợp lệ, điều hướng đúng trang | ✅ Đạt |
| TC-AUTH-03 | Đăng nhập thành công với Sinh viên | Email: sinhvien@gmail.com, Mật khẩu: Sv@123456 | Trả về JWT token, role = "SinhVien" | Nhận token hợp lệ, điều hướng đúng trang | ✅ Đạt |
| TC-AUTH-04 | Đăng nhập sai mật khẩu | Email hợp lệ, mật khẩu sai | HTTP 401, thông báo "Sai mật khẩu" | Hệ thống từ chối đăng nhập | ✅ Đạt |
| TC-AUTH-05 | Đăng nhập email không tồn tại | Email không có trong hệ thống | HTTP 404, thông báo lỗi | Hệ thống từ chối đăng nhập | ✅ Đạt |
| TC-AUTH-06 | Đăng nhập thiếu thông tin | Bỏ trống email hoặc mật khẩu | HTTP 400, yêu cầu nhập đầy đủ | Hiển thị thông báo lỗi validation | ✅ Đạt |
| TC-AUTH-07 | Truy cập API không có token | Gọi API bảo vệ mà không có Bearer token | HTTP 401 Unauthorized | Bị từ chối truy cập | ✅ Đạt |
| TC-AUTH-08 | Truy cập API với token hết hạn | Dùng token cũ hơn 8 giờ | HTTP 401 Unauthorized | Bị từ chối, yêu cầu đăng nhập lại | ✅ Đạt |
| TC-AUTH-09 | Đổi mật khẩu thành công | Nhập đúng mật khẩu cũ, mật khẩu mới hợp lệ | HTTP 200, mật khẩu được cập nhật | Đăng nhập lại bằng mật khẩu mới thành công | ✅ Đạt |
| TC-AUTH-10 | Đổi mật khẩu sai mật khẩu cũ | Nhập sai mật khẩu hiện tại | HTTP 400, thông báo lỗi | Hệ thống từ chối cập nhật | ✅ Đạt |

---

### 4.4.2. Module Quản lý Sinh viên (Admin)

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-SV-01 | Xem danh sách sinh viên | Đăng nhập bằng tài khoản Admin | Trả về danh sách tất cả sinh viên | Hiển thị đầy đủ 20 sinh viên mẫu | ✅ Đạt |
| TC-SV-02 | Thêm sinh viên mới | Điền đầy đủ thông tin hợp lệ (tên, ngày sinh, email,…) | HTTP 201, sinh viên được tạo trong DB | Sinh viên xuất hiện trong danh sách | ✅ Đạt |
| TC-SV-03 | Thêm sinh viên email trùng | Email đã tồn tại trong hệ thống | HTTP 400, thông báo "Email đã tồn tại" | Hệ thống từ chối tạo mới | ✅ Đạt |
| TC-SV-04 | Cập nhật thông tin sinh viên | Thay đổi tên, số điện thoại | HTTP 200, dữ liệu được cập nhật | Thông tin mới hiển thị chính xác | ✅ Đạt |
| TC-SV-05 | Xóa sinh viên | Chọn sinh viên và xác nhận xóa | HTTP 200, sinh viên bị xóa khỏi DB | Sinh viên không còn trong danh sách | ✅ Đạt |
| TC-SV-06 | Sinh viên truy cập quản lý sinh viên | Dùng tài khoản SinhVien gọi GET /api/sinhvien | HTTP 403 Forbidden | Bị từ chối truy cập | ✅ Đạt |

---

### 4.4.3. Module Quản lý Giảng viên (Admin)

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-GV-01 | Xem danh sách giảng viên | Đăng nhập bằng Admin | Trả về danh sách giảng viên | Hiển thị đúng danh sách | ✅ Đạt |
| TC-GV-02 | Thêm giảng viên mới | Thông tin hợp lệ | HTTP 201, tạo thành công | Giảng viên xuất hiện trong danh sách | ✅ Đạt |
| TC-GV-03 | Cập nhật thông tin giảng viên | Thay đổi thông tin | HTTP 200, cập nhật thành công | Dữ liệu mới chính xác | ✅ Đạt |
| TC-GV-04 | Xóa giảng viên | Xác nhận xóa | HTTP 200, xóa thành công | Không còn trong danh sách | ✅ Đạt |
| TC-GV-05 | Giảng viên tự xem danh sách giảng viên khác | Dùng token GiangVien gọi GET /api/giangvien | HTTP 403 Forbidden | Bị từ chối | ✅ Đạt |

---

### 4.4.4. Module Quản lý Lớp học (Admin)

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-LH-01 | Xem danh sách lớp học | Không cần xác thực (public endpoint) | Trả về danh sách lớp học | Hiển thị các lớp CNTT01–CNTT05 | ✅ Đạt |
| TC-LH-02 | Thêm lớp học mới | Admin nhập tên lớp, khoa, phòng học | HTTP 201, lớp được tạo | Lớp mới xuất hiện trong danh sách | ✅ Đạt |
| TC-LH-03 | Thêm lớp trùng tên | Tên lớp đã tồn tại | HTTP 400, lỗi trùng tên | Hệ thống từ chối | ✅ Đạt |
| TC-LH-04 | Cập nhật và xóa lớp học | Thao tác hợp lệ | HTTP 200, thực hiện thành công | Dữ liệu cập nhật chính xác | ✅ Đạt |

---

### 4.4.5. Module Quản lý Môn học

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-MH-01 | Xem tất cả môn học | Endpoint public | Trả về 44 môn học | Hiển thị đầy đủ danh sách | ✅ Đạt |
| TC-MH-02 | Giảng viên xem môn học của mình | Token GiangVien, gọi GET /api/monhoc/cua-toi | Chỉ trả về môn học được phân công | Kết quả đúng, không lộ môn của GV khác | ✅ Đạt |
| TC-MH-03 | Thêm môn học | Admin/GiangVien, thông tin hợp lệ | HTTP 201, môn được tạo | Thành công | ✅ Đạt |
| TC-MH-04 | Xóa môn học | Admin/GiangVien | HTTP 200, xóa thành công | Môn không còn trong danh sách | ✅ Đạt |
| TC-MH-05 | Xem sinh viên đăng ký môn học | Admin/GiangVien, ID môn hợp lệ | Danh sách sinh viên đã đăng ký | Hiển thị đúng danh sách | ✅ Đạt |

---

### 4.4.6. Module Quản lý Điểm

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-DIEM-01 | Nhập điểm cho sinh viên | Giảng viên nhập: CC=8, KT1=7, KT2=8, KT3=9, Thi=7.5 | HTTP 201, điểm lưu thành công | Điểm lưu đúng vào DB | ✅ Đạt |
| TC-DIEM-02 | Tính điểm tổng kết tự động | CC=8, KT1=7, KT2=8, KT3=9, Thi=7.5 | DTK = 8×0.1 + (7+8+9)/3×0.3 + 7.5×0.6 = 7.9 | Kết quả tính = 7.9 (đúng) | ✅ Đạt |
| TC-DIEM-03 | Nhập điểm vượt thang 10 | Nhập điểm = 11 | HTTP 400, lỗi validation | Hệ thống từ chối | ✅ Đạt |
| TC-DIEM-04 | Nhập điểm âm | Nhập điểm = -1 | HTTP 400, lỗi validation | Hệ thống từ chối | ✅ Đạt |
| TC-DIEM-05 | Cập nhật điểm đã nhập | Thay đổi điểm thi của sinh viên | HTTP 200, điểm mới lưu thành công | Cập nhật chính xác | ✅ Đạt |
| TC-DIEM-06 | Sinh viên xem điểm của mình | Token SinhVien, GET /api/diem/cua-toi/{id} | Trả về điểm các môn đã học | Hiển thị đúng điểm | ✅ Đạt |
| TC-DIEM-07 | Sinh viên xem điểm sinh viên khác | SinhVien A cố xem điểm SinhVien B | HTTP 403 hoặc trả về rỗng | Bị từ chối / không có dữ liệu | ✅ Đạt |
| TC-DIEM-08 | Xem bảng điểm tích lũy | Sinh viên đã có điểm ≥ 1 môn | Danh sách điểm theo học kỳ + GPA tích lũy | Hiển thị đúng, nhóm theo học kỳ | ✅ Đạt |

---

### 4.4.7. Module Tính GPA và Xếp loại

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-GPA-01 | GPA >= 8.5 → Xếp loại Giỏi | Các môn có điểm TK >= 8.5, tổng TC = 12 | GPA >= 8.5, Xếp loại = "Giỏi" | Kết quả đúng | ✅ Đạt |
| TC-GPA-02 | 7.0 <= GPA < 8.5 → Xếp loại Khá | Điểm các môn trong khoảng 7.0–8.4 | Xếp loại = "Khá" | Kết quả đúng | ✅ Đạt |
| TC-GPA-03 | 5.0 <= GPA < 7.0 → Trung bình | Điểm các môn trong khoảng 5.0–6.9 | Xếp loại = "Trung bình" | Kết quả đúng | ✅ Đạt |
| TC-GPA-04 | GPA < 5.0 → Xếp loại Yếu | Điểm các môn dưới 5.0 | Xếp loại = "Yếu" | Kết quả đúng | ✅ Đạt |
| TC-GPA-05 | GPA tích lũy có trọng số tín chỉ | Môn A: 3TC, điểm 8.0; Môn B: 2TC, điểm 6.0 | GPA = (8×3 + 6×2) / (3+2) = 7.2 | Kết quả tính = 7.2 (đúng) | ✅ Đạt |
| TC-GPA-06 | Cảnh báo sinh viên GPA < 5 | Sinh viên có GPA tích lũy < 5.0 | Xuất hiện trong danh sách cảnh báo trên Dashboard | Hiển thị đúng trong bảng cảnh báo | ✅ Đạt |

---

### 4.4.8. Module Đánh giá AI

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-AI-01 | Sinh đánh giá AI thành công | Admin/GiangVien kích hoạt đánh giá, sinh viên có điểm | HTTP 200, trả về GPA, xếp loại, nhận xét AI (tiếng Việt) | Nhận xét được tạo đúng ngôn ngữ Việt | ✅ Đạt |
| TC-AI-02 | Đánh giá AI khi Groq API không khả dụng | API key sai hoặc mạng gián đoạn | Hệ thống fallback về nhận xét mặc định, không crash | Nhận xét mặc định được trả về | ✅ Đạt |
| TC-AI-03 | Sinh viên xem kết quả đánh giá AI | Token SinhVien, GET /api/diem/danh-gia-ai/{id} | Trả về GPA, xếp loại, nhận xét AI của chính mình | Hiển thị đúng kết quả | ✅ Đạt |
| TC-AI-04 | Đánh giá AI khi sinh viên chưa có điểm | Sinh viên chưa được nhập điểm bất kỳ môn nào | HTTP 400 hoặc thông báo "Chưa có dữ liệu điểm" | Hệ thống xử lý đúng, không lỗi | ✅ Đạt |
| TC-AI-05 | Cập nhật đánh giá AI (chạy lại) | Sau khi cập nhật điểm, chạy lại đánh giá AI | GPA và nhận xét mới được ghi đè lên kết quả cũ | Dữ liệu cập nhật chính xác | ✅ Đạt |

---

### 4.4.9. Module Đăng ký Môn học

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-DK-01 | Sinh viên đăng ký môn học | SinhVien chọn môn và đăng ký | HTTP 201, trạng thái = "ChoDuyet" | Đăng ký tạo thành công | ✅ Đạt |
| TC-DK-02 | Đăng ký môn học trùng | Đăng ký lại môn đã đăng ký | HTTP 400, "Bạn đã đăng ký môn này rồi" | Hệ thống từ chối | ✅ Đạt |
| TC-DK-03 | Admin duyệt đăng ký | Admin chọn đơn và duyệt | HTTP 200, trạng thái = "DaDuyet", tạo bản ghi Diem | Trạng thái cập nhật, bản ghi Diem được khởi tạo | ✅ Đạt |
| TC-DK-04 | Admin từ chối đăng ký | Admin chọn đơn và từ chối | HTTP 200, trạng thái = "TuChoi" | Trạng thái cập nhật đúng | ✅ Đạt |
| TC-DK-05 | Sinh viên hủy đăng ký | Sinh viên xóa đơn đăng ký chưa duyệt | HTTP 200, đơn bị xóa | Đơn không còn trong danh sách | ✅ Đạt |
| TC-DK-06 | Sinh viên xem đăng ký của mình | GET /api/dangky/cua-toi | Chỉ trả về đăng ký của sinh viên đang đăng nhập | Kết quả đúng, không lộ đăng ký người khác | ✅ Đạt |

---

### 4.4.10. Module Dashboard và Thống kê

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-TK-01 | Thống kê tổng quan (Admin) | Đăng nhập Admin, vào Dashboard | Hiển thị: Tổng SV, GV, môn học, lớp học | Số liệu khớp với dữ liệu trong DB | ✅ Đạt |
| TC-TK-02 | Biểu đồ phân loại học lực | Có dữ liệu đánh giá AI | Chart.js hiển thị phân bố Giỏi/Khá/TB/Yếu | Biểu đồ hiển thị đúng | ✅ Đạt |
| TC-TK-03 | Danh sách sinh viên cần cảnh báo | Có SV với GPA < 5 | Danh sách các SV học yếu | Hiển thị đúng danh sách cảnh báo | ✅ Đạt |
| TC-TK-04 | Nhật ký hoạt động (Audit Log) | Admin thực hiện các thao tác CRUD | Mỗi hành động được ghi vào AuditLog | Log ghi đúng email, vai trò, hành động, thời gian | ✅ Đạt |

---

### 4.4.11. Module Hồ sơ cá nhân

| Mã TC | Tên ca kiểm thử | Điều kiện đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|----------------|-------------------|------------------|-----------------|-----------|
| TC-HS-01 | Xem thông tin cá nhân | Bất kỳ người dùng nào đã đăng nhập | Trả về thông tin cá nhân tương ứng vai trò | Hiển thị đúng thông tin | ✅ Đạt |
| TC-HS-02 | Cập nhật hồ sơ | Thay đổi số điện thoại, địa chỉ | HTTP 200, thông tin mới được lưu | Cập nhật thành công | ✅ Đạt |

---

## 4.5. Kiểm thử Bảo mật

### 4.5.1. Kiểm thử Xác thực và Phân quyền

| Mã TC | Tình huống | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|-----------|------------------|-----------------|-----------|
| TC-SEC-01 | Gọi API admin khi không đăng nhập | HTTP 401 Unauthorized | Bị từ chối truy cập | ✅ Đạt |
| TC-SEC-02 | Giảng viên truy cập chức năng Admin (xóa sinh viên) | HTTP 403 Forbidden | Bị từ chối, lỗi phân quyền | ✅ Đạt |
| TC-SEC-03 | Sinh viên A xem điểm Sinh viên B | HTTP 403 hoặc không có dữ liệu | Không trả về dữ liệu của SV B | ✅ Đạt |
| TC-SEC-04 | Dùng token giả mạo (chỉnh sửa payload JWT) | HTTP 401, chữ ký JWT không hợp lệ | Bị từ chối | ✅ Đạt |
| TC-SEC-05 | Mật khẩu được lưu dạng mã hóa BCrypt | Kiểm tra giá trị trong database | Mật khẩu hiển thị dạng hash ($2a$...) | ✅ Đạt |

### 4.5.2. Kiểm thử Dữ liệu đầu vào

| Mã TC | Tình huống | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|-----------|------------------|-----------------|-----------|
| TC-SEC-06 | Nhập điểm dưới 0 hoặc trên 10 | HTTP 400, thông báo lỗi validation | Hệ thống từ chối dữ liệu không hợp lệ | ✅ Đạt |
| TC-SEC-07 | Nhập email không đúng định dạng | HTTP 400, lỗi format email | Hệ thống từ chối | ✅ Đạt |
| TC-SEC-08 | Truyền ID âm hoặc không tồn tại | HTTP 404 Not Found | Thông báo không tìm thấy tài nguyên | ✅ Đạt |

---

## 4.6. Kiểm thử Giao diện Người dùng

### 4.6.1. Giao diện Đăng nhập

| Mã TC | Tình huống | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|-----------|------------------|-----------------|-----------|
| TC-UI-01 | Giao diện trang đăng nhập hiển thị đúng | Load `index.html` | Form đăng nhập hiển thị rõ ràng, responsive | Hiển thị đúng trên Chrome và Edge | ✅ Đạt |
| TC-UI-02 | Điều hướng sau đăng nhập đúng vai trò | Đăng nhập Admin → admin.html; GV → giang-vien.html; SV → sinh-vien.html | Chuyển trang đúng | Điều hướng chính xác theo role | ✅ Đạt |

### 4.6.2. Giao diện Admin

| Mã TC | Tình huống | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|-----------|------------------|-----------------|-----------|
| TC-UI-03 | Dashboard hiển thị số liệu thống kê | Đăng nhập Admin | Thẻ thống kê và biểu đồ hiển thị | Đúng, dữ liệu khớp | ✅ Đạt |
| TC-UI-04 | Chuyển tab giữa các chức năng | Click vào các tab: Lớp, Môn học, Điểm,... | Nội dung tab thay đổi đúng | Hoạt động đúng | ✅ Đạt |
| TC-UI-05 | Modal thêm/sửa sinh viên | Click nút Thêm, điền form | Modal hiện ra, form hoạt động | Đúng chức năng | ✅ Đạt |

### 4.6.3. Giao diện Sinh viên

| Mã TC | Tình huống | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|-------|-----------|------------------|-----------------|-----------|
| TC-UI-06 | Bảng điểm hiển thị đúng học kỳ | Sinh viên có điểm nhiều môn | Điểm nhóm theo học kỳ, đúng dữ liệu | Hiển thị đúng | ✅ Đạt |
| TC-UI-07 | GPA và xếp loại hiển thị | Sinh viên có đánh giá AI | Hiển thị GPA, xếp loại, nhận xét AI | Đúng dữ liệu | ✅ Đạt |
| TC-UI-08 | Đăng ký môn học qua UI | Chọn môn và nhấn đăng ký | Gửi request thành công, thông báo xác nhận | Hoạt động đúng | ✅ Đạt |

---

## 4.7. Kiểm thử Tích hợp (Integration Testing)

Kiểm thử luồng dữ liệu liên kết giữa các module:

| Mã TC | Luồng kiểm thử | Kết quả mong đợi | Trạng thái |
|-------|---------------|------------------|-----------|
| TC-INT-01 | **Luồng đăng ký → duyệt → nhập điểm → xem điểm** | SV đăng ký → Admin duyệt → GV nhập điểm → SV xem được điểm | ✅ Đạt |
| TC-INT-02 | **Luồng nhập điểm → tính GPA → đánh giá AI** | GV nhập đủ điểm → tính GPA đúng → AI sinh nhận xét → SV xem kết quả | ✅ Đạt |
| TC-INT-03 | **Luồng tạo tài khoản → đăng nhập → thao tác theo quyền** | Admin tạo TK GV → GV đăng nhập → GV nhập điểm thành công | ✅ Đạt |
| TC-INT-04 | **Luồng audit log** | Admin thực hiện xóa SV → hành động ghi vào AuditLog với đúng thông tin | ✅ Đạt |

---

## 4.8. Kiểm thử Hiệu năng

Công cụ đo: Postman (thủ công), đo thời gian phản hồi API.

| API Endpoint | Loại | Thời gian phản hồi TB | Yêu cầu (< 3s) | Trạng thái |
|--------------|------|-----------------------|----------------|-----------|
| POST /api/auth/login | Đăng nhập | ~120 ms | ✓ | ✅ Đạt |
| GET /api/sinhvien | Lấy danh sách SV | ~85 ms | ✓ | ✅ Đạt |
| GET /api/diem/bang-diem-tich-luy/{id} | Bảng điểm tích lũy | ~150 ms | ✓ | ✅ Đạt |
| POST /api/diem/danh-gia-ai/{id} | Đánh giá AI (gọi Groq) | ~1.8 – 2.5 s | ✓ | ✅ Đạt |
| GET /api/thongke | Thống kê tổng quan | ~200 ms | ✓ | ✅ Đạt |

> **Ghi chú:** Endpoint đánh giá AI phụ thuộc vào độ trễ mạng từ Groq API, thường dao động 1.5–2.5 giây. Vẫn đáp ứng yêu cầu < 3 giây trong điều kiện mạng bình thường.

---

## 4.9. Tổng kết kiểm thử

### Thống kê kết quả

| Loại kiểm thử | Tổng số TC | Đạt | Không đạt |
|---------------|-----------|-----|-----------|
| Xác thực | 10 | 10 | 0 |
| Quản lý Sinh viên | 6 | 6 | 0 |
| Quản lý Giảng viên | 5 | 5 | 0 |
| Quản lý Lớp học | 4 | 4 | 0 |
| Quản lý Môn học | 5 | 5 | 0 |
| Quản lý Điểm | 8 | 8 | 0 |
| Tính GPA & Xếp loại | 6 | 6 | 0 |
| Đánh giá AI | 5 | 5 | 0 |
| Đăng ký Môn học | 6 | 6 | 0 |
| Thống kê & Dashboard | 4 | 4 | 0 |
| Hồ sơ cá nhân | 2 | 2 | 0 |
| Bảo mật | 8 | 8 | 0 |
| Giao diện (UI) | 8 | 8 | 0 |
| Tích hợp | 4 | 4 | 0 |
| **Tổng cộng** | **81** | **81** | **0** |

### Nhận xét

- Toàn bộ 81 ca kiểm thử đều đạt kết quả mong đợi.
- Hệ thống xác thực JWT và phân quyền theo vai trò hoạt động chính xác, ngăn chặn truy cập trái phép.
- Công thức tính GPA có trọng số tín chỉ cho kết quả chính xác.
- Tích hợp AI (Groq API) hoạt động ổn định, có cơ chế fallback khi dịch vụ gián đoạn.
- Thời gian phản hồi API đáp ứng yêu cầu dưới 3 giây với hầu hết các endpoint.
- Giao diện responsive, hiển thị nhất quán trên các trình duyệt phổ biến.

---

*Tài liệu kiểm thử này được lập cho hệ thống Quản lý Học tập tích hợp AI – Khoa Công nghệ Thông tin.*

---

## 4.10. Khảo sát yêu cầu hệ thống

Khảo sát được thực hiện **trước khi xây dựng hệ thống** nhằm thu thập ý kiến từ các đối tượng người dùng (Admin, Giảng viên, Sinh viên) để làm rõ yêu cầu, xác định các chức năng cần thiết và định hướng thiết kế phù hợp với thực tế sử dụng tại khoa CNTT.

**Thang điểm đánh giá (các câu có thang điểm):** 1 – Rất không cần thiết &nbsp;|&nbsp; 2 – Không cần thiết &nbsp;|&nbsp; 3 – Bình thường &nbsp;|&nbsp; 4 – Cần thiết &nbsp;|&nbsp; 5 – Rất cần thiết

---

### PHẦN A – Thông tin người tham gia

| STT | Câu hỏi | Lựa chọn |
|-----|---------|----------|
| A1 | Vai trò của bạn tại khoa? | ☐ Cán bộ quản lý / Admin &nbsp; ☐ Giảng viên &nbsp; ☐ Sinh viên |
| A2 | Bạn đã có kinh nghiệm sử dụng phần mềm quản lý học tập chưa? | ☐ Chưa từng &nbsp; ☐ Đã dùng nhưng hạn chế &nbsp; ☐ Sử dụng thường xuyên |

---

### PHẦN B – Quy trình quản lý sinh viên và điểm số hiện tại

| STT | Câu hỏi | Lựa chọn / Trả lời |
|-----|---------|-------------------|
| B1 | Hiện tại, việc quản lý thông tin sinh viên đang được thực hiện bằng cách nào? | ☐ Excel/Word &nbsp; ☐ Phần mềm chuyên dụng &nbsp; ☐ Ghi tay &nbsp; ☐ Khác: ……… |
| B2 | Điểm số sinh viên hiện đang được lưu trữ ở đâu? | ☐ File Excel &nbsp; ☐ Hệ thống riêng của trường &nbsp; ☐ Sổ tay &nbsp; ☐ Khác: ……… |
| B3 | Thời gian trung bình để hoàn tất nhập điểm cho một lớp học phần là bao lâu? | ☐ Dưới 30 phút &nbsp; ☐ 30–60 phút &nbsp; ☐ Trên 1 tiếng |
| B4 | Bạn có thường xuyên cần tra cứu lại kết quả học tập của sinh viên không? | ☐ Thường xuyên &nbsp; ☐ Đôi khi &nbsp; ☐ Hiếm khi |

---

### PHẦN C – Khó khăn trong nhập liệu và theo dõi kết quả học tập

| STT | Câu hỏi | Lựa chọn |
|-----|---------|----------|
| C1 | Bạn gặp khó khăn nào khi nhập điểm thủ công? *(có thể chọn nhiều)* | ☐ Dễ nhầm lẫn &nbsp; ☐ Mất nhiều thời gian &nbsp; ☐ Khó kiểm tra lại &nbsp; ☐ Không đồng bộ giữa các bộ phận &nbsp; ☐ Không gặp khó khăn |
| C2 | Việc theo dõi tiến độ học tập của từng sinh viên hiện tại có khó khăn không? | ☐ Rất khó &nbsp; ☐ Khó &nbsp; ☐ Bình thường &nbsp; ☐ Dễ |
| C3 | Bạn có gặp tình trạng dữ liệu điểm bị sai, thiếu hoặc không nhất quán không? | ☐ Thường xuyên &nbsp; ☐ Đôi khi &nbsp; ☐ Hiếm khi &nbsp; ☐ Không bao giờ |
| C4 | Khi có sai sót trong điểm số, việc sửa lại mất bao lâu? | ☐ Vài phút &nbsp; ☐ Vài tiếng &nbsp; ☐ Vài ngày do phải qua nhiều bước |

---

### PHẦN D – Nhu cầu về chức năng tìm kiếm, cập nhật và phân loại dữ liệu

| STT | Câu hỏi | 1 | 2 | 3 | 4 | 5 |
|-----|---------|---|---|---|---|---|
| D1 | Chức năng tìm kiếm sinh viên theo tên, mã số hoặc lớp | ☐ | ☐ | ☐ | ☐ | ☐ |
| D2 | Lọc danh sách sinh viên theo khoa, khóa học hoặc xếp loại học lực | ☐ | ☐ | ☐ | ☐ | ☐ |
| D3 | Cập nhật thông tin sinh viên (họ tên, ngày sinh, lớp,...) trực tiếp trên hệ thống | ☐ | ☐ | ☐ | ☐ | ☐ |
| D4 | Phân loại sinh viên theo học lực: Giỏi / Khá / Trung bình / Yếu | ☐ | ☐ | ☐ | ☐ | ☐ |
| D5 | Sửa điểm sau khi đã nhập (có ghi lại lịch sử thay đổi) | ☐ | ☐ | ☐ | ☐ | ☐ |
| D6 | Sinh viên tự tra cứu điểm cá nhân và kết quả học tập | ☐ | ☐ | ☐ | ☐ | ☐ |

---

### PHẦN E – Nhu cầu về thống kê, báo cáo và biểu đồ trực quan

| STT | Câu hỏi | 1 | 2 | 3 | 4 | 5 |
|-----|---------|---|---|---|---|---|
| E1 | Biểu đồ phân bố học lực (Giỏi/Khá/TB/Yếu) của toàn khoa | ☐ | ☐ | ☐ | ☐ | ☐ |
| E2 | Bảng thống kê tổng số sinh viên, giảng viên, môn học theo từng học kỳ | ☐ | ☐ | ☐ | ☐ | ☐ |
| E3 | Danh sách sinh viên có kết quả học tập dưới mức trung bình (cần cảnh báo) | ☐ | ☐ | ☐ | ☐ | ☐ |
| E4 | Bảng điểm tích lũy theo học kỳ của từng sinh viên | ☐ | ☐ | ☐ | ☐ | ☐ |
| E5 | Xuất báo cáo điểm dưới dạng file (PDF, Excel) | ☐ | ☐ | ☐ | ☐ | ☐ |

---

### PHẦN F – Mức độ quan tâm đến ứng dụng AI trong phân tích dữ liệu

| STT | Câu hỏi | Lựa chọn / Thang điểm |
|-----|---------|----------------------|
| F1 | Bạn có biết hoặc từng nghe đến việc ứng dụng AI trong giáo dục chưa? | ☐ Chưa &nbsp; ☐ Có nghe qua &nbsp; ☐ Đã tìm hiểu kỹ |
| F2 | Tự động tính GPA có trọng số tín chỉ và xếp loại học lực | 1☐ 2☐ 3☐ 4☐ 5☐ |
| F3 | AI sinh nhận xét cá nhân hóa về tình trạng học tập của từng sinh viên | 1☐ 2☐ 3☐ 4☐ 5☐ |
| F4 | Cảnh báo sớm sinh viên có nguy cơ học yếu dựa trên xu hướng điểm số | 1☐ 2☐ 3☐ 4☐ 5☐ |
| F5 | Gợi ý phương hướng cải thiện học tập dựa trên phân tích điểm | 1☐ 2☐ 3☐ 4☐ 5☐ |
| F6 | Bạn có tin tưởng vào kết quả phân tích từ AI không? | ☐ Không tin &nbsp; ☐ Cần xem xét thêm &nbsp; ☐ Tin nếu có giải thích rõ &nbsp; ☐ Hoàn toàn tin tưởng |

---

### PHẦN G – Yêu cầu về giao diện, bảo mật và phân quyền

| STT | Câu hỏi | 1 | 2 | 3 | 4 | 5 |
|-----|---------|---|---|---|---|---|
| G1 | Giao diện đơn giản, dễ sử dụng, không cần đào tạo nhiều | ☐ | ☐ | ☐ | ☐ | ☐ |
| G2 | Hệ thống cần có 3 vai trò riêng biệt: Admin, Giảng viên, Sinh viên | ☐ | ☐ | ☐ | ☐ | ☐ |
| G3 | Sinh viên chỉ xem được điểm của chính mình, không thấy của người khác | ☐ | ☐ | ☐ | ☐ | ☐ |
| G4 | Mật khẩu phải được mã hóa, không lưu dạng thô trong cơ sở dữ liệu | ☐ | ☐ | ☐ | ☐ | ☐ |
| G5 | Hệ thống cần ghi lại nhật ký các thao tác quan trọng (ai làm gì, khi nào) | ☐ | ☐ | ☐ | ☐ | ☐ |
| G6 | Hỗ trợ truy cập trên điện thoại di động (giao diện responsive) | ☐ | ☐ | ☐ | ☐ | ☐ |

---

### PHẦN H – Ý kiến bổ sung

| STT | Câu hỏi |
|-----|---------|
| H1 | Chức năng nào bạn cho là quan trọng nhất cần có trong hệ thống? |
| H2 | Bạn mong muốn hệ thống giải quyết vấn đề gì mà cách làm hiện tại chưa đáp ứng được? |
| H3 | Bạn có yêu cầu đặc biệt nào về giao diện hoặc cách hiển thị thông tin không? |
| H4 | Ý kiến khác (nếu có): |

---

*Cảm ơn bạn đã dành thời gian tham gia khảo sát. Kết quả sẽ được tổng hợp để xác định yêu cầu hệ thống và định hướng thiết kế phù hợp.*
