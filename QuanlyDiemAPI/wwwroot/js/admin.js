requireAuth();
if (getRole() !== 'Admin') window.location.href = '/index.html';

let svList = [], gvList = [], mhList = [], lhList = [];
let _tabTimer = null;

// ===== INIT =====
window.onload = async () => {
  await Promise.all([loadSV(), loadGV(), loadMH(), loadLHData()]);
  loadStats();
  _startTabTimer('dashboard');
};

function _startTabTimer(tab) {
  if (_tabTimer) { clearInterval(_tabTimer); _tabTimer = null; }
  const refreshFn = {
    dashboard: loadStats,
    dangky:    loadDangKyAdmin,
    nhatky:    loadAuditLog,
  }[tab];
  if (refreshFn) {
    _tabTimer = setInterval(refreshFn, 30000);
  }
}

function showTab(tab) {
  document.querySelectorAll('[id^="tab-"]').forEach(el => el.style.display = 'none');
  document.getElementById('tab-' + tab).style.display = '';
  document.querySelectorAll('.sidebar nav a').forEach((a, i) => a.classList.remove('active'));
  const titles = {dashboard:'Trang chủ',lophoc:'Lớp học',monhoc:'Môn học',
    diem:'Điểm số',dangky:'Duyệt đăng ký môn học',ai:'Đánh giá AI',
    taikhoan:'Quản lý tài khoản',nhatky:'Nhật ký hệ thống'};
  document.getElementById('pageTitle').textContent = titles[tab] || '';

  if (tab === 'nhatky') loadAuditLog();
  if (tab === 'lophoc') loadLH();
  if (tab === 'diem') { Promise.all([loadSV(), loadMH()]).then(() => { populateDropdowns(); loadDiem(); }); }
  if (tab === 'dangky') loadDangKyAdmin();
  if (tab === 'ai') { loadSV().then(() => { populateAIDropdown(); loadAIResults(); }); }
  if (tab === 'taikhoan') loadTaiKhoan();
  if (tab === 'dashboard') loadStats();

  _startTabTimer(tab);
}

let rankChartInstance = null;

async function loadStats() {
  try {
    const tk = await api.get('/thongke');
    const g = document.getElementById('statsGrid');
    g.innerHTML = `
      <div class="stat-card"><div class="num">${tk.totalSV}</div><div class="label">👨‍🎓 Sinh viên</div></div>
      <div class="stat-card green"><div class="num">${tk.totalGV}</div><div class="label">👨‍🏫 Giảng viên</div></div>
      <div class="stat-card orange"><div class="num">${tk.totalMH}</div><div class="label">📚 Môn học</div></div>
      <div class="stat-card purple"><div class="num">${tk.avgGpa > 0 ? tk.avgGpa.toFixed(2) : '—'}</div><div class="label">GPA trung bình</div></div>
    `;

    // Biểu đồ phân bố học lực
    const rd = tk.rankDistribution;
    const total = Object.values(rd).reduce((a, b) => a + b, 0);
    const noData = document.getElementById('chartNoData');
    if (total === 0) {
      if (noData) noData.style.display = '';
    } else {
      if (noData) noData.style.display = 'none';
      const ctx = document.getElementById('chartRank')?.getContext('2d');
      if (ctx) {
        if (rankChartInstance) rankChartInstance.destroy();
        rankChartInstance = new Chart(ctx, {
          type: 'doughnut',
          data: {
            labels: ['Giỏi', 'Khá', 'Trung bình', 'Yếu'],
            datasets: [{ data: [rd['Giỏi'], rd['Khá'], rd['Trung bình'], rd['Yếu']],
              backgroundColor: ['#43a047','#1a73e8','#fb8c00','#e53935'],
              borderWidth: 2 }]
          },
          options: { plugins: { legend: { position: 'bottom' } }, cutout: '60%' }
        });
      }
    }

    // Danh sách cảnh báo
    const cb = document.getElementById('canhBaoList');
    if (cb) {
      if (!tk.sinhVienCanhBao.length) {
        cb.innerHTML = '<p style="color:#2e7d32;font-size:.9rem">✅ Không có sinh viên nào dưới ngưỡng cảnh báo</p>';
      } else {
        cb.innerHTML = tk.sinhVienCanhBao.map(s => `
          <div class="canh-bao-item">
            <div><div class="name">${s.fullName}</div><div style="font-size:.8rem;color:#888">${s.cls || ''}</div></div>
            <div class="gpa">GPA ${s.gpa.toFixed(2)}</div>
          </div>`).join('');
      }
      if (tk.chuaDanhGia > 0)
        cb.innerHTML += `<p style="color:#888;font-size:.82rem;margin-top:.5rem">⚠ ${tk.chuaDanhGia} sinh viên chưa được đánh giá AI</p>`;
    }
  } catch(e) {
    // fallback: dùng count từ list đã load
    const g = document.getElementById('statsGrid');
    g.innerHTML = `
      <div class="stat-card"><div class="num">${svList.length}</div><div class="label">Sinh viên</div></div>
      <div class="stat-card green"><div class="num">${gvList.length}</div><div class="label">Giảng viên</div></div>
      <div class="stat-card orange"><div class="num">${mhList.length}</div><div class="label">Môn học</div></div>
    `;
  }
}

