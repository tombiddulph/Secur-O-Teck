using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecuroteckWebApplicationCore.Models
{
    public class LogArchive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        /// <summary>
        /// Database generated log Id
        /// </summary>
        [Required]
        public Guid LogId { get; set; }

        /// <summary>
        /// The body of the log
        /// </summary>
        [Required]
        public string LogString { get; set; }

        /// <summary>
        /// The time at which the log was created
        /// </summary>
        [Required]
        public DateTime LogDateTime { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ApiKey { get; set; }
    }
}
