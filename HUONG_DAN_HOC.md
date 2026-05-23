# Tài liệu học: Hệ thống Quản lý Điểm (QuanlyDiem)

> Đây là tài liệu giải thích toàn bộ dự án từ A → Z: cấu trúc thư mục, từng file HTML/CSS/JS/C# làm gì, tại sao viết vậy, và cách mọi thứ kết nối với nhau.

---

## MỤC LỤC

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Cấu trúc thư mục](#2-cấu-trúc-thư-mục)
3. [Phần HTML — Giao diện người dùng](#3-phần-html--giao-diện-người-dùng)
4. [Phần CSS — Style.css](#4-phần-css--stylecss)
5. [Phần JavaScript — api.js](#5-phần-javascript--apijs)
6. [Phần JavaScript — admin.js](#6-phần-javascript--adminjs)
7. [Phần Backend C# — Models (Mô hình dữ liệu)](#7-phần-backend-c--models-mô-hình-dữ-liệu)
8. [Phần Backend C# — AppDbContext (Kết nối DB)](#8-phần-backend-c--appdbcontext-kết-nối-db)
9. [Phần Backend C# — Services (Dịch vụ)](#9-phần-backend-c--services-dịch-vụ)
10. [Phần Backend C# — Controllers (Xử lý API)](#10-phần-backend-c--controllers-xử-lý-api)
11. [Phần Backend C# — Program.cs (Khởi động ứng dụng)](#11-phần-backend-c--programcs-khởi-động-ứng-dụng)
12. [Luồng dữ liệu — Ví dụ thực tế](#12-luồng-dữ-liệu--ví-dụ-thực-tế)
13. [Bảng tổng hợp API Endpoints](#13-bảng-tổng-hợp-api-endpoints)

---

## 1. Tổng quan kiến trúc

Dự án này dùng mô hình **Client–Server**:

```
TRÌNH DUYỆT (Client)                    SERVER (ASP.NET Core)
┌──────────────────────┐                ┌──────────────────────┐
│  HTML + CSS + JS     │  ←── HTTP ──→  │  Controllers (C#)    │
│  (wwwroot/)          │                │  Services (C#)       │
│                      │                │  Models (C#)         │
│  - index.html        │                │  AppDbContext (EF)   │
│  - admin.html        │                └──────────┬───────────┘
│  - sinh-vien.html    │                           │
│  - giang-vien.html   │                    MySQL Database
│  - style.css         │                  (bảng dữ liệu)
│  - api.js            │
│  - admin.js          │
└──────────────────────┘
```

**Luồng hoạt động đơn giản:**
1. Người dùng mở trình duyệt → tải file HTML
2. HTML dùng JavaScript gọi API (fetch)
3. ASP.NET Core nhận request, xử lý, truy vấn MySQL
4. Trả kết quả JSON về cho JavaScript
5. JavaScript hiển thị lên HTML

---

## 2. Cấu trúc thư mục

```
QuanlyDiem/
│
├── CLAUDE.md                    ← Hướng dẫn dự án (đã có)
├── HUONG_DAN_HOC.md             ← File này (tài liệu học)
├── migration_add_hocky.sql      ← Script thêm cột HocKy vào DB
│
└── QuanlyDiemAPI/               ← Toàn bộ code ASP.NET Core
    │
    ├── Program.cs               ← ĐIỂM KHỞI ĐỘNG của ứng dụng
    │
    ├── Data/
    │   └── AppDbContext.cs      ← Kết nối với MySQL, định nghĩa bảng
    │
    ├── Models/                  ← Các lớp đại diện cho bảng DB
    │   ├── NguoiDung.cs         ← Bảng tài khoản đăng nhập
    │   ├── SinhVien.cs          ← Bảng sinh viên
    │   ├── GiangVien.cs         ← Bảng giảng viên
    │   ├── LopHoc.cs            ← Bảng lớp học
    │   ├── MonHoc.cs            ← Bảng môn học
    │   ├── Diem.cs              ← Bảng điểm
    │   └── DanhgiaAI.cs         ← Bảng đánh giá AI
    │
    ├── DTOs/                    ← Lớp truyền dữ liệu (nhận/gửi)
    │   ├── AuthDtos.cs
    │   ├── SinhVienDtos.cs
    │   ├── GiangVienDtos.cs
    │   ├── LopHocDtos.cs
    │   ├── MonHocDtos.cs
    │   ├── DiemDtos.cs
    │   └── TaiKhoanDtos.cs
    │
    ├── Services/                ← Logic nghiệp vụ
    │   ├── JwtService.cs        ← Tạo token đăng nhập
    │   ├── GpaService.cs        ← Tính điểm, GPA, xếp loại
    │   └── AiService.cs         ← Gọi Gemini AI sinh nhận xét
    │
    ├── Controllers/             ← Xử lý request HTTP
    │   ├── AuthController.cs    ← Đăng nhập / đổi mật khẩu
    │   ├── SinhVienController.cs
    │   ├── GiangVienController.cs
    │   ├── LopHocController.cs
    │   ├── MonHocController.cs
    │   ├── DiemController.cs    ← Nhập/xem điểm, đánh giá AI
    │   ├── ProfileController.cs ← Cập nhật hồ sơ cá nhân
    │   ├── TaiKhoanController.cs← Quản lý tài khoản
    │   └── ThongKeController.cs ← Thống kê dashboard
    │
    └── wwwroot/                 ← File tĩnh phục vụ cho trình duyệt
        ├── index.html           ← Trang đăng nhập
        ├── css/
        │   └── style.css        ← Toàn bộ CSS
        ├── js/
        │   ├── api.js           ← Thư viện dùng chung (gọi API, validate…)
        │   └── admin.js         ← Logic riêng cho trang admin
        └── pages/
            ├── admin.html       ← Dashboard Admin
            ├── sinh-vien.html   ← Trang Sinh Viên
            └── giang-vien.html  ← Trang Giảng Viên
```

---

## 3. Phần HTML — Giao diện người dùng

### 3.1 index.html — Trang đăng nhập

**Mục đích:** Trang đầu tiên người dùng thấy. Nhập email + mật khẩu → đăng nhập.

**Cấu trúc HTML chính:**
```html
<div class="auth-container">           <!-- Hộp trắng giữa màn hình -->
  <div class="auth-card">
    <h1>Quản lý Điểm</h1>
    <form id="loginForm">
      <input type="email" id="email" />
      <input type="password" id="password" />
      <button type="submit">Đăng nhập</button>
    </form>
    <div id="errorMsg"></div>           <!-- Hiện lỗi nếu sai mật khẩu -->
  </div>
</div>
```

**JavaScript trong file (inline):**
```javascript
// Khi trang load xong, xóa input để tránh trình duyệt tự điền
window.onload = () => {
    document.getElementById('email').value = '';
    document.getElementById('password').value = '';
};

// Khi nhấn Đăng nhập
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();  // Ngăn trang reload
    
    // Gọi API POST /api/auth/login
    const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, matKhau: password })
    });
    
    const data = await res.json();
    
    // Lưu token vào localStorage để dùng lại
    localStorage.setItem('token', data.token);
    localStorage.setItem('role', data.role);
    localStorage.setItem('userId', data.userId);
    localStorage.setItem('profileId', data.profileId);
    
    // Chuyển trang theo vai trò
    if (data.role === 'Admin')     → /pages/admin.html
    if (data.role === 'GiangVien') → /pages/giang-vien.html
    if (data.role === 'SinhVien')  → /pages/sinh-vien.html
});
```

**Tại sao dùng localStorage?**
Vì khi chuyển sang trang khác (admin.html), trang mới cần biết token để gọi API. localStorage lưu dữ liệu trên trình duyệt, không mất khi chuyển trang.

---

### 3.2 admin.html — Dashboard Admin

**Mục đích:** Admin quản lý toàn bộ hệ thống: sinh viên, giảng viên, lớp, môn, điểm, tài khoản, AI.

**Cấu trúc layout:**
```html
<body>
  <div class="layout">

    <!-- Sidebar bên trái -->
    <div class="sidebar">
      <div class="sidebar-logo">QuanlyDiem</div>
      <nav>
        <a data-tab="dashboard">Dashboard</a>
        <a data-tab="lophoc">Lớp học</a>
        <a data-tab="monhoc">Môn học</a>
        <a data-tab="diem">Điểm</a>
        <a data-tab="taikhoan">Tài khoản</a>
        <a data-tab="danhgiai">Đánh giá AI</a>
      </nav>
    </div>

    <!-- Nội dung chính bên phải -->
    <div class="main-content">
      
      <!-- Topbar: hiện tên user, nút đổi mật khẩu, đăng xuất -->
      <div class="topbar">...</div>

      <!-- Các tab nội dung (chỉ 1 tab hiện tại một thời điểm) -->
      <div id="tab-dashboard" class="tab-content active">...</div>
      <div id="tab-lophoc"    class="tab-content">...</div>
      <div id="tab-monhoc"    class="tab-content">...</div>
      <div id="tab-diem"      class="tab-content">...</div>
      <div id="tab-taikhoan"  class="tab-content">...</div>
      <div id="tab-danhgiai"  class="tab-content">...</div>

    </div>
  </div>

  <!-- Các Modal (popup) -->
  <div id="modalLopHoc" class="modal">...</div>
  <div id="modalMonHoc" class="modal">...</div>
  <!-- ... -->
</body>
```

**Tab Dashboard chứa gì:**
```html
<div id="tab-dashboard">
  <!-- Thẻ thống kê tổng quan -->
  <div class="stats-grid">
    <div class="stat-card" id="statSinhVien">Số sinh viên: ??</div>
    <div class="stat-card" id="statGiangVien">Số giảng viên: ??</div>
    <div class="stat-card" id="statLopHoc">Số lớp: ??</div>
    <div class="stat-card" id="statMonHoc">Số môn: ??</div>
  </div>

  <!-- Biểu đồ phân bố xếp loại (Chart.js) -->
  <canvas id="rankChart"></canvas>

  <!-- Bảng cảnh báo sinh viên GPA thấp -->
  <table id="warnTable">...</table>
</div>
```

**Tab Lớp học — ví dụ về CRUD:**
```html
<div id="tab-lophoc">
  <!-- Nút thêm + bộ lọc tìm kiếm -->
  <button id="btnThemLopHoc">+ Thêm lớp</button>
  <input id="searchLop" placeholder="Tìm kiếm..." />

  <!-- Bảng danh sách lớp -->
  <table id="tableLopHoc">
    <thead>
      <tr>
        <th>Tên lớp</th>
        <th>Khoa</th>
        <th>Phòng</th>
        <th>Sĩ số</th>
        <th>Cố vấn</th>
        <th>Thao tác</th>  <!-- Nút Sửa / Xóa -->
      </tr>
    </thead>
    <tbody id="bodyLopHoc">
      <!-- JavaScript điền dữ liệu vào đây -->
    </tbody>
  </table>
</div>
```

**Modal (popup) thêm/sửa lớp:**
```html
<div id="modalLopHoc" class="modal">
  <div class="modal-content">
    <h3>Thêm / Sửa lớp học</h3>
    <form id="formLopHoc">
      <input type="hidden" id="lopHocId" />  <!-- id ẩn, dùng khi sửa -->
      <input id="tenLop" placeholder="Tên lớp" />
      <input id="khoaLop" placeholder="Khoa" />
      <input id="phongLop" placeholder="Phòng" />
      <select id="coVanLop">
        <option>-- Chọn cố vấn --</option>
        <!-- JavaScript điền giảng viên vào -->
      </select>
      <button type="submit">Lưu</button>
      <button type="button" id="btnHuyLop">Hủy</button>
    </form>
  </div>
</div>
```

---

### 3.3 sinh-vien.html — Trang Sinh Viên

**Cấu trúc tab:**

| Tab | Nội dung |
|-----|----------|
| `bang-diem` | Bảng điểm tích lũy: hiện GPA, từng học kỳ, từng môn |
| `hoc-luc` | Đánh giá học lực AI: GPA, xếp loại, nhận xét |
| `monhoc` | Danh sách môn học theo học kỳ |
| `ho-so` | Thông tin cá nhân: xem + sửa (chỉ phone/email) |

**Bảng điểm quan trọng — hiểu cách tính:**
```html
<!-- Hiển thị GPA tích lũy lớn ở đầu -->
<div class="gpa-display">
  <span id="gpaTichLuy">8.50</span>   <!-- Ví dụ GPA -->
  <span id="xepLoaiTichLuy" class="badge badge-A">Giỏi</span>
  <span>Tổng tín chỉ: <span id="tongTC">120</span></span>
</div>

<!-- Từng học kỳ -->
<div class="hocky-section">
  <h4>Học kỳ 1 — GPA: 8.20 — Khá — 18 TC</h4>
  <table>
    <tr>
      <td>Giải tích</td>        <!-- Tên môn -->
      <td>3</td>                <!-- Tín chỉ -->
      <td>8.5  7.0  8.0  7.5</td>  <!-- CC KT1 KT2 KT3 -->
      <td>9.0</td>              <!-- Điểm thi -->
      <td>8.65</td>             <!-- Điểm tổng kết -->
      <td class="badge badge-A">A</td>  <!-- Xếp loại -->
    </tr>
  </table>
</div>
```

---

### 3.4 giang-vien.html — Trang Giảng Viên

**Cấu trúc tab:**

| Tab | Nội dung |
|-----|----------|
| `nhap-diem` | Chọn môn → hiện danh sách sinh viên → nhập/sửa điểm |
| `mon-duoc-phan-cong` | Môn học giảng viên đang dạy |
| `ho-so` | Hồ sơ cá nhân |

**Form nhập điểm:**
```html
<div id="tab-nhap-diem">
  <!-- Bước 1: Chọn môn học -->
  <select id="selectMon">
    <option>-- Chọn môn học --</option>
    <!-- Chỉ hiện môn được phân công cho GV này -->
  </select>

  <!-- Bước 2: Bảng sinh viên của môn đó -->
  <table id="tableNhapDiem">
    <thead>
      <tr>
        <th>Họ tên SV</th>
        <th>Lớp</th>
        <th>CC (0-10)</th>
        <th>KT1</th>
        <th>KT2</th>
        <th>KT3</th>
        <th>Thi</th>
        <th>Tổng kết</th>
        <th>Thao tác</th>
      </tr>
    </thead>
    <tbody id="bodyNhapDiem">
      <!-- JavaScript điền vào -->
    </tbody>
  </table>

  <!-- Nút xuất CSV -->
  <button id="btnExportCSV">Xuất CSV</button>
</div>
```

---

## 4. Phần CSS — style.css

File CSS dài ~500 dòng, chia theo từng nhóm thành phần:

### 4.1 Trang đăng nhập
```css
/* Nền gradient tím-xanh */
.auth-page {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    display: flex;
    align-items: center;    /* Căn giữa theo chiều dọc */
    justify-content: center; /* Căn giữa theo chiều ngang */
}

/* Hộp trắng giữa màn hình */
.auth-card {
    background: white;
    border-radius: 12px;
    padding: 40px;
    box-shadow: 0 20px 60px rgba(0,0,0,0.1);
    width: 400px;
}
```

### 4.2 Layout (Sidebar + Main)
```css
/* Bố cục 2 cột */
.layout {
    display: flex;           /* Flexbox: sidebar | main đứng cạnh nhau */
    height: 100vh;
}

/* Sidebar cố định bên trái */
.sidebar {
    width: 250px;
    min-width: 250px;
    background: #1e293b;     /* Màu tối */
    color: white;
    display: flex;
    flex-direction: column;
}

/* Phần nội dung chiếm phần còn lại */
.main-content {
    flex: 1;                 /* flex:1 → chiếm hết không gian còn lại */
    overflow-y: auto;        /* Cuộn dọc khi nội dung dài */
    background: #f1f5f9;
}
```

### 4.3 Badge xếp loại
```css
/* Badge màu sắc cho điểm chữ */
.badge-A   { background: #d1fae5; color: #065f46; }  /* Xanh lá nhạt */
.badge-B   { background: #dbeafe; color: #1e40af; }  /* Xanh lam nhạt */
.badge-C   { background: #fef3c7; color: #92400e; }  /* Vàng nhạt */
.badge-D   { background: #ffe4e6; color: #9f1239; }  /* Đỏ nhạt */
.badge-F   { background: #fee2e2; color: #991b1b; }  /* Đỏ */

/* Badge xếp loại học lực */
.badge-gioi { background: #d1fae5; color: #065f46; }
.badge-kha  { background: #dbeafe; color: #1e40af; }
.badge-tb   { background: #fef3c7; color: #92400e; }
.badge-yeu  { background: #fee2e2; color: #991b1b; }
```

### 4.4 Modal (popup)
```css
/* Lớp nền tối che phía sau */
.modal {
    display: none;           /* Ẩn mặc định */
    position: fixed;         /* Đè lên toàn màn hình */
    inset: 0;                /* top:0 right:0 bottom:0 left:0 */
    background: rgba(0,0,0,0.5);
    z-index: 1000;
    align-items: center;
    justify-content: center;
}

/* Khi JS thêm class 'active', modal hiện ra */
.modal.active {
    display: flex;
}

/* Hộp nội dung bên trong modal */
.modal-content {
    background: white;
    border-radius: 12px;
    padding: 32px;
    max-width: 500px;
    width: 90%;
}
```

### 4.5 Responsive (điện thoại)
```css
/* Khi màn hình nhỏ hơn 768px (điện thoại) */
@media (max-width: 768px) {
    .sidebar {
        width: 60px;         /* Thu sidebar lại */
    }
    .sidebar-text {
        display: none;       /* Ẩn chữ, chỉ hiện icon */
    }
    .stats-grid {
        grid-template-columns: 1fr 1fr;  /* 2 cột thay vì 4 */
    }
}
```

---

## 5. Phần JavaScript — api.js

Đây là file "thư viện dùng chung" — tất cả các trang đều nhúng file này.

### 5.1 Lấy thông tin từ localStorage
```javascript
// Các hàm đọc thông tin đã lưu khi đăng nhập
const getToken     = () => localStorage.getItem('token');
const getRole      = () => localStorage.getItem('role');
const getUserId    = () => localStorage.getItem('userId');
const getProfileId = () => localStorage.getItem('profileId');
```

**Tại sao cần profileId?**
Hệ thống có 2 ID tách biệt:
- `userId` = ID trong bảng `NguoiDung` (tài khoản đăng nhập)
- `profileId` = ID trong bảng `SinhVien` hoặc `GiangVien` (hồ sơ thông tin)

Ví dụ: Sinh viên Nguyễn Văn A có userId=5, profileId=3.  
Khi xem điểm, cần profileId để gọi `GET /api/diem/cua-toi/3`.

### 5.2 Hàm gọi API trung tâm
```javascript
// Mọi request đều qua hàm này — tự động thêm JWT token
async function apiRequest(method, url, body = null) {
    const headers = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`  // Đính kèm token
    };
    
    const options = { method, headers };
    if (body) options.body = JSON.stringify(body);
    
    const response = await fetch(url, options);
    
    // Nếu 401 (hết hạn/chưa đăng nhập) → về trang login
    if (response.status === 401) {
        logout();
        return null;
    }
    
    return response;
}

// Các hàm tiện ích
const api = {
    get:    (url)        => apiRequest('GET', url),
    post:   (url, body)  => apiRequest('POST', url, body),
    put:    (url, body)  => apiRequest('PUT', url, body),
    delete: (url)        => apiRequest('DELETE', url),
};
```

### 5.3 Toast thông báo
```javascript
// Hiện thông báo nhỏ góc phải màn hình
function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;  // success | error | warning
    toast.textContent = message;
    document.body.appendChild(toast);
    
    // Tự biến mất sau 3 giây
    setTimeout(() => toast.remove(), 3000);
}

// Cách dùng:
showToast('Thêm lớp thành công!');           // Thông báo xanh
showToast('Email đã tồn tại', 'error');      // Thông báo đỏ
showToast('Đang xử lý...', 'warning');       // Thông báo vàng
```

### 5.4 Thư viện Validate (V)
```javascript
const V = {
    // Kiểm tra tên: 2-100 ký tự, không phải số thuần
    name: (val) => {
        if (!val || val.trim().length < 2) return 'Tên phải có ít nhất 2 ký tự';
        if (val.trim().length > 100) return 'Tên không quá 100 ký tự';
        return null;  // null = hợp lệ
    },
    
    // Kiểm tra email hợp lệ
    email: (val) => {
        if (!val) return 'Email không được để trống';
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) return 'Email không hợp lệ';
        return null;
    },
    
    // Kiểm tra điểm số (0-10)
    score: (val) => {
        const n = parseFloat(val);
        if (isNaN(n) || n < 0 || n > 10) return 'Điểm phải từ 0 đến 10';
        return null;
    },
    
    // Kiểm tra tín chỉ (1-10)
    credits: (val) => {
        const n = parseInt(val);
        if (isNaN(n) || n < 1 || n > 10) return 'Tín chỉ phải từ 1 đến 10';
        return null;
    }
};

// Cách dùng khi submit form:
const errTen = V.name(inputTen.value);
if (errTen) {
    showToast(errTen, 'error');
    return;  // Dừng lại, không gửi lên server
}
```

### 5.5 Xuất CSV
```javascript
function exportToCSV(data, filename) {
    // Lấy các key làm header
    const headers = Object.keys(data[0]);
    
    // Ghép thành chuỗi CSV
    const rows = data.map(row =>
        headers.map(h => `"${row[h] ?? ''}"`).join(',')
    );
    const csv = [headers.join(','), ...rows].join('\n');
    
    // Tạo file ảo và tự động tải xuống
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
}
```

---

## 6. Phần JavaScript — admin.js

File xử lý toàn bộ logic trang admin (615 dòng). Chia theo từng tính năng:

### 6.1 Khởi tạo khi trang load
```javascript
document.addEventListener('DOMContentLoaded', async () => {
    // 1. Kiểm tra đăng nhập
    requireAuth('Admin');  // Nếu không phải Admin → về login
    
    // 2. Load dữ liệu cần thiết trước
    await Promise.all([
        loadSinhViens(),   // Tải danh sách SV (dùng cho nhiều chỗ)
        loadGiangViens(),  // Tải danh sách GV
        loadMonHocs(),     // Tải danh sách môn học
        loadLopHocs(),     // Tải danh sách lớp học
    ]);
    
    // 3. Hiện dashboard
    loadDashboard();
    
    // 4. Gắn sự kiện cho sidebar
    setupTabSwitching();
});
```

### 6.2 Chuyển tab
```javascript
function setupTabSwitching() {
    document.querySelectorAll('[data-tab]').forEach(link => {
        link.addEventListener('click', (e) => {
            const tabName = e.target.dataset.tab;
            
            // Ẩn tất cả tab
            document.querySelectorAll('.tab-content').forEach(t => {
                t.classList.remove('active');
            });
            
            // Hiện tab được chọn
            document.getElementById(`tab-${tabName}`).classList.add('active');
            
            // Cập nhật active trên sidebar
            document.querySelectorAll('[data-tab]').forEach(l => {
                l.classList.remove('active');
            });
            e.target.classList.add('active');
        });
    });
}
```

### 6.3 Load dữ liệu và điền vào bảng — Ví dụ Lớp Học
```javascript
async function loadLopHocs() {
    const res  = await api.get('/api/lophoc');
    const data = await res.json();    // Mảng [{id, className, department, ...}, ...]
    
    const tbody = document.getElementById('bodyLopHoc');
    tbody.innerHTML = '';  // Xóa dữ liệu cũ
    
    data.forEach(lop => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${lop.className}</td>
            <td>${lop.department ?? '—'}</td>
            <td>${lop.room ?? '—'}</td>
            <td>${lop.soLuong}</td>
            <td>${lop.advisorName ?? '—'}</td>
            <td>
                <button onclick="editLopHoc(${lop.id})">Sửa</button>
                <button onclick="deleteLopHoc(${lop.id})">Xóa</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}
```

### 6.4 CRUD — Thêm/Sửa Lớp Học
```javascript
// Mở modal thêm mới (id trống)
document.getElementById('btnThemLopHoc').addEventListener('click', () => {
    document.getElementById('lopHocId').value = '';  // id rỗng = thêm mới
    document.getElementById('formLopHoc').reset();
    document.getElementById('modalLopHoc').classList.add('active');
});

// Mở modal sửa (có id)
async function editLopHoc(id) {
    const res  = await api.get(`/api/lophoc/${id}`);
    const data = await res.json();
    
    // Điền dữ liệu vào form
    document.getElementById('lopHocId').value = data.id;
    document.getElementById('tenLop').value   = data.className;
    document.getElementById('khoaLop').value  = data.department;
    document.getElementById('phongLop').value = data.room;
    
    document.getElementById('modalLopHoc').classList.add('active');
}

// Submit form — phân biệt thêm mới hay sửa
document.getElementById('formLopHoc').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const id = document.getElementById('lopHocId').value;  // Có id = sửa, rỗng = thêm
    const body = {
        className:  document.getElementById('tenLop').value,
        department: document.getElementById('khoaLop').value,
        room:       document.getElementById('phongLop').value,
        advisorId:  parseInt(document.getElementById('coVanLop').value) || null,
    };
    
    let res;
    if (id) {
        res = await api.put(`/api/lophoc/${id}`, body);   // Sửa
    } else {
        res = await api.post('/api/lophoc', body);         // Thêm
    }
    
    if (res.ok) {
        showToast('Lưu thành công!');
        document.getElementById('modalLopHoc').classList.remove('active');
        loadLopHocs();  // Tải lại bảng
    } else {
        const err = await res.json();
        showToast(err.message, 'error');
    }
});

// Xóa
async function deleteLopHoc(id) {
    if (!confirm('Bạn có chắc muốn xóa lớp này?')) return;
    
    const res = await api.delete(`/api/lophoc/${id}`);
    if (res.ok) {
        showToast('Xóa thành công!');
        loadLopHocs();
    }
}
```

### 6.5 Dashboard — Biểu đồ Chart.js
```javascript
async function loadDashboard() {
    // Gọi API thống kê
    const res  = await api.get('/api/thongke');
    const data = await res.json();
    
    // Điền số liệu
    document.getElementById('statSinhVien').textContent = data.soSinhVien;
    document.getElementById('statGiangVien').textContent = data.soGiangVien;
    
    // Vẽ biểu đồ tròn phân bố xếp loại
    const ctx = document.getElementById('rankChart').getContext('2d');
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ['Giỏi', 'Khá', 'Trung bình', 'Yếu', 'Chưa đánh giá'],
            datasets: [{
                data: [
                    data.soGioi,
                    data.soKha,
                    data.soTrungBinh,
                    data.soYeu,
                    data.chuaDanhGia
                ],
                backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#ef4444', '#94a3b8']
            }]
        }
    });
}
```

---

## 7. Phần Backend C# — Models (Mô hình dữ liệu)

Models là các lớp C# đại diện cho bảng trong database. Entity Framework Core (EF Core) dùng các lớp này để tự tạo/truy vấn bảng MySQL.

### 7.1 NguoiDung.cs — Bảng tài khoản
```csharp
// Tương đương bảng MySQL: NguoiDungs
public class NguoiDung
{
    public int    Id         { get; set; }  // PRIMARY KEY (tự tăng)
    public string Email      { get; set; }  // NOT NULL
    public string MatKhau    { get; set; }  // Mật khẩu đã mã hóa bcrypt
    public string MatKhauGoc { get; set; }  // Mật khẩu gốc (cho admin xem)
    public string Role       { get; set; }  // "Admin" | "GiangVien" | "SinhVien"

    // Navigation properties (quan hệ 1-1)
    public SinhVien?  SinhVien  { get; set; }  // null nếu không phải sinh viên
    public GiangVien? GiangVien { get; set; }  // null nếu không phải giảng viên
}
```

**Tại sao tách NguoiDung ra riêng?**  
Vì Admin không có hồ sơ riêng, trong khi SinhVien và GiangVien có thông tin chi tiết khác nhau. Tách ra giúp mỗi bảng chỉ chứa đúng trường cần thiết.

### 7.2 SinhVien.cs
```csharp
public class SinhVien
{
    public int      Id         { get; set; }
    public string   FullName   { get; set; }
    public DateOnly? Dob       { get; set; }  // Ngày sinh
    public string?  Gender     { get; set; }  // "Nam" | "Nữ"
    public string?  Phone      { get; set; }
    public string   Email      { get; set; }
    public string?  Class      { get; set; }  // Tên lớp (text, đồng bộ từ LopHoc)
    public string?  Department { get; set; }  // Khoa
    public string?  Course     { get; set; }  // Khóa học (VD: K2022)

    // KHÓA NGOẠI
    public int?     NguoiDungId { get; set; } // Liên kết với tài khoản
    public NguoiDung? NguoiDung { get; set; }

    public int?     LopHocId { get; set; }    // Liên kết với lớp học
    public LopHoc?  LopHoc   { get; set; }

    // Quan hệ 1-nhiều: một SV có nhiều điểm
    public ICollection<Diem> Diems { get; set; } = [];

    // Quan hệ 1-1: một SV có tối đa 1 đánh giá AI
    public DanhgiaAI? DanhgiaAI { get; set; }
}
```

### 7.3 Diem.cs — Bảng điểm
```csharp
public class Diem
{
    public int   Id         { get; set; }
    public int   SinhVienId { get; set; }   // FOREIGN KEY → SinhVien
    public int   MonHocId   { get; set; }   // FOREIGN KEY → MonHoc
    
    // Các cột điểm
    public float CC   { get; set; }   // Điểm chuyên cần (0-10)
    public float KT1  { get; set; }   // Kiểm tra 1
    public float KT2  { get; set; }   // Kiểm tra 2
    public float KT3  { get; set; }   // Kiểm tra 3
    public float Exam { get; set; }   // Điểm thi cuối kỳ

    public int?  GiangVienId { get; set; }  // Ai nhập điểm này

    // Navigation properties
    public SinhVien?  SinhVien  { get; set; }
    public MonHoc?    MonHoc    { get; set; }
    public GiangVien? GiangVien { get; set; }
}
```

**Ràng buộc quan trọng:** Trong AppDbContext, có ràng buộc `Unique(SinhVienId, MonHocId)` — tức là **mỗi sinh viên chỉ có 1 bản ghi điểm cho 1 môn**. Không thể nhập điểm 2 lần cho cùng 1 sinh viên + môn học.

---

## 8. Phần Backend C# — AppDbContext (Kết nối DB)

AppDbContext là cầu nối giữa code C# và database MySQL thông qua **Entity Framework Core**.

```csharp
public class AppDbContext : DbContext
{
    // Mỗi DbSet = một bảng trong MySQL
    public DbSet<NguoiDung>  NguoiDungs  { get; set; }
    public DbSet<GiangVien>  GiangViens  { get; set; }
    public DbSet<SinhVien>   SinhViens   { get; set; }
    public DbSet<LopHoc>     LopHocs     { get; set; }
    public DbSet<MonHoc>     MonHocs     { get; set; }
    public DbSet<Diem>       Diems       { get; set; }
    public DbSet<DanhgiaAI>  DanhgiaAIs  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ràng buộc: 1 sinh viên chỉ có 1 điểm / môn
        modelBuilder.Entity<Diem>()
            .HasIndex(d => new { d.SinhVienId, d.MonHocId })
            .IsUnique();

        // Khi xóa SinhVien → KHÔNG xóa Diem, chỉ set SinhVienId = null
        modelBuilder.Entity<Diem>()
            .HasOne(d => d.SinhVien)
            .WithMany(s => s.Diems)
            .OnDelete(DeleteBehavior.SetNull);
        
        // ... các quan hệ khác
    }
}
```

**Cách EF Core hoạt động:**
```csharp
// Thay vì viết SQL:
// SELECT * FROM SinhViens WHERE LopHocId = 5
// Chỉ cần viết C#:
var svList = await db.SinhViens
    .Where(s => s.LopHocId == 5)
    .ToListAsync();

// EF tự dịch thành SQL và thực thi
```

---

## 9. Phần Backend C# — Services (Dịch vụ)

### 9.1 GpaService.cs — Tính điểm và GPA

```csharp
public class GpaService
{
    // Công thức: CC×10% + KT_trung_bình×30% + Thi×60%
    public static float TinhDiemTongKet(Diem d)
    {
        float ktTb = (d.KT1 + d.KT2 + d.KT3) / 3f;
        return MathF.Round(d.CC * 0.1f + ktTb * 0.3f + d.Exam * 0.6f, 2);
    }

    // GPA = tổng(điểm_môn × tín_chỉ) / tổng(tín_chỉ)
    // Ví dụ: Giải tích 3TC điểm 8.0, Lập trình 4TC điểm 9.0
    // GPA = (8.0×3 + 9.0×4) / (3+4) = (24+36)/7 = 8.57
    public static float TinhGpa(IEnumerable<Diem> diems)
    {
        float tongDiemTinChi = 0;
        int tongTinChi = 0;
        foreach (var d in diems)
        {
            if (d.MonHoc is null) continue;
            float dtk = TinhDiemTongKet(d);
            tongDiemTinChi += dtk * d.MonHoc.Credits;
            tongTinChi     += d.MonHoc.Credits;
        }
        return tongTinChi == 0 ? 0 : MathF.Round(tongDiemTinChi / tongTinChi, 2);
    }

    // Xếp loại học lực
    public static string XepLoai(float gpa) => gpa switch
    {
        >= 8.5f => "Giỏi",        // 8.5 - 10.0
        >= 7.0f => "Khá",         // 7.0 - 8.4
        >= 5.0f => "Trung bình",  // 5.0 - 6.9
        _       => "Yếu"          // 0.0 - 4.9
    };
}
```

### 9.2 JwtService.cs — Tạo token đăng nhập

**JWT (JSON Web Token)** là chuỗi mã hóa chứa thông tin người dùng. Frontend lưu token này và gửi kèm mỗi request để server biết ai đang gọi.

```csharp
public class JwtService(IConfiguration config)
{
    public string GenerateToken(NguoiDung user)
    {
        // Khóa bí mật (lấy từ appsettings.json)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        
        // Thông tin nhúng vào token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // userId
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)  // "Admin" / "GiangVien" / "SinhVien"
        };

        var token = new JwtSecurityToken(
            issuer:   config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(8),  // Hết hạn sau 8 tiếng
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
        // Ví dụ kết quả: "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiI1In0.abc123..."
    }
}
```

**Luồng JWT:**
```
1. Đăng nhập  → Server tạo token → Gửi về Frontend
2. Frontend   → Lưu vào localStorage
3. Mọi request → Frontend gửi kèm: Authorization: Bearer <token>
4. Server     → Giải mã token → Biết userId, role
5. [Authorize(Roles="Admin")] → Tự động kiểm tra role
```

### 9.3 AiService.cs — Gọi Gemini AI

```csharp
public class AiService(IConfiguration config, IHttpClientFactory httpFactory)
{
    public async Task<string> SinhNhanXetAsync(
        string tenSinhVien, float gpa, string xepLoai, IEnumerable<string> danhSachMon)
    {
        var apiKey = config["AI:GeminiApiKey"];
        
        // Nếu không có API key → dùng nhận xét mặc định
        if (string.IsNullOrEmpty(apiKey))
            return SinhNhanXetCoBan(gpa, xepLoai);

        // Tạo prompt gửi cho Gemini
        var prompt = $"""
            Bạn là trợ lý giáo dục. Hãy viết một nhận xét ngắn (2-3 câu) bằng tiếng Việt về tình trạng học tập:
            - Tên: {tenSinhVien}
            - GPA: {gpa:F2}/10
            - Xếp loại: {xepLoai}
            - Các môn học: {string.Join(", ", danhSachMon)}
            Nhận xét phải mang tính động viên và gợi ý cải thiện nếu cần.
            """;

        // Gọi API Gemini
        var url  = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        var body = JsonSerializer.Serialize(new {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        });

        var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
        
        // Đọc kết quả từ JSON trả về
        var json = await response.Content.ReadAsStringAsync();
        // → trích xuất text từ candidates[0].content.parts[0].text
        return extractedText;
    }

    // Nhận xét mặc định khi không có API key
    private static string SinhNhanXetCoBan(float gpa, string xepLoai) => xepLoai switch
    {
        "Giỏi"         => $"Kết quả xuất sắc với GPA {gpa:F2}. Tiếp tục phát huy!",
        "Khá"          => $"GPA {gpa:F2} khá tốt. Cố gắng thêm để đạt loại Giỏi.",
        "Trung bình"   => $"GPA {gpa:F2} trung bình. Cần nỗ lực cải thiện.",
        _              => $"GPA {gpa:F2} thấp. Cần kế hoạch học tập nghiêm túc."
    };
}
```

---

## 10. Phần Backend C# — Controllers (Xử lý API)

Controllers nhận request HTTP → xử lý logic → trả về JSON.

### 10.1 AuthController.cs — Đăng nhập

```csharp
[ApiController]
[Route("api/[controller]")]  // → route: api/auth
public class AuthController(AppDbContext db, JwtService jwt) : ControllerBase
{
    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        // 1. Tìm user theo email
        var user = await db.NguoiDungs
            .Include(u => u.SinhVien)   // Lấy kèm hồ sơ SV nếu có
            .Include(u => u.GiangVien)  // Lấy kèm hồ sơ GV nếu có
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        // 2. Kiểm tra mật khẩu (bcrypt)
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.MatKhau, user.MatKhau))
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

        // 3. Lấy profileId (ID trong bảng SV hoặc GV)
        int? profileId = user.Role switch
        {
            "SinhVien"  => user.SinhVien?.Id,
            "GiangVien" => user.GiangVien?.Id,
            _           => null  // Admin không có profileId
        };

        // 4. Tạo JWT token
        var token = jwt.GenerateToken(user);

        // 5. Trả về
        return Ok(new AuthResponse(token, user.Role, user.Id, profileId));
    }

    // PUT api/auth/doi-mat-khau
    [HttpPut("doi-mat-khau")]
    [Authorize]  // Phải đăng nhập mới dùng được
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauRequest req)
    {
        // Lấy userId từ JWT token (đã decode tự động)
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.NguoiDungs.FindAsync(userId);

        // Kiểm tra mật khẩu cũ
        if (!BCrypt.Net.BCrypt.Verify(req.MatKhauCu, user.MatKhau))
            return BadRequest(new { message = "Mật khẩu cũ không đúng" });

        // Lưu mật khẩu mới (đã hash)
        user.MatKhau = BCrypt.Net.BCrypt.HashPassword(req.MatKhauMoi);
        await db.SaveChangesAsync();
        return Ok(new { message = "Đổi mật khẩu thành công" });
    }
}
```

### 10.2 DiemController.cs — Quản lý điểm

Đây là controller phức tạp nhất, xử lý nhiều trường hợp:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Tất cả endpoint đều yêu cầu đăng nhập
public class DiemController(AppDbContext db, AiService ai) : ControllerBase
{
    // GET api/diem — Admin/GV xem tất cả điểm
    [HttpGet]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> GetAll([FromQuery] int? sinhVienId, [FromQuery] int? monHocId)
    {
        var query = db.Diems.Include(d => d.SinhVien).Include(d => d.MonHoc).AsQueryable();

        // PHÂN QUYỀN: Giảng viên chỉ thấy điểm môn mình dạy
        if (User.IsInRole("GiangVien"))
        {
            var nguoiDungId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var gv = await db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);
            query = query.Where(d => d.MonHoc!.GiangVienId == gv.Id);
        }

        // Lọc thêm nếu có tham số
        if (sinhVienId.HasValue) query = query.Where(d => d.SinhVienId == sinhVienId);
        if (monHocId.HasValue)   query = query.Where(d => d.MonHocId == monHocId);

        var list = await query.Select(d => new DiemResponse(
            d.Id, d.SinhVienId, d.SinhVien!.FullName,
            d.MonHocId, d.MonHoc!.SubjectName, d.MonHoc.Credits, d.MonHoc.HocKy,
            d.CC, d.KT1, d.KT2, d.KT3, d.Exam,
            GpaService.TinhDiemTongKet(d)  // Tính điểm TK ngay khi query
        )).ToListAsync();

        return Ok(list);
    }

    // GET api/diem/bang-diem-tich-luy/{sinhVienId}
    // Trả về bảng điểm tích lũy theo từng học kỳ + GPA tổng
    [HttpGet("bang-diem-tich-luy/{sinhVienId}")]
    public async Task<IActionResult> GetBangDiemTichLuy(int sinhVienId)
    {
        var diems = await db.Diems
            .Include(d => d.MonHoc)
            .Where(d => d.SinhVienId == sinhVienId)
            .ToListAsync();

        // Nhóm theo học kỳ
        var grouped = diems
            .GroupBy(d => d.MonHoc!.HocKy)
            .OrderBy(g => g.Key)
            .Select(g => new DiemHocKyResponse(
                hocKy:   g.Key,
                gpa:     GpaService.TinhGpa(g),
                xepLoai: GpaService.XepLoai(GpaService.TinhGpa(g)),
                tongTC:  g.Sum(d => d.MonHoc!.Credits),
                diems:   g.Select(d => MapDiem(d)).ToList()
            )).ToList();

        var gpaTichLuy = GpaService.TinhGpa(diems);
        return Ok(new BangDiemTichLuyResponse(gpaTichLuy, GpaService.XepLoai(gpaTichLuy), ..., grouped));
    }

    // POST api/diem/danh-gia-ai/{sinhVienId}
    // Tính GPA → gọi AI → lưu đánh giá
    [HttpPost("danh-gia-ai/{sinhVienId}")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> DanhGiaAI(int sinhVienId)
    {
        var diems = await db.Diems.Include(d => d.MonHoc)
            .Where(d => d.SinhVienId == sinhVienId).ToListAsync();

        var gpa    = GpaService.TinhGpa(diems);
        var rank   = GpaService.XepLoai(gpa);
        var monHocs = diems.Select(d => d.MonHoc!.SubjectName);

        // Gọi AI
        var comment = await ai.SinhNhanXetAsync(sv.FullName, gpa, rank, monHocs);

        // Lưu vào DB (thêm mới hoặc cập nhật)
        var existing = await db.DanhgiaAIs.FirstOrDefaultAsync(x => x.StudentId == sinhVienId);
        if (existing is null)
            db.DanhgiaAIs.Add(new DanhgiaAI { StudentId = sinhVienId, Gpa = gpa, Rank = rank, AiComment = comment });
        else
            { existing.Gpa = gpa; existing.Rank = rank; existing.AiComment = comment; }

        await db.SaveChangesAsync();
        return Ok(new DanhgiaAIResponse(sinhVienId, sv.FullName, gpa, rank, comment));
    }
}
```

