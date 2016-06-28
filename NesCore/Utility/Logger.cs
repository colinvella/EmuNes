using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class Logger
    {
        public delegate void WriteHandler(string entry);

        public static void Debug(string entry)
        {
            Write?.Invoke("DEBUG: " + entry);
        }

        public static void Info(string entry)
        {
            Write?.Invoke("INFO:  " + entry);
        }

        public static void Warn(string entry)
        {
            Write?.Invoke("WARN:  " + entry);
        }

        public static void Error(string entry)
        {
            Write?.Invoke("ERROR: " + entry);
        }

        public static WriteHandler Write { get; set; }
    }
}
