-- =======================================================
-- MIGRATION: Thêm cột HocKy + cập nhật danh sách môn học
-- Chạy file này nếu database quanlydiem đã tồn tại
-- Nếu DB chưa tồn tại → bỏ qua, hệ thống tự tạo khi khởi động
-- =======================================================

USE quanlydiem;

-- Bước 1: Thêm cột HocKy nếu chưa có
ALTER TABLE MonHocs
  ADD COLUMN IF NOT EXISTS HocKy INT NOT NULL DEFAULT 1;

-- Bước 2: Xóa môn học cũ (nếu muốn bắt đầu sạch với danh sách mới)
-- ** Chú ý: Chỉ chạy 2 dòng dưới nếu bảng Diems không có dữ liệu quan trọng **
-- SET FOREIGN_KEY_CHECKS = 0;
-- TRUNCATE TABLE MonHocs;
-- SET FOREIGN_KEY_CHECKS = 1;

-- Bước 3: Thêm môn học theo đúng chương trình CNTT (8 học kỳ)
-- Dùng INSERT IGNORE để không bị lỗi nếu chạy lại
INSERT IGNORE INTO MonHocs (SubjectName, Credits, Department, HocKy) VALUES
  -- ===== HỌC KỲ 1 (Năm 1 - HK1) =====
  ('Tin học đại cương',            3, 'CNTT',               1),
  ('Toán cao cấp',                 4, 'Toán',               1),
  ('Vật lý',                       3, 'Khoa học tự nhiên',  1),
  ('Anh văn 1',                    3, 'Ngoại ngữ',          1),
  ('Giáo dục thể chất 1',          1, 'Thể dục',            1),
  ('Triết học Mác - Lênin',        3, 'Lý luận chính trị',  1),
  -- ===== HỌC KỲ 2 (Năm 1 - HK2) =====
  ('Toán rời rạc',                 3, 'Toán',               2),
  ('Anh văn 2',                    3, 'Ngoại ngữ',          2),
  ('Kỹ thuật lập trình',           3, 'CNTT',               2),
  ('Giáo dục thể chất 2',          1, 'Thể dục',            2),
  ('Lịch sử Đảng Cộng sản Việt Nam', 2, 'Lý luận chính trị', 2),
  -- ===== HỌC KỲ 3 (Năm 2 - HK1) =====
  ('Cấu trúc dữ liệu và giải thuật', 3, 'CNTT',              3),
  ('Cơ sở dữ liệu',                3, 'CNTT',               3),
  ('Phương pháp luận lập trình',   2, 'CNTT',               3),
  ('Xác suất thống kê',            3, 'Toán',               3),
  ('Kinh tế chính trị Mác - Lênin', 3, 'Lý luận chính trị', 3),
  -- ===== HỌC KỲ 4 (Năm 2 - HK2) =====
  ('Lập trình hướng đối tượng',    3, 'CNTT',               4),
  ('Cấu trúc máy tính và Hệ điều hành', 3, 'CNTT',          4),
  ('Mạng máy tính',                3, 'CNTT',               4),
  ('Nhập môn công nghệ phần mềm',  3, 'CNTT',               4),
  ('Tư tưởng Hồ Chí Minh',        2, 'Lý luận chính trị',  4),
  ('Chủ nghĩa xã hội khoa học',   2, 'Lý luận chính trị',  4),
  -- ===== HỌC KỲ 5 (Năm 3 - HK1) =====
  ('Phân tích thiết kế thuật toán', 3, 'CNTT',              5),
  ('Lập trình ứng dụng Java',      3, 'CNTT',               5),
  ('Thiết kế web',                 3, 'CNTT',               5),
  ('Phân tích và quản lý yêu cầu phần mềm', 3, 'CNTT',     5),
  -- ===== HỌC KỲ 6 (Năm 3 - HK2) =====
  ('Phương pháp phát triển phần mềm hướng đối tượng', 3, 'CNTT', 6),
  ('Công nghệ .Net',               3, 'CNTT',               6),
  ('Công nghệ ASP.NET',            3, 'CNTT',               6),
  ('Phân tích thiết kế hệ thống',  3, 'CNTT',               6),
  ('Quản lý dự án CNTT',           3, 'CNTT',               6),
  -- ===== HỌC KỲ 7 (Năm 4 - HK1) =====
  ('An toàn thông tin',            3, 'CNTT',               7),
  ('Trí tuệ nhân tạo',             3, 'CNTT',               7),
  ('Kiến trúc và thiết kế phần mềm', 3, 'CNTT',            7),
  ('Lập trình cho thiết bị di động', 3, 'CNTT',             7),
  ('Kiểm thử và đảm bảo chất lượng phần mềm', 3, 'CNTT',   7),
  -- ===== HỌC KỲ 8 (Năm 4 - HK2) =====
  ('Vận hành và bảo trì phần mềm', 3, 'CNTT',               8),
  ('Thực tập tốt nghiệp',          4, 'CNTT',               8);
