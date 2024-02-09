using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NppPluginNET;
using System.Web.UI;

namespace nppTranslateCS
{
    public class TranslateSettings
    {
        Pair clientCredentials = new Pair();
        List<Pair> allLanguages = new List<Pair>();
        Pair languagePreference = new Pair();

        private string settingsFilePath;

        public TranslateSettings(string settingsPath)
        {
            settingsFilePath = settingsPath;

            loadSettings();
        }

        public void loadSettings()
        {
            StringBuilder clientCred = new StringBuilder(255);
            Win32.GetPrivateProfileString("BING", "ClientIDAndSecret", "", clientCred, 255, settingsFilePath);
            
            if(clientCred.ToString().Contains(";"))
            {
                clientCredentials.First = clientCred.ToString().Split(';')[0];
                clientCredentials.Second = clientCred.ToString().Split(';')[1];
            }


            StringBuilder allLangs = new StringBuilder(1023);
            Win32.GetPrivateProfileString("TRANSLATE", "ALLLANGUAGES", "", allLangs, 1023, settingsFilePath);
            
            if(allLangs.ToString().Contains(";"))
            {
                string[] allLangPair = allLangs.ToString().Split(';');
            
                foreach (string codeNamePair in allLangPair)
                {
                    string[] pair = codeNamePair.Split(':');
                    allLanguages.Add(new Pair(pair[0], pair[1]));
                }
            }

            StringBuilder langPref = new StringBuilder(255);
            Win32.GetPrivateProfileString("TRANSLATE", "LANGUAGEPREF", "", langPref, 255, settingsFilePath);

            if(langPref.ToString().Contains(":"))
            {
                string[] pref = langPref.ToString().Split(':');
                languagePreference = new Pair(pref[0], pref[1]);
            }


        }

        public void persistSettings()
        {
            Win32.WritePrivateProfileString("BING", "ClientIDAndSecret", clientCredentials.First+";"+clientCredentials.Second, settingsFilePath);


            List<string> writableStringList = new List<string>();

            foreach (Pair codeNamePair in allLanguages)
            {
                writableStringList.Add(codeNamePair.First + ":" + codeNamePair.Second);
            }

            string writableString = string.Join(";", writableStringList.ToArray());

            Win32.WritePrivateProfileString("TRANSLATE", "ALLLANGUAGES", writableString, settingsFilePath);

            string langPrefStr = languagePreference.First + ":" + languagePreference.Second;

            Win32.WritePrivateProfileString("TRANSLATE", "LANGUAGEPREF", langPrefStr, settingsFilePath);
        }

        public Pair getClientCredentials()
        {
            return clientCredentials;
        }

        public void setClientCredentials(Pair clientCredentials)
        {
            this.clientCredentials = clientCredentials;
        }

        
        public List<Pair> getAllLanguages()
        {
            return allLanguages;
        }

        public void setAllLanguages(List<Pair> allLanguages)
        {
            this.allLanguages = allLanguages;
        }

        public Pair getLanguagePreference()
        {
            return languagePreference;
        }

        public void setLanguagePreference(Pair pref)
        {
            this.languagePreference = pref;
        }

    }
}
