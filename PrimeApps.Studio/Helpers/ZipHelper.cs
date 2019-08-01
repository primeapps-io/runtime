using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PrimeApps.Studio.Helpers
{
	public static class ZipHelper
	{
		public static void CreateZip(string zipName, string dump)
		{
			using (MemoryStream zipStream = new MemoryStream())
			{
				using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
				{
					//Zip'in içine yazılacak olan file'ı oluşturuluyor
					ZipArchiveEntry entry = zip.CreateEntry($"{zipName}.sql");

					using (Stream entryStream = entry.Open())
					{

						var info = new UTF8Encoding(true).GetBytes(dump);
						new MemoryStream(info).CopyTo(entryStream);

					}

				}
				zipStream.Position = 0;

				using (var fs = File.Create($"{zipName}.zip"))
				{
					fs.Write(zipStream.ToArray(), 0, Convert.ToInt32(zipStream.Length));
				}
			}
		}

		public static void Unzip(string zipName)
		{
			ZipFile.ExtractToDirectory(zipName, @".\" + $"{ zipName}");
		}

	}
}