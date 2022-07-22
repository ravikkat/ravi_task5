using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace QueueTaskConsumeAPI.Models
{


    public class TaskConsumeDBContext : DbContext
    {
        public TaskConsumeDBContext(DbContextOptions<TaskConsumeDBContext> options)
            : base(options)
        {
        }

        public DbSet<TaskConsume> TodoItems { get; set; }
    }
}
