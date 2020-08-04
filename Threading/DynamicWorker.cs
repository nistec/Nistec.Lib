﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Threading
{

    public enum DynamicWaitType
    {
        None,
        DynamicWait,
        ResetEvent,
    }

    public interface IDynamicWait
    {
        int DynamicWaitAck(bool ack);
        //void EventSet();

        bool EnableResetEvent { get; }

        bool EnableDynamicWait { get; }

        //int DynamicWait { get;}
    }

    public class DynamicWait : IDynamicWait
    {

        public static IDynamicWait Empty { get { return new DynamicWait(); } }

        public int DynamicWaitAck(bool ack) { return 0; }
        //void EventSet();

        public bool EnableResetEvent { get { return false; } }

        public bool EnableDynamicWait { get { return false; } }

    }
    /*
     * Sample:
     DynamicWorker ActionWorker;

        public void StartDynamicWorker() {

            if (EnablePersistQueue)
            {
                if (ActionWorker != null)
                    return;

                ActionWorker = new DynamicWorker()
                {
                    ActionLog = (LogLevel level,string message) =>
                    {
                        if (_Logger != null)
                            _Logger.Log((LoggerLevel)level, message);
                    },
                    ActionTask = () =>
                    {
                        try
                        {
                            var item = Dequeue();
                            if (item != null)
                            {
                                OnItemReceived(item);
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_Logger != null)
                                _Logger.Exception("Topic Sender Worker error ", ex);
                        }
                        return false;
                    },
                    Interval = 100,
                    MaxThreads = 1
                };
                ActionWorker.Start();
            }
        }
    */
    public class DynamicInterval
    {
        public DynamicInterval(int defaultWait = 1000, int minWait=100, int largeWait=5000,int maxThread=1) {

            IntervalWait = defaultWait;
            MaxThread = (maxThread < 1 || maxThread > DynamicWorker.MAXTHREAD) ? DynamicWorker.MAXTHREAD : maxThread;

            MinWait = (minWait < TinyWait || minWait> largeWait) ? TinyWait : minWait;
            LargeWait = (largeWait > MaxWait || largeWait< minWait) ? MaxWait : largeWait;
            MedWait = (LargeWait - MinWait) / 3;

            LargeWaitStep = LargeWait / 10;
            MedWaitStep = MedWait / 10;
            MinWaitStep = MinWait / 10;
        }
        const int MaxWait = 1000000;
        const int TinyWait = 10;

        int LargeWait = 5000;
        int MedWait = 500;
        int MinWait = 100;

        int LargeWaitStep = 1000;
        int MedWaitStep = 100;
        int MinWaitStep = 10;
        int TinyWaitStep = 1;

        int IntervalWait = 0;
        int _MaxThread = 1;
        //public bool EnableDynamicWait { get; set; }

        public int MaxThread { get; private set; }
        
        public int DynamicWait
        {
            get { return (int)(IntervalWait < TinyWait ? TinyWait : IntervalWait); }
        }

        public void Sleep()
        {
            Thread.Sleep(DynamicWait);
        }
        public int DynamicWaitAck(bool ack)
        {
            if (ack)
            {
                if (IntervalWait > LargeWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - LargeWaitStep);
                else if (IntervalWait > MedWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - MedWaitStep);
                else if (IntervalWait > MinWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - MinWaitStep);
                else if (IntervalWait > TinyWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - TinyWaitStep);
                //return (int)Interlocked.Decrement(ref IntervalWait);
            }
            else
            {
                if (IntervalWait < MaxWait)
                    return (int)Interlocked.Increment(ref IntervalWait);
            }
            return (int)IntervalWait;
        }
    }

    /// <summary>
    /// ThreadWorker
    /// </summary>
    public class DynamicWorker : IDynamicWait
    {
        public const int MAXTHREAD = 10;

        public Func<bool> ActionTask { get; set; }
        public Action<LogLevel, string> ActionLog { get; set; }
        public int MaxThreads { get; set; }
        public int Interval { get; set; }
        public string Name { get; set; }
        public DynamicWaitType WaitType { get; private set; }
        public bool EnableDynamicWait { get; private set; }
        public bool EnableResetEvent { get; private set; }

        //bool _EnableResetEvent;
        //public bool EnableResetEvent { get { return _EnableResetEvent; } set{ _EnableResetEvent = (value && EnableDynamicWait)?false: value; } }

        //public bool EnableDynamicWait { get { return DynamicWait != null; } }

        public DynamicInterval DynamicWait { get; private set; }

        public DynamicWorker(DynamicWaitType waitType = DynamicWaitType.DynamicWait, int maxThread = 1, int interval = 1000)
        {
            MaxThreads = (maxThread < 1 || maxThread > MAXTHREAD) ? MAXTHREAD : maxThread;
            Interval = (interval < 1) ? 1000 : interval;
            EnableResetEvent = false;
            Name = "ActionWorker";
            WaitType = waitType;
            if (waitType== DynamicWaitType.DynamicWait)
            {
                DynamicWait = new DynamicInterval(Interval);
            }
            else if(waitType== DynamicWaitType.ResetEvent){
                EnableResetEvent = true;
            }
        }

        public DynamicWorker(DynamicInterval dynamicWait)
        {
            DynamicWait = dynamicWait;
            MaxThreads = dynamicWait.MaxThread;
            Interval = dynamicWait.DynamicWait;
            EnableResetEvent = false;
            Name = "ActionWorker";
            WaitType = DynamicWaitType.DynamicWait;
        }

        #region background worker

        volatile bool KeepAlive;
        Thread[] thWorker;
        //int synchronized;
        bool _Pause;
        int _PauseInterval=1000;

        public void Start()
        {

            //thDequeueWorker = new Thread[MaxThreads];
            thWorker = new Thread[MaxThreads];

            for (int i = 0; i < MaxThreads; i++)
            {
                thWorker[i] = new Thread(new ThreadStart(Worker));
                thWorker[i].IsBackground = true;
                thWorker[i].Start();
            }
        }

        public void Stop()
        {
            KeepAlive = false;
        }
        public void Shutdown(bool waitForWorkers)
        {
            try
            {
                KeepAlive = false;

                ActionLog(LogLevel.Error, Name + " DynamicWorker Stoping...");
                if (waitForWorkers) {
                    Thread.Sleep(3000);
                    //if (thWorker != null)
                    //{
                    //    for (int i = 0; i < thWorker.Length; i++)
                    //    {
                    //        thWorker[i].Interrupt();
                    //        thWorker[i].Join(5000);
                    //    }
                    //}
                }
                ActionLog(LogLevel.Error, Name + " DynamicWorker Stoped!");
            }
            catch (ThreadInterruptedException ex)
            {
                /* Clean up. */
                ActionLog(LogLevel.Error, Name + " DynamicWorker on Stop throws ThreadInterruptedException: " + ex.Message);
            }
            catch (Exception ex)
            {
                ActionLog(LogLevel.Error, Name + " DynamicWorker on Stop throws the error: " + ex.Message);
            }
        }
        public bool Pause(bool on)
        {
            return Pause(on ? 60 : 0);
        }
        public bool Pause(int intervalSeconds = 60)
        {
            _Pause = intervalSeconds > 0;
            _PauseInterval = 1000 * ((intervalSeconds < 1) ? 60 : intervalSeconds);
            return _Pause;
        }
        public int DynamicWaitAck(bool ack)
        {
            if (EnableResetEvent)
                ResetEvent.Set();

            else if (EnableDynamicWait)
                return DynamicWait.DynamicWaitAck(ack);
            return 0;
        }
        public void EventSet()
        {
            if (EnableResetEvent)
                ResetEvent.Set();
        }

        bool lockWasTaken = false;
        object _locker = new object();
        //long delay;

        //public void Delay(TimeSpan time)
        //{
        //    Interlocked.Exchange(ref delay, (long)time.TotalMilliseconds);
        //}

        static readonly AutoResetEvent ResetEvent = new AutoResetEvent(false);

        protected bool WorkerAction() {

            bool ack = ActionTask();

            //when EnableDynamicWait is true, the ack is for calc DynamicWait()
            if (EnableDynamicWait)
                DynamicWait.DynamicWaitAck(ack);
            //when EnableResetEvent is true, if ack is true the ResetEvent.Set() should set here, otherwise it is by ActionTask
            else if (EnableResetEvent && ack)
                ResetEvent.Set();

            return ack;
        }

        protected void Worker()
        {
            ActionLog( LogLevel.Debug,Name+ " started!");
            KeepAlive = true;
            while (KeepAlive)
            {
                try
                {
                    //if (Interlocked.Read(ref delay) > 0)
                    //{
                    //    Thread.Sleep((int)delay);
                    //    Interlocked.Exchange(ref delay, 0);
                    //}

                    if (_Pause)
                        Thread.Sleep(_PauseInterval);
                    else
                    {
                        
                        Monitor.Enter(_locker);
                        lockWasTaken = true;

                        var task = Task.Factory.StartNew(() =>
                           WorkerAction()
                        );

                        if (EnableResetEvent)
                            ResetEvent.WaitOne();


                        //if (0 == Interlocked.Exchange(ref synchronized, 1))
                        //{
                        //    if (_Pause)
                        //        Thread.Sleep(_PauseInterval);
                        //    else
                        //    {
                        //        if (EnableDynamicWait)
                        //            DynamicWait.DynamicWaitAck(ActionTask());
                        //        else
                        //            ActionTask();
                        //    }
                    }
                }
                catch (Exception ex)
                {
                    ActionLog(LogLevel.Error, Name +" error " + ex.Message);
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                    //Interlocked.Exchange(ref synchronized, 0);
                }

                if (EnableDynamicWait)
                    DynamicWait.Sleep();
                else
                    Thread.Sleep(Interval);
            }
            ActionLog(LogLevel.Debug, Name + " stoped!");
        }

        #endregion
    }

    /*
        var cancellationTokenSource = new CancellationTokenSource();
        var task = Repeat.Interval(
                TimeSpan.FromSeconds(15),
                () => CheckDatabaseForNewReports(), cancellationTokenSource.Token);
        The Repeat class looks like this:
    */

    public static class ThreadWorker
    {
        public static Task Worker(
            TimeSpan pollInterval,
            Action action,
            CancellationToken token)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                () =>
                {
                    for (; ; )
                    {
                        if (token.WaitCancellationRequested(pollInterval))
                            break;

                        action();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

    }

    static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}
