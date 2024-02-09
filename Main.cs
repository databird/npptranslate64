using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;
using System.Collections.Generic;
using System.Diagnostics;
using nppTranslateCS.Forms;
using System.Web.UI;
using Microsoft.VisualBasic.Logging;
using System.Reflection;
using System.Globalization;
using System.Threading;

namespace nppTranslateCS
{
    class Main
    {
        #region " Fields "
#if DEBUG
        internal const string PluginName = "Translate-Debug";
#else
        internal const string PluginName = "Translate";
#endif

        static String pluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static string iniFilePath = null;
        static string logDirectoryPath = null;
        //static string logFilePath = null;
        static int idMyDlg = -1;
        static frmTranslateSettings dlgTrSettings = new frmTranslateSettings();
        static frmBingCredentials dlgBingSettings = new frmBingCredentials();
        static public TranslateSettingsModel trSettingsModel = new TranslateSettingsModel();
        static TranslateSettingsController translateSettingsController;
        static TrOD translateEngine;


        static Main()
        {
            //Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("de-AT");

            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            String iniDirectoryFilePath = Path.Combine(sbIniFilePath.ToString(), PluginName);
            if (!Directory.Exists(iniDirectoryFilePath)) Directory.CreateDirectory(iniDirectoryFilePath);
            iniFilePath = Path.Combine(iniDirectoryFilePath, PluginName + ".ini");

            //Create directory for logging if not exisits
            
            String[] configPathArray = iniDirectoryFilePath.Split(new char[]{'\\'});
            configPathArray.SetValue("logs", configPathArray.Length - 2);//change ...config/translate => logs/translate
            logDirectoryPath = String.Join("\\", configPathArray);

            if (!Directory.Exists(logDirectoryPath)) Directory.CreateDirectory(logDirectoryPath);
            //logFilePath = Path.Combine(logDirectoryPath, "Translate" + ".log");

            

            //It is gaurunteed to have directory created after this
            try
            {
                initializeTraceListner();
                Util.writeInfoLog("################ Translate plugin (Version: " + pluginVersion + ") initializing...");

                if (!File.Exists(iniFilePath))
                {
                    FileStream fs = File.Create(iniFilePath);
                    fs.Dispose();
                }
                else
                {
                    //File exists, check for migration from older version
                    migrateIfRequired();
                }
                //Here, It is gaurunteed to have a settings file either crerated or preexisting or migrated(blank)
                
                translateSettingsController = new TranslateSettingsController(iniFilePath);
                translateSettingsController.setBingSettingsForm(dlgBingSettings);
                translateSettingsController.setTranslateSettingsForm(dlgTrSettings);
                

                translateSettingsController.setModel(trSettingsModel);
                dlgBingSettings.setController(translateSettingsController);
                dlgTrSettings.setController(translateSettingsController);

                translateSettingsController.loadModel();

                translateEngine = new TrOD(trSettingsModel);

                logSystemInfo();
            }
            catch (Exception ex)
            {
                handleException(ex);
            }

            
        }

        private static void logSystemInfo()
        {
            bool is64BitProcess = (IntPtr.Size == 8);
            bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

            Util.writeInfoLog("OSVersion: " + Environment.OSVersion.ToString());
            Util.writeInfoLog("Is64Bit: " + is64BitOperatingSystem.ToString());
            Util.writeInfoLog(".NET (CLR) Version: " + Environment.Version.ToString());

            Util.writeInfoLog("Default Language Info:");
            logCultureInfo(CultureInfo.InstalledUICulture);
            Util.writeInfoLog("Current Culture Info:");
            logCultureInfo(Thread.CurrentThread.CurrentCulture);
        }

        private static void logCultureInfo(CultureInfo ci)
        {
            Util.writeInfoLog(String.Format(" * Name: {0}", ci.Name));
            Util.writeInfoLog(String.Format(" * Display Name: {0}", ci.DisplayName));
            Util.writeInfoLog(String.Format(" * English Name: {0}", ci.EnglishName));
            Util.writeInfoLog(String.Format(" * 2-letter ISO Name: {0}", ci.TwoLetterISOLanguageName));
            Util.writeInfoLog(String.Format(" * 3-letter ISO Name: {0}", ci.ThreeLetterISOLanguageName));
            Util.writeInfoLog(String.Format(" * 3-letter Win32 API Name: {0}", ci.ThreeLetterWindowsLanguageName));
        }

        private static void logEncodingInfo()
        {
            // DEBUG !!!
            /*StringBuilder bufferEncoding = new StringBuilder();
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETBUFFERENCODING, Win32.MAX_PATH, bufferEncoding);

            StringBuilder currentNativeLangEncoding = new StringBuilder();
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTNATIVELANGENCODING, Win32.MAX_PATH, currentNativeLangEncoding);

            Util.writeInfoLog("bufferEncoding: " + bufferEncoding.ToString());
            Util.writeInfoLog("currentNativeLangEncoding: " + currentNativeLangEncoding.ToString());*/
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        internal static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }
        

        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            PluginBase.SetCommand(0, "Translate Selected", TranslateText, new ShortcutKey(true, true, false, Keys.Z));
            PluginBase.SetCommand(1, "Translate Selected-Swapped Preference", TranslateText_Reverse, new ShortcutKey(true, true, true, Keys.Z));
            PluginBase.SetCommand(2, "Translate CamelCase/underscore_case", TranslateCodeString, new ShortcutKey(true, true, false, Keys.X));
            PluginBase.SetCommand(3, "Translate Engine Settings", setBINGCredentials); 
            PluginBase.SetCommand(4, "Language Settings", setLanguagePreference);
            PluginBase.SetCommand(5, "About", AboutDlg);
            PluginBase.SetCommand(6, "Help", LaunchHelp);
            idMyDlg = 6;
        }

        internal static void PluginCleanUp()
        {
            Util.writeInfoLog("################ Translate plugin cleaning up"); 
            translateSettingsController.persistModel();
        }

        #endregion

        
        internal static IntPtr GetCurrentEditHandle()
        {
            IntPtr curScintilla = PluginBase.GetCurrentScintilla();
            return curScintilla;
        }


        


        internal static String getSelectedText()
        {
            Util.BEGINFUN("getSelectedText");

            try
            {
                IntPtr editHandle = GetCurrentEditHandle();

                int cpMin = (int)Win32.SendMessage(editHandle, SciMsg.SCI_GETSELECTIONSTART, 0, 0);
                int cpMax = (int)Win32.SendMessage(editHandle, SciMsg.SCI_GETSELECTIONEND, 0, 0);

                Sci_TextRange tr = new Sci_TextRange(cpMin, cpMax, cpMax - cpMin + 1);
            
                Win32.SendMessage(editHandle, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);

                string selected = tr.lpstrText;
                if (selected.Length < 1)
                {
                    return "";
                }

                Util.writeInfoLog("Selected text range: " + selected);

                logEncodingInfo();

                Encoding w1252 = Encoding.GetEncoding(1252);
                string converted = Encoding.UTF8.GetString(w1252.GetBytes(selected));

                Util.writeInfoLog("Final selected text after conversion: " + converted);

                Util.ENDFUN("getSelectedText");

                return converted;

            }
            catch (Exception ex)
            {
                handleException(ex);
                return "";
            }
        }


        internal static void TranslateText()
        {
            Util.BEGINFUN("TranslateText");
            try
            {
                string text = getSelectedText();

                if (text.Length == 0)
                    return;

                if (trSettingsModel.alwaysDisplayLangDialog)
                    setLanguagePreference();

                Pair langPref = getLanguagePreference();

                //readProxySettings();

                String result = translateEngine.Translate((string)langPref.First, (string)langPref.Second, text);

                showTranslationResults((string)langPref.First, (string)langPref.Second, result);
            }
            catch (Exception ex)
            {
                handleException(ex);
                return ;
            }
            Util.ENDFUN("TranslateText");

        }

