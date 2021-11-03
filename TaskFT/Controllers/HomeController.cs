using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TaskFT.Models;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Web.Helpers;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace TaskFT.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext _db;
        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile postedFile)
        {

            string path = ("wwwroot/Images");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = Path.GetFileName(postedFile.FileName);
            using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                postedFile.CopyTo(stream);
                ViewBag.Message += $"<b>{fileName}</b> uploaded.<br />";

                var image = Image.Load(postedFile.OpenReadStream());
                image.Mutate(x => x.Resize(256, 256));
                image.Save(Path.Combine(path, "_256" + fileName));
                image.Mutate(x => x.Resize(56, 56));
                image.Save(Path.Combine(path, "_56" + fileName));

                image.Mutate(x => x.Crop(30, 30));
                image.Save(Path.Combine(path, "cropped" + fileName));

                List<ImgModel> imgs = new List<ImgModel>();
                imgs.Add(new ImgModel { Name = fileName, Path = Path.Combine(path, fileName) });
                imgs.Add(new ImgModel { Name = "_256" + fileName, Path = Path.Combine(path, "_256" + fileName) });
                imgs.Add(new ImgModel { Name = "_56" + fileName, Path = Path.Combine(path, "_56" + fileName) });
                imgs.Add(new ImgModel { Name = "cropped" + fileName, Path = Path.Combine(path, "cropped" + fileName) });

                _db.Images.AddRange(imgs);
                _db.SaveChanges();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            var imgs = _db.Images.ToList();
            return View(imgs);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
