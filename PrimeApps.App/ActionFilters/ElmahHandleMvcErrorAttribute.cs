using System.Web.Mvc;
using Elmah;

namespace PrimeApps.App.ActionFilters
{
    public class ElmahHandleMvcErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            if (context.ExceptionHandled)
                ErrorSignal.FromCurrentContext().Raise(context.Exception);
        }
    }
}

