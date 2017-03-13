using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using System.Web.SessionState;
using ZYN.BLOG.IBLL;

namespace ZYN.BLOG.WebHelper
{
    public class OperateHelper
    {
        public IBLL.IServiceSession serviceSession;

        public OperateHelper()
        {
            serviceSession = Inject.SpringHelper.GetObject<IServiceSession>("ServiceSession");
        }

        public static OperateHelper Current
        {
            get
            {
                OperateHelper operateHelper = CallContext.GetData(typeof(OperateHelper).Name) as OperateHelper;
                if (operateHelper == null)
                {
                    operateHelper = new OperateHelper();
                    CallContext.SetData(typeof(OperateHelper).Name, operateHelper);
                }
                return operateHelper;
            }
        }

        #region  Http上下文 及 相关属性
        public HttpContext ContextHttp
        {
            get
            {
                return HttpContext.Current;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return ContextHttp.Response;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return ContextHttp.Request;
            }
        }

        public HttpSessionState Session
        {
            get
            {
                return ContextHttp.Session;
            }
        }
        #endregion
    }
}
