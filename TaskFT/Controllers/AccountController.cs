using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskFT.Models;

namespace TaskFT.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email, Age = model.Age };
                IFormFile postedFile = model.ProfilePicture;
                string path = $"/Images/{user.Id}/ProileImage";
                string newPath = $"wwwroot/Images/{user.Id}/ProileImage";

                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                var FileExtension = Path.GetExtension(Path.GetFileName(postedFile.FileName));
                string fileName = Convert.ToString(Guid.NewGuid());
                fileName += FileExtension;

                var image = Image.Load(postedFile.OpenReadStream());

                if (!Directory.Exists(Path.Combine(newPath, "Original")))
                {
                    Directory.CreateDirectory(Path.Combine(newPath, "Original"));
                }
                
                image.Save(Path.Combine(newPath, "Original", fileName));
                ViewBag.Message += $"<b>{fileName}</b> uploaded.<br />";
                
                Dictionary<int, string> names = new Dictionary<int, string>();
                names.Add(0, "256_size");
                names.Add(1, "56_size");

                Dictionary<int, int> sizes = new Dictionary<int, int>();
                sizes.Add(0, 256);
                sizes.Add(1, 56);

                for (int i = 0; i < names.Count; i++)
                {
                    if (!Directory.Exists(Path.Combine(newPath, $"{names[i]}")))
                    {
                        Directory.CreateDirectory(Path.Combine(newPath, $"{names[i]}"));
                    }
                    image.Mutate(x => x.Resize(sizes[i], sizes[i]));
                    image.Save(Path.Combine(newPath, $"{names[i]}", fileName));
                }

                var size = image.Size();
                var l = size.Width / 4;
                var t = size.Height / 4;
                var r = 3 * (size.Width / 4);
                var b = 3 * (size.Height / 4);

                if (!Directory.Exists(Path.Combine(newPath, "cropped")))
                {
                    Directory.CreateDirectory(Path.Combine(newPath, "cropped"));
                }
                image.Mutate(x => x.Crop(Rectangle.FromLTRB(l, t, r, b)));
                image.Save(Path.Combine(newPath, "cropped", fileName));

                List<ImgModel> imgs = new List<ImgModel>(4);
                imgs.Add(new ImgModel { Name = fileName, Path = Path.Combine(path, "original", fileName) });
                imgs.Add(new ImgModel { Name = fileName, Path = Path.Combine(path, "256_size", fileName) });
                imgs.Add(new ImgModel { Name = fileName, Path = Path.Combine(path, "56_size", fileName) });
                imgs.Add(new ImgModel { Name = fileName, Path = Path.Combine(path, "cropped", fileName) });

                _db.Images.AddRange(imgs);
                _db.SaveChanges();

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

    }
}