// ===== UTILS =====
function closeModal(id) { document.getElementById(id).classList.remove('show'); }
function openModal(id)  { document.getElementById(id).classList.add('show'); }

function filterTable(tableId, text) {
  const rows = document.querySelectorAll(`#${tableId} tbody tr`);
  rows.forEach(r => {
    r.style.display = r.textContent.toLowerCase().includes(text.toLowerCase()) ? '' : 'none';
  });
}

function rankBadge(rank) {
  const map = {Giỏi:'gioi',Khá:'kha','Trung bình':'tb',Yếu:'yeu'};
  return `<span class="badge badge-${map[rank]||'tb'}">${rank}</span>`;
}

// ===== SINH VIÊN / GIẢNG VIÊN — chỉ load data cho dropdown =====
async function loadSV() {
  try { svList = await api.get('/sinhvien'); } catch(e) {}
}
async function loadGV() {
  try { gvList = await api.get('/giangvien'); } catch(e) {}
}

// Load danh sách lớp cho dropdown tài khoản (tách khỏi loadLH để dùng ở onload)
async function loadLHData() {
  try { lhList = await api.get('/lophoc'); } catch(e) {}
}

function fillClassDropdown(selectedLopHocId) {
  const sel = document.getElementById('tkClass');
  if (!sel) return;
  sel.innerHTML = '<option value="">-- Chọn lớp --</option>' +
    lhList.map(l => `<option value="${l.id}" ${l.id == selectedLopHocId ? 'selected' : ''}>${l.className}${l.department ? ' — ' + l.department : ''}</option>`).join('');
}

// ===== LỚP HỌC =====
async function loadLH() {
  try {
    lhList = await api.get('/lophoc');
    const tb = document.querySelector('#tableLH tbody');
    tb.innerHTML = lhList.map((l,i) => `
      <tr style="cursor:pointer" onclick="viewSVInLop(${l.id},'${l.className.replace(/'/g,"\\'")}')">
        <td>${i+1}</td><td>${l.className}</td><td>${l.department||''}</td>
        <td>${l.room||''}</td>
        <td style="color:#1a73e8;font-weight:600">${l.sinhVienCount} SV</td>
        <td>${l.advisorName||''}</td>
        <td onclick="event.stopPropagation()">
          <button class="btn btn-warning btn-sm" onclick="editLH(${l.id})">Sửa</button>
          <button class="btn btn-danger btn-sm" onclick="deleteLH(${l.id})">Xóa</button>
        </td>
      </tr>`).join('');
  } catch(e) { showToast(e.message, true); }
}

function openModalLH() {
  document.getElementById('lhId').value = '';
  ['lhName','lhDept','lhRoom'].forEach(f => document.getElementById(f).value = '');
  const sel = document.getElementById('lhAdvisor');
  sel.innerHTML = '<option value="">-- Chọn giảng viên --</option>' +
    gvList.map(g => `<option value="${g.id}">${g.fullName}</option>`).join('');
  openModal('modalLH');
}

function editLH(id) {
  const l = lhList.find(x => x.id === id);
  if (!l) return;
  document.getElementById('lhId').value = l.id;
  document.getElementById('lhName').value = l.className;
  document.getElementById('lhDept').value = l.department || '';
  document.getElementById('lhRoom').value = l.room || '';
  const sel = document.getElementById('lhAdvisor');
  sel.innerHTML = '<option value="">-- Chọn giảng viên --</option>' +
    gvList.map(g => `<option value="${g.id}" ${g.id===l.advisorId?'selected':''}>${g.fullName}</option>`).join('');
  openModal('modalLH');
}

async function saveLH() {
  const id = document.getElementById('lhId').value;
  const body = {
    className:  document.getElementById('lhName').value.trim(),
    department: document.getElementById('lhDept').value.trim() || null,
    room:       document.getElementById('lhRoom').value.trim() || null,
    advisorId:  parseInt(document.getElementById('lhAdvisor').value) || null,
  };
  if (!validateFields([
    ['lhName', V.required(body.className, 'Tên lớp học') || V.text(body.className, 'Tên lớp học')],
    ['lhDept', V.text(body.department, 'Khoa')],
  ])) return;
  try {
    if (id) await api.put('/lophoc/' + id, body);
    else    await api.post('/lophoc', body);
    showToast('Lưu thành công'); closeModal('modalLH'); loadLH();
  } catch(e) { showToast(e.message, true); }
}

async function deleteLH(id) {
  if (!confirm('Xóa lớp học này?')) return;
  try { await api.delete('/lophoc/' + id); showToast('Xóa thành công'); loadLH(); }
  catch(e) { showToast(e.message, true); }
}

