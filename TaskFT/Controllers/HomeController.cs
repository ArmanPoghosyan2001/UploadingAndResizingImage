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
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.FileProviders;

namespace TaskFT.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext _db;
        private IHostingEnvironment Environment;
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

            string fileName = Path.GetFileName(postedFile.FileName);
            using (FileStream stream = new FileStream(Path.Combine(newPath, fileName), FileMode.Create))
            {
                postedFile.CopyTo(stream);
                ViewBag.Message += $"<b>{fileName}</b> uploaded.<br />";

                var image = Image.Load(postedFile.OpenReadStream());
                image.Mutate(x => x.Resize(256, 256));
                image.Save(Path.Combine(newPath, "_256" + fileName));
                image.Mutate(x => x.Resize(56, 56));
                image.Save(Path.Combine(newPath, "_56" + fileName));

                image.Mutate(x => x.Crop(30, 30));
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

            //foreach (var item in imgs)
            //{
            //    ImageResponseModel image = new ImageResponseModel
            //    {
            //        ImagePath = "/Images/" + item.Name
            //    };
            //    imageResponseModels.Add(image);
            //}
            return View(imgs);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    static class _Path
    {
        public static string Combine(string firstPart,string secondPart)
        {
            return $"{firstPart}/{secondPart}";
        }
    }
}
