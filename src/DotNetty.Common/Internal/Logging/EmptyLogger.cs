using System;

namespace DotNetty.Common.Internal.Logging
{
    public class EmptyLogger : AbstractInternalLogger
    {
        public EmptyLogger(string name):base(name)
        {
        }
        public override bool TraceEnabled => false;

        public override bool DebugEnabled => false;

        public override bool InfoEnabled => false;

        public override bool WarnEnabled => false;

        public override bool ErrorEnabled => false;

        public override void Debug(string msg)
        {
            //throw new NotImplementedException();
        }

        public override void Debug(string format, object arg)
        {
            //throw new NotImplementedException();
        }

        public override void Debug(string format, object argA, object argB)
        {
            //throw new NotImplementedException();
        }

        public override void Debug(string format, params object[] arguments)
        {
            //throw new NotImplementedException();
        }

        public override void Debug(string msg, Exception t)
        {
            //throw new NotImplementedException();
        }

        public override void Error(string msg)
        {
            //throw new NotImplementedException();
        }

        public override void Error(string format, object arg)
        {
            //throw new NotImplementedException();
        }

        public override void Error(string format, object argA, object argB)
        {
            //throw new NotImplementedException();
        }

        public override void Error(string format, params object[] arguments)
        {
            //throw new NotImplementedException();
        }

        public override void Error(string msg, Exception t)
        {
            //throw new NotImplementedException();
        }

        public override void Info(string msg)
        {
            //throw new NotImplementedException();
        }

        public override void Info(string format, object arg)
        {
            //throw new NotImplementedException();
        }

        public override void Info(string format, object argA, object argB)
        {
            //throw new NotImplementedException();
        }

        public override void Info(string format, params object[] arguments)
        {
            //throw new NotImplementedException();
        }

        public override void Info(string msg, Exception t)
        {
            //throw new NotImplementedException();
        }

        public override void Trace(string msg)
        {
            //throw new NotImplementedException();
        }

        public override void Trace(string format, object arg)
        {
            //throw new NotImplementedException();
        }

        public override void Trace(string format, object argA, object argB)
        {
            //throw new NotImplementedException();
        }

        public override void Trace(string format, params object[] arguments)
        {
            //throw new NotImplementedException();
        }

        public override void Trace(string msg, Exception t)
        {
            //throw new NotImplementedException();
        }

        public override void Warn(string msg)
        {
            //throw new NotImplementedException();
        }

        public override void Warn(string format, object arg)
        {
            //throw new NotImplementedException();
        }

        public override void Warn(string format, params object[] arguments)
        {
            //throw new NotImplementedException();
        }

        public override void Warn(string format, object argA, object argB)
        {
            //throw new NotImplementedException();
        }

        public override void Warn(string msg, Exception t)
        {
            //throw new NotImplementedException();
        }
    }
}