async function viewSVInLop(lopHocId, className) {
  document.getElementById('modalSVInLopTitle').textContent = `Lớp ${className}`;
  document.getElementById('modalSVInLopBody').innerHTML = '<p style="color:#888;padding:.5rem 0">Đang tải...</p>';
  openModal('modalSVInLop');
  try {
    const lh   = lhList.find(x => x.id === lopHocId);
    const list = await api.get('/sinhvien?lopHocId=' + lopHocId);

    const infoRows = [
      ['Tên lớp',    lh?.className],
      ['Khoa',       lh?.department],
      ['Phòng học',  lh?.room],
      ['Cố vấn HT', lh?.advisorName],
      ['Số SV',      `<strong style="color:#1a73e8">${list.length}</strong>`],
    ];

    const infoHtml = `
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:.4rem 1.5rem;margin-bottom:1rem;padding:.75rem;background:#f5f7ff;border-radius:8px;font-size:.88rem">
        ${infoRows.map(([label, val]) => `
          <div>
            <span style="color:#888">${label}:</span>
            <span style="margin-left:.35rem;font-weight:500">${val || '<span style="color:#ccc">—</span>'}</span>
          </div>`).join('')}
      </div>`;

    const tableHtml = !list.length
      ? '<p style="color:#888;text-align:center;padding:1rem 0">Chưa có sinh viên nào trong lớp này.</p>'
      : `<div style="max-height:300px;overflow-y:auto;border:1px solid #eee;border-radius:6px">
          <table style="margin:0">
            <thead style="position:sticky;top:0;z-index:1;background:#e8eaf6">
              <tr><th>#</th><th>Họ tên</th><th>Giới tính</th><th>Ngày sinh</th><th>Khóa</th><th>Email</th></tr>
            </thead>
            <tbody>
              ${list.map((s, i) => `
                <tr>
                  <td>${i + 1}</td>
                  <td><strong>${s.fullName}</strong></td>
                  <td>${s.gender || '<span style="color:#ccc">—</span>'}</td>
                  <td>${s.dob || '<span style="color:#ccc">—</span>'}</td>
                  <td>${s.course || '<span style="color:#ccc">—</span>'}</td>
                  <td style="font-size:.82rem;color:#666">${s.email}</td>
                </tr>`).join('')}
            </tbody>
          </table>
        </div>`;

    document.getElementById('modalSVInLopBody').innerHTML = infoHtml + tableHtml;
  } catch(e) {
    document.getElementById('modalSVInLopBody').innerHTML =
      `<div class="alert alert-error">${e.message}</div>`;
  }
}

// ===== MÔN HỌC =====
async function loadMH() {
  try {
    mhList = await api.get('/monhoc');
    renderMH(mhList);
  } catch(e) {}
}

function renderMH(list) {
  const tb = document.querySelector('#tableMH tbody');
  if (!tb) return;
  tb.innerHTML = (list || mhList).map((m,i) => `
    <tr>
      <td>${i+1}</td>
      <td><span class="badge badge-tb" style="font-size:.78rem">HK ${m.hocKy}</span></td>
      <td>${m.subjectName}</td><td>${m.credits}</td><td>${m.department||''}</td>
      <td>${m.giangVienName||'<span style="color:#bbb">—</span>'}</td>
      <td>
        <button class="btn btn-warning btn-sm" onclick="editMH(${m.id})">Sửa</button>
        <button class="btn btn-danger btn-sm" onclick="deleteMH(${m.id})">Xóa</button>
      </td>
    </tr>`).join('') || '<tr><td colspan="7" style="text-align:center;color:#888">Chưa có môn học</td></tr>';
}

function filterMHByHocKy() {
  const hk = document.getElementById('filterHocKyMH')?.value;
  renderMH(hk ? mhList.filter(m => m.hocKy == hk) : mhList);
}

function openModalMH() {
  document.getElementById('mhId').value = '';
  document.getElementById('mhName').value = '';
  document.getElementById('mhCredits').value = 3;
  document.getElementById('mhHocKy').value = '1';
  document.getElementById('mhDept').value = '';
  document.getElementById('modalMHTitle').textContent = 'Thêm môn học';
  fillGVDropdownMH(null);
  openModal('modalMH');
}

function fillGVDropdownMH(selectedId) {
  const sel = document.getElementById('mhGV');
  if (!sel) return;
  sel.innerHTML = '<option value="">-- Không phân công --</option>' +
    gvList.map(g => `<option value="${g.id}" ${g.id == selectedId ? 'selected' : ''}>${g.fullName}</option>`).join('');
}

function editMH(id) {
  const m = mhList.find(x => x.id === id);
  if (!m) return;
  document.getElementById('mhId').value = m.id;
  document.getElementById('mhName').value = m.subjectName;
  document.getElementById('mhCredits').value = m.credits;
  document.getElementById('mhHocKy').value = m.hocKy || 1;
  document.getElementById('mhDept').value = m.department || '';
  document.getElementById('modalMHTitle').textContent = 'Sửa môn học';
  fillGVDropdownMH(m.giangVienId);
  openModal('modalMH');
}

