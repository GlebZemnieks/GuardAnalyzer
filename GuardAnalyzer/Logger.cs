using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonDar.Core.Utils
{
    /*
     * Example of using 
     * >> Logger.setLogger(new ConsoleLogger());
     * >> Logger.log("TestModule", "Start test №21");
     */

    public interface LoggerInterface
    {
        void Log(string msg);
        void Log(string compoment, string msg);
        void Log(string component, string msg, Exception ex);
    }

    public class Logger
    {
        //private static LoggerInterface logger = new ConsoleLogger();
        private static LoggerInterface logger = new EmptyLogger();

        public void SetLogger(LoggerInterface logger)
        {
            Logger.logger = logger;
        }

        public static void Log(string msg)
        {
            Logger.logger.Log(msg);
        }

        public static void Log(string component, string msg)
        {
            Logger.logger.Log(component, msg);
        }

        public static void Log(string component, string msg, Exception ex)
        {
            Logger.logger.Log(component, msg, ex);
        }

    }

    public class EmptyLogger : LoggerInterface
    {

        void LoggerInterface.Log(string msg)
        {
        }

        void LoggerInterface.Log(string compoment, string msg)
        {
        }
        void LoggerInterface.Log(string component, string msg, Exception ex)
        {
        }
    }

    public class ConsoleLogger : LoggerInterface
    {

        void LoggerInterface.Log(string msg)
        {
            Console.WriteLine(msg);
        }

        void LoggerInterface.Log(string compoment, string msg)
        {
            Console.WriteLine(compoment + " : " + msg);
        }
        void LoggerInterface.Log(string component, string msg, Exception ex)
        {
            Console.WriteLine(component + " : " + msg + " : " + ex.Message);
        }
    }
}
