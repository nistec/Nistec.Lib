using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable  CS1591

namespace Nistec
{

    public static class IpExtension
    {
        //var ismatch = IsInSubnet("3.239.154.125", @"3.239.153.0/24");
        public static bool IsInSubnet(string ipAddress, string cidr)
        {

            string[] parts = cidr.Split('/');

            int baseAddress = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);

            int address = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);

            int mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

            return ((baseAddress & mask) == (address & mask));

        }
        public static bool IsInSubnet(this IPAddress ipAddress, string cidr)
        {

            string[] parts = cidr.Split('/');

            int baseAddress = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);

            int address = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);

            int mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

            return ((baseAddress & mask) == (address & mask));

        }

        public static List<string> IpRangeToCidr(string ipStart, string ipEnd)
        {
            long start = IpToLong(ipStart);
            long end = IpToLong(ipEnd);
            var result = new List<string>();

            while (end >= start)
            {
                byte maxSize = 32;
                while (maxSize > 0)
                {
                    long mask = DoMask(maxSize - 1);
                    long maskBase = start & mask;

                    if (maskBase != start)
                    {
                        break;
                    }

                    maxSize--;
                }
                double x = Math.Log(end - start + 1) / Math.Log(2);
                byte maxDiff = (byte)(32 - Math.Floor(x));
                if (maxSize < maxDiff)
                {
                    maxSize = maxDiff;
                }
                string ip = LongToIp(start);
                result.Add(ip + "/" + maxSize);
                start += (long)Math.Pow(2, (32 - maxSize));
            }
            return result;
        }

        public static List<string> IpRangeToCidr(int ipStart, int ipEnd)
        {
            long start = ipStart;
            long end = ipEnd;
            var result = new List<string>();

            while (end >= start)
            {
                byte maxSize = 32;
                while (maxSize > 0)
                {
                    long mask = DoMask(maxSize - 1);
                    long maskBase = start & mask;

                    if (maskBase != start)
                    {
                        break;
                    }

                    maxSize--;
                }
                double x = Math.Log(end - start + 1) / Math.Log(2);
                byte maxDiff = (byte)(32 - Math.Floor(x));
                if (maxSize < maxDiff)
                {
                    maxSize = maxDiff;
                }
                string ip = LongToIp(start);
                result.Add(ip + "/" + maxSize);
                start += (long)Math.Pow(2, (32 - maxSize));
            }
            return result;
        }

        public static long DoMask(int s)
        {
            return (long)(Math.Pow(2, 32) - Math.Pow(2, (32 - s)));
        }

        public static string LongToIp(long ipAddress)
        {
            System.Net.IPAddress ip;
            if (System.Net.IPAddress.TryParse(ipAddress.ToString(), out ip))
            {
                return ip.ToString();
            }
            return "";
        }

        public static long IpToLong(string ipAddress)
        {
            System.Net.IPAddress ip;
            if (System.Net.IPAddress.TryParse(ipAddress, out ip))
            {
                return (((long)ip.GetAddressBytes()[0] << 24) | ((long)ip.GetAddressBytes()[1] << 16) | ((long)ip.GetAddressBytes()[2] << 8) | ip.GetAddressBytes()[3]);
            }
            return -1;
        }
    }

    // Helper extension methods on the TPL Task class
    public static class TaskExtension
    {
        public static void TimerWait(int interval)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            var timer = new System.Timers.Timer(interval); // 2 seconds
            //Timer timer = new Timer(interval); // 5 seconds
           // timer.Elapsed += (s, e) => Console.WriteLine("Timer fired!");
            timer.Elapsed += (s, e) =>
            {
                Console.WriteLine("Timer finished!");
                waitHandle.Set(); // Signal the waiting thread
                timer.Stop();
            };

            timer.Start();
            waitHandle.WaitOne(); // Worker waits here until timer completes

            //Console.WriteLine("Worker resumes after timer.");


            //var timer = new System.Timers.Timer(interval); // 2 seconds
            //timer.Elapsed += (s, e) => Console.WriteLine("Timer fired!");
            //timer.AutoReset = false;
            //timer.Start();
        }
        public static void WaitOne(int interval)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            mre.WaitOne(interval); // Waits for 2 seconds or until signaled
        }

        public static bool WaitCancellationRequested(
           this CancellationToken token,
           TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }


        // Attempts to dispose of a Task, but will not propagate the exception.  
        // Returns false instead if the Task could not be disposed.
        public static bool TryDispose(this Task source, bool shouldMarkExceptionsHandled = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            try
            {
                // no sense attempting to dispose unless we are completed, otherwise we know we'll throw
                // and why add the overhead.
                if (source.IsCompleted)
                {
                    if (shouldMarkExceptionsHandled && source.Exception != null)
                    {
                        // handle all parts of aggregate exception (true == handled)
                        source.Exception.Flatten().Handle(x => true);
                    }

                    source.Dispose();
                    return true;
                }
            }

            catch (Exception)
            {
                // consume any other possible exception on dispose so dispose is as safe as possible
            }

            // return false if any exception occurred or because task has not yet completed.
            return false;
        }
    }
}