async function saveMH() {
  const id = document.getElementById('mhId').value;
  const body = {
    subjectName: document.getElementById('mhName').value.trim(),
    credits:     parseInt(document.getElementById('mhCredits').value),
    hocKy:       parseInt(document.getElementById('mhHocKy').value) || 1,
    department:  document.getElementById('mhDept').value.trim() || null,
    giangVienId: parseInt(document.getElementById('mhGV')?.value) || null,
  };
  if (!validateFields([
    ['mhName',    V.required(body.subjectName, 'Tên môn học')],
    ['mhCredits', V.credits(body.credits)],
    ['mhDept',    V.text(body.department, 'Khoa')],
  ])) return;
  try {
    if (id) await api.put('/monhoc/' + id, body);
    else    await api.post('/monhoc', body);
    showToast('Lưu thành công'); closeModal('modalMH');
    await loadMH(); filterMHByHocKy(); loadStats();
  } catch(e) { showToast(e.message, true); }
}

async function deleteMH(id) {
  if (!confirm('Xóa môn học này?')) return;
  try { await api.delete('/monhoc/' + id); showToast('Xóa thành công'); await loadMH(); renderMH(); }
  catch(e) { showToast(e.message, true); }
}

// ===== ĐIỂM =====
function populateDropdowns() {
  const sv = document.getElementById('filterSV');
  sv.innerHTML = '<option value="">-- Chọn sinh viên --</option>' +
    svList.map(s => `<option value="${s.id}">${s.fullName}</option>`).join('');

  const mh = document.getElementById('filterMH');
  mh.innerHTML = '<option value="">-- Tất cả môn --</option>' +
    mhList.map(m => `<option value="${m.id}">${m.subjectName}</option>`).join('');

  document.getElementById('diemSV').innerHTML = svList.map(s => `<option value="${s.id}">${s.fullName}</option>`).join('');
  document.getElementById('diemMH').innerHTML = mhList.map(m => `<option value="${m.id}">HK${m.hocKy} - ${m.subjectName} (${m.credits} TC)</option>`).join('');
}

async function loadDiem() {
  const svId = document.getElementById('filterSV').value;
  const mhId = document.getElementById('filterMH').value;
  let url = '/diem?';
  if (svId) url += 'sinhVienId=' + svId + '&';
  if (mhId) url += 'monHocId=' + mhId;
  try {
    const list = await api.get(url);
    const tb = document.querySelector('#tableDiem tbody');
    const fmt = v => v != null ? v : '—';
    tb.innerHTML = list.map(d => {
      const dtk = d.diemTongKet;
      const loai = dtk != null ? (dtk >= 8.5 ? 'A' : dtk >= 7.0 ? 'B' : dtk >= 5.5 ? 'C' : dtk >= 4.0 ? 'D' : 'F') : null;
      const dtkHtml = dtk != null
        ? `<strong>${dtk.toFixed(2)}</strong> <span class="badge badge-${loai}">${loai}</span>`
        : '<span style="color:#aaa">Chưa đủ</span>';
      return `<tr>
        <td>${d.sinhVienName}</td><td>${d.monHocName}</td><td>${d.credits}</td>
        <td>${fmt(d.cc)}</td><td>${fmt(d.kT1)}</td><td>${fmt(d.kT2)}</td><td>${fmt(d.kT3)}</td><td>${fmt(d.exam)}</td>
        <td>${dtkHtml}</td>
      </tr>`;
    }).join('');
  } catch(e) {}
}

function openModalDiem() {
  document.getElementById('diemId').value = '';
  ['diemCC','diemKT1','diemKT2','diemKT3','diemExam'].forEach(f => document.getElementById(f).value = '');
  openModal('modalDiem');
}

function editDiem(id, svId, mhId, cc, kt1, kt2, kt3, exam) {
  document.getElementById('diemId').value = id;
  document.getElementById('diemSV').value = svId;
  document.getElementById('diemMH').value = mhId;
  document.getElementById('diemCC').value   = cc   != null ? cc   : '';
  document.getElementById('diemKT1').value  = kt1  != null ? kt1  : '';
  document.getElementById('diemKT2').value  = kt2  != null ? kt2  : '';
  document.getElementById('diemKT3').value  = kt3  != null ? kt3  : '';
  document.getElementById('diemExam').value = exam != null ? exam : '';
  openModal('modalDiem');
}

