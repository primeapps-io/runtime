using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.App.Extensions
{
    public static class HttpRequestExtensions
    {
	    /// <summary>
	    /// Retrieve the raw body as a string from the Request.Body stream
	    /// </summary>
	    /// <param name="request">Request instance to apply to</param>
	    /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
	    /// <returns></returns>
	    public static async Task<Stream> ReadAsStreamAsync(this HttpRequest request, Encoding encoding = null)
	    {
		    if (encoding == null)
			    encoding = Encoding.UTF8;

		    using (StreamReader reader = new StreamReader(request.Body, encoding))
		    {
			    var result = await reader.ReadToEndAsync();
			    // convert string to stream
			    byte[] byteArray = Encoding.UTF8.GetBytes(result);
			    //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			   return new MemoryStream(byteArray);

			}
	    }
	}
}
