using ddac7.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Services
{
    public static class Common
    {

        public static List<Appointment> GetAppointmentsList(CloudTable table)
        {
            TableQuery<Appointment> query = new TableQuery<Appointment>();
            List<Appointment> appointments = new List<Appointment>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Appointment> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;
                foreach (Appointment apps in resultSegment.Results)
                {
                    appointments.Add(apps);
                }
            }
            while (token != null);

            return appointments;
        }
    }
}
