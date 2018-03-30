using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecuroteckWebApplicationCore.Models
{
    public class User
    {


        public User()
        {
            if (ApiKey == null)
            {
                ApiKey = Guid.NewGuid().ToString();
            }
        }
        
        /// <summary>
        /// Gets or sets the users unique api key
        /// </summary>
        [Key]
        public string ApiKey { get; set; }
        
        

        /// <summary>
        /// Gets or sets the users name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the users collection of logs
        /// </summary>
        public virtual ICollection<Log> Logs { get; set; }
    }


   



   

    


}