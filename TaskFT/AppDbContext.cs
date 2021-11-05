using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskFT.Models;

namespace TaskFT
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<ImgModel> Images { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            Database.EnsureCreated();
        }
    }
}
