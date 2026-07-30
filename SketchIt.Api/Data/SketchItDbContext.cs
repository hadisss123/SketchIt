using Microsoft.EntityFrameworkCore;
using SketchIt.Api.Models;

public class SketchItDbContext : DbContext
{
    public SketchItDbContext(DbContextOptions<SketchItDbContext> options)
        : base(options) { }

    public DbSet<Room> Rooms { get; set; }

}

