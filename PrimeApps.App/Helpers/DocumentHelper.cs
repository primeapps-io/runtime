using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Document;

namespace PrimeApps.App.Helpers
{
    public static class DocumentHelper
    {
        public static bool Upload(Stream stream, out DocumentUploadResult result)
        {
            var parser = new HttpMultipartParser(stream, "file");
            result = null;

            //if it is not successfully parsed return.
            if (!parser.Success)
                return false;

            //check the file size if it is 0 bytes then return.
            if (parser.FileContents.Length <= 0)
            {
                result = new DocumentUploadResult();
                return false;
            }

            //declare chunk variables
            var chunk = 0;//current chunk
            var chunks = 1;//total chunk count

            var uniqueName = string.Empty;

            //if parser has more then 1 parameters, it means that request is chunked.
            if (parser.Parameters.Count > 1)
            {
                //calculate chunk variables.
                chunk = int.Parse(parser.Parameters["chunk"]);
                chunks = int.Parse(parser.Parameters["chunks"]);

                //get the file name from parser
                if (parser.Parameters.ContainsKey("name"))
                    uniqueName = parser.Parameters["name"];
            }

            if (string.IsNullOrEmpty(uniqueName))
                uniqueName = Guid.NewGuid().ToString().Replace("-", "") + "--" + parser.Filename;

            //send stream and parameters to storage upload helper method for temporary upload.
            Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

            result = new DocumentUploadResult
            {
                UniqueName = uniqueName,
                FileName = parser.Filename,
                ContentType = parser.ContentType,
                Chunks = chunks
            };

            return true;
        }

        public static string Save(DocumentUploadResult result, string containerName)
        {
            var blob = Storage.CommitFile(result.UniqueName, result.UniqueName, result.ContentType, containerName, result.Chunks);
            var fileUrl = $"{ConfigurationManager.AppSettings.Get("BlobUrl")}{blob.Uri.AbsolutePath}";

            return fileUrl;
        }

        public static async Task<int> UploadSampleDocuments(Guid instanceId, int appId, string language)
        {
            if (appId == 1 || appId == 2 || appId == 5)
            {
                // Upload quote template
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri($"http://file.ofisim.com/static/quote-template-{language}.docx"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", $"quote-template-{language}.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    Storage.CommitFile($"quote-template-{language}.docx", $"templates/quote-template-{language}.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }
            }

            if (appId == 3 || appId == 4)
            {
                // Upload female picture

                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/female.jpg"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "female.jpg", "image/jpeg");
                    Storage.CommitFile("female.jpg", "female.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload male picture
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/male.jpg"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "male.jpg", "image/jpeg");
                    Storage.CommitFile("male.jpg", "male.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload empty profile picture
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/empty-profile.png"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "empty-profile.png", "image/jpeg");
                    Storage.CommitFile("empty-profile.png", "empty-profile.png", "image/jpeg", $"record-detail-{instanceId}", 1);
                }
            }

            if (appId == 4)
            {
                //1 Upload calisma-belgesi.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/calisma-belgesi.docx"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "calisma-belgesi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    Storage.CommitFile("calisma-belgesi.docx", "templates/calisma-belgesi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //2 Upload izin-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/izin-formu.docx"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "izin-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    Storage.CommitFile("izin-formu.docx", "templates/izin-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //3 Upload arac-zimmet-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/arac-zimmet-formu.docx"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "arac-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    Storage.CommitFile("arac-zimmet-formu.docx", "templates/arac-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //4 Upload calisan-ozgecmisi.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/calisan-ozgecmisi.docx"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "calisan-ozgecmisi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    Storage.CommitFile("calisan-ozgecmisi.docx", "templates/calisan-ozgecmisi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //5 Upload cihaz-zimmet-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/cihaz-zimmet-formu.docx"));

                    Storage.UploadFile(0, new MemoryStream(fileContent), "temp", "cihaz-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    Storage.CommitFile("cihaz-zimmet-formu.docx", "templates/cihaz-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

            }

            return 0;
        }

        public static string GetType(string name)
        {
            var type = name.Split('.')[1];
            switch (type)
            {
                case "gif":
                    return "image/bmp";
                case "bmp":
                    return "image/bmp";
                case "jpeg":
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "tif":
                case "tiff":
                    return "image/tiff";
                case "doc":
                    return "application/msword";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "pdf":
                    return "application/pdf";
                case "ppt":
                    return "application/vnd.ms-powerpoint";
                case "pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xls":
                    return "application/vnd.ms-excel";
                case "csv":
                    return "text/csv";
                case "xml":
                    return "text/xml";
                case "txt":
                    return "text/plain";
                case "zip":
                    return "application/zip";
                case "ogg":
                    return "application/ogg";
                case "mp3":
                    return "audio/mpeg";
                case "wma":
                    return "audio/x-ms-wma";
                case "wav":
                    return "audio/x-wav";
                case "wmv":
                    return "audio/x-ms-wmv";
                case "swf":
                    return "application/x-shockwave-flash";
                case "avi":
                    return "video/avi";
                case "mp4":
                    return "video/mp4";
                case "mpeg":
                    return "video/mpeg";
                case "mpg":
                    return "video/mpeg";
                case "qt":
                    return "video/quicktime";
                default:
                    return "image/jpeg";
            }
        }
    }
}