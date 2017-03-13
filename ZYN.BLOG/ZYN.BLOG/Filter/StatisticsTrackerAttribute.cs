﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZYN.BLOG.Filter
{
    /// <summary>
    /// 监控类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class StatisticsTrackerAttribute : ActionFilterAttribute, IExceptionFilter
    {
        private readonly string Key = "_thisOnActionMonitorLog_";

        #region Action时间监控
        /// <summary>
        /// Action执行前
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MonitorLog MonLog = new MonitorLog();
            MonLog.ExecuteStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff", DateTimeFormatInfo.InvariantInfo));
            MonLog.ControllerName = filterContext.RouteData.Values["controller"] as string;
            MonLog.ActionName = filterContext.RouteData.Values["action"] as string;

            filterContext.Controller.ViewData[Key] = MonLog;  //将日志监控对象存入当前这个Controller的ViewData视图中
        }

        /// <summary>
        /// Action执行后
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            MonitorLog MonLog = filterContext.Controller.ViewData[Key] as MonitorLog;
            MonLog.ExecuteEndTime = DateTime.Now;
            MonLog.FormCollections = filterContext.HttpContext.Request.Form; //form表单提交的数据
            MonLog.QueryCollections = filterContext.HttpContext.Request.QueryString; //Url 参数

            //Action执行完之后调用一次Log4net记录日志
            LoggerHelper.Monitor(MonLog.GetLoginfo());
        }
        #endregion

        #region View 视图生成时间监控
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            MonitorLog MonLog = filterContext.Controller.ViewData[Key] as MonitorLog;

            MonLog.ExecuteStartTime = DateTime.Now;
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            MonitorLog MonLog = filterContext.Controller.ViewData[Key] as MonitorLog;

            MonLog.ExecuteEndTime = DateTime.Now;

            LoggerHelper.Monitor(MonLog.GetLoginfo(MonitorLog.MonitorType.View));

            //执行结束后清楚这个ViewData
            filterContext.Controller.ViewData.Remove(Key);
        }
        #endregion

        #region 错误日志

        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                string ControllerName = string.Format("{0}Controller", filterContext.RouteData.Values["controller"] as string);
                string ActionName = filterContext.RouteData.Values["action"] as string;
                string ErrorMsg = string.Format("在执行 controller[{0}] 的 action[{1}] 时产生异常", ControllerName, ActionName);
                LoggerHelper.Error(ErrorMsg, filterContext.Exception);
            }
        }
        #endregion
    }
}