using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Data;

namespace WebsiteE_Commerce.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly QuanLyHangHoaContext _context;
        private readonly IWebHostEnvironment _host;

        public HangHoaController(QuanLyHangHoaContext context, IWebHostEnvironment host)
        {
            _context = context;
            _host = host;
        }

        // --- 1. DANH SÁCH (INDEX) ---
        public async Task<IActionResult> Index()
        {
            var data = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .OrderByDescending(h => h.MaHh) // Mới nhất lên đầu
                .ToListAsync();
            return View(data);
        }

        // --- 2. TẠO MỚI (GET) ---
        public IActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy");
            return View();
        }

        // --- 3. TẠO MỚI (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HangHoa hangHoa)
        {
            // Bỏ qua validate các object quan hệ
            ModelState.Remove("MaLoaiNavigation");
            ModelState.Remove("MaNccNavigation");

            if (ModelState.IsValid)
            {
                // 1. Xử lý tên hình
                if (hangHoa.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hangHoa.ImageFile.FileName);
                    string uploadPath = Path.Combine(_host.WebRootPath, "images");

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hangHoa.ImageFile.CopyToAsync(stream);
                    }
                    hangHoa.Hinh = fileName;
                }
                else
                {
                    hangHoa.Hinh = "default.jpg";
                }

                // 2. Tự động gán dữ liệu mặc định
                hangHoa.NgaySx = DateTime.Now;
                hangHoa.SoLanXem = 0;

                _context.Add(hangHoa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại Dropdown
            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // --- 4. CHỈNH SỬA (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null) return NotFound();

            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // --- 5. CHỈNH SỬA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HangHoa hangHoa)
        {
            if (id != hangHoa.MaHh) return NotFound();

            ModelState.Remove("MaLoaiNavigation");
            ModelState.Remove("MaNccNavigation");

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu người dùng chọn ảnh mới
                    if (hangHoa.ImageFile != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hangHoa.ImageFile.FileName);
                        string filePath = Path.Combine(_host.WebRootPath, "images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await hangHoa.ImageFile.CopyToAsync(stream);
                        }
                        hangHoa.Hinh = fileName;
                    }
                    // Nếu không chọn ảnh mới, EF Core sẽ tự giữ nguyên giá trị cũ nếu bạn không gán đè

                    _context.Update(hangHoa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.HangHoas.Any(e => e.MaHh == hangHoa.MaHh)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // --- 6. CHI TIẾT (DETAILS) ---
        // (Thêm vào để tránh lỗi nếu bạn bấm nút "Chi tiết")
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaHh == id);

            if (hangHoa == null) return NotFound();

            return View(hangHoa);
        }

        // --- 7. XÓA (DELETE) - Phần quan trọng để sửa lỗi 404 ---

        // GET: Hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaHh == id);

            if (hangHoa == null) return NotFound();

            return View(hangHoa);
        }

        // POST: Thực hiện xóa trong Database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa != null)
            {
                // (Tùy chọn) Xóa ảnh cũ nếu cần thiết
                // if (hangHoa.Hinh != "default.jpg") { ... xóa file ảnh ... }

                _context.HangHoas.Remove(hangHoa);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}