using BookCollectorWebAPI.Models;
using Newtonsoft.Json.Linq;
using Telegram.Bot;

namespace BookCollector
{
    public class FileHandler
    {
        private readonly IConfiguration _config;
        private readonly ITelegramBotClient _bot;
        enum ImageExtension
        {
            bmp, 
            jpeg,
            jpg, 
            png
        };
        enum BookDocumentExtension 
        {
            pdf,
            epub,
            djvu,
            fb2
        };
        public FileHandler(IConfiguration config, ITelegramBotClient bot)
        {
            _config = config;
            _bot = bot;
        }
        public bool IsImage(TelegramFileInfo documentInfo)
        {
            var documentName = documentInfo.Name;
            var extension = documentName[(documentName.LastIndexOf(".")+1)..];
            return Enum.TryParse<ImageExtension>((extension.ToLower()), out _);
        }

        public bool IsBookDocument(TelegramFileInfo documentInfo)
        {
            var documentName = documentInfo.Name;
            var extension = documentName[(documentName.LastIndexOf(".") + 1)..];
            return Enum.TryParse<BookDocumentExtension>((extension.ToLower()), out _);
        }

        public async Task<bool> IsSecureFileAsync(TelegramFileInfo fileInfo)
        {
            // Step 1: Initialize
            var httpClient = new HttpClient();
            string virusTotalScanUrl = _config.GetSection("VirusTotal:ScanUrl").Value!;
            string virusTotalReportUrl = _config.GetSection("VirusTotal:ReportUrl").Value!;
            string apiKey = _config.GetSection("VirusTotal:ApiKey").Value!;

            // Step 2: Upload file to VirusTotal for scanning
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileInfo.Bytes);
            content.Add(fileContent, "file", "file");

            var response = await httpClient.PostAsync(virusTotalScanUrl + $"?apikey={apiKey}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var scanId = responseContent.Trim(); // Assuming response contains scanId directly

            // Step 3: Retrieve scan report
            response = await httpClient.GetAsync(virusTotalReportUrl + $"?apikey={apiKey}&resource={scanId}");
            response.EnsureSuccessStatusCode();

            dynamic reportResponse = JObject.Parse(responseContent);
            int? positives = reportResponse.positives ?? null;

            // Step 4: Analyze scan result
            if (positives != null && positives > 0)
            {
                Console.WriteLine($"File is infected. VirusTotal detected {positives} positives.");
                return false;
            }
            else
            {
                Console.WriteLine("File is clean. No threats detected by VirusTotal.");
                return true;
            }
            

        }

        public async Task<TelegramFileInfo?> DownloadFileAsBytesAsync(string? fileId)
        {
            if (fileId == null)
                return null;
            var file = await _bot.GetFileAsync(fileId);
            if (file == null) return null;
            using (var fileStream = new MemoryStream())
            {
                await _bot.DownloadFileAsync(file.FilePath!, fileStream);
                return new TelegramFileInfo(fileStream.ToArray(), file.FilePath![file.FilePath!.LastIndexOf("/")..]);
            }
        }
    }

    
}
