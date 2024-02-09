using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NppPluginNET;
using System.Web.UI;
using nppTranslateCS.Forms;
using System.Windows.Forms;

namespace nppTranslateCS
{
    public class TranslateSettingsController
    {
        TranslateSettingsModel model;
        frmTranslateSettings frmTrSettings;
        frmBingCredentials frmBingSettings;
        string dataSourcePath;


        public TranslateSettingsController(string dataSourcePath)
        {
            this.dataSourcePath = dataSourcePath;
        }

        public void setModel(TranslateSettingsModel model)
        {
            this.model = model;
        }

        public void setTranslateSettingsForm(frmTranslateSettings form)
        {
            this.frmTrSettings = form;
        }

        public void setBingSettingsForm(frmBingCredentials form)
        {
            this.frmBingSettings = form;
        }

        public void loadModel()
        {
            StringBuilder engine = new StringBuilder(255);
            Win32.GetPrivateProfileString("ENGINE", "engine", "", engine, 255, dataSourcePath);

            if(engine.ToString().Equals("BING"))
            {
                model.setEngine(TranslateSettingsModel.Engine.BING);
            }
            else if(engine.ToString().Equals("MYMEMORY"))
            {
                model.setEngine(TranslateSettingsModel.Engine.MYMEMORY);
            }
            else if (engine.ToString().Equals("DEEPL"))
            {
                model.setEngine(TranslateSettingsModel.Engine.DEEPL);
            }

            StringBuilder email = new StringBuilder(255);
            Win32.GetPrivateProfileString("MYMEMORY", "email", "", email, 255, dataSourcePath);

            model.email = email.ToString();

            StringBuilder apiKey = new StringBuilder(255);
            Win32.GetPrivateProfileString("DEEPL", "apiKey", "", apiKey, 255, dataSourcePath);

            model.deeplApiKey = apiKey.ToString();

            StringBuilder clientCred = new StringBuilder(255);
            Win32.GetPrivateProfileString("BING", "ClientIDAndSecret", "", clientCred, 255, dataSourcePath);
            
            if(clientCred.ToString().Contains(";"))
            {
                model.setClientCredentials(new Pair(clientCred.ToString().Split(';')[0], clientCred.ToString().Split(';')[1]));
            }

            StringBuilder allLangs = new StringBuilder(1023);
            Win32.GetPrivateProfileString("TRANSLATE", "ALLLANGUAGES", "", allLangs, 1023, dataSourcePath);
            
            if(allLangs.ToString().Contains(";"))
            {
                List<Pair> allLanguages = new List<Pair>();

                string[] allLangPair = allLangs.ToString().Split(';');
            
                foreach (string codeNamePair in allLangPair)
                {
                    string[] pair = codeNamePair.Split(':');
                    allLanguages.Add(new Pair(pair[0], pair[1]));
                }
                model.setAllLanguages(allLanguages);
            }

            StringBuilder langPref = new StringBuilder(255);
            Win32.GetPrivateProfileString("TRANSLATE", "LANGUAGEPREF", "", langPref, 255, dataSourcePath);

            if(langPref.ToString().Contains(":"))
            {
                string[] pref = langPref.ToString().Split(':');
                model.setLanguagePreference(new Pair(pref[0], pref[1]));
            }

            StringBuilder alwaysDisplayLangDialog = new StringBuilder(255);
            Win32.GetPrivateProfileString("TRANSLATE", "alwaysDisplayLangDialog", "False", alwaysDisplayLangDialog, 255, dataSourcePath);
            model.alwaysDisplayLangDialog = Boolean.Parse(alwaysDisplayLangDialog.ToString());

            StringBuilder logInfos = new StringBuilder(255);
            Win32.GetPrivateProfileString("GLOBAL", "logInfos", "False", logInfos, 255, dataSourcePath);
            model.logInfos = Boolean.Parse(logInfos.ToString());
        }

        public void persistModel()
        {

            String engine = "MYMEMORY";
            if(model.getEngine().Equals(TranslateSettingsModel.Engine.BING))
            {
                engine = "BING";
            }
            if (model.getEngine().Equals(TranslateSettingsModel.Engine.DEEPL))
            {
                engine = "DEEPL";
            }

            Win32.WritePrivateProfileString("ENGINE", "engine", engine, dataSourcePath);

            Win32.WritePrivateProfileString("MYMEMORY", "email", model.email, dataSourcePath);

            Win32.WritePrivateProfileString("BING", "ClientIDAndSecret", model.getClientCredentials().First + ";" + model.getClientCredentials().Second, dataSourcePath);

            Win32.WritePrivateProfileString("DEEPL", "apiKey", model.deeplApiKey, dataSourcePath);

            List<string> writableStringList = new List<string>();

            foreach (Pair codeNamePair in model.getAllLanguages())
            {
                writableStringList.Add(codeNamePair.First + ":" + codeNamePair.Second);
            }

            string writableString = string.Join(";", writableStringList.ToArray());

            Win32.WritePrivateProfileString("TRANSLATE", "ALLLANGUAGES", writableString, dataSourcePath);

            string langPrefStr = model.getLanguagePreference().First + ":" + model.getLanguagePreference().Second;

            Win32.WritePrivateProfileString("TRANSLATE", "LANGUAGEPREF", langPrefStr, dataSourcePath);

            Win32.WritePrivateProfileString("TRANSLATE", "alwaysDisplayLangDialog", model.alwaysDisplayLangDialog.ToString(), dataSourcePath);
            Win32.WritePrivateProfileString("GLOBAL", "logInfos", model.logInfos.ToString(), dataSourcePath);
        }



        public void onClose(Form frm)
        {
            if (frm.GetType().Equals(typeof(frmBingCredentials)))
            {
                updateModel(this.frmBingSettings);
            }
            else if (frm.GetType().Equals(typeof(frmTranslateSettings)))
            {
                updateModel(this.frmTrSettings);
            }

            persistModel();
        }


        private void updateModel(frmBingCredentials frm)
        {
            this.model.setClientCredentials(new Pair(frm.getBINGClientID(), frm.getBINGClientSecret()));

            String currentEngine = model.getEngine().ToString();
            
            int selectedEngineIndex = frm.getSelectedEngineIndex();
            switch (selectedEngineIndex)
            {
                case 0:
                    model.setEngine(TranslateSettingsModel.Engine.MYMEMORY);
                    break;
                case 1:
                    model.setEngine(TranslateSettingsModel.Engine.BING);
                    break;
                case 2:
                    model.setEngine(TranslateSettingsModel.Engine.DEEPL);
                    break;
            }
            model.email = frm.getEmail();
            model.deeplApiKey = frm.getDeeplApiKey();
            model.logInfos = (Boolean)frm.getLogInfos();

            //Clear the languages if engine change happened
            if (!currentEngine.Equals(model.getEngine().ToString()))
            {
                model.setAllLanguages(new List<Pair>());
                model.setLanguagePreference(new Pair());
            }

        }

        private void updateModel(frmTranslateSettings frm)
        {
            this.model.setLanguagePreference(
                new Pair(getLanguageCode((string)frm.getPreferredLanguages().First), 
                    getLanguageCode((string)frm.getPreferredLanguages().Second)));
            this.model.alwaysDisplayLangDialog = (Boolean)frm.getAlwaysDisplayLangDialog();
        }

        private void populateBINGCredentials()
        {
            
            this.frmBingSettings.setBINGClientID((string)this.model.getClientCredentials().First);
            this.frmBingSettings.setBINGClientSecret((string)this.model.getClientCredentials().Second);
        }

        public void onLoad(Form frm)
        {
            if (frm.GetType().Equals(typeof(frmBingCredentials)))
            {
                updateView(this.frmBingSettings);
            }
            else if (frm.GetType().Equals(typeof(frmTranslateSettings)))
            {
                updateView(this.frmTrSettings);
            }
        }

        private void updateView(frmBingCredentials frm)
        {
            switch (model.getEngine())
            {
                case TranslateSettingsModel.Engine.MYMEMORY:
                    this.frmBingSettings.setEngineSelection(0);
                    break;
                case TranslateSettingsModel.Engine.BING:
                    this.frmBingSettings.setEngineSelection(1);
                    break;
                case TranslateSettingsModel.Engine.DEEPL:
                    this.frmBingSettings.setEngineSelection(2);
                    break;

            }
            frm.setEmail(model.email);
            frm.setDeeplApiKey(model.deeplApiKey);
            frm.setLogInfos((Boolean)model.logInfos);

            populateBINGCredentials();
        }

        private void updateView(frmTranslateSettings frm)
        {
            frm.clearLanguages();

            foreach(Pair lang in model.getAllLanguages())
            {
                frm.addFromLanguage((string)lang.Second);
                frm.addToLanguage((string)lang.Second);
                
            }
            Pair langDescriptionPair = new Pair(getLanguageDescription((string)model.getLanguagePreference().First), getLanguageDescription((string)model.getLanguagePreference().Second) );
            frm.setPreferredLanguages(langDescriptionPair);

            frm.setAlwaysDisplayLangDialog((Boolean)model.alwaysDisplayLangDialog);
        }

        private String getLanguageDescription(String langCode)
        {
            foreach(Pair lang in model.getAllLanguages())
            {
                if (lang.First.Equals(langCode))
                    return (string)lang.Second;
            }
            return null;
        }

        private String getLanguageCode(String langDescription)
        {
            foreach (Pair lang in model.getAllLanguages())
            {
                if (lang.Second.Equals(langDescription))
                    return (string)lang.First;
            }
            return null;
        }



        internal void updateEngine(TranslateSettingsModel.Engine engine)
        {
            throw new NotImplementedException();
        }
    }
}
