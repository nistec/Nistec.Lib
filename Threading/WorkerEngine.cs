using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Generic;
using System.Diagnostics;

namespace Nistec.Threading
{
    public class WorkerEngine
    {
        private Task _workerTask;
        private bool _running;
        long _ActiveConnections;
        //int _MaxConnection = 10;
        //private int _delay=10;

        protected long ActiveConnections { get { return _ActiveConnections; } }
        public bool IsMultiTask { get; set; }
        public int MaxWorkers { get; set; }
        public int MaxConnection { get; set; }
        public int Interval { get; set; }
        public ListenerState State { get; set; }
        public CancellationToken Token { get; set; }
        public string Name { get; set; }


        public WorkerEngine(int delay = 100)
        {
            Name = GetMethodFullName();
            Interval = delay;
            MaxConnection = 10;
            MaxWorkers = 2;
            //Token = token;
            State = ListenerState.Initilaized;
        }

        internal static string GetMethodFullName()
        {
            var frame = new System.Diagnostics.StackFrame();
            return frame.GetMethod().ReflectedType.FullName + "." + frame.GetMethod().Name;
        }

        protected virtual void OnError(string messaage)
        {

        }
        protected virtual void LogInfo(string messaage)
        {

        }

        protected virtual void OnActionTask(CancellationToken token)
        {

        }
        protected virtual void OnStart(bool started)
        {

        }
        protected virtual void OnStop(bool stoped)
        {

        }
        protected virtual void OnStateChanged(ListenerState state)
        {

            State = state;
            LogInfo(Name + " " + state.ToString());
            //if (ActionState != null)
            //    ActionState(state);
            //ActionLog(LogLevel.Debug, Name + " " + state.ToString());
        }
        public void Start()
        {
            StartMulti(Token);
        }

        //NOT USED
        public void Start(CancellationToken token)
        {
            _running = true;
            State = ListenerState.Started;
            _workerTask = Task.Run(async () =>
            {
                OnStart(true);

                while (!token.IsCancellationRequested && _running)
                {
                    try
                    {
                        if (Interlocked.Read(ref _ActiveConnections) > MaxConnection)
                        {
                            LogInfo($"ActiveConnections {_ActiveConnections} exceeds MaxConnection: {MaxConnection}");
                            await Task.Delay(100).ConfigureAwait(false);
                            continue;
                        }
                        Interlocked.Increment(ref _ActiveConnections);

                        await DoWorkAsync(token).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex.Message);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _ActiveConnections);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(Interval))
                              .ConfigureAwait(false);
                }
            }, token);
        }
        public void StartMulti(CancellationToken token)
        {
            _running = true;
            State = ListenerState.Started;
            LogInfo($"Start WorkerEngine: {Name} MaxWorkers {MaxWorkers} , MaxConnection: {MaxConnection}");

            var tasks = new List<Task>();

            for (int i = 0; i < MaxWorkers; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested && _running)
                    {
                        try
                        {
                            if (_Pause)
                            {
                                await Task.Delay(_PauseInterval).ConfigureAwait(false);
                                continue;
                            }
                            //if (Interlocked.Read(ref _ActiveConnections) > MaxConnection)
                            //{
                            //    LogInfo($"ActiveConnections {_ActiveConnections} exceeds MaxConnection: {MaxConnection}");
                            //    await Task.Delay(100).ConfigureAwait(false);
                            //    continue;
                            //}
                            Interlocked.Increment(ref _ActiveConnections);
                            //LogInfo($"ActiveConnections {_ActiveConnections} , MaxConnection: {MaxConnection}");

                            await DoWorkAsync(token).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            OnError(ex.Message);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _ActiveConnections);
                        }
                        await Task.Delay(TimeSpan.FromMilliseconds(Interval))
                                  .ConfigureAwait(false);
                    }
                }, token));
            }
        }

        public void Stop()
        {
            _running = false;
            _workerTask?.Wait(5000);
            State = ListenerState.Stoped;
            OnStop(true);
        }

        private async Task DoWorkAsync(CancellationToken token)
        {
            OnActionTask(token);
            // Your recurring logic goes here
            await Task.Delay(100).ConfigureAwait(false);
        }


        public string Command(string cmd, bool wait)
        {
            switch (cmd)
            {
                case "Stop":
                    Stop(); break;
                case "Start":
                    Start(); break;
                case "Shutdown":
                //Shutdown(wait); break;
                default:
                    return "Commnd not suppported, " + cmd;
            }
            return State.ToString();
        }

        public NameValueArgs Report()
        {
            var args = new NameValueArgs();
            args.Add("Name", Name);
            args.Add("MaxWorkers", MaxWorkers);
            args.Add("Interval", Interval);
            //args.Add("WaitType", WaitType.ToString());
            //args.Add("EnableDynamicWait", EnableDynamicWait);
            //args.Add("EnableResetEvent", EnableResetEvent);
            args.Add("State", State.ToString());
            args.Add("MaxConnection", MaxConnection);
            args.Add("IsMultiTask", IsMultiTask);
            args.Add("ActiveConnections", ActiveConnections);
            return args;
        }
        bool _Pause;
        int _PauseInterval = 1000;
        public bool Pause(OnOffState onOff, int delay)
        {
            //if (ActionWorker == null)
            //    return false;
            _Pause = onOff == OnOffState.On;// ActionWorker.Pause(onOff);

            if (_Pause)
            {
                Interlocked.Exchange(ref _PauseInterval, Math.Max(delay, 1000));
                State = ListenerState.Paused;
                OnStateChanged(State);
                //OnEvent($"AgentSessionListener.Pause", $"State: {State}, HostName: {HostName}");
                //OnInfo($"AgentSessionListener Paused: {HostName}");
            }
            else
            {
                Interlocked.Exchange(ref _PauseInterval, 0);
                State = ListenerState.Started;
                OnStateChanged(State);
                //OnInfo($"AgentSessionListener No Paused: {HostName}");
            }
            return _Pause;
        }
    }
}
