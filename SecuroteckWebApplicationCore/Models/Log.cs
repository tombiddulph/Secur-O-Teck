using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecuroteckWebApplicationCore.Models
{
    /// <summary>
    /// The log class for logging user interaction
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Database generated log Id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid LogId { get; set; }

        /// <summary>
        /// The body of the log
        /// </summary>
        public string LogString { get; set; }

        /// <summary>
        /// The time at which the log was created
        /// </summary>
        public DateTime LogDateTime { get; set; }
    }
}
