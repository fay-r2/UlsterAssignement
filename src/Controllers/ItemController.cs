namespace todo.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Storage.Queues;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using todo.Models;


    // [Route("api/[controller]")]
    // [ApiController]
    public class ItemController : Controller
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly QueueClient _queueClient;
        public ItemController(ICosmosDbService cosmosDbService, QueueClient queueClient)
        {
            _cosmosDbService = cosmosDbService;
            _queueClient = queueClient;

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [ActionName("Index")]
        public async Task<IActionResult> Index(string start, string end, string sensorId)
        {
            ViewBag.start = start;
            ViewBag.end = end;
            if (!String.IsNullOrEmpty(sensorId))
            {
                ViewBag.sensorId = sensorId;
                return View(await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.SensorUUID = \"{sensorId}\""));

            }


            var result = await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.TimeDate <= \"{start}\" AND c.TimeDate >= \"{end}\"");
            if (result.Any())
            {
                return View(result);
            }
            else
            {
                return View(await _cosmosDbService.GetItemsAsync("SELECT * FROM c"));
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [ActionName("Create")]
        public IActionResult Create()
        {
            return View();
        }


        [Route("PostQueue")]
        [HttpPost]
        public async Task<System.Web.Mvc.HttpStatusCodeResult> PostToQueue([FromBody] SensorDataItem sensorDataItem)
        {
            sensorDataItem.Id = Guid.NewGuid().ToString();
            sensorDataItem.SensorUUID = sensorDataItem.SensorHardwareID + "_" + sensorDataItem.SensorClass.ToString() + "_" + sensorDataItem.DeviceMfg.ToString();

            DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long dateTimeOffSet = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000;
            double nanoseconds = dateTimeOffSet;
            sensorDataItem.TimeStamp = nanoseconds;
            DateTime result = epochTime.AddTicks((long)(nanoseconds / 100));

            string errors = Validate(sensorDataItem);


            if (errors != null)
            {
                return new System.Web.Mvc.HttpStatusCodeResult(HttpStatusCode.BadRequest, errors.ToString());
            }

            var message = JsonConvert.SerializeObject(sensorDataItem,
            new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await _queueClient.SendMessageAsync(message);
            return new System.Web.Mvc.HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private static string Validate(SensorDataItem sensorDataItem)
        {
            var errors = new List<string>();


            if (sensorDataItem.SensorHardwareID == null || !sensorDataItem.SensorHardwareID.Any())
            {
                errors.Add("SensorHardwareID is empty");
            }

            if (sensorDataItem.SensorClass == (1 ^ 2))
            {
                errors.Add("SensorClass should equal 1 or 2");
            }

            if (sensorDataItem.DeviceMfg != 2)
            {
                errors.Add("DeviceMfg should equal 2");
            }

            if (sensorDataItem.BlobJson.Reading < -50 || sensorDataItem.BlobJson.Reading > 150)
            {
                errors.Add("Sensor reading out of range (-50°C to 150°C)");
            }

            if (errors.Any())
            {
                var errorBuilder = new StringBuilder();

                errorBuilder.AppendLine("Invalid item, reason: ");

                foreach (var error in errors)
                {
                    errorBuilder.AppendLine(error);
                }

                return errorBuilder.ToString();
            }
            return null;
        }


        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<ActionResult> CreateAsync([Bind("Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                item.Id = Guid.NewGuid().ToString();
                await _cosmosDbService.AddItemAsync(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<ActionResult> EditAsync([Bind("Id,SensorUUID,SensorHardwareID,TimeStamp,DeviceMfg,SensorClass,Width,Height,Unit,Reading,FrameData")] SensorDataItemID item)

        {
            if (ModelState.IsValid)
            {
                await _cosmosDbService.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [ActionName("Edit")]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            SensorDataItemID item = await _cosmosDbService.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            SensorDataItemID item = await _cosmosDbService.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await _cosmosDbService.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            return View(await _cosmosDbService.GetItemAsync(id));
        }
    }
}
