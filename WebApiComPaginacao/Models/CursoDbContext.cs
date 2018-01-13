using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApiComPaginacao.Models
{
    public class CursoDbContext : DbContext
    {
        public CursoDbContext() : base("CursoConnectionString")
        {
            Database.Log = d => System.Diagnostics.Debug.WriteLine(d);
        }
        public DbSet<Curso> Cursos { get; set; }
    }
}