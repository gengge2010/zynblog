using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;

namespace ZYN.BLOG.SiteSearch
{
    /// <summary>
    /// 石英钟定时任务
    /// </summary>
    public class QuartzTick
    {
        //网站一启动就执行此方法
        public static void StartJob()
        {
            IScheduler sched;  //计划者（IScheduler）、工作（IJob）、触发器（Trigger）
            ISchedulerFactory sfactory = new StdSchedulerFactory();
            sched = sfactory.GetScheduler();
            JobDetail job = new JobDetail("job1", "group1", typeof(QuartzJob));//IndexJob为实现了IJob接口的类,IndexJob是要干的活儿

            //指定开始时间
            DateTime ts = TriggerUtils.GetNextGivenSecondDate(null, 20);//20秒后开始第一次运行

            //指定多久执行一次
            TimeSpan interval = TimeSpan.FromMinutes(10);//每隔10分钟执行一次

            //触发器
            Trigger trigger = new SimpleTrigger("trigger1", "group1", "job1", "group1", ts, null,
                    SimpleTrigger.RepeatIndefinitely, interval);//每若干小时运行一次，小时间隔由appsettings中的IndexIntervalHour参数指定

            sched.AddJob(job, true);
            sched.ScheduleJob(trigger);
            sched.Start();
        }
    }
}
