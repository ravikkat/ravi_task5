using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QueueTaskAPI.Models
{


    public class QueueTaskDBContexts : DbContext
    {
        public QueueTaskDBContexts(DbContextOptions<QueueTaskDBContexts> options)
            : base(options)
        {
        }

        public DbSet<QueueTask> TodoItems { get; set; }
    }
}
