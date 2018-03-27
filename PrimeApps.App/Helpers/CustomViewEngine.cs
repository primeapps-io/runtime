using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrimeApps.App.Helpers
{
    public class CustomViewEngine : RazorViewEngine
    {
        public CustomViewEngine()
        {
            var viewLocationFormats = new[]
            {
                "~/ViewsMvc/{1}/{0}.cshtml",
                "~/ViewsMvc/{1}/{0}.vbhtml",
                "~/ViewsMvc/Shared/{0}.cshtml",
                "~/ViewsMvc/Shared/{0}.vbhtml"
            };

            var masterLocationFormats = new[]
            {
                "~/ViewsMvc/{1}/{0}.cshtml",
                "~/ViewsMvc/{1}/{0}.vbhtml",
                "~/ViewsMvc/Shared/{0}.cshtml",
                "~/ViewsMvc/Shared/{0}.vbhtml"
            };

            var partialViewLocationFormats = new[]
            {
                "~/ViewsMvc/{1}/{0}.cshtml",
                "~/ViewsMvc/{1}/{0}.vbhtml",
                "~/ViewsMvc/Shared/{0}.cshtml",
                "~/ViewsMvc/Shared/{0}.vbhtml"
            };

            ViewLocationFormats = viewLocationFormats;
            MasterLocationFormats = masterLocationFormats;
            PartialViewLocationFormats = partialViewLocationFormats;
        }
    }
}