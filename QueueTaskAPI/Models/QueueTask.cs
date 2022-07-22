using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QueueTaskAPI.Models
{
    //public enum Estatus
    //{
    //    STARTED= 1, 
    //    IN_PROGRESS=2,
    //    COMPLETED=3,
    //    FAILED=4
    //}
    public class QueueTask
    {
        [Key]
        public int TaskId { get; set; }
        public string TaskDescription { get; set; }
        public string Priority { get; set; }
        public string TaskStatus { get; set; }

        public int? CustomerId { get; set; }

    }
}
