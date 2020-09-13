using System;
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
        private static IFormFileCollection FILES;
        private const string _BLOBURL = "https://clinicappointmentdev2.blob.core.windows.net/doctor/";

        [HttpPost]
        public async Task<ActionResult> UploadAsync()
        {
            try
            {
                var userid = _userManager.GetUserId(User);
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
                if (AzureBlobService.blob_files != null)
                {
                    String url = _BLOBURL;
                    url += AzureBlobService.blob_files;
                    await DeleteImage(url);
                    await DeleteImage2(url);
                }
                FILES = files;
                await _azureBlobService.UploadAsync(files,userid);
                await _azureBlobService.UploadAsync2(files);
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

            var userid = _userManager.GetUserId(User);
            var clinicid = (from a in _context.Clinic
                            where a.UserID.Equals(userid)
                            select a.Id).Single();
            var clinicname = (from a in _context.Clinic
                            where a.Id.Equals(clinicid)
                            select a.ClinicName).Single();

            var model = new BlobModel();
            var result = (from a in _context.Doctor
                          where a.Clinic_Id.Equals(clinicid)
                          select new BlobModel()
                          {
                              id = a.id,
                              DoctorName = a.DoctorName,
                              DoctorContactNumber = a.DoctorContactNumber,
                              imgurl = a.profileImage,
                              clinic_name = clinicname,
                              clinic_id = clinicid
                              
                          }).ToList();

            model.doctor = result;
            if (result.Count() != 0)
                model.profileImage = await _azureBlobService.ListAsync("view",model);

            else { AzureBlobService.blob_files = null; return View(model); }

            if (model.profileImage.Count().Equals(0))
            {
                AzureBlobService.blob_files = null;
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
                var clinicid = (from a in _context.Clinic
                                where a.UserID.Equals(userId)
                                select a.Id).Single();
                var addDoctor = new Doctor
                {
                    DoctorName = doctor.DoctorName,
                    DoctorContactNumber = doctor.DoctorContactNumber,
                    profileImage = AzureBlobService.blob_files,
                    id = userId,
                    Clinic_Id = clinicid
                    
                };

                if (AzureBlobService.blob_files != null)
                {
                    String url = _BLOBURL;
                    url += AzureBlobService.blob_files;
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
                model.profileImage = await _azureBlobService.ListAsync("add",model);
                if (model.profileImage.Count().Equals(0))
                {
                    AzureBlobService.blob_files = null;
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
                CloudTable table = TableStorage("AppointmentTable");
                List<Appointment> appointments = Common.GetAppointmentsList(table);

                foreach (var item in appointments)
                {
                    if (item.PartitionKey.Equals(clinic.Id.ToString()))
                    {
                        
                        await EditTable(item.PartitionKey, item.RowKey, null, clinic.ClinicName);
                    }
                }
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
         
            var clinicID = (from a in _context.Clinic
                            where a.UserID.Equals(userId)
                            select a.Id).Single();
            CloudTable table = TableStorage("AppointmentTable");

            string pendingList = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicID.ToString()),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Waiting for Approval")
            );

            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(pendingList);


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

        public async Task<ActionResult> Approve(string PartitionKey, string RowKey)
        {       
            await EditTable(PartitionKey, RowKey, "Approve", null);
            TempData["message"] = "Appointment ID: " + RowKey + " status has updated to Approve.";
            return RedirectToAction("PendingAppointment", "Clinic");
        }

        public async Task<ActionResult> Reject(string PartitionKey, string RowKey)
        {
            await EditTable(PartitionKey, RowKey, "Reject", null);
            TempData["message"] = "Appointment ID: " + RowKey + " status has updated to Reject.";
            return RedirectToAction("PendingAppointment", "Clinic");
        }

        public async Task<ActionResult> Completed(string PartitionKey, string RowKey)
        {
            await EditTable(PartitionKey, RowKey, "Completed", null);
            TempData["message"] = "Appointment ID: " + RowKey + " status has updated to Completed.";
            return RedirectToAction("CompleteAppointment", "Clinic");
        }

        public async Task<ActionResult> EditTable(string PartitionKey, string RowKey, string status, string edited_clinic)
        {
            CloudTable table = TableStorage("AppointmentTable");
            TableOperation retrieveOperation = TableOperation.Retrieve<Appointment>(PartitionKey, RowKey);
            TableResult result = table.ExecuteAsync(retrieveOperation).Result;
            Appointment updateEntity = (Appointment)result.Result;
            
            if (updateEntity != null)
            {
                //Change the description
                if (status != null)
                {
                    updateEntity.appStatus = status;
                    await BusServiceQueue.SendQueueMsg(updateEntity.userID + "," + "Your Appointment: " + RowKey + " at " +updateEntity.clinicName +" has been approved."+","+"status");
                }
                if (edited_clinic != null)
                    updateEntity.clinicName = edited_clinic;
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
            
            var clinicID = (from a in _context.Clinic
                            where a.UserID.Equals(userId)
                            select a.Id).Single();
            CloudTable table = TableStorage("AppointmentTable");

            string completeList = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicID.ToString()),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Approve")
            );

            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(completeList);

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

        public ActionResult SearchForAppointment()
        {
            var userId = _userManager.GetUserId(User);
            var clinicId = (from a in _context.Clinic
                              where a.UserID.Equals(userId)
                              select a.Id).Single();
            ViewBag.ClinicId = clinicId;
            return View();
        }

        public ActionResult GetAppointment(string PartitionName, string RowName)
        {
            try
            {
                CloudTable table = TableStorage("AppointmentTable");

                TableOperation retrieveOperation = TableOperation.Retrieve<Appointment>(PartitionName, RowName);

                TableResult result = table.ExecuteAsync(retrieveOperation).Result;

                if (result.Etag != null)
                    return View(result);
                else
                    TempData["message"] = "There is no record for Appointment ID: " + RowName + ", please re-enter the Appointment ID.";
                return RedirectToAction("SearchForAppointment", "Clinic");
            }
            catch (Exception ex)
            {
                ViewBag.msg = "Error: " + ex.ToString();
            }
            return View();
        }

        public ActionResult HistoryAppointment()
        {
            var userId = _userManager.GetUserId(User);

            var clinicID = (from a in _context.Clinic
                            where a.UserID.Equals(userId)
                            select a.Id).Single();
            CloudTable table = TableStorage("AppointmentTable");

            string historyList = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicID.ToString()),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Completed")
            );

            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(historyList);

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

        public ActionResult FeedbackReceived()
        {
            var userId = _userManager.GetUserId(User);

            var clinicID = (from a in _context.Clinic
                            where a.UserID.Equals(userId)
                            select a.Id).Single();

            CloudTable table = TableStorage("FeedbackTable");

            string feedbackList = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicID.ToString()),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("appStatus", QueryComparisons.Equal, "Waiting for Approval")
            );

            TableQuery<Feedback> query = new TableQuery<Feedback>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, clinicID.ToString()));


            List<Feedback> feedbacks = new List<Feedback>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Feedback> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (Feedback fb in resultSegment.Results)
                {
                    feedbacks.Add(fb);
                }
            }
            while (token != null);

            return View(feedbacks);
        }

        public ActionResult FeedbackDetails(string PartitionKey, string RowKey)
        {
            try
            {
                CloudTable table = TableStorage("FeedbackTable");

                TableOperation retrieveOperation = TableOperation.Retrieve<Feedback>(PartitionKey, RowKey);

                TableResult result = table.ExecuteAsync(retrieveOperation).Result;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["message"] = "There is an error occured. Error: " + ex.ToString();
                return View();
            }

        }
    }
}