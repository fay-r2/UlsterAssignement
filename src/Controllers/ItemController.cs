namespace todo.Controllers
{
    using System;
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
        public async Task<IActionResult> Index()
        {
            return View(await _cosmosDbService.GetItemsAsync("SELECT * FROM c"));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [ActionName("Create")]
        public IActionResult Create()
        {
            return View();
        }


        [Route("PostQueue")]
        [HttpPost]
        public async Task PostToQueue([FromBody] SensorDataItem sensorDataItem)
        {
            sensorDataItem.Id = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(sensorDataItem,
            new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await _queueClient.SendMessageAsync(message);
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
