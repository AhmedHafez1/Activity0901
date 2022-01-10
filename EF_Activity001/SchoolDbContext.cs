using System;
using Microsoft.EntityFrameworkCore;

namespace EF_Activity001
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}
