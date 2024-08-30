using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using cldvPart1.Models;

namespace cldvPart1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=cldvpart1b;AccountKey=SpLgyyqFaEilnRzTlSmnikR+FwCONq7PB6iBaK5FQi/Ta9Lq/6dY/SSt3U5xDh0SZqlZuQdlq7cy+ASt+lbYhg==;EndpointSuffix=core.windows.net ";
        private readonly string _tableName = "customerprofile";
        private readonly string _queueName = "customerqueue";
        private readonly string _fileShareName = "customerfileshare";
        private readonly string _blobContainerName = "customerblob";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult abcRetail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> abcRetail([FromBody] CustomerProfile customerProfile)
        {
            try
            {
                var tableClient = new TableClient(_connectionString, _tableName);
                customerProfile.RowKey = Guid.NewGuid().ToString(); // Set a unique RowKey
                await tableClient.UpsertEntityAsync(customerProfile);

                var queueClient = new QueueClient(_connectionString, _queueName);
                await queueClient.CreateIfNotExistsAsync();
                string message = $"Customer profile created/updated: {customerProfile.Name}";
                await queueClient.SendMessageAsync(message);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving customer profile.");
                return StatusCode(500, "Internal server error.");
            }
        }

        public async Task<IActionResult> DeleteCustomerProfile(string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = new TableClient(_connectionString, _tableName);
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
                return Ok("Customer profile deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer profile.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage()
        {
            var file = Request.Form.Files[0];
            if (file != null && file.Length > 0)
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_blobContainerName);
                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(file.FileName);
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                return Ok("File uploaded.");
            }

            return BadRequest("File not uploaded.");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileToShare()
        {
            var file = Request.Form.Files[0];
            if (file != null && file.Length > 0)
            {
                var shareClient = new ShareClient(_connectionString, _fileShareName);
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(file.FileName);
                using (var stream = file.OpenReadStream())
                {
                    await fileClient.CreateAsync(stream.Length);
                    await fileClient.UploadRangeAsync(new Azure.HttpRange(0, stream.Length), stream);
                }

                return Ok("File uploaded to share.");
            }

            return BadRequest("File not uploaded.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}