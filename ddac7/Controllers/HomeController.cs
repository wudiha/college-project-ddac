using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ddac7.Models;
using ddac7.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ddac7.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly UserManager<ClinicAppUser> _userManager;
        private readonly AuthDbContext _context;

        public HomeController(UserManager<ClinicAppUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Clinic")) 
            {
                return LocalRedirect("~/Clinic/Index");
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        public IActionResult AnotherTestView()
        {
            return View();
        }
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ViewClinicList(string SearchString)
        {
            var result = (from a in _context.Clinic
                          where a.Status.Equals("Open")
                          select new Clinic()
                          {
                              Id = a.Id,
                              ClinicName = a.ClinicName,
                              ClinicDesc = a.ClinicDesc,
                              ContactNum = a.ContactNum,
                              ContactEmail = a.ContactEmail,
                              Status = a.Status,
                              UserID = a.UserID
                          }).ToList();

            if (!String.IsNullOrEmpty(SearchString))
            {
                result = result.Where(s => 
                    CultureInfo.CurrentCulture.CompareInfo.IndexOf
                    (s.ClinicName, SearchString, CompareOptions.IgnoreCase)>=0).ToList();
            }

            return View(result);
        }

        // View Clinic Details Page
        public async Task<IActionResult> ClinicDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clinic = await _context.Clinic
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            return View(clinic);
        }

        public ActionResult AddAppointment(int? id)
        {
            ViewBag.clinicId = id;
            return View();
        }
        public CloudTable TableStorage(String tableName)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot Configuration = builder.Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorageConnectionString-1"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        public List<Appointment> GetAppointmentsList(CloudTable table) {
           
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
        public ActionResult AddAppointmentIntoTable([Bind("Name,Age,AppointmentDateTime,PartitionKey")] Appointment app)
        {
            CloudTable table = TableStorage("AppointmentTable");
            var userid = _userManager.GetUserId(User);
            var clinicName =(from a in _context.Clinic
                         where a.Id.ToString().Equals(app.PartitionKey)
                         select a.ClinicName).Single();

            List<Appointment> appointments = GetAppointmentsList(table);
            int c = appointments.Count();
            string rowkey = "";

            if(c+1<10)
            rowkey = "C00" + (appointments.Count()+1).ToString(); 
            else if(c+1>10&&c+1<100)
            rowkey = "C0" + (appointments.Count() + 1).ToString();
            else 
            rowkey = "C" + (appointments.Count() + 1).ToString();

            var createAppointment = new Appointment
            {
                PartitionKey = app.PartitionKey,
                RowKey = rowkey,
                Name = app.Name,
                Age = app.Age,
                AppointmentDateTime = app.AppointmentDateTime,
                userID = userid,
                clinicName = clinicName,
                appStatus = "Waiting for Approval"
            };

            try
            {
                TableOperation insertOperation = TableOperation.Insert(createAppointment);
                TableResult result = table.ExecuteAsync(insertOperation).Result;
                if (result.Etag != null)
                {
                    TempData["message"] = "Your appointment request is now waiting for approval.";
                    return RedirectToAction("ViewAppointmentRecord", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "Unable to send your appointment request, Error: " + ex.ToString(); ;
            }

            return RedirectToAction("ViewAppointmentRecord", "Home");
        }

        public ActionResult ViewAppointmentRecord()
        {
            var userid = _userManager.GetUserId(User);
            CloudTable table = TableStorage("AppointmentTable");
           
            TableQuery<Appointment> query = new TableQuery<Appointment>()
                .Where(TableQuery.GenerateFilterCondition("userID", QueryComparisons.Equal, userid));

            List<Appointment> appointments = new List<Appointment>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Appointment> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (Appointment app in resultSegment.Results)
                {
                    appointments.Add(app);
                }
            }
            while (token != null);

            return View(appointments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
