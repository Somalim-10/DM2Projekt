using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models;

namespace DM2Projekt.Data
{
    public class DM2ProjektContext : DbContext
    {
        public DM2ProjektContext (DbContextOptions<DM2ProjektContext> options)
            : base(options)
        {
        }

        public DbSet<DM2Projekt.Models.Room> Room { get; set; } = default!;
        public DbSet<DM2Projekt.Models.User> User { get; set; } = default!;
    }
}
