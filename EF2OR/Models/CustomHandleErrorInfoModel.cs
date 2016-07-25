using System;
using System.Web.Mvc;

namespace EF2OR.Models
{
    public class CustomHandleErrorInfoModel : HandleErrorInfo
    {
        public Guid ErrorId { get; set; }

        public CustomHandleErrorInfoModel(Exception exception, string controllerName, string actionName, Guid errorId) : base(exception, controllerName, actionName)
        {
            ErrorId = errorId;
        }
    }
}
