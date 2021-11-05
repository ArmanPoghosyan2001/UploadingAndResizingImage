using Microsoft.AspNetCore.Mvc;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.FileProviders;
using System;

namespace TaskFT.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHostingEnvironment Environment;
        public HomeController(AppDbContext db, IHostingEnvironment environment)
        {
            _db = db;
            Environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile postedFile)
        {
            string path = "/Images";
            string newPath = "wwwroot/Images";
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            var FileExtension = Path.GetExtension(Path.GetFileName(postedFile.FileName));
            string fileName = Convert.ToString(Guid.NewGuid());
            fileName += FileExtension;
            using (FileStream stream = new FileStream(Path.Combine(newPath, fileName), FileMode.Create))
            {
                postedFile.CopyTo(stream);
                ViewBag.Message += $"<b>{fileName}</b> uploaded.<br />";

                var image = Image.Load(postedFile.OpenReadStream());

                image.Mutate(x => x.Resize(256, 256));
                image.Save(Path.Combine(newPath, "_256" + fileName));

                image.Mutate(x => x.Resize(56, 56));
                image.Save(Path.Combine(newPath, "_56" + fileName));

                var size = image.Size();
                var l = size.Width / 4;
                var t = size.Height / 4;
                var r = 3 * (size.Width / 4);
                var b = 3 * (size.Height / 4);

                image.Mutate(x => x.Crop(Rectangle.FromLTRB(l,t,r,b)));
                image.Save(Path.Combine(newPath, "cropped" + fileName));

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
