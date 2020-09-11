using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Models
{
    public class Feedback : TableEntity
    {
        public Feedback(int clinicID, string feedbackId)
        {
            this.PartitionKey = clinicID.ToString();
            this.RowKey = feedbackId;
        }

        public Feedback() { }

        [Display(Name = "Appointment ID")]
        public string appId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Insert your feedback:")]
        public string FeedbackDetails { get; set; }

        public string userID { get; set; }

    }
}