﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Models
{
    public class Appointment : TableEntity
    {
        public Appointment(int clinicID, string appId)
        {
            this.PartitionKey = clinicID.ToString();
            this.RowKey = appId;
        }

        public Appointment() { }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Range(1, 150)]
        [Display(Name = "Age")]
        public int Age { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime AppointmentDateTime { get; set; }

        public string userID { get; set; }

        public string clinicName { get; set; }

        public string appStatus { get; set; }
    }
}
