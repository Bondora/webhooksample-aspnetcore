using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using webhooksite.Config;
using IOFile = System.IO.File;

namespace webhooksite.Controllers
{
    [Route("/api/hook")]
    [ApiController]
    public class HookController : Controller
    {
        private readonly ILogger logger;
        private readonly DataConfig dataConfig;

        public HookController(ILogger<HookController> logger, IOptions<DataConfig> dataConfig)
        {
            this.logger = logger;
            this.dataConfig = dataConfig.Value;
        }

        [HttpGet]
        public string Ping()
        {
            return $"OK, saving data to: {dataConfig.DataDir}, compress: {dataConfig.Compress}.";
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task Post()
        {
            var now = DateTime.UtcNow;

            var name = $"{now:yyyyMMdd_HH}/{now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.json";
            logger.LogInformation("Saving to file: {0}", name);

            await SaveContent(name, Request.Body);
        }

        private async Task SaveContent(string name, Stream input)
        {
            name = Path.Combine(dataConfig.DataDir, name);

            Directory.CreateDirectory(Path.GetDirectoryName(name));

            if (dataConfig.Compress)
                name += ".gz";

            Stream output = IOFile.Create(name);

            if (dataConfig.Compress)
                output = new GZipStream(output, CompressionLevel.Optimal);

            using (output)
                await input.CopyToAsync(output);
        }
    }
}
