using System;
using System.Diagnostics;

namespace nppTranslateCS
{
    public class Util
    {
        public static bool isStringEmpty(String str)
        {
            if ((str != null) && (str.Length > 0))
                return false;
            else
                return true;
        }
        public static void writeInfoLog(String str)
        {
            if (Main.trSettingsModel.logInfos)
                Trace.TraceInformation(str);
        }

        public static void writeErrorLog(String str)
        {
            Trace.TraceError(str);
        }
        public static void writeWarningLog(String str)
        {
            Trace.TraceWarning(str);
        }

        public static void BEGINFUN(String txt)
        {
            writeInfoLog("BEGIN -- " + txt);
        }

        public static void ENDFUN(String txt)
        {
            writeInfoLog("END -- " + txt);
        }
    }

}