        internal static void setBINGCredentials()
        {
            try
            {
                dlgBingSettings.ShowDialog();
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
        }

        internal static void setLanguagePreference()
        {
            try
            {
                if (initLanguages())
                {
                    dlgTrSettings.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
        }

        internal static Pair getLanguagePreference()
        {
            Util.writeInfoLog("getting Language Preference");

            if (initLanguages())
            {
               return trSettingsModel.getLanguagePreference();
            }        
            
            return new Pair("", "");
        }


        internal static Boolean initLanguages()
        {
            try
            {
                if (trSettingsModel.getAllLanguages().Count == 0)
                {
                    Util.writeInfoLog("All languages empty in the model, trying to fetch..");

                    List<Pair> fetchedList = new List<Pair>();
                    fetchedList.AddRange(translateEngine.GetSupportedLanguages());     
                    trSettingsModel.setAllLanguages(fetchedList);
                    trSettingsModel.setLanguagePreference(translateEngine.getDefaultLanguagePreference());
                }
                return true;
            }
            catch (Exception ex)
            {
                handleException(ex);
                return false;
            }
        }


        internal static void AboutDlg()
        {
            string aboutText = "Translate Plugin For Notepad++\n\nVersion: " + pluginVersion + "\nAuthor: Shaleen Mishra\nContact: shaleen.mishra@gmail.com\nPorting to 64bits: databird sàrl\nwww.databird.ch";
            MessageBox.Show(System.Windows.Forms.Control.FromHandle(GetCurrentEditHandle()), aboutText, PluginName, MessageBoxButtons.OK);
        }


        internal static void LaunchHelp()
        {
            Process.Start("https://sourceforge.net/apps/mediawiki/npptranslate/index.php?title=Main_Page");
        }



        internal static void TranslateText_Reverse() 
        {
            Util.BEGINFUN("TranslateText_Reverse");
            try
            {
                string text = getSelectedText();

                if (text.Length == 0)
                    return;

                Pair langPref = getLanguagePreference();

                if(langPref.First.Equals("AUTO"))
                {
                    MessageBox.Show(System.Windows.Forms.Control.FromHandle(GetCurrentEditHandle()), "This feature is not available for auto-detect settings!\nChange configuration file to a valid source language code and Retry.", PluginName, MessageBoxButtons.OK);
                    return;
                }
                //readProxySettings();

                String result = translateEngine.Translate((String)langPref.Second, (string)langPref.First, text);

                showTranslationResults((string)langPref.Second, (string)langPref.First, result);
            }
            catch (Exception ex)
            {
                handleException(ex);
            }

            Util.ENDFUN("TranslateText_Reverse");
        }

        internal static string DecoupleMixedCase(string inStr)
        {
            try
            {
                bool hasLower = false, hasUpper = false;
                int len = inStr.Length;
                StringBuilder outStr = new StringBuilder();

                for (int i = 0; i < len; i++)
                {
                    if (!Char.IsLower(inStr[i]))
                    {
                        hasUpper = true;
                        outStr.Append(" ");
                    }
                    else
                        hasLower = true;

                    outStr.Append(inStr[i]);
                }

                if (!(hasUpper && hasLower))
                    return inStr;
                else
                    return outStr.ToString();
            }
            catch (Exception ex)
            {
                handleException(ex);
                return inStr;
            }
        }

        internal static string replaceUndescores(string inStr)
        {
            return inStr.Replace('_',' ');
        }

        internal static void TranslateCodeString()
        {
            try
            {
                string selectedText = getSelectedText();

                if (selectedText.Equals(""))
                    return;

                string processedText = DecoupleMixedCase(replaceUndescores(selectedText));

                Pair fromTo = getLanguagePreference();

                //readProxySettings();

                string result = translateEngine.Translate((string)fromTo.First, (string)fromTo.Second, processedText);

                showTranslationResults((string)fromTo.First, (string)fromTo.Second, result);
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
        }




        internal static void CopyTranslatedTextDataToClipBoard(string strData)
        {
            Clipboard.SetText(strData);
        }

        internal static void showTranslationResults(string from, string to, string transResult)
        {
            Util.BEGINFUN("showTranslationResults");

            try
            {
                /*StringBuilder transDisplay = new StringBuilder();
                transDisplay.Append(from);
                transDisplay.Append(" ==> ");
                transDisplay.Append(to);

                transDisplay.Append("\n\n**********************************************************\n\n");

                transDisplay.Append(transResult);

                transDisplay.Append("\n\n**********************************************************\n\n");

                transDisplay.Append("Do you want to copy translated text to clipboard?\n");

                DialogResult selection = MessageBox.Show(System.Windows.Forms.Control.FromHandle(GetCurrentEditHandle()), transDisplay.ToString(), "Translation Result, powered by " + trSettingsModel.getEngine().ToString(), MessageBoxButtons.YesNo);
                if (selection.Equals(DialogResult.Yes))
                {
                    CopyTranslatedTextDataToClipBoard(transResult);

                }*/
                // databird sàrl
                CopyTranslatedTextDataToClipBoard(transResult);
                MessageBox.Show("Translation:\n" + transResult + "\n\ncopied to clipboard!");
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
            Util.ENDFUN("showTranslationResults");
        }

        internal static void handleException(Exception e) 
        {

            Util.writeInfoLog(e.Message);
            Util.writeInfoLog(e.StackTrace);
            

            MessageBoxIcon messageType = MessageBoxIcon.Error;

            
            String message = "Unknown Error";

            try
            {

                if (e.GetType().Equals(typeof(BINGClientCredentialException)))
                {
                    message = "Client ID and Client Secret must be provided in order to use BING translate engine. Please review engine settings and try again.";
                    messageType = MessageBoxIcon.Exclamation;
                    MessageBox.Show(message, "Translate Error", MessageBoxButtons.OK, messageType);
                    dlgBingSettings.ShowDialog();
                    return;

                }
                if (e.GetType().Equals(typeof(InvalidLanguagePreferenceException)))
                {
                    message = "Invalid language combination for translation. Please review language settings and try again.";
                    messageType = MessageBoxIcon.Exclamation;
                    MessageBox.Show(message, "Translate Error", MessageBoxButtons.OK, messageType);
                    dlgTrSettings.ShowDialog();
                    return;
                }
                else
                {
                    message = "Unable to translate. Please check engine settings and language preference. \n\nDetailed Error Message: [ " + e.Message + " ]";
                    MessageBox.Show(message, "Translate Error", MessageBoxButtons.OK, messageType);
                }
                
            }
            finally
            {
                Util.writeErrorLog(message);
            }

        }

        
        internal static void migrateIfRequired()
        {
            Util.BEGINFUN("migrateIfRequired");

            //No direct way to get current Version in < 2.0.0.0, ge it indirectly;

            String strInstalledVersion = "0.4.0.0"; //default if not known

            StringBuilder installedVersion = new StringBuilder(255);
            Win32.GetPrivateProfileString("VERSION", "version", "", installedVersion, 255, iniFilePath);

            if(installedVersion.ToString().Length>0)
            {
                strInstalledVersion = installedVersion.ToString();
            }
            Util.writeInfoLog("Installed version (" + installedVersion.ToString() + ") ");

#if DEBUG
            //MessageBox.Show("Existing installed version: "+strInstalledVersion);
#endif
            executeMigrationPath(strInstalledVersion);

            Util.ENDFUN("migrateIfRequired");
        }

        internal static void executeMigrationPath(String versionStr)
        {
            Version installedVersion = new Version(versionStr);
            Version currentVersion = new Version(pluginVersion);
            Version version_2_1_0_0 = new Version("2.1.0.0");
            Version version_3_0_0_0 = new Version("3.0.0.0");
            Version version_3_1_0_0 = new Version("3.1.0.0");
            Version version_3_2_0_0 = new Version("3.2.0.0");

            if ( installedVersion < version_2_1_0_0)
            {
                migrateLegacyTo2_1_0();
            }

            if (installedVersion < version_3_0_0_0)
            {
                migrate2_1_0_0To3_0_0_0();
            }

            if (installedVersion < version_3_1_0_0)
            {
                migrate3_0_0_0To3_1_0_0();
            }

            if (installedVersion < version_3_2_0_0)
            {
                migrate3_1_0_0To3_2_0_0();
            }

            //just for good practice, that current version is also up to date
            if (currentVersion < version_3_2_0_0)
                throw new Exception("Check the version, migration path exists for a version which is great than current version.");

            updateVersion();

        }

        

        private static void updateVersion()
        {
            Win32.WritePrivateProfileString("VERSION", "version", pluginVersion, iniFilePath);
        }

        private static void migrate3_0_0_0To3_1_0_0()
        {
            StringBuilder engine = new StringBuilder(255);
            Win32.GetPrivateProfileString("ENGINE", "engine", "", engine, 255, iniFilePath);

            StringBuilder pref = new StringBuilder(255);
            Win32.GetPrivateProfileString("TRANSLATE", "LANGUAGEPREF", "", pref, 255, iniFilePath);

            if (!Util.isStringEmpty(pref.ToString()))
            {
                if (pref.ToString().StartsWith("AUTO") && engine.ToString().Equals("MYMEMORY"))
                    Win32.WritePrivateProfileString("TRANSLATE", "LANGUAGEPREF", "", iniFilePath);
            }

            StringBuilder allLanguages = new StringBuilder(1023);
            Win32.GetPrivateProfileString("TRANSLATE", "ALLLANGUAGES", "", allLanguages, 1023, iniFilePath);

            if (!Util.isStringEmpty(allLanguages.ToString()))
            {
                if (allLanguages.ToString().Contains("AUTO:AUTO") && engine.ToString().Equals("MYMEMORY"))
                    Win32.WritePrivateProfileString("TRANSLATE", "ALLLANGUAGES", "", iniFilePath);
            }
            
        }


        private static void migrate2_1_0_0To3_0_0_0()
        {
            Win32.WritePrivateProfileString("ENGINE", "engine", "MYMEMORY", iniFilePath);
            Win32.WritePrivateProfileString("MYMEMORY", "email", "", iniFilePath);
        }
        private static void migrate3_1_0_0To3_2_0_0()
        {
            Win32.WritePrivateProfileString("DEEPL", "apiKey", "", iniFilePath);
            Win32.WritePrivateProfileString("TRANSLATE", "alwaysDisplayLangDialog", "", iniFilePath);
            Win32.WritePrivateProfileString("GLOBAL", "logInfos", "", iniFilePath);
        }

        internal static void migrateLegacyTo2_1_0()
        {      
            System.IO.File.WriteAllText(iniFilePath, string.Empty);
        }

        internal static void initializeTraceListner()
        {
            CustomTraceListener listner = new CustomTraceListener();
            listner.BaseFileName = PluginName + ".log";
            listner.TraceOutputOptions = TraceOptions.DateTime;
            listner.DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.ThrowException;
            listner.Location= LogFileLocation.Custom;
            listner.CustomLocation = logDirectoryPath;
            //listner.MaxFileSize = 1024;
            listner.LogFileCreationSchedule = LogFileCreationScheduleOption.Daily;
            listner.AutoFlush = true;

            Trace.Listeners.Add(listner);
            
        }
    }
}