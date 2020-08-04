using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

#if DEBUG
namespace God.Core
{ 
    /// 纳秒级计时器
    ///
    public class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long _startTime;
        private long _stopTime;
        private readonly long _freq;

        /// <summary>
        /// 纳秒计数器
        /// </summary>
        public HiPerfTimer()
        {
            _startTime = 0;
            _stopTime = 0;

            if (QueryPerformanceFrequency(out _freq) == false)
            {
                // 不支持高性能计数器 
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// 开始计时器
        /// </summary>
        public void Start()
        {
            // 来让等待线程工作 
            Thread.Sleep(0);
            QueryPerformanceCounter(out _startTime);
        }

        /// <summary>
        /// 启动一个新的计时器
        /// </summary>
        /// <returns></returns>
        public static HiPerfTimer StartNew()
        {
            HiPerfTimer timer = new HiPerfTimer();
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop()
        {
            QueryPerformanceCounter(out _stopTime);
        }

        /// <summary>
        /// 时器经过时间(单位：秒)
        /// </summary>
        public double Duration => (_stopTime - _startTime) / (double)_freq;
    }
}
#endif