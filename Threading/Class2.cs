using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nistec.Threading
{


   
    public class AsyncOperation<TRequest, TResult> where TResult:IDisposable
    {
        #region members

        bool Listen;
        readonly object mlock = new object();
        #endregion

        //ConcurrentDictionary<Guid, ServerCom> proxy = new ConcurrentDictionary<Guid, ServerCom>(); 

        /// <summary>
        /// Use the pipe classes in the System.IO.Pipes namespace to create the 
        /// named pipe. This solution is recommended.
        /// </summary>
        private void RunAsync()
        {
            while (Listen)
            {

                var key = Guid.NewGuid();
                InvokePackage<TRequest, TResult> package = null;

                try
                {
                    lock (mlock)
                    {
                        package = new InvokePackage<TRequest, TResult >();
                    }
                    
                    // Wait for the client to connect.
                    AsyncCallback myCallback = new AsyncCallback(WaitForAsyncCallback);
                    IAsyncResult asyncResult = pipeServerAsync.BeginWaitForConnection(myCallback, package);
                    //manualReset = new ManualResetEvent(false);

                    package.ManualReset.WaitOne();

                }
                catch (Exception ex)
                {
                    OnFault("The pipe sync server throws the error: ", ex);
                    //Log.Exception("The pipe sync server throws the error: ", ex, true);
                    //Close(pipeServerAsync,true);
                }
                finally
                {
                    if (package != null)
                    {
                        package.Dispose();
                    }
                    //proxy.TryRemove(key, out server);

                    //if (pipeServerAsync != null)
                    //{
                    //    if (connected && pipeServerAsync.IsConnected)
                    //    {
                    //        pipeServerAsync.Disconnect();
                    //    }
                    //    pipeServerAsync.Close();
                    //    pipeServerAsync = null;
                    //}
                }
                Thread.Sleep(10);
            }
            //Console.WriteLine("{0} Pipe server async stop listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);
        }


        private void WaitForAsyncCallback(IAsyncResult result)
        {
            bool connected = false;
            InvokePackage<TResult> package = null;
            NamedPipeServerStream pipeServerAsync = null;
            TRequest message = default(TRequest);
            try
            {
                package = (InvokePackage<TResult>)result.AsyncState;

                //var key=(Guid)result.AsyncState;

                //if(!proxy.TryGetValue(key, out server))
                //{
                //    return;
                //}
                pipeServerAsync = server.ServerStream;

                pipeServerAsync.EndWaitForConnection(result);

                connected = true;

                //pipeServerAsync.WaitForPipeDrain();

                OnClientConnected();

                //BeginRead(message);

                message = ReadRequest(pipeServerAsync);

                NetStream res = ExecRequset(message);

                WriteResponse(pipeServerAsync, res);

                pipeServerAsync.WaitForPipeDrain();

                Console.WriteLine("Debuger-RunAsyncCallback. end: " + server.Uid.ToString());

            }
            catch (OperationCanceledException oex)
            {
                OnFault("Pipe server error, The pipe was canceled: ", oex);
                //Log.Exception("Pipe server error, The pipe was canceled: ", oex);
            }
            catch (Exception ex)
            {
                OnFault("Pipe server error: ", ex);
                //Log.Exception("Pipe server error: ", ex, true);
            }
            finally
            {
                //if (pipeServerAsync != null)
                //{
                //    if (connected && pipeServerAsync.IsConnected)
                //    {
                //        pipeServerAsync.Disconnect();
                //    }
                //    pipeServerAsync.Close();
                //    pipeServerAsync = null;
                //}
                if (message != null)
                {
                    message.Dispose();
                    message = default(TRequest);
                }
                package.ManualReset.Set();
                //if (Listen)
                //{
                //    Thread.Sleep(10);
                //    RunAsync();
                //}
            }
            //Thread.Sleep(10);
        }
    }
}
