using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// HttpMultipartParser
/// Reads a multipart http data stream and returns the file name, content type and file content.
/// Also, it returns any additional form parameters in a Dictionary.
/// </summary>
namespace PrimeApps.Studio.Helpers
{
    public class HttpMultipartParser
    {
        public HttpMultipartParser(Stream stream, string filePartName)
        {
            FilePartName = filePartName;
            this.Parse(stream, Encoding.ASCII);
        }

        public HttpMultipartParser(Stream stream, Encoding encoding, string filePartName)
        {
            FilePartName = filePartName;
            this.Parse(stream, encoding);
        }

        private void Parse(Stream stream, Encoding encoding)
        {
            this.Success = false;

            // Read the stream into a byte array
            byte[] data = Extensions.ToByteArray(stream);

            // Copy to a string for header parsing
            string content = encoding.GetString(data);

            // The first line should contain the delimiter
            int delimiterEndIndex = content.IndexOf("\r\n");

            if (delimiterEndIndex > -1)
            {
                string delimiter = content.Substring(0, content.IndexOf("\r\n"));

                string[] sections = content.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in sections)
                {
                    if (s.Contains("Content-Disposition") || s.Contains("content-disposition"))
                    {
                        // If we find "Content-Disposition", this is a valid multi-part section
                        // Now, look for the "name" parameter
                        Match nameMatch = new Regex(@"(?<=name\=\"")(.*?)(?=\"")").Match(s);
                        string name = nameMatch.Value.Trim().ToLower();

                        if (name == FilePartName)
                        {
                            // Look for Content-Type
                            Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");

                            if (content.Contains("content-type"))
                                re = new Regex(@"(?<=content\-type:)(.*?)(?=\r\n\r\n)");

                            if (content.Contains("Content-Length"))//Cordova file transfer plugin sends Content-Length when chunked upload. This is workaround solution for this bug. 
                                re = new Regex(@"(?<=Content\-Length:)(.*?)(?=\r\n\r\n)");

                            if (content.Contains("content-length"))
                                re = new Regex(@"(?<=content\-length:)(.*?)(?=\r\n\r\n)");

                            Match contentTypeMatch = re.Match(content);

                            // Look for filename
                            re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                            Match filenameMatch = re.Match(content);

                            // Did we find the required values?
                            if (contentTypeMatch.Success && filenameMatch.Success)
                            {
                                // Set properties
                                this.ContentType = contentTypeMatch.Value.Trim();
                                this.Filename = filenameMatch.Value.Trim();

                                if (content.Contains("Content-Length") || content.Contains("content-length"))//Cordova file transfer plugin sends Content-Length when chunked upload. This is workaround solution for this bug. 
                                    this.ContentType = "image/jpeg";

                                // Get the start & end indexes of the file contents
                                int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                                byte[] delimiterBytes = encoding.GetBytes("\r\n" + delimiter);
                                int endIndex = Extensions.IndexOf(data, delimiterBytes, startIndex);

                                int contentLength = endIndex - startIndex;

                                // Extract the file contents from the byte array
                                byte[] fileData = new byte[contentLength];

                                Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                                this.FileContents = fileData;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(name))
                        {
                            // Get the start & end indexes of the file contents
                            int startIndex = nameMatch.Index + nameMatch.Length + "\r\n\r\n".Length;
                            Parameters.Add(name, s.Substring(startIndex).TrimEnd(new char[] { '\r', '\n' }).Trim());
                        }
                    }
                }

                // If some data has been successfully received, set success to true
                if (FileContents != null || Parameters.Count != 0)
                    this.Success = true;
            }
        }

        public IDictionary<string, string> Parameters = new Dictionary<string, string>();

        public string FilePartName
        {
            get;
            private set;
        }

        public bool Success
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }

        public int UserId
        {
            get;
            private set;
        }

        public string ContentType
        {
            get;
            private set;
        }

        public string Filename
        {
            get;
            private set;
        }

        public byte[] FileContents
        {
            get;
            private set;
        }
    }
}