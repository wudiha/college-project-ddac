﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ddac7.Areas.Identity.Data;
using ddac7.Models;
using ddac7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ddac7.Controllers
{
    [Authorize(Roles = "Clinic")]
    public class ClinicController : Controller
    {
        private readonly UserManager<ClinicAppUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IBlobService _azureBlobService;
        private  static IFormFileCollection FILES;

        [HttpPost]
        public async Task<ActionResult> UploadAsync()
        {
            try
            {
                var request = await HttpContext.Request.ReadFormAsync();
                if (request.Files == null)
                {
                    return BadRequest("Could not upload files");
                }
                var files = request.Files;
                if (files.Count == 0)
                {
                    return BadRequest("Could not upload empty files");
                }
                if (AzureBlobService.sss!=null)
                {
                    String url = "https://clinicappointmentdev.blob.core.windows.net/doctor/";
                    url += AzureBlobService.sss;
                    await DeleteImage(url);
                    await DeleteImage2(url);
                }
                FILES = files;
                await _azureBlobService.UploadAsync(files);
                return RedirectToAction("TestView");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteImage(string fileUri)
        {
            try
            {
                await _azureBlobService.DeleteAsync(fileUri);
                return RedirectToAction("TestView");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteImage2(string fileUri)
        {
            try
            {
                await _azureBlobService.DeleteAsync2(fileUri);
                return RedirectToAction("TestView");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                await _azureBlobService.DeleteAllAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadDoctor()
        {
            try
            {
                await _azureBlobService.UploadAsync2(FILES);
                return RedirectToAction("TestView");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
           
        }


        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
        public ClinicController(AuthDbContext context, UserManager<ClinicAppUser> userManager, IBlobService azureBlobService)
        {
            _userManager = userManager;
            _context = context;
            _azureBlobService = azureBlobService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddDoctor()
        {

            
            var model = new BlobModel();
            var result = (from a in _context.Doctor
                          select new BlobModel()
                          {
                              id = a.id,
                              DoctorName = a.DoctorName,
                              DoctorContactNumber = a.DoctorContactNumber,
                              imgurl = a.profileImage
                             
                          }).ToList();

            model.doctor = result;
            
            model.profileImage = await _azureBlobService.ListAsync("view");
            if (model.profileImage.Count().Equals(0))
            {
                AzureBlobService.sss = null;
            }
            //dynamic mymodel = new ExpandoObject();

            return View(model);
           
        }

        //

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDoctorProfile([Bind("Id,DoctorName,DoctorContactNumber,profileImage")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser.Id;

                var addDoctor = new Doctor
                {
                    DoctorName = doctor.DoctorName,
                    DoctorContactNumber = doctor.DoctorContactNumber,
                    profileImage = AzureBlobService.sss,
                    id = userId
                };

                if (AzureBlobService.sss != null)
                {
                    String url = "https://clinicappointmentdev.blob.core.windows.net/doctor/";
                    url += AzureBlobService.sss;
                    await DeleteImage(url);
                }

                _context.Add(addDoctor);
                await _context.SaveChangesAsync();
               
                TempData["message"] = "Doctor " + doctor.DoctorName + " has been created.";

                return RedirectToAction("Index");
            }

            return View();

        }
       
        public async Task<IActionResult> TestView()
        {
            try
            {
                

                var model = new BlobModel();
                model.profileImage = await _azureBlobService.ListAsync("add");
                if(model.profileImage.Count().Equals(0))
                {
                    AzureBlobService.sss = null;
                }
                //dynamic mymodel = new ExpandoObject();
            
                
                
                return View(model);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }
        public async Task<IActionResult> CreateClinic()
        {
            var userCheck = await _userManager.GetUserAsync(User);
            var userId = userCheck.Id;

            var allclinic = _context.Clinic.ToList();
            var check = allclinic.Where(x => x.UserID.Equals(userId)).ToList();

            if (check.Count == 0)
            {
                TempData["message"] = "To manage your clinic, please add your clinic information.";
                return View();
            }
            else
            {
                return RedirectToAction("ManageClinic", new { uID = userId });
            }

        }

        public IActionResult ManageClinic(string uID)
        {
            var result = (from a in _context.Clinic
                          where a.UserID.Equals(uID)
                          select new Clinic()
                          {
                              Id = a.Id,
                              ClinicName = a.ClinicName,
                              ClinicDesc = a.ClinicDesc,
                              ContactNum = a.ContactNum,
                              ContactEmail = a.ContactEmail,
                              Status = a.Status,
                              UserID = a.UserID
                          }).First();

            return View(result);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClinicName,ClinicDesc,ContactNum,ContactEmail,Status,UserID")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser.Id;

                var createClinic = new Clinic
                {
                    ClinicName = clinic.ClinicName,
                    ClinicDesc = clinic.ClinicDesc,
                    ContactNum = clinic.ContactNum,
                    ContactEmail = clinic.ContactEmail,
                    Status = clinic.Status,
                    UserID = userId
                };

                _context.Add(createClinic);
                await _context.SaveChangesAsync();
                TempData["message"] = "Clinic " + clinic.ClinicName + " has been created.";

                return RedirectToAction("ManageClinic", new { uID = userId });
            }

            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClinicName,ClinicDesc,ContactNum,ContactEmail,Status,UserID")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                    _context.Update(clinic);
                    TempData["message"] = "Your clinic details has been updated.";
                    await _context.SaveChangesAsync();

                return RedirectToAction("ManageClinic", new { uID = clinic.UserID });
            }
            return View(clinic);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, [Bind("Id,ClinicName,ClinicDesc,ContactNum,ContactEmail,Status,UserID")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                _context.Update(clinic);
                TempData["message"] = "Your clinic status has been changed to " + clinic.Status + ".";
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageClinic", new { uID = clinic.UserID });
            }
            return View(clinic);

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

        public ActionResult PendingAppointment()
        {
            var userId = _userManager.GetUserId(User);
            var clinicName = (from a in _context.Clinic
                              where a.UserID.Equals(userId)
                              select a.ClinicName).Single();

            CloudTable table = TableStorage("AppointmentTable");

            string pendingList = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicName),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Waiting for Approval")
            );

            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(pendingList);


           List <Appointment> appointments = new List<Appointment>();
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

        public async Task<ActionResult> Approve (string PartitionKey, string RowKey)
        {
            await EditTable(PartitionKey, RowKey, "Approve");
            return RedirectToAction("PendingAppointment", "Clinic");
        }

        public async Task<ActionResult> Reject(string PartitionKey, string RowKey)
        {
            await EditTable(PartitionKey, RowKey, "Reject");
            return RedirectToAction("PendingAppointment", "Clinic");
        }

        public async Task<ActionResult> EditTable(string PartitionKey, string RowKey, string status)
        {
            CloudTable table = TableStorage("AppointmentTable");
            TableOperation retrieveOperation = TableOperation.Retrieve<Appointment>(PartitionKey, RowKey);
            TableResult result = table.ExecuteAsync(retrieveOperation).Result;
            Appointment updateEntity = (Appointment)result.Result;

            if (updateEntity != null)
            {
                //Change the description
                updateEntity.appStatus = status;

                // Create the InsertOrReplace TableOperation
                TableOperation update = TableOperation.Replace(updateEntity);

                // Execute the operation.
                await table.ExecuteAsync(update);
            }

            return RedirectToAction("PendingAppointment", "Clinic");
        }



        public ActionResult CompleteAppointment()
        {
            var userId = _userManager.GetUserId(User);
            var clinicName = (from a in _context.Clinic
                              where a.UserID.Equals(userId)
                              select a.ClinicName).Single();

            CloudTable table = TableStorage("AppointmentTable");

            string completeList = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Reject"),
            TableOperators.Or,
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Approve")
            );

            string completeList2 = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicName),
            TableOperators.And,
            completeList
            );

            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(completeList2);

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
    }
}