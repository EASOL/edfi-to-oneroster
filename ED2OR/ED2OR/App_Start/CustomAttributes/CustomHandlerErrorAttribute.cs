using ED2OR.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ED2OR.App_Start.CustomAttributes
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {

        public CustomHandleErrorAttribute()
        {
        }

        public override void OnException(ExceptionContext filterContext)
        {
            Guid errorId = Guid.NewGuid();
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        error = true,
                        message = filterContext.Exception.Message
                    }
                };
            }
            else
            {
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new CustomHandleErrorInfoModel(filterContext.Exception, controllerName, actionName, errorId);

                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<CustomHandleErrorInfoModel>(model),
                    TempData = filterContext.Controller.TempData
                };
            }
            //if (filterContext.RequestContext != null && filterContext.RequestContext.HttpContext != null && filterContext.RequestContext.HttpContext.Request.Form != null)
            //{
            //    string serializedData = string.Empty;
            //    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(NameValueCollection));
            //    using (System.IO.StringWriter strWriter = new System.IO.StringWriter())
            //    {
            //        serializer.Serialize(strWriter,filterContext.RequestContext.HttpContext.Request.Form);
            //        strWriter.Close();
            //        serializedData = strWriter.GetStringBuilder().ToString();
            //    }
            //}
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
