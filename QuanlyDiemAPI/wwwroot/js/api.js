const API = '/api';

function toggleSidebar() {
  document.querySelector('.sidebar').classList.toggle('open');
  document.querySelector('.sidebar-overlay').classList.toggle('show');
}
function closeSidebar() {
  document.querySelector('.sidebar').classList.remove('open');
  document.querySelector('.sidebar-overlay').classList.remove('show');
}
document.addEventListener('click', e => {
  if (e.target.closest('.sidebar nav a')) closeSidebar();
});

function getToken()    { return sessionStorage.getItem('token'); }
function getRole()     { return sessionStorage.getItem('role'); }
function getUserId()   { return sessionStorage.getItem('userId'); }
function getProfileId(){ return sessionStorage.getItem('profileId'); }

async function request(method, path, body) {
  const headers = { 'Content-Type': 'application/json' };
  const token = getToken();
  if (token) headers['Authorization'] = 'Bearer ' + token;
  const res = await fetch(API + path, {
    method, headers,
    body: body ? JSON.stringify(body) : undefined
  });
  if (res.status === 401) { logout(); return null; }
  const data = await res.json().catch(() => null);
  if (!res.ok) {
    const msg = data?.message
      || (data?.errors ? Object.values(data.errors).flat().join('; ') : null)
      || data?.title
      || `Lỗi ${res.status}`;
    throw new Error(msg);
  }
  return data;
}

const api = {
  get:    (path)       => request('GET',    path),
  post:   (path, body) => request('POST',   path, body),
  put:    (path, body) => request('PUT',    path, body),
  delete: (path)       => request('DELETE', path),
};

function logout() {
  sessionStorage.clear();
  window.location.href = '/index.html';
}

function requireAuth() {
  if (!getToken()) window.location.href = '/index.html';
}

function showToast(msg, isError = false) {
  const t = document.getElementById('toast');
  if (!t) return;
  t.textContent = msg;
  t.style.background = isError ? '#c62828' : '#2e7d32';
  t.style.display = 'block';
  setTimeout(() => t.style.display = 'none', isError ? 4000 : 3000);
}

// ===== Phân trang =====
function paginate(items, page, size) {
  return items.slice((page - 1) * size, page * size);
}

function renderPager(containerId, total, current, size, onPageFn) {
  const c = document.getElementById(containerId);
  if (!c) return;
  const pages = Math.ceil(total / size);
  if (pages <= 1) { c.innerHTML = ''; return; }
  let html = '<div class="pagination">';
  if (current > 1) html += `<button onclick="${onPageFn}(${current - 1})">‹</button>`;
  const start = Math.max(1, current - 2), end = Math.min(pages, current + 2);
  if (start > 1) html += `<button onclick="${onPageFn}(1)">1</button>${start > 2 ? '<span>…</span>' : ''}`;
  for (let i = start; i <= end; i++)
    html += `<button class="${i === current ? 'active' : ''}" onclick="${onPageFn}(${i})">${i}</button>`;
  if (end < pages) html += `${end < pages - 1 ? '<span>…</span>' : ''}<button onclick="${onPageFn}(${pages})">${pages}</button>`;
  if (current < pages) html += `<button onclick="${onPageFn}(${current + 1})">›</button>`;
  html += '</div>';
  c.innerHTML = html;
}

// ===== Export CSV =====
function exportCSV(tableId, filename) {
  const rows = document.querySelectorAll(`#${tableId} tr`);
  const lines = Array.from(rows).map(r =>
    Array.from(r.cells)
      .slice(0, -1) // bỏ cột Thao tác
      .map(c => `"${c.innerText.replace(/"/g, '""').trim()}"`)
      .join(',')
  );
  const csv = '﻿' + lines.join('\n'); // BOM UTF-8 cho Excel
  const a = document.createElement('a');
  a.href = 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv);
  a.download = filename + '_' + new Date().toLocaleDateString('vi-VN').replace(/\//g, '-') + '.csv';
  a.click();
}

