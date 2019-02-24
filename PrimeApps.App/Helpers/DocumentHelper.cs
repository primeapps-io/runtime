using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Storage;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Helpers
{
	public interface IDocumentHelper
	{
		bool Upload(Stream stream, out DocumentUploadResult result);
		bool UploadExcel(Stream stream, out DocumentUploadResult result);
		Task<string> Save(DocumentUploadResult result, string containerName);
		Task<int> UploadSampleDocuments(Guid instanceId, int appId, string language, IPlatformRepository _platformRepository);
		string GetMimeType(string name);

	}
	public class DocumentHelper : IDocumentHelper
	{
		private IConfiguration _configuration;
		public DocumentHelper(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public bool Upload(Stream stream, out DocumentUploadResult result)
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
			AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType, _configuration).Wait();

			result = new DocumentUploadResult
			{
				UniqueName = uniqueName,
				FileName = parser.Filename,
				ContentType = parser.ContentType,
				Chunks = chunks
			};

			return true;
		}

		public bool UploadExcel(Stream stream, out DocumentUploadResult result)
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
			AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType, _configuration).Wait();

			result = new DocumentUploadResult
			{
				UniqueName = uniqueName,
				FileName = parser.Filename,
				ContentType = parser.ContentType,
				Chunks = chunks
			};

			return true;
		}

		public async Task<string> Save(DocumentUploadResult result, string containerName)
		{
			var blob = await AzureStorage.CommitFile(result.UniqueName, result.UniqueName, result.ContentType, containerName, result.Chunks, _configuration);
			var storageUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);
			var fileUrl = "";
			if (!string.IsNullOrEmpty(storageUrl))
			{
				fileUrl = $"{storageUrl}{blob.Uri.AbsolutePath}";
			}

			return fileUrl;
		}

		public async Task<int> UploadSampleDocuments(Guid instanceId, int appId, string language, IPlatformRepository _platformRepository)
		{
			var templates = await _platformRepository.GetAppTemplate(appId, AppTemplateType.Document, language, null);
			foreach (var template in templates)
			{
				var req = JsonConvert.DeserializeObject<JObject>(template.Settings);
				//Upload quote template
				using (var httpClient = new HttpClient())
				{
					var fileContent = await httpClient.GetByteArrayAsync(new Uri(template.Content));

					await AzureStorage.UploadFile(0, new MemoryStream(fileContent), "temp", (string)req["FileName"], (string)req["MimeType"], _configuration);
					await AzureStorage.CommitFile((string)req["FileName"], (string)req["FolderName"], (string)req["MimeType"], $"inst-{instanceId}", 1, _configuration);
				}
			}

			return 0;
		}

		public string GetMimeType(string name)
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