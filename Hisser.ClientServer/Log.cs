using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hisser.ClientServer
{
    public static class Log
    {
        public const int LOGLEVEL_NON = 0;
        public const int LOGLEVEL_ERR = 1;
        public const int LOGLEVEL_WRN = 2;
        public const int LOGLEVEL_INF = 3;

        private static StreamWriter _target = null;
        private static int _loglevel = LOGLEVEL_ERR;

        public static void WriteInfo(object sender, string message)
        {
            if (_loglevel >= LOGLEVEL_INF)
            {
                Write(sender, "INF", message);
            }
        }

        public static void WriteError(object sender, string message)
        {
            if (_loglevel >= LOGLEVEL_ERR)
            {
                Write(sender, "ERR", message);
            }
        }

        private static void Write(object sender, string type, string message)
        {
            if (_target != null && sender != null)
            {
                _target.WriteLine(string.Format("{0};{1};{2};{3}", DateTime.Now, type, sender.GetType().Name, message));
            }
            else
            {
                _target.WriteLine(string.Format("{0};{1};{2};{3}", DateTime.Now, type, "?", message));
            }
        }

        public static void Start(StreamWriter target, int loglevel)
        {
            _target = target;
            _loglevel = loglevel;
        }

        public static void Stop()
        {
            _target = null;
        }
    }
}