### 10.3 Các Controller đơn giản hơn

**SinhVienController** — CRUD sinh viên:
- `GET  /api/sinhvien` → Lấy tất cả (có thể lọc theo lớp)
- `GET  /api/sinhvien/{id}` → Lấy 1 sinh viên
- `POST /api/sinhvien` → Thêm sinh viên (chỉ Admin)
- `PUT  /api/sinhvien/{id}` → Sửa (chỉ Admin)
- `DELETE /api/sinhvien/{id}` → Xóa (chỉ Admin)

**GiangVienController** — CRUD giảng viên (chỉ Admin truy cập)

**LopHocController** — CRUD lớp học (chỉ Admin thêm/sửa/xóa, GET cho phép anonymous)

**MonHocController** — CRUD môn học:
- `GET /api/monhoc` → Public, lọc theo học kỳ
- `GET /api/monhoc/cua-toi` → Giảng viên xem môn mình dạy
- `GET /api/monhoc/{id}/sinh-vien` → SV đã có điểm trong môn đó

---

## 11. Phần Backend C# — Program.cs (Khởi động ứng dụng)

Đây là file đầu tiên chạy khi bật server. Nó cấu hình mọi thứ.

```csharp
var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════
// 1. ĐĂNG KÝ SERVICES (Dependency Injection)
// ═══════════════════════════════════════════

// Kết nối MySQL qua EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidIssuer   = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// CORS — cho phép Frontend (chạy ở port khác) gọi API
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));

// Đăng ký các service tự viết
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddHttpClient();  // Cho AiService dùng HttpClient

builder.Services.AddControllers();

// ═══════════════════════════════
// 2. KHỞI ĐỘNG ỨNG DỤNG
// ═══════════════════════════════
var app = builder.Build();

// MIDDLEWARE (thứ tự quan trọng!)
app.UseCors();
app.UseAuthentication();  // Phải trước Authorization
app.UseAuthorization();
app.UseStaticFiles();     // Phục vụ wwwroot/ (HTML, CSS, JS)
app.MapControllers();

// SPA fallback: URL không khớp → trả index.html
app.MapFallbackToFile("index.html");

// ═══════════════════════════════════════
// 3. TẠO DB VÀ SEED DỮ LIỆU MẶC ĐỊNH
// ═══════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();  // Tạo bảng nếu chưa có

    // Seed tài khoản mặc định nếu DB trống
    if (!db.NguoiDungs.Any())
    {
        db.NguoiDungs.AddRange(
            new NguoiDung { Email = "admin@gmail.com",     MatKhau = BCrypt.HashPassword("admin123"),     Role = "Admin"     },
            new NguoiDung { Email = "giaovien@gmail.com",  MatKhau = BCrypt.HashPassword("giaovien123"),  Role = "GiangVien" },
            new NguoiDung { Email = "sinhvien@gmail.com",  MatKhau = BCrypt.HashPassword("sinhvien123"),  Role = "SinhVien"  }
        );
        
        // Seed 45 môn học (8 học kỳ chương trình CNTT)
        db.MonHocs.AddRange(/* 45 môn */);
        db.SaveChanges();
    }
}

app.Run();
```

**Tại sao cần middleware theo thứ tự?**
```
Request vào
    ↓
CORS (cho phép từ domain khác)
    ↓
Authentication (giải mã JWT → biết là ai)
    ↓
Authorization (kiểm tra quyền → có được dùng không)
    ↓
StaticFiles (nếu là file → trả file luôn)
    ↓
Controllers (xử lý API logic)
```
Nếu đảo thứ tự (VD: Authorization trước Authentication) → lỗi vì chưa biết ai thì không thể kiểm tra quyền.

---

## 12. Luồng dữ liệu — Ví dụ thực tế

### Ví dụ 1: Admin nhập điểm cho sinh viên

```
1. Admin mở tab "Điểm" trên admin.html
2. JS admin.js gọi: GET /api/sinhvien → load danh sách SV
3. Admin chọn SV "Nguyễn Văn A" (id=5) và môn "Giải tích" (id=3)
4. Admin nhập: CC=8, KT1=7, KT2=8, KT3=7.5, Thi=9
5. Nhấn "Lưu" → JS gọi:
   POST /api/diem
   Body: { sinhVienId:5, monHocId:3, cc:8, kt1:7, kt2:8, kt3:7.5, exam:9 }
   Header: Authorization: Bearer eyJhbGci...
6. DiemController nhận → validate điểm 0-10 ✓
7. Kiểm tra chưa có điểm này trước đó ✓
8. Lưu vào bảng Diems
9. Trả về: { message: "Nhập điểm thành công", id: 15 }
10. JS hiện toast "Nhập điểm thành công", tải lại bảng
```

### Ví dụ 2: Admin đánh giá AI cho sinh viên

```
1. Admin click "Đánh giá AI" cho SV id=5
2. JS gọi: POST /api/diem/danh-gia-ai/5
3. DiemController:
   a. Lấy tất cả điểm của SV 5 (có kèm MonHoc)
   b. GpaService.TinhGpa() → GPA = 8.57
   c. GpaService.XepLoai(8.57) → "Giỏi"
   d. AiService.SinhNhanXetAsync("Nguyễn Văn A", 8.57, "Giỏi", ["Giải tích", ...])
      → Gọi Gemini API với prompt
      → Nhận: "Nguyễn Văn A đạt kết quả xuất sắc với GPA 8.57. Các môn học được nắm vững. Tiếp tục phát huy và có thể tham gia nghiên cứu khoa học."
   e. Lưu vào bảng DanhgiaAIs
4. Trả về JSON → JS hiện lên màn hình
```

### Ví dụ 3: Sinh viên xem bảng điểm

```
1. SV đăng nhập → nhận token, profileId=3
2. Vào sinh-vien.html → JS gọi:
   GET /api/diem/bang-diem-tich-luy/3
3. Server lấy tất cả điểm của SV 3, nhóm theo học kỳ:
   HK1: [Giải tích 3TC-8.65, CTDL 3TC-7.50, ...] → GPA_HK1 = 8.12
   HK2: [OOP 4TC-9.10, CSDL 3TC-8.20, ...]       → GPA_HK2 = 8.71
   GPA tích lũy = (8.12×20 + 8.71×18) / 38 = 8.39 → Khá
4. JS render ra bảng với từng học kỳ, từng môn, badge màu
```

---

## 13. Bảng tổng hợp API Endpoints

| Method | URL | Mô tả | Quyền |
|--------|-----|--------|-------|
| POST | `/api/auth/login` | Đăng nhập | Public |
| PUT | `/api/auth/doi-mat-khau` | Đổi mật khẩu | Đã đăng nhập |
| GET | `/api/sinhvien` | Danh sách sinh viên | Admin, GV |
| POST | `/api/sinhvien` | Thêm sinh viên | Admin |
| PUT | `/api/sinhvien/{id}` | Sửa sinh viên | Admin |
| DELETE | `/api/sinhvien/{id}` | Xóa sinh viên | Admin |
| GET | `/api/giangvien` | Danh sách giảng viên | Admin |
| POST | `/api/giangvien` | Thêm giảng viên | Admin |
| GET | `/api/lophoc` | Danh sách lớp | Public |
| POST | `/api/lophoc` | Thêm lớp | Admin |
| GET | `/api/monhoc` | Danh sách môn (lọc HK) | Public |
| GET | `/api/monhoc/cua-toi` | Môn GV đang dạy | GV |
| GET | `/api/monhoc/{id}/sinh-vien` | SV có điểm trong môn | Admin, GV |
| POST | `/api/monhoc` | Thêm môn | Admin, GV |
| GET | `/api/diem` | Tất cả điểm (lọc được) | Admin, GV |
| GET | `/api/diem/cua-toi/{svId}` | Điểm của 1 SV | Đã đăng nhập |
| GET | `/api/diem/bang-diem-tich-luy/{svId}` | Bảng điểm theo HK | Đã đăng nhập |
| POST | `/api/diem` | Nhập điểm | Admin, GV |
| PUT | `/api/diem/{id}` | Sửa điểm | Admin, GV |
| POST | `/api/diem/danh-gia-ai/{svId}` | Đánh giá AI | Admin, GV |
| GET | `/api/diem/danh-gia-ai/{svId}` | Xem đánh giá AI | Đã đăng nhập |
| GET | `/api/thongke` | Thống kê dashboard | Admin |
| GET | `/api/profile` | Xem hồ sơ bản thân | Đã đăng nhập |
| PUT | `/api/profile` | Sửa hồ sơ | Đã đăng nhập |

---

## Tóm tắt luồng học

Nếu bạn muốn hiểu dự án từ đầu đến cuối, hãy đọc theo thứ tự này:

1. **Models** (7 file .cs) → Hiểu cấu trúc dữ liệu
2. **AppDbContext.cs** → Hiểu bảng DB và quan hệ
3. **GpaService.cs** → Hiểu công thức tính điểm
4. **JwtService.cs** → Hiểu cơ chế xác thực
5. **AuthController.cs** → Hiểu luồng đăng nhập
6. **DiemController.cs** → Hiểu phần phức tạp nhất
7. **Program.cs** → Hiểu cách khởi động và cấu hình
8. **api.js** → Hiểu cầu nối Frontend–Backend
9. **index.html** → Trang đơn giản nhất để hiểu luồng HTML
10. **admin.js + admin.html** → Hiểu trang phức tạp nhất

---

*Tài liệu này được tạo tự động từ mã nguồn thực tế của dự án. Ngày tạo: 2026-05-13*
