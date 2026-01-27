using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Data;

namespace WebsiteE_Commerce.Controllers
{
    public class ShopController : Controller
    {
        private readonly QuanLyHangHoaContext _context;

        public ShopController(QuanLyHangHoaContext context)
        {
            _context = context;
        }

       [HttpGet]
public async Task<IActionResult> GetProducts(string? search, int? category, string? sort)
{
    var query = _context.HangHoas
        .Include(h => h.MaLoaiNavigation)
        .AsQueryable();

    // Xử lý search (quan trọng!)
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(h => 
            h.TenHh.Contains(search) || 
            (h.TenAlias != null && h.TenAlias.Contains(search))
        );
    }

    // Xử lý category
    if (category.HasValue)
    {
        query = query.Where(h => h.MaLoai == category.Value);
    }

    // Xử lý sort
    query = sort switch
    {
        "asc" => query.OrderBy(h => h.DonGia),
        "desc" => query.OrderByDescending(h => h.DonGia),
        "newest" => query.OrderByDescending(h => h.MaHh),
        _ => query.OrderBy(h => h.TenHh)
    };

    var products = await query.ToListAsync();
    return PartialView("_ProductListPartial", products);
}
    [HttpGet]
public async Task<IActionResult> SearchAndSort(string? search, int? category, string? sort)
{
    var query = _context.HangHoas
        .Include(h => h.MaLoaiNavigation)
        .AsQueryable();

    // Tìm kiếm theo tên
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(h => 
            h.TenHh.Contains(search) || 
            (h.TenAlias != null && h.TenAlias.Contains(search))
        );
    }

    // Lọc theo danh mục
    if (category.HasValue)
    {
        query = query.Where(h => h.MaLoai == category.Value);
    }

    // Sắp xếp
    query = sort switch
    {
        "asc" => query.OrderBy(h => h.DonGia),
        "desc" => query.OrderByDescending(h => h.DonGia),
        "newest" => query.OrderByDescending(h => h.MaHh),
        _ => query.OrderBy(h => h.TenHh)
    };

    var products = await query.ToListAsync();
    return PartialView("_ProductListPartial", products);
}
        // GET: /Shop hoặc /Shop/Index
        public async Task<IActionResult> Index(string? search, int? category, string? sort)
        {
            // Lấy tất cả sản phẩm kèm theo thông tin loại
            var query = _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h => 
                    h.TenHh.Contains(search) || 
                    (h.TenAlias != null && h.TenAlias.Contains(search))
                );
            }

            // Lọc theo danh mục
            if (category.HasValue)
            {
                query = query.Where(h => h.MaLoai == category.Value);
            }

            // Sắp xếp
            query = sort switch
            {
                "asc" => query.OrderBy(h => h.DonGia),
                "desc" => query.OrderByDescending(h => h.DonGia),
                "newest" => query.OrderByDescending(h => h.MaHh),
                _ => query.OrderBy(h => h.TenHh)
            };

            var products = await query.ToListAsync();

            // Lấy danh sách loại cho dropdown
            ViewBag.Categories = await _context.Loais.ToListAsync();

            return View(products);
        }

        // GET: /Shop/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .FirstOrDefaultAsync(h => h.MaHh == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // API endpoint cho tìm kiếm tự động (AJAX)
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var suggestions = await _context.HangHoas
                .Where(h => h.TenHh.Contains(term) || (h.TenAlias != null && h.TenAlias.Contains(term)))
                .Take(5)
                .Select(h => new
                {
                    id = h.MaHh,
                    name = h.TenHh,
                    price = h.DonGia,
                    image = h.Hinh
                })
                .ToListAsync();
            return Json(suggestions);
        }
    }
}