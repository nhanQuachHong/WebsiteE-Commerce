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

        // Hiện danh sách hàng hóa
        public async Task<IActionResult> Index()
        {
            var list = await _context.HangHoas.Include(h => h.MaLoaiNavigation).ToListAsync();
            return View(list);
        }

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

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HangHoa hangHoa)
        {
            ModelState.Remove("MaLoaiNavigation");
            ModelState.Remove("MaNccNavigation");
            if (ModelState.IsValid)
            {
                _context.Add(hangHoa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            return View(hangHoa);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null) return NotFound();
            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            return View(hangHoa);
        }

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
            return View(hangHoa);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var hangHoa = await _context.HangHoas.Include(h => h.MaLoaiNavigation).FirstOrDefaultAsync(m => m.MaHh == id);
            if (hangHoa == null) return NotFound();
            return View(hangHoa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa != null) _context.HangHoas.Remove(hangHoa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- AJAX: Kiểm tra trùng tên ---
        [HttpGet]
        public IActionResult KiemTraTenAjax(string tenSp)
        {
            var exists = _context.HangHoas.Any(x => x.TenHh == tenSp);
            return Json(new { isExisted = exists });
        }

        // --- AJAX: Upload ảnh ---
        [HttpPost]
        public IActionResult UploadHinhAjax(IFormFile fHinh)
        {
            if (fHinh != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fHinh.FileName);
                string path = Path.Combine(_host.WebRootPath, "images", fileName);
                if (!Directory.Exists(Path.Combine(_host.WebRootPath, "images")))
                {
                    Directory.CreateDirectory(Path.Combine(_host.WebRootPath, "images"));
                }
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    fHinh.CopyTo(stream);
                }
                return Json(new { success = true, fileName = fileName });
            }
            return Json(new { success = false });
        }
    }
}