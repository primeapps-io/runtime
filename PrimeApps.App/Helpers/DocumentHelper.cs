using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PrimeApps.App.Storage;
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
            AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

            result = new DocumentUploadResult
            {
                UniqueName = uniqueName,
                FileName = parser.Filename,
                ContentType = parser.ContentType,
                Chunks = chunks
            };

            return true;
        }

        public static bool UploadExcel(Stream stream, out DocumentUploadResult result)
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
            //TODO Removed
            //Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

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
            var blob = AzureStorage.CommitFile(result.UniqueName, result.UniqueName, result.ContentType, containerName, result.Chunks);
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

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", $"quote-template-{language}.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile($"quote-template-{language}.docx", $"templates/quote-template-{language}.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }
            }

            if (appId == 3 || appId == 4)
            {
                // Upload female picture

                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/female.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "female.jpg", "image/jpeg");
                    AzureStorage.CommitFile("female.jpg", "female.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload male picture
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/male.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "male.jpg", "image/jpeg");
                    AzureStorage.CommitFile("male.jpg", "male.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload empty profile picture
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/empty-profile.png"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "empty-profile.png", "image/jpeg");
                    AzureStorage.CommitFile("empty-profile.png", "empty-profile.png", "image/jpeg", $"record-detail-{instanceId}", 1);
                }
            }

            if (appId == 4)
            {
                //1 Upload calisma-belgesi.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/calisma-belgesi.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "calisma-belgesi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("calisma-belgesi.docx", "templates/calisma-belgesi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //2 Upload izin-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/izin-formu.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "izin-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("izin-formu.docx", "templates/izin-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //3 Upload arac-zimmet-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/arac-zimmet-formu.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "arac-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("arac-zimmet-formu.docx", "templates/arac-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //4 Upload calisan-ozgecmisi.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/calisan-ozgecmisi.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "calisan-ozgecmisi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("calisan-ozgecmisi.docx", "templates/calisan-ozgecmisi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //5 Upload cihaz-zimmet-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/cihaz-zimmet-formu.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "cihaz-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("cihaz-zimmet-formu.docx", "templates/cihaz-zimmet-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

            }
            if (appId == 6)
            {
                //1 Upload bireysel-siparis-teslim-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/bireysel-siparis-teslim-formu.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "bireysel-siparis-teslim-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("bireysel-siparis-teslim-formu.docx", "templates/calisma-belgesi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //2 Upload firmaya-ozel-teklif-kdv-dahil.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/firmaya-ozel-teklif-kdv-dahil.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "firmaya-ozel-teklif-kdv-dahil.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("firmaya-ozel-teklif-kdv-dahil.docx", "templates/firmaya-ozel-teklif-kdv-dahil.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //3 Upload firmaya-ozel-teklif-kdvsiz.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/firmaya-ozel-teklif-kdvsiz.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "firmaya-ozel-teklif-kdvsiz.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("firmaya-ozel-teklif-kdvsiz.docx", "templates/firmaya-ozel-teklif-kdvsiz.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //4 Upload gorev-ciktisi.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/gorev-ciktisi.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "gorev-ciktisi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("gorev-ciktisi.docx", "templates/gorev-ciktisi.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //5 Upload ozel-pasta-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/ozel-pasta-formu.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "ozel-pasta-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("ozel-pasta-formu.docx", "templates/ozel-pasta-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                //6 Upload ozel-pasta-formu.docx
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/ozel-pasta-formu.docx"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "ozel-pasta-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    AzureStorage.CommitFile("ozel-pasta-formu.docx", "templates/ozel-pasta-formu.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"inst-{instanceId}", 1);
                }

                // Upload kiralama-alani1.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/kiralama-alani1.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "kiralama-alani1.jpg", "image/jpeg");
                    AzureStorage.CommitFile("kiralama-alani1.jpg", "kiralama-alani1.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload kiralama-alani2.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/kiralama-alani2.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "kiralama-alani2.jpg", "image/jpeg");
                    AzureStorage.CommitFile("kiralama-alani2.jpg", "kiralama-alani2.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload kiralama-alani3.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/kiralama-alani3.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "kiralama-alani3.jpg", "image/jpeg");
                    AzureStorage.CommitFile("kiralama-alani3.jpg", "kiralama-alani3.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload kiralama-alani4.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/kiralama-alani4.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "kiralama-alani4.jpg", "image/jpeg");
                    AzureStorage.CommitFile("kiralama-alani4.jpg", "kiralama-alani4.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload ozel-pasta1.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/ozel-pasta1.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "ozel-pasta1.jpg", "image/jpeg");
                    AzureStorage.CommitFile("ozel-pasta1.jpg", "ozel-pasta1.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload ozel-pasta2.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/ozel-pasta2.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "ozel-pasta2.jpg", "image/jpeg");
                    AzureStorage.CommitFile("ozel-pasta2.jpg", "ozel-pasta2.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload ozel-pasta3.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/ozel-pasta3.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "ozel-pasta3.jpg", "image/jpeg");
                    AzureStorage.CommitFile("ozel-pasta3.jpg", "ozel-pasta3.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
                }

                // Upload ozel-pasta4.jpg
                using (var httpClient = new HttpClient())
                {
                    var fileContent = await httpClient.GetByteArrayAsync(new Uri("http://file.ofisim.com/static/livasmart/ozel-pasta4.jpg"));

                    AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", "ozel-pasta4.jpg", "image/jpeg");
                    AzureStorage.CommitFile("ozel-pasta4.jpg", "ozel-pasta4.jpg", "image/jpeg", $"record-detail-{instanceId}", 1);
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