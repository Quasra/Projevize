using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;

public class FileManagerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly string rootPath = @"C:\Your\Root\Directory";

    // Yapıcı metod: ApplicationDbContext enjekte ediliyor
    public FileManagerController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Index metodu: Dosya veya klasörleri listelemek için kullanılır
    public IActionResult Index(string folderPath = "")
    {
        // Veritabanından dosyaları çek
        var files = _context.FileManagerViewModel
                            .Where(f => f.Path.Contains(folderPath))
                            .ToList();

        // Dosyaların yüklendiğinden emin olun
        ViewBag.CurrentPath = folderPath;

        return View("~/Views/Home/Index.cshtml", files);  // Dosyaları View'a geçirelim
    }

    

    // UploadFile metodu: Dosya yüklemek için kullanılır
    [HttpPost]
    public async Task<IActionResult> UploadFile(string folderPath, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["UploadMessage"] = "No file selected.";
            return RedirectToAction("Index", new { folderPath });
        }

        // Hedef dizini belirle
        var targetFolder = Path.Combine(rootPath, folderPath ?? "").TrimEnd(Path.DirectorySeparatorChar);

        // Hedef dizin kontrolü
        if (!targetFolder.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            TempData["UploadMessage"] = "Invalid folder path.";
            return RedirectToAction("Index", new { folderPath });
        }

        // Hedef dizin yoksa oluştur
        if (!Directory.Exists(targetFolder))
        {
            try
            {
                Directory.CreateDirectory(targetFolder);
            }
            catch (Exception ex)
            {
                TempData["UploadMessage"] = $"Error creating directory: {ex.Message}";
                return RedirectToAction("Index", new { folderPath });
            }
        }

        // Dosyanın tam yolu
        var filePath = Path.Combine(targetFolder, file.FileName);

        // Dosya modeli oluştur
        var fileManagerViewModel = new FileManagerViewModel
        {
            Name = file.FileName,
            Path = filePath,
            Size = file.Length,
            ModifiedDate = DateTime.Now,
            Type = "File",
            UserId = "SomeUserId" // Gerekirse kullanıcı bilgisi ekleyin
        };

        try
        {
            // Dosyayı sisteme kaydet
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Veritabanına kaydet
            _context.FileManagerViewModel.Add(fileManagerViewModel);
            await _context.SaveChangesAsync();

            TempData["UploadMessage"] = "File uploaded successfully.";
        }
        catch (Exception ex)
        {
            TempData["UploadMessage"] = $"Error occurred: {ex.Message}";
        }

        return RedirectToAction("Index", new { folderPath });
    }

    // Delete metodu: Dosya veya klasör silmek için kullanılır
    public async Task<ActionResult> Delete(string path, string type)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(type))
        {
            return BadRequest("Path or type cannot be empty.");
        }

        var decodedPath = WebUtility.UrlDecode(path);
        var fullPath = Path.Combine(rootPath, decodedPath);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid("Access to the specified path is not allowed.");
        }

        if (type == "Folder" && !Directory.Exists(fullPath))
        {
            return NotFound($"Folder not found: {fullPath}");
        }

        if (type == "File" && !System.IO.File.Exists(fullPath))
        {
            return NotFound($"File not found: {fullPath}");
        }

        try
        {
            if (type == "Folder")
            {
                Directory.Delete(fullPath, true);
            }
            else
            {
                System.IO.File.Delete(fullPath);
            }

            var fileManagerViewModels = await _context.FileManagerViewModel
                .FirstOrDefaultAsync(f => f.Path == fullPath);

            if (fileManagerViewModels != null)
            {
                _context.FileManagerViewModel.Remove(fileManagerViewModels);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { folderPath = Path.GetDirectoryName(decodedPath) });
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while deleting: {ex.Message}");
        }
    }


    // Download metodu: Dosya indirmek için kullanılır
    public async Task<ActionResult> Download(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest("File path cannot be empty.");
        }

        var decodedFilePath = WebUtility.UrlDecode(filePath);
        var fullPath = Path.Combine(rootPath, decodedFilePath.TrimStart(Path.DirectorySeparatorChar));

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid("Access to the specified file is not allowed.");
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound($"The specified file does not exist: {fullPath}");
        }

        try
        {
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while processing the file: {ex.Message}");
        }
    }
    public IActionResult GetImage(string imageName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", imageName);
        if (System.IO.File.Exists(filePath))
        {
            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "image/jpeg");
        }
        return NotFound();
    }


    [HttpPost]
    public async Task<IActionResult> CreateFolder(string folderName, string folderPath = "")
    {
        if (string.IsNullOrEmpty(folderName))
        {
            TempData["ErrorMessage"] = "Dosya adı boş olamaz.";
            return RedirectToAction("Index", new { folderPath });
        }

        var targetPath = Path.Combine(rootPath, folderPath, folderName.Trim());

        if (Directory.Exists(targetPath))
        {
            TempData["ErrorMessage"] = "Bu isimde bir klasör zaten mevcut.";
            return RedirectToAction("Index", new { folderPath });
        }

        try
        {
            // Klasör oluşturuluyor
            Directory.CreateDirectory(targetPath);

            // Yeni klasör modelini oluşturuyoruz
            var newFolder = new FileManagerViewModel
            {
                Name = folderName,
                Path = targetPath,
                Type = "Folder", // Type'ı "Folder" olarak belirliyoruz
                ModifiedDate = DateTime.Now,
                Size = 0 // Klasör olduğu için boyut 0
            };

            // Veritabanına ekliyoruz
            _context.FileManagerViewModel.Add(newFolder);
            await _context.SaveChangesAsync(); // Değişiklikleri kaydediyoruz

            TempData["SuccessMessage"] = "Klasör başarıyla oluşturuldu.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Klasör oluşturulurken hata oluştu: {ex.Message}";
        }

        return RedirectToAction("Index", new { folderPath });
    }



}