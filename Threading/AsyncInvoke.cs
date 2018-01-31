using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace Nistec.Threading
{

    /// <summary>
    /// AsyncInvoke
    /// </summary>
     public class AsyncInvokeWithProgress : IDisposable//:Component
    {
        #region members
        protected System.Windows.Forms.Timer ExecutionTimer;

        public event EventHandler StartProgressEvent;
        public event EventHandler StopProgressEvent;
         public event EventHandler AsyncCancelExecuting;
        //public event AsyncResultEventHandler ExecutingResultEvent;
         public event AsyncCallEventHandler AsyncCompleted;
         public event AsyncProgressEventHandler AsyncProgress;
        public event AsyncCallEventHandler AsyncExecutingWorker;
    
        private AsyncProgressLevel _MessageLevel;
        #endregion

        #region ctor

         public AsyncInvokeWithProgress(AsyncProgressLevel level)
        {
            _MessageLevel = level;
            InitializeComponent();
        }

         public virtual void Dispose()//bool disposing)
         {
             //base.Dispose(disposing);
         }

        #endregion

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExecutionTimer = new System.Windows.Forms.Timer();// (this.components);
            // 
            // ExecutionTimer
            // 
            this.ExecutionTimer.Interval = 200;
            this.ExecutionTimer.Tick += new System.EventHandler(this.ExecutionTimer_Tick);
         }
        #endregion

        #region current memebers

        protected object _currentResult;
        protected DateTime _currentExecutionStart;
        protected DateTime _currentExecutionEnd;
        protected TimeSpan _currentExecutionTime;
        protected IAsyncResult _asyncResult;
        protected Exception _currentException;
        protected string _currentTime;
        protected string _currentMessage;
        protected AsyncState _currentAsyncState;
  
        private delegate void RunAsyncCallDelegate(object args);

        #endregion

        #region properties

         public AsyncProgressLevel AsyncProgressLevel
        {
            get { return _MessageLevel; }
            set { _MessageLevel = value; }
        }
        public AsyncState AsyncState
        {
            get { return _currentAsyncState; }
        }
         public IAsyncResult AsyncResult
        {
            get { return _asyncResult; }
        }
        public string CurrentTime
        {
            get { return _currentTime; }
        }
        public object CurrentResult
        {
            get { return _currentResult; }
        }
        public Exception CurrentException
        {
            get { return _currentException; }
        }
         public string CurrentMessage
        {
            get { return _currentMessage; }
        }

        #endregion

        #region public methods

         public void AsyncBeginInvoke(object args)
        {
            try
            {
                _currentAsyncState = AsyncState.Started;
                _currentException = null;
                _currentExecutionStart = DateTime.Now;
                _currentTime = "00:00:00";
                OnStartProgress(EventArgs .Empty);

                DateTime dt = DateTime.Now;
                RunAsyncCallDelegate msc = new RunAsyncCallDelegate(RunAsyncCall);
                AsyncCallback cb = new AsyncCallback(RunAsyncCallback);
                _asyncResult = msc.BeginInvoke(args,cb,null);
                ExecutionTimer.Enabled = true;
                OnExecuting();
            }
            catch (Exception ex)
            {
                OnStopProgress(EventArgs.Empty);
                ExecutingTrace(ex.Message, AsyncProgressLevel.Error);
            }
        }

        public void StopCurrentExecution()
        {

            ExecutingTrace("Stop Executing ", AsyncProgressLevel.Info);

            try
            {
                OnStopProgress(EventArgs.Empty);
                ExecutionTimer.Enabled = false;
                ExecutingTrace("Execution terminated.", AsyncProgressLevel.Info);
             }
            catch
            {
                ExecutingTrace("Unable to stop execution", AsyncProgressLevel.Error);
            }
            _currentAsyncState = AsyncState.Canceled;
            OnCancelExecuting(EventArgs.Empty);
        }
        #endregion

        #region private methods

        private void RunAsyncCallback(IAsyncResult ar)
        {
            Thread th = Thread.CurrentThread;
            RunAsyncCallDelegate msc = (RunAsyncCallDelegate)((AsyncResult)ar).AsyncDelegate;
            msc.EndInvoke(ar);
        }

         protected virtual void RunAsyncCall(object args)
         {
             _currentResult = args;
             if (AsyncExecutingWorker != null)
                 AsyncExecutingWorker(this, new AsyncCallEventArgs(args));
            
         }

        private void ExecutionTimer_Tick(object sender, System.EventArgs e)
        {
            OnExecutionTimerTick(e);
        }

        protected virtual void OnExecutionTimerTick(System.EventArgs e)
        {
            DateTime dtn = DateTime.Now;
            TimeSpan ts = dtn.Subtract(_currentExecutionStart);
            _currentTime = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
            ExecutingTrace(_currentTime, AsyncProgressLevel.Progress);
            OnExecuting();

            if (_asyncResult.IsCompleted)
            {
                ExecutionTimer.Enabled = false;
                _currentExecutionEnd = DateTime.Now;
                _currentExecutionTime = _currentExecutionEnd.Subtract(_currentExecutionStart);
                ExecutionResult(_currentExecutionTime);//_currentDataTable,
            }
        }
       
        protected virtual void ExecutionResult(TimeSpan _currentExecutionTime)
        {
            try
            {
                OnExecuting();

                if (_currentException != null)
                    throw _currentException;

                string msgTrace = string.Format("Execute Result: {0} \r\n ExecutionTime: {1} \r\n", _asyncResult, _currentExecutionTime.TotalMilliseconds);
                ExecutingTrace(msgTrace, AsyncProgressLevel.Info);

            }
            catch (Exception ex)
            {
                ExecutingTrace(ex.Message, AsyncProgressLevel.Error);
            }
            finally
            {
                ExecutionTimer.Enabled = false;
                OnStopProgress(EventArgs.Empty);
                OnAsyncCompleted(new AsyncCallEventArgs(_currentResult));// AsyncResultEventArgs(_asyncResult));
            }

        }
        #endregion

        #region override

         protected virtual void OnCancelExecuting(EventArgs e)
         {
             if (AsyncCancelExecuting != null)
                 AsyncCancelExecuting(this, e);
         }

        protected virtual void OnStartProgress(EventArgs e)
        {
            if (StartProgressEvent != null)
                StartProgressEvent(this, e);
        }
        protected virtual void OnStopProgress(EventArgs e)
        {
            if (StopProgressEvent != null)
                StopProgressEvent(this, e);
        }
        protected virtual void OnExecuting()
        {

        }
         private void ExecutingTrace(string s, AsyncProgressLevel lvl)
        {
            if (lvl == AsyncProgressLevel.Progress )
            {
                _currentTime = s;
                if (AsyncProgress != null && (_MessageLevel == AsyncProgressLevel.Progress || _MessageLevel == AsyncProgressLevel.All))
                    OnAsyncProgress(new AsyncProgressEventArgs(s, lvl));
            }
            else if (lvl == AsyncProgressLevel.Info)
            {
                _currentMessage = s;
                if (AsyncProgress != null && (_MessageLevel == AsyncProgressLevel.Info || _MessageLevel == AsyncProgressLevel.All))
                    OnAsyncProgress(new AsyncProgressEventArgs(s, lvl));
            }
            else if (lvl == AsyncProgressLevel.Error)
            {
                _currentMessage = s;
                if (AsyncProgress != null && (_MessageLevel == AsyncProgressLevel.Error || _MessageLevel == AsyncProgressLevel.All))
                    OnAsyncProgress(new AsyncProgressEventArgs(s, lvl));
            }
        }

         protected virtual void OnAsyncProgress(AsyncProgressEventArgs e)
        {
            if (AsyncProgress != null)
                AsyncProgress(this, e);
        }

         protected virtual void OnAsyncCompleted(AsyncCallEventArgs e)
        {
            _currentAsyncState = AsyncState.Completed;
            if (AsyncCompleted != null)
                AsyncCompleted(this, e);
        }

        #endregion

    }

    /// <summary>
    /// AsyncInvoke
    /// </summary>
    public class AsyncInvoke : IDisposable//:Component
    {
        #region members
        protected System.Windows.Forms.Timer ExecutionTimer;

        public event EventHandler AsyncCancelExecuting;
        //public event AsyncResultEventHandler ExecutingResultEvent;
        public event AsyncCallEventHandler AsyncCompleted;
        //public event AsyncCallEventHandler AsyncExecutingWorker;

        
        #endregion

        #region ctor

        public AsyncInvoke()
        {
            InitializeComponent();
        }

        public virtual void Dispose()//bool disposing)
        {
            //base.Dispose(disposing);
        }

        #endregion

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExecutionTimer = new System.Windows.Forms.Timer();// (this.components);
            // 
            // ExecutionTimer
            // 
            this.ExecutionTimer.Interval = 200;
            this.ExecutionTimer.Tick += new System.EventHandler(this.ExecutionTimer_Tick);
        }
        #endregion

        #region current memebers

        protected object _currentResult;
        protected DateTime _currentExecutionStart;
        protected DateTime _currentExecutionEnd;
        protected TimeSpan _currentExecutionTime;
        protected IAsyncResult _asyncResult;
        protected Exception _currentException;
        //protected string _currentTime;
        //protected string _currentMessage;
        protected AsyncState _currentAsyncState;

        private delegate void RunAsyncCallDelegate(object args);

        #endregion

        #region properties

        public Func<object, object> AsyncAcion { get; set; }

        public AsyncState AsyncState
        {
            get { return _currentAsyncState; }
        }
        public IAsyncResult AsyncResult
        {
            get { return _asyncResult; }
        }
        TimeSpan _Duration;
        public TimeSpan Duration
        {
            get { return _Duration; }
        }

        public string CurrentTime
        {
            get { return _currentTime; }
        }
        public object CurrentResult
        {
            get { return _currentResult; }
        }
        public Exception CurrentException
        {
            get { return _currentException; }
        }
        public string CurrentMessage
        {
            get { return _currentMessage; }
        }

        #endregion

        #region public methods

        public void AsyncBeginInvoke(object args)
        {
            try
            {
                _currentAsyncState = AsyncState.Started;
                _currentException = null;
                _currentExecutionStart = DateTime.Now;

                DateTime dt = DateTime.Now;
                RunAsyncCallDelegate msc = new RunAsyncCallDelegate(RunAsyncCall);
                AsyncCallback cb = new AsyncCallback(RunAsyncCallback);
                _asyncResult = msc.BeginInvoke(args, cb, null);
                ExecutionTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                OnError("AsyncBeginInvoke error ", ex);
            }
        }

        public void StopCurrentExecution()
        {

            try
            {
                ExecutionTimer.Enabled = false;
            }
            catch (Exception ex)
            {
                OnError("Unable to stop execution", ex);
            }
            _currentAsyncState = AsyncState.Canceled;
            OnCancelExecuting(EventArgs.Empty);
        }
        #endregion

        #region private methods

        private void RunAsyncCallback(IAsyncResult ar)
        {
            RunAsyncCallDelegate msc = (RunAsyncCallDelegate)((AsyncResult)ar).AsyncDelegate;
            msc.EndInvoke(ar);
        }


        protected virtual void RunAsyncCall(object args)
        {
            _currentResult = args;
            //if (AsyncExecutingWorker != null)
            //    AsyncExecutingWorker(this, new AsyncCallEventArgs(args));
        }

        private void ExecutionTimer_Tick(object sender, System.EventArgs e)
        {
            OnExecutionTimerTick(e);
        }

        protected virtual void OnExecutionTimerTick(System.EventArgs e)
        {
            DateTime dtn = DateTime.Now;
            TimeSpan ts = dtn.Subtract(_currentExecutionStart);

            if (_asyncResult.IsCompleted)
            {
                ExecutionTimer.Enabled = false;
                _currentExecutionEnd = DateTime.Now;
                _currentExecutionTime = _currentExecutionEnd.Subtract(_currentExecutionStart);
                ExecutionResult(_currentExecutionTime);//_currentDataTable,
            }
        }

        protected virtual void ExecutionResult(TimeSpan _currentExecutionTime)
        {
            try
            {
                if (_currentException != null)
                    throw _currentException;

                string msgTrace = string.Format("Execute Result: {0} \r\n ExecutionTime: {1} \r\n", _asyncResult, _currentExecutionTime.TotalMilliseconds);

            }
            catch (Exception ex)
            {
                OnError("ExecutionResult error ",ex);
            }
            finally
            {
                ExecutionTimer.Enabled = false;
                OnAsyncCompleted(new AsyncCallEventArgs(_currentResult));// AsyncResultEventArgs(_asyncResult));
            }

        }
        #endregion

        #region override

        protected virtual void OnCancelExecuting(EventArgs e)
        {
            if (AsyncCancelExecuting != null)
                AsyncCancelExecuting(this, e);
        }
      
        protected virtual void OnAsyncCompleted(AsyncCallEventArgs e)
        {
            _currentAsyncState = AsyncState.Completed;
            if (AsyncCompleted != null)
                AsyncCompleted(this, e);
        }
        protected virtual void OnError(string message , Exception ex)
        {
            _currentException = ex;
        }
        #endregion

    }
}
