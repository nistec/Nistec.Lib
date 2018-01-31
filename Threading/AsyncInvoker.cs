using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nistec.Threading
{

    #region  Invoke delegate

    /// <summary>
    /// Invoke Completed EventHandler
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void InvokeCompletedEventHandler<TRequest, TResult>(object sender, InvokeCompletedEventArgs<TRequest, TResult> e);

    /// <summary>
    ///  Invoke Completed EventArgs
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class InvokeCompletedEventArgs<TRequest, TResult> : EventArgs
    {
        // Fields
        private TResult item;
        private IAsyncResult result;
        private AsyncInvoker<TRequest, TResult> sender;
        //private ItemState state;

        // Methods
        internal InvokeCompletedEventArgs(AsyncInvoker<TRequest, TResult> sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
        }

        /// <summary>
        /// Get or Set AsyncResult
        /// </summary>
        public IAsyncResult AsyncResult
        {
            get
            {
                return this.result;
            }
            set
            {
                this.result = value;
            }
        }
        /// <summary>
        /// Get items collection.
        /// </summary>
        public TResult Item
        {
            get
            {
                if (this.item == null)
                {
                    try
                    {
                        this.item = this.sender.EndInvoke(this.result);
                    }
                    catch
                    {
                        throw;
                    }
                }
                return this.item;
            }
        }


    }
    #endregion

    public class InvokePackage<TRequest> :IDisposable //where TResult: IDisposable
    {
        public InvokePackage()
        {
            Uid = UUID.UniqueId();
            ManualReset = new ManualResetEvent(false);
        }
        public TRequest Request;
        //public TResult Result;
        public readonly long Uid;
        public ManualResetEvent ManualReset;

        public void Dispose()
        {
            if (ManualReset != null)
            {
                ManualReset.Close();
                ManualReset.Dispose();
                ManualReset = null;
            }

            if (Request != null)
            {
                //if (Item.IsConnected)
                //{
                //    Item.Disconnect();
                //}
                //Item.Close();
                Request.Dispose();
                Request = default(Request);
            }
        }
    }

    /// <summary>
    /// Invoke Item Callback delegate
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="request"></param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public delegate TResult InvokeItemCallback<TRequest, TResult>(TimeSpan timeout, InvokePackage<TRequest> package);


    /// <summary>
    /// Async Finder class
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class AsyncInvoker<TRequest,TResult> //: IFinder<T>
    {
        
        Func<TRequest, TResult> AsyncAction { get; set; }

        #region  asyncInvoke
        /// <summary>
        /// DefaultMaxTimeout
        /// </summary>
        public static readonly TimeSpan DefaultTimeOut = TimeSpan.FromMilliseconds(4294967295);

        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;

        /// <summary>
        /// Invoke Completed event
        /// </summary>
        public event InvokeCompletedEventHandler<TRequest, TResult> InvokeCompleted;


        /// <summary>
        /// OnFindCompleted
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnInvokeCompleted(InvokeCompletedEventArgs<TRequest, TResult> e)
        {
            if (InvokeCompleted != null)
                InvokeCompleted(this, e);
        }

        #region find types

        //internal static ICollection<T> NewCollection
        //{
        //    get { return new List<T>(); }
        //}
        #endregion
        /// <summary>
        ///  Invoke using arguments.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static TResult Invoke(TRequest request, InvokeItemCallback<TRequest, TResult> caller)
        {
            AsyncInvoker<TRequest, TResult> invoker = new AsyncInvoker<TRequest, TResult>();
            return invoker.AsyncInvoke(request, caller);
        }
        /// <summary>
        /// ctor.
        /// </summary>
        public AsyncInvoker()
        {
            //this.owner = owner;
            resetEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Async Invoke.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public TResult AsyncInvoke(TRequest request, InvokeItemCallback<TRequest, TResult> caller)
        {

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(DefaultTimeOut, request, CreateCallBack(), caller);
            //Thread.Sleep(10);

            //result.AsyncWaitHandle.WaitOne();
            while (!result.IsCompleted)
            {
                Thread.Sleep(10);
            }
            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            T item = caller.EndInvoke(result);
            //AsyncCompleted(item);
            return item;

        }

        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <param name="caller"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke(InvokeItemCallback<TRequest, TResult> caller, TRequest request)
        {
            return BeginInvoke(caller, DefaultTimeOut, request);
        }
        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <param name="caller"></param>
        /// <param name="timeout"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke(InvokeItemCallback<TRequest, TResult> caller, TimeSpan timeout, TRequest request)
        {
            return BeginInvoke(caller, timeout, request);
        }


        /// <summary>
        /// Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="timeout"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="request"></param>
        /// <returns>The <see cref="T:System.IAsyncResult"></see> that identifies the posted asynchronous request.</returns>
        public IAsyncResult BeginInvoke(InvokeItemCallback<TRequest, TResult> caller, TimeSpan timeout, object state, AsyncCallback callback, TRequest request)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 4294967295L))
            {
                throw new ArgumentException("InvalidParameter", "timeout");
            }

            if (callback == null)
            {
                callback = CreateCallBack();
            }
            var package = new InvokePackage<TRequest>() {Request=request };

            

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(timeout, package, callback, caller);
            package.ManualReset.WaitOne();
           
            return result;
        }


        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public TResult WaitAsyncCallback(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            InvokeItemCallback<TRequest, TResult> caller = (InvokeItemCallback<TRequest, TResult>)asyncResult.AsyncState;
            var package = (InvokePackage<TRequest>)asyncResult.AsyncState;
            AsyncAction(request);

            // Call EndInvoke to retrieve the results.
            TResult item = (TResult)caller.EndInvoke(asyncResult);
            this.resetEvent.Set();

            return item;
        }

        private AsyncCallback CreateCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }

        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            OnInvokeCompleted(new InvokeCompletedEventArgs<TRequest, TResult>(this, asyncResult));
        }

        #endregion

    }
}
