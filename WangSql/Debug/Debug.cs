using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WangSql
{
    /// <summary>
    /// 调试模式，打印日志。正式环境务必关闭调试模式。
    /// </summary>
    internal class Debug
    {
        private static readonly DbugQueue dbugQueue;
        private static readonly DebugMonitor debugMonitor;
        private static readonly object _obj_lock = new object();

        static Debug()
        {
            if (dbugQueue == null)
                lock (_obj_lock)
                    if (dbugQueue == null)
                    {
                        dbugQueue = new DbugQueue();
                        debugMonitor = new DebugMonitor();
                        debugMonitor.Start();
                    }
        }

        public static void WriteLine(string msg)
        {
            dbugQueue.Enqueue(msg);
        }

        public static List<string> Dequeue(int count = 1)
        {
            return dbugQueue.Dequeue(count);
        }
    }

    #region 队列
    internal class DbugQueue
    {
        private ConcurrentQueue<string> Data = new ConcurrentQueue<string>();
        private int Size = 10000;

        public DbugQueue()
        {
        }

        public DbugQueue(int size)
        {
            Size = size;
        }

        public void Enqueue(string msg)
        {
            if (Data.Count < Size)
            {
                Data.Enqueue(msg);
            }
        }

        public List<string> Dequeue(int count = 1)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (Data.TryDequeue(out string str))
                {
                    result.Add(str);
                }
            }
            return result;
        }
    }
    #endregion

    #region 定时任务
    internal class BaseThreadLisence
    {
        private Thread _Thread;
        private bool _IsRun = false;
        /// <summary>
        /// 间隔 毫秒
        /// </summary>
        public int ThreadTime { get; set; }
        /// <summary>
        /// 是否为后台线程
        /// </summary>
        public bool IsBackground { get; set; }

        public BaseThreadLisence()
        {
            ThreadTime = 1000;
            IsBackground = true;

            _Thread = new Thread(new ThreadStart(this.threadWork));
            _Thread.IsBackground = IsBackground;
        }

        protected virtual void p_dowork()
        {
        }

        public void Start()
        {
            if (this._IsRun) return;
            if (this._Thread != null)
                this._Thread.Start();
            _IsRun = true;
        }
        public void Stop()
        {
            this._IsRun = false;
        }

        private void threadWork()
        {
            while (true)
            {
                try
                {
                    if (this._IsRun)
                        this.p_dowork();
                }
                catch { }
                finally
                {
                    Thread.Sleep(this.ThreadTime);
                }
            }
        }
    }
    internal class DebugMonitor : BaseThreadLisence
    {
        private bool _d = false;

        public DebugMonitor()
        {
            this.ThreadTime = 2000;
        }

        protected override void p_dowork()
        {
            try
            {
                if (!_d)
                {
                    _d = true;
                    var list = Debug.Dequeue(100);
                    foreach (var item in list)
                    {
                        Console.WriteLine(item);
                    }
                    _d = false;
                }
            }
            catch
            {
                _d = false;
            }
            finally
            {
                Thread.Sleep(ThreadTime);
            }
        }
    }
    #endregion

}
