using Microsoft.AspNetCore.Mvc;
using Internet_1.Models;
using Internet_1.Repositories;
using System.Linq;

namespace Internet_1.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext db;

        // Constructor Dependency Injection
        public NotesController(ApplicationDbContext context)
        {
            db = context;
        }

        // Not Listeleme
        [HttpGet]
        public JsonResult ListAjax()
        {
            var notes = db.Notes.Select(note => new
            {
                note.Id,
                note.Title,
                note.Content
            }).ToList();

            return Json(notes);
        }

        // Not Ekleme veya Güncelleme
        [HttpPost]
        public JsonResult AddUpdateAjax(Note note)
        {
            try
            {
                if (note.Id == 0)
                {
                    // Yeni not ekleme
                    db.Notes.Add(note);
                }
                else
                {
                    // Mevcut not güncelleme
                    var existingNote = db.Notes.Find(note.Id);
                    if (existingNote != null)
                    {
                        existingNote.Title = note.Title;
                        existingNote.Content = note.Content;
                    }
                    else
                    {
                        return Json(new { status = false, message = "Not bulunamadı!" });
                    }
                }

                db.SaveChanges(); // Değişiklikleri kaydet
                return Json(new { status = true, message = "Not başarıyla kaydedildi!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Not Detay Getirme
        [HttpGet]
        public JsonResult GetByIdAjax(int id)
        {
            try
            {
                var note = db.Notes.Where(n => n.Id == id).Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Content
                }).FirstOrDefault();

                if (note == null)
                    return Json(new { status = false, message = "Not bulunamadı!" });

                return Json(note);
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Not Silme
        [HttpDelete]
        public JsonResult DeleteAjax(int id)
        {
            try
            {
                var note = db.Notes.Find(id);
                if (note != null)
                {
                    db.Notes.Remove(note);
                    db.SaveChanges();
                    return Json(new { status = true, message = "Not başarıyla silindi!" });
                }
                return Json(new { status = false, message = "Not bulunamadı!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"Hata: {ex.Message}" });
            }
        }
    }
}