async function saveDiem() {
  const id   = document.getElementById('diemId').value;
  const svId = document.getElementById('diemSV').value;
  const mhId = document.getElementById('diemMH').value;
  const cc   = document.getElementById('diemCC').value;
  const kt1  = document.getElementById('diemKT1').value;
  const kt2  = document.getElementById('diemKT2').value;
  const kt3  = document.getElementById('diemKT3').value;
  const exam = document.getElementById('diemExam').value;
  if (!validateFields([
    ['diemSV', V.required(svId, 'Sinh viên')],
    ['diemMH', V.required(mhId, 'Môn học')],
  ])) return;
  const toScore = v => (v === '' || v == null) ? null : parseFloat(v);
  const body = {
    sinhVienId: parseInt(svId),
    monHocId:   parseInt(mhId),
    cc:   toScore(cc),
    kT1:  toScore(kt1),
    kT2:  toScore(kt2),
    kT3:  toScore(kt3),
    exam: toScore(exam),
  };
  try {
    if (id) await api.put('/diem/' + id, body);
    else    await api.post('/diem', body);
    showToast('Lưu điểm thành công'); closeModal('modalDiem'); loadDiem();
  } catch(e) { showToast(e.message, true); }
}

// ===== DUYỆT ĐĂNG KÝ MÔN HỌC =====
async function loadDangKyAdmin() {
  const trangThai = document.getElementById('filterTrangThaiDK')?.value || '';
  const url = '/dangky' + (trangThai ? '?trangThai=' + trangThai : '');
  try {
    const list = await api.get(url);
    const tb = document.querySelector('#tableDangKyAdmin tbody');
    if (!list.length) {
      tb.innerHTML = '<tr><td colspan="8" style="text-align:center;color:#888">Không có đăng ký nào</td></tr>';
      return;
    }
    const trangThaiMap = {ChoDuyet:'⏳ Chờ duyệt', DaDuyet:'✅ Đã duyệt', TuChoi:'❌ Từ chối'};
    const badgeMap    = {ChoDuyet:'badge-tb', DaDuyet:'badge-gioi', TuChoi:'badge-yeu'};
    tb.innerHTML = list.map((dk, i) => `<tr>
      <td>${i+1}</td>
      <td><strong>${dk.sinhVienName}</strong></td>
      <td>${dk.monHocName}</td>
      <td>HK ${dk.hocKy}</td>
      <td>${dk.credits} TC</td>
      <td>${new Date(dk.ngayDangKy).toLocaleDateString('vi-VN')}</td>
      <td><span class="badge ${badgeMap[dk.trangThai]||'badge-tb'}">${trangThaiMap[dk.trangThai]||dk.trangThai}</span></td>
      <td>${dk.trangThai === 'ChoDuyet' ? `
        <button class="btn btn-primary btn-sm" onclick="duyetDK(${dk.id})">Duyệt</button>
        <button class="btn btn-danger btn-sm"  onclick="tuChoiDK(${dk.id})">Từ chối</button>
      ` : '—'}</td>
    </tr>`).join('');
  } catch(e) { showToast(e.message, true); }
}

async function duyetDK(id) {
  try {
    await api.put('/dangky/' + id + '/duyet');
    showToast('Đã duyệt đăng ký');
    loadDangKyAdmin();
  } catch(e) { showToast(e.message, true); }
}

async function tuChoiDK(id) {
  if (!confirm('Từ chối đăng ký này?')) return;
  try {
    await api.put('/dangky/' + id + '/tu-choi');
    showToast('Đã từ chối đăng ký');
    loadDangKyAdmin();
  } catch(e) { showToast(e.message, true); }
}

// ===== AI =====
let aiResultsAll = [];

function populateAIDropdown() {
  const sel = document.getElementById('selectSVAI');
  if (!sel) return;
  sel.innerHTML = '<option value="">-- Chọn sinh viên --</option>' +
    svList.map(s => `<option value="${s.id}">${s.fullName}</option>`).join('');
}

function aiResultCard(data) {
  const rankColor = {Giỏi:'#43a047',Khá:'#1a73e8','Trung bình':'#fb8c00',Yếu:'#e53935'};
  const color = rankColor[data.rank] || '#666';
  return `
    <div class="ai-result" style="border-left:4px solid ${color}">
      <div style="display:flex;align-items:center;gap:1.25rem;flex-wrap:wrap;margin-bottom:.75rem">
        <div>
          <div style="font-size:.75rem;color:#888;margin-bottom:.1rem">GPA tích lũy</div>
          <div class="gpa-big" style="font-size:2.2rem;color:${color}">${data.gpa.toFixed(2)}<span style="font-size:.95rem;color:#aaa">/10</span></div>
        </div>
        <div>
          ${rankBadge(data.rank)}
          <div style="color:#555;font-size:.9rem;margin-top:.35rem;font-weight:600">${data.studentName}</div>
        </div>
      </div>
      <div class="comment" style="background:#f8f4ff;border-radius:8px;padding:.75rem 1rem;font-size:.9rem;line-height:1.6;color:#444">
        <span style="font-weight:600;color:var(--primary)">Nhận xét AI:</span> ${data.aiComment || 'Chưa có nhận xét'}
      </div>
    </div>`;
}

async function danhGiaAI() {
  const id = document.getElementById('selectSVAI').value;
  if (!id) return showToast('Chọn sinh viên trước', true);
  const res = document.getElementById('aiResult');
  res.innerHTML = '<p style="color:#888;padding:1rem;text-align:center">Đang phân tích bằng AI...</p>';
  try {
    const data = await api.post('/diem/danh-gia-ai/' + id);
    res.innerHTML = aiResultCard(data);
    loadAIResults();
  } catch(e) {
    res.innerHTML = `<div class="alert alert-error">${e.message}</div>`;
  }
}

async function danhGiaAITatCa() {
  const btn = document.getElementById('btnTatCa');
  const progressText = document.getElementById('aiProgressText');
  const progressBar  = document.getElementById('aiProgressBar');
  const progressFill = document.getElementById('aiProgressFill');

  btn.disabled = true;
  btn.textContent = 'Đang đánh giá...';
  progressBar.style.display = '';
  progressFill.style.width = '10%';
  progressText.textContent = 'Đang gọi AI, vui lòng chờ...';

  // Animate progress bar while waiting
  let pct = 10;
  const ticker = setInterval(() => {
    pct = Math.min(pct + 3, 85);
    progressFill.style.width = pct + '%';
  }, 800);

  try {
    const res = await api.post('/diem/danh-gia-ai/tat-ca');
    clearInterval(ticker);
    progressFill.style.width = '100%';
    progressText.textContent = `Hoàn tất! Đã đánh giá ${res.success} sinh viên, bỏ qua ${res.skip} sinh viên chưa có đủ điểm.`;
    showToast(`Đánh giá AI thành công ${res.success} sinh viên`);
    loadAIResults();
    loadStats();
  } catch(e) {
    clearInterval(ticker);
    progressText.textContent = 'Có lỗi xảy ra: ' + e.message;
    showToast(e.message, true);
  } finally {
    btn.disabled = false;
    btn.textContent = 'Đánh giá tất cả';
    setTimeout(() => { progressBar.style.display = 'none'; progressFill.style.width = '0%'; }, 4000);
  }
}

async function loadAIResults() {
  const tbody = document.querySelector('#tableAI tbody');
  if (!tbody) return;
  try {
    aiResultsAll = await api.get('/diem/danh-gia-ai');
    renderAITable(aiResultsAll);
    renderAIStats(aiResultsAll);
  } catch(e) {
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:#888">Chưa có dữ liệu đánh giá.</td></tr>';
  }
}

function renderAIStats(list) {
  const grid = document.getElementById('aiStatsGrid');
  if (!grid) return;
  const counts = {Giỏi:0,Khá:0,'Trung bình':0,Yếu:0};
  list.forEach(x => { if (counts[x.rank] !== undefined) counts[x.rank]++; });
  const total = list.length;
  grid.innerHTML = `
    <div class="stat-card green"><div class="num">${counts['Giỏi']}</div><div class="label">Giỏi</div></div>
    <div class="stat-card"><div class="num">${counts['Khá']}</div><div class="label">Khá</div></div>
    <div class="stat-card orange"><div class="num">${counts['Trung bình']}</div><div class="label">Trung bình</div></div>
    <div class="stat-card" style="--card-accent:#e53935"><div class="num" style="color:#e53935">${counts['Yếu']}</div><div class="label">Yếu</div></div>
    <div class="stat-card purple"><div class="num">${total}</div><div class="label">Tổng đã đánh giá</div></div>
  `;
}

function renderAITable(list) {
  const tbody = document.querySelector('#tableAI tbody');
  if (!list.length) {
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:#888">Chưa có dữ liệu đánh giá.</td></tr>';
    return;
  }
  const rankColor = {Giỏi:'#43a047',Khá:'#1a73e8','Trung bình':'#fb8c00',Yếu:'#e53935'};
  tbody.innerHTML = list.map((r, i) => `
    <tr>
      <td>${i + 1}</td>
      <td><strong>${r.studentName}</strong></td>
      <td style="text-align:center;font-weight:700;color:${rankColor[r.rank]||'#333'}">${r.gpa.toFixed(2)}</td>
      <td style="text-align:center">${rankBadge(r.rank)}</td>
      <td style="font-size:.83rem;color:#555;max-width:360px">${r.aiComment || '<span style="color:#ccc">—</span>'}</td>
      <td>
        <button class="btn btn-sm btn-warning" onclick="reEval(${r.studentId})">Đánh giá lại</button>
      </td>
    </tr>`).join('');
}

function filterAITable() {
  const text = (document.getElementById('searchAI')?.value || '').toLowerCase();
  const rank = document.getElementById('filterRankAI')?.value || '';
  const filtered = aiResultsAll.filter(r =>
    (!text || r.studentName.toLowerCase().includes(text)) &&
    (!rank || r.rank === rank)
  );
  renderAITable(filtered);
}

async function reEval(id) {
  const res = document.getElementById('aiResult');
  const svName = aiResultsAll.find(r => r.studentId === id)?.studentName || '';
  showToast(`Đang đánh giá lại ${svName}...`);
  try {
    const data = await api.post('/diem/danh-gia-ai/' + id);
    res.innerHTML = aiResultCard(data);
    // scroll to result
    document.getElementById('selectSVAI').value = id;
    res.scrollIntoView({ behavior: 'smooth', block: 'center' });
    loadAIResults();
  } catch(e) { showToast(e.message, true); }
}

// ===== QUẢN LÝ TÀI KHOẢN =====
let tkList = [];

const roleLabel = { Admin: '👑 Admin', GiangVien: '👨‍🏫 Giảng viên', SinhVien: '👨‍🎓 Sinh viên' };
const roleBadgeClass = { Admin: 'badge-gioi', GiangVien: 'badge-kha', SinhVien: 'badge-tb' };

async function loadTaiKhoan() {
  try {
    tkList = await api.get('/taikhoan');
    renderTaiKhoan(tkList);
  } catch(e) { showToast(e.message, true); }
}

function renderTaiKhoan(list) {
  const tb = document.querySelector('#tableTK tbody');
  if (!tb) return;
  tb.innerHTML = list.map((tk, i) => `
    <tr style="cursor:pointer" onclick="viewTaiKhoan(${tk.id})" title="Click để xem chi tiết">
      <td>${i+1}</td>
      <td><strong>${tk.email}</strong></td>
      <td><span class="badge ${roleBadgeClass[tk.role]||'badge-tb'}">${roleLabel[tk.role]||tk.role}</span></td>
      <td>${tk.fullName || '<span style="color:#bbb">—</span>'}</td>
      <td>${tk.class ? tk.class + (tk.department ? ' / ' + tk.department : '') : (tk.department || '<span style="color:#bbb">—</span>')}</td>
      <td>${tk.phone || '<span style="color:#bbb">—</span>'}</td>
      <td onclick="event.stopPropagation()">
        <button class="btn btn-warning btn-sm" onclick="editTK(${tk.id})">Sửa</button>
        <button class="btn btn-danger btn-sm" onclick="deleteTK(${tk.id})">Xóa</button>
      </td>
    </tr>`).join('') || '<tr><td colspan="7" style="text-align:center;color:#888">Chưa có tài khoản</td></tr>';
}

function filterByRole() {
  const role = document.getElementById('filterRole').value;
  renderTaiKhoan(role ? tkList.filter(t => t.role === role) : tkList);
}

// Xem chi tiết tài khoản (click vào dòng)
function viewTaiKhoan(id) {
  const tk = tkList.find(x => x.id === id);
  if (!tk) return;

  const isSV = tk.role === 'SinhVien';
  const isGV = tk.role === 'GiangVien';

  const rows = [
    ['Email', tk.email],
    ['Vai trò', roleLabel[tk.role] || tk.role],
    ['Họ tên', tk.fullName],
    ['Ngày sinh', tk.dob],
    ['Giới tính', tk.gender],
    ['Số điện thoại', tk.phone],
    ['Khoa / Bộ môn', tk.department],
    ...(isSV ? [['Lớp', tk.class], ['Khóa', tk.course]] : []),
  ];

  document.getElementById('tkDetailContent').innerHTML = `
    <table style="width:100%;font-size:.92rem">
      ${rows.map(([label, val]) => `
        <tr>
          <td style="padding:.5rem .75rem;color:#888;white-space:nowrap;width:140px">${label}</td>
          <td style="padding:.5rem .75rem;font-weight:500">${val || '<span style="color:#ccc">—</span>'}</td>
        </tr>`).join('')}
    </table>`;

  document.getElementById('btnEditFromDetail').onclick = () => {
    closeModal('modalTKDetail');
    editTK(id);
  };
  openModal('modalTKDetail');
}

function toggleProfileFields(currentClass = '') {
  const role = document.getElementById('tkRole').value;
  const pf = document.getElementById('profileFields');
  const sv = document.getElementById('svOnlyFields');
  const hint = document.getElementById('tkPassHint');
  pf.style.display = role === 'Admin' ? 'none' : '';
  if (sv) {
    sv.style.display = role === 'SinhVien' ? '' : 'none';
    if (role === 'SinhVien') fillClassDropdown(currentClass);  // currentClass is now lopHocId
  }
  hint.style.display = document.getElementById('tkId').value ? '' : 'none';
}

function openModalTK() {
  document.getElementById('tkId').value = '';
  document.getElementById('tkPassHint').style.display = 'none';
  ['tkEmail','tkPass','tkFullName','tkPhone','tkDept','tkCourse'].forEach(f => document.getElementById(f).value = '');
  document.getElementById('tkDob').value = '';
  document.getElementById('tkGender').value = '';
  document.getElementById('tkRole').value = 'SinhVien';
  document.getElementById('modalTKTitle').textContent = 'Tạo tài khoản mới';
  toggleProfileFields();
  openModal('modalTK');
}

function editTK(id) {
  const tk = tkList.find(x => x.id === id);
  if (!tk) return;
  document.getElementById('tkId').value = tk.id;
  document.getElementById('tkEmail').value = tk.email;
  document.getElementById('tkPass').value = tk.matKhauGoc || '';
  document.getElementById('tkRole').value = tk.role;
  document.getElementById('tkFullName').value = tk.fullName || '';
  document.getElementById('tkDob').value = tk.dob || '';
  document.getElementById('tkGender').value = tk.gender || '';
  document.getElementById('tkPhone').value = tk.phone || '';
  document.getElementById('tkDept').value = tk.department || '';
  document.getElementById('tkCourse').value = tk.course || '';
  document.getElementById('tkPassHint').style.display = '';
  document.getElementById('modalTKTitle').textContent = 'Chỉnh sửa tài khoản';
  toggleProfileFields(tk.lopHocId || null);
  openModal('modalTK');
}

async function saveTK() {
  const id       = document.getElementById('tkId').value;
  const role     = document.getElementById('tkRole').value;
  const pass     = document.getElementById('tkPass').value;
  const email    = document.getElementById('tkEmail').value.trim();
  const fullName = document.getElementById('tkFullName').value.trim();
  const phone    = document.getElementById('tkPhone').value.trim();

  const passErr = !id
    ? V.required(pass, 'Mật khẩu')
    : (pass && pass.length < 6 ? 'Mật khẩu tối thiểu 6 ký tự' : null);

  if (!validateFields([
    ['tkEmail',    V.email(email)],
    ['tkPass',     passErr],
    ['tkFullName', role !== 'Admin' ? V.name(fullName, 'Họ tên') : null],
    ['tkPhone',    V.phone(phone)],
    ['tkDept',     V.text(document.getElementById('tkDept').value.trim(), 'Khoa')],
  ])) return;

  const body = {
    email,
    matKhau:    pass || null,
    role,
    fullName:   fullName || 'Admin',
    dob:        document.getElementById('tkDob').value || null,
    gender:     document.getElementById('tkGender').value || null,
    phone:      phone || null,
    department: document.getElementById('tkDept').value.trim() || null,
    lopHocId:   parseInt(document.getElementById('tkClass').value) || null,
    course:     document.getElementById('tkCourse').value.trim() || null,
  };

  try {
    if (id) await api.put('/taikhoan/' + id, body);
    else    await api.post('/taikhoan', body);
    showToast(id ? 'Cập nhật thành công' : 'Tạo tài khoản thành công');
    closeModal('modalTK');
    await Promise.all([loadTaiKhoan(), loadSV(), loadGV()]);
    loadStats();
  } catch(e) { showToast(e.message, true); }
}


async function deleteTK(id) {
  const tk = tkList.find(x => x.id === id);
  if (!tk) return;
  if (!confirm(`Xóa tài khoản "${tk.email}" (${roleLabel[tk.role]})?`)) return;
  try {
    await api.delete('/taikhoan/' + id);
    showToast('Xóa tài khoản thành công');
    await Promise.all([loadTaiKhoan(), loadSV(), loadGV()]);
    loadStats();
  } catch(e) { showToast(e.message, true); }
}

// ===== NHẬT KÝ HỆ THỐNG =====
let _auditPage = 1;
const _auditPageSize = 30;

async function loadAuditLog(page = 1) {
  _auditPage = page;
  const tbody = document.querySelector('#tableAudit tbody');
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:#888">Đang tải...</td></tr>';
  try {
    const data = await api.get(`/auditlog?page=${page}&pageSize=${_auditPageSize}`);
    const roleColors = { Admin: '#e53935', GiangVien: '#1a73e8', SinhVien: '#43a047' };
    tbody.innerHTML = data.logs.length ? data.logs.map((r, i) => `
      <tr>
        <td>${(page - 1) * _auditPageSize + i + 1}</td>
        <td style="white-space:nowrap">${r.thoiGian}</td>
        <td>${r.email}</td>
        <td><span style="color:${roleColors[r.vaiTro]||'#666'};font-weight:600">${r.vaiTro}</span></td>
        <td><strong>${r.hanhDong}</strong></td>
        <td style="color:#555;font-size:.85rem">${r.chiTiet || '—'}</td>
      </tr>`).join('')
    : '<tr><td colspan="6" style="text-align:center;color:#888">Chưa có nhật ký</td></tr>';

    document.getElementById('auditTotal').textContent = `Tổng: ${data.total} bản ghi`;

    // Phân trang
    const totalPages = Math.ceil(data.total / _auditPageSize);
    const pager = document.getElementById('auditPager');
    pager.innerHTML = '';
    if (totalPages > 1) {
      for (let p = 1; p <= totalPages; p++) {
        const btn = document.createElement('button');
        btn.className = 'btn' + (p === page ? ' btn-primary' : '');
        btn.textContent = p;
        btn.onclick = () => loadAuditLog(p);
        pager.appendChild(btn);
      }
    }
  } catch(e) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:#e53935">${e.message}</td></tr>`;
  }
}
