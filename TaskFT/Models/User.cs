using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskFT.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ProilePicture { get; set; }
    }
}
