using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualBasic.Logging;

namespace nppTranslateCS
{
    public class CustomTraceListener : FileLogTraceListener
    {
        public CustomTraceListener()
            : base()
        {
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string data)
        {
            base.WriteLine(String.Format("{0}    {1}    {2}", DateTime.Now.ToLocalTime().ToString(), eventType.ToString().PadRight(15 - (eventType.ToString().Length)), data));
        }
    }
}
