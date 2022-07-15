using DotNetty.Common.Internal.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DotNetty.Unity
{
    public enum Level
    {
        ALL = 0,
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        OFF
    }

    public class UnityLoggerFactory
    {
        public static readonly UnityLoggerFactory Default = new UnityLoggerFactory(Level.ALL);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitialize()
        {
            InternalLoggerFactory.Factory = Default.GetLogger;
        }

        private Dictionary<string, IInternalLogger> repositories = new Dictionary<string, IInternalLogger>();
        public Level Level { get; set; }

        public UnityLoggerFactory(Level level)
        {
            this.Level = level;
        }

        public IInternalLogger GetLogger(string name)
        {
            IInternalLogger log;
            if (repositories.TryGetValue(name, out log))
                return log;

            log = new UnityLoggerImpl(name, Level);
            repositories[name] = log;
            return log;
        }

        private class UnityLoggerImpl : AbstractInternalLogger
        {
            private Level level = Level.DEBUG;
            public UnityLoggerImpl(string name) : base(name)
            {
            }

            public UnityLoggerImpl(string name, Level level) : base(name)
            {
                this.level = level;
            }

            public override bool TraceEnabled => Level.TRACE >= level;

            public override bool DebugEnabled => Level.DEBUG >= level;

            public override bool InfoEnabled => Level.INFO >= level;

            public override bool WarnEnabled => Level.WARN >= level;

            public override bool ErrorEnabled => Level.ERROR >= level;

            public override void Trace(string msg)
            {
                LogFormat(Level.TRACE, msg);
            }

            public override void Trace(string format, object arg)
            {
                LogFormat(Level.TRACE, format, arg);
            }

            public override void Trace(string format, object argA, object argB)
            {
                LogFormat(Level.TRACE, format, argA, argB);
            }

            public override void Trace(string format, params object[] arguments)
            {
                LogFormat(Level.TRACE, format, arguments);
            }

            public override void Trace(string msg, Exception t)
            {
                LogFormat(Level.TRACE, msg, t);
            }

            public override void Debug(string msg)
            {
                LogFormat(Level.DEBUG, msg);
            }

            public override void Debug(string format, object arg)
            {
                LogFormat(Level.DEBUG, format, arg);
            }

            public override void Debug(string format, object argA, object argB)
            {
                LogFormat(Level.DEBUG, format, argA, argB);
            }

            public override void Debug(string format, params object[] arguments)
            {
                LogFormat(Level.DEBUG, format, arguments);
            }

            public override void Debug(string msg, Exception t)
            {
                LogFormat(Level.DEBUG, msg, t);
            }

            public override void Info(string msg)
            {
                LogFormat(Level.INFO, msg);
            }

            public override void Info(string format, object arg)
            {
                LogFormat(Level.INFO, format, arg);
            }

            public override void Info(string format, object argA, object argB)
            {
                LogFormat(Level.INFO, format, argA, argB);
            }

            public override void Info(string format, params object[] arguments)
            {
                LogFormat(Level.INFO, format, arguments);
            }

            public override void Info(string msg, Exception t)
            {
                LogFormat(Level.INFO, msg, t);
            }

            public override void Warn(string msg)
            {
                LogFormat(Level.WARN, msg);
            }

            public override void Warn(string format, object arg)
            {
                LogFormat(Level.WARN, format, arg);
            }

            public override void Warn(string format, object argA, object argB)
            {
                LogFormat(Level.WARN, format, argA, argB);
            }

            public override void Warn(string format, params object[] arguments)
            {
                LogFormat(Level.WARN, format, arguments);
            }

            public override void Warn(string msg, Exception t)
            {
                LogFormat(Level.WARN, msg, t);
            }
            public override void Error(string msg)
            {
                LogFormat(Level.ERROR, msg);
            }

            public override void Error(string format, object arg)
            {
                LogFormat(Level.ERROR, format, arg);
            }

            public override void Error(string format, object argA, object argB)
            {
                LogFormat(Level.ERROR, format, argA, argB);
            }

            public override void Error(string format, params object[] arguments)
            {
                LogFormat(Level.ERROR, format, arguments);
            }

            public override void Error(string msg, Exception t)
            {
                LogFormat(Level.ERROR, msg, t);
            }

            private void LogFormat(Level level, string message, Exception e)
            {
                switch (level)
                {
                    case Level.OFF:
                        break;
                    case Level.TRACE:
                    case Level.DEBUG:
                    case Level.INFO:
                        {
                            UnityEngine.Debug.Log(Format(level, message, e));
                            break;
                        }
                    case Level.WARN:
                        {
                            UnityEngine.Debug.LogWarning(Format(level, message, e));
                            break;
                        }
                    case Level.ERROR:
                        {
                            UnityEngine.Debug.LogError(Format(level, message, e));
                            break;
                        }
                }
            }

            private void LogFormat(Level level, string message, params object[] arguments)
            {
                switch (level)
                {
                    case Level.OFF:
                        break;
                    case Level.TRACE:
                    case Level.DEBUG:
                    case Level.INFO:
                        {
                            UnityEngine.Debug.Log(Format(level, message, arguments));
                            break;
                        }
                    case Level.WARN:
                        {
                            UnityEngine.Debug.LogWarning(Format(level, message, arguments));
                            break;
                        }
                    case Level.ERROR:
                        {
                            UnityEngine.Debug.LogError(Format(level, message, arguments));
                            break;
                        }
                }
            }

            private string Format(Level level, string message, Exception e)
            {
                StringBuilder buf = new StringBuilder();
                buf.AppendFormat("[{0}] {1} - {2} Exception:{3}", level, Name, message, e);
                return buf.ToString();
            }

            private string Format(Level level, string message, params object[] arguments)
            {
                StringBuilder buf = new StringBuilder();
                if (string.IsNullOrEmpty(message) || arguments == null || arguments.Length <= 0)
                {
                    buf.AppendFormat("[{0}] {1} - {2}", level, Name, message);
                }
                else if (message.IndexOf("{}") >= 0)
                {
                    foreach (var obj in arguments)
                    {
                        message = message.Replace("{}", obj == null ? string.Empty : obj.ToString());
                    }
                    buf.AppendFormat("[{0}] {1} - {2}", level, Name, message);
                }
                else
                {
                    buf.AppendFormat("[{0}] {1} - {2}", level, Name, string.Format(message, arguments));
                }
                return buf.ToString();
            }
        }
    }
}
