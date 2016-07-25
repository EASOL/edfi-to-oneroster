using EF2OR.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EF2OR.App_Start.CustomAttributes
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
            Dictionary<string, string> formValuesDictionary = CreateDictionaryFromForm(filterContext.HttpContext.Request.Form);
            LogError(errorId, filterContext.Exception, formValuesDictionary);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        internal static void LogError(Guid errorId, Exception ex, Dictionary<string, string> dataObject)
        {
            try
            {
                using (Models.ApplicationDbContext ctx = new ApplicationDbContext())
                {
                    ErrorLog errorLogInfo = new ErrorLog();
                    errorLogInfo.ErrorId = errorId;
                    errorLogInfo.Message = ex.ToString();
                    XElement objectData = new System.Xml.Linq.XElement("ObjectData");
                    foreach (var singleItem in dataObject)
                    {
                        XElement propertyInfoElement = new XElement("PropertyInfo");
                        propertyInfoElement.SetAttributeValue("Name", singleItem.Key);
                        propertyInfoElement.SetAttributeValue("Value", singleItem.Value);
                        objectData.Add(propertyInfoElement);
                    }
                    errorLogInfo.Data = objectData.ToString();
                    ctx.ErrorLogs.Add(errorLogInfo);
                    ctx.SaveChanges();
                }

            }
            catch (Exception)
            {
                //do nothing
            }
        }

        internal static Dictionary<string, string> CreateDictionaryFromForm(NameValueCollection form)
        {
            Dictionary<string, string> formValuesDictionary = new Dictionary<string, string>();
            for (int i = 0; i < form.Count; i++)
            {
                try
                {
                    formValuesDictionary.Add(form.GetKey(i), form[form.GetKey(i)]);
                }
                catch (Exception)
                {

                }
            }
            return formValuesDictionary;
        }

    }
}
