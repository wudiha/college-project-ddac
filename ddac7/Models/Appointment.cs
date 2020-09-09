using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Models
{
    public class Appointment : TableEntity
    {
        public Appointment(string clinicName, string appId)
        {
            this.PartitionKey = clinicName;
            this.RowKey = appId;
        }

        public Appointment() { }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Age")]
        public int Age { get; set; }

        [Display(Name = "Date")]
        public DateTime AppointmentDateTime { get; set; }

        public string userID { get; set; }
    }
}
