using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebsiteE_Commerce.Data;
using WebsiteE_Commerce.Models;

namespace WebsiteE_Commerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly QuanLyHangHoaContext _context;

    public HomeController(ILogger<HomeController> logger,  QuanLyHangHoaContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.TopProducts = _context.HangHoas
            .OrderByDescending(p => p.SoLanXem)
            .Take(3)
            .ToList();
            
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
