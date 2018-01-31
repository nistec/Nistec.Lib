using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nistec.Runtime
{
    public class SysDiagnostics
    {

        PerformanceCounter pc;

       
        //CPUCounter.NextValue();
        public static PerformanceCounter CPUCounter()
        {
         return  new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        //MemCounter.NextValue();
        public static PerformanceCounter MemCounter()
        {
            return new PerformanceCounter("Memory", "Available MBytes");
        }

        public static PerformanceCounter CurrentProcessCPUCounter()
        {
            return new PerformanceCounter("Process", "% Processor Time",
            Process.GetCurrentProcess().ProcessName);
        }

        public static PerformanceCounter CurrentProcessMemCounter()
        {
            return new PerformanceCounter("Process", "Working Set",
            Process.GetCurrentProcess().ProcessName);
        }

    }
}
