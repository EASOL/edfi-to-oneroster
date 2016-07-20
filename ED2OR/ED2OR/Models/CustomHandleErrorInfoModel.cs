using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ED2OR.Models
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
