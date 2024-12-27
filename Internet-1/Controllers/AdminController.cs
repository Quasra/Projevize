using Internet_1.Models;
using Internet_1.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Internet_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        // Tek bir constructor ile tüm bağımlılıkları tanımlıyoruz
        public AdminController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            // Dosya sayısını al ve ViewBag ile sayfaya gönder
            var fileCount = _context.FileManagerViewModel.Count();
            ViewBag.FileCount = fileCount;

            return View();
        }

        [HttpGet]
        [AllowAnonymous] // Test amaçlı anonim erişime izin veriyoruz
        public async Task<IActionResult> CreateAdmin()
        {
            // Admin rolü var mı kontrol et, yoksa oluştur
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new AppRole { Name = "Admin" });
                if (!roleResult.Succeeded)
                {
                    return Content("Admin rolü oluşturulamadı: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            // Admin kullanıcı oluştur
            var adminUser = await _userManager.FindByNameAsync("YeniAdmin");
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = "YeniAdmin",
                    Email = "yeniadmin@example.com",
                    FullName = "Yeni Admin",
                    PhotoUrl = "no-img.png"
                };

                var userResult = await _userManager.CreateAsync(adminUser, "YeniAdmin123!");
                if (!userResult.Succeeded)
                {
                    return Content("Admin kullanıcı oluşturulamadı: " + string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }

                // Admin rolünü kullanıcıya ata
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    return Content("Admin rolü atanamadı: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                return Content("Yeni admin başarıyla oluşturuldu: Kullanıcı adı: YeniAdmin, Şifre: YeniAdmin123!");
            }

            return Content("Admin kullanıcı zaten mevcut.");
        }
    }
}