// ===== Validate dùng chung =====
const V = {
  name(val, label = 'Họ tên') {
    if (!val || !val.trim()) return `${label} không được để trống`;
    if (/[0-9!@#$%^&*()+={}\[\];':"\\|<>?\/~`]/.test(val.trim()))
      return `${label} không được chứa số hoặc ký tự đặc biệt (#, @, &, ...)`;
    return null;
  },
  text(val, label) {
    if (!val || !val.trim()) return null; // không bắt buộc
    if (/[!@#$%^&*()+={}\[\];'"\\|<>?\/~`]/.test(val.trim()))
      return `${label} không được chứa ký tự đặc biệt (#, @, &, ...)`;
    return null;
  },
  email(val) {
    if (!val || !val.trim()) return 'Email không được để trống';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val.trim()))
      return 'Email không hợp lệ';
    return null;
  },
  phone(val) {
    if (!val || !val.trim()) return null;
    if (!/^0\d{9}$/.test(val.trim()))
      return 'Số điện thoại phải có đúng 10 chữ số và bắt đầu bằng 0';
    return null;
  },
  required(val, label) {
    if (!val || !val.toString().trim()) return `${label} không được để trống`;
    return null;
  },
  score(val, label) {
    if (val === '' || val === null || val === undefined)
      return `${label} không được để trống`;
    const n = parseFloat(val);
    if (isNaN(n) || n < 0 || n > 10) return `${label} phải là số từ 0 đến 10`;
    return null;
  },
  credits(val) {
    const n = parseInt(val);
    if (isNaN(n) || n < 1 || n > 10) return 'Số tín chỉ phải từ 1 đến 10';
    return null;
  },
};

// Hiển thị lỗi ngay dưới từng ô input — trả về false nếu có lỗi
function validateFields(fields) {
  let hasError = false;
  // Xóa lỗi cũ
  fields.forEach(([id]) => {
    const el = document.getElementById(id);
    if (!el) return;
    el.classList.remove('input-invalid');
    const e = el.parentNode.querySelector('.field-error');
    if (e) e.remove();
  });
  // Gắn lỗi mới
  fields.forEach(([id, msg]) => {
    if (!msg) return;
    hasError = true;
    const el = document.getElementById(id);
    if (!el) return;
    el.classList.add('input-invalid');
    const e = document.createElement('span');
    e.className = 'field-error';
    e.textContent = msg;
    el.parentNode.appendChild(e);
    const clear = () => { el.classList.remove('input-invalid'); e.remove(); };
    el.addEventListener('input',  clear, { once: true });
    el.addEventListener('change', clear, { once: true });
  });
  return !hasError;
}

// ===== Modal đổi mật khẩu (dùng chung) =====
function openDoiMK() { document.getElementById('modalDoiMK')?.classList.add('show'); }
function closeDoiMK() {
  const m = document.getElementById('modalDoiMK');
  if (m) { m.classList.remove('show'); ['mkCu','mkMoi','mkNhapLai'].forEach(f => { const el = document.getElementById(f); if(el) el.value=''; }); }
}

async function saveDoiMK() {
  const cu  = document.getElementById('mkCu').value;
  const moi = document.getElementById('mkMoi').value;
  const nl  = document.getElementById('mkNhapLai').value;
  if (!cu || !moi) return showToast('Vui lòng nhập đủ thông tin', true);
  if (moi !== nl)  return showToast('Mật khẩu mới không khớp', true);
  if (moi.length < 6) return showToast('Mật khẩu tối thiểu 6 ký tự', true);
  try {
    await api.put('/auth/doi-mat-khau', { matKhauCu: cu, matKhauMoi: moi });
    showToast('Đổi mật khẩu thành công');
    closeDoiMK();
  } catch(e) { showToast(e.message, true); }
}
