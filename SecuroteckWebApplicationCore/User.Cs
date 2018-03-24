using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SecuroteckWebApplicationCore
{
    public class User
    {
        [Key]
        public string ApiKey { get; set; }

        public string UserName { get; set; }

        public virtual ICollection<Log> Logs { get; set; }
    }

    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string LogId { get; set; }

        public string LogString { get; set; }
        public DateTime LogDateTime { get; set; }
    }
}
