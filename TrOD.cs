using System;
using System.Collections.Generic;
using System.Web.UI;

namespace nppTranslateCS
{
    class TrOD : ITranslateEngine
    {
        private TranslateSettingsModel settings;
        private ITranslateEngine engine;

        public TrOD(TranslateSettingsModel inParam)
        {
            settings = inParam;

        }

        public string Translate(string from, string to, string text)
        {
            updateEngineBasedOnPreference();

            if(Util.isStringEmpty(from) || Util.isStringEmpty(to))
            {
                throw new InvalidLanguagePreferenceException();
            }
            if(engine.ToString().Equals(TranslateSettingsModel.Engine.MYMEMORY.ToString()) && from.Equals("AUTO"))
            {
                throw new InvalidLanguagePreferenceException();
            }

            Util.writeInfoLog("Fetching translation with translation params: ");
            Util.writeInfoLog(" * from: " + from);
            Util.writeInfoLog(" * to: " + to);
            Util.writeInfoLog(" * text: " + text);

            
            String result = engine.Translate(from, to, text);
            Util.writeInfoLog("Returning translation result: " + result);
            return result;
        }

        public List<Pair> GetSupportedLanguages()
        {
            Util.writeInfoLog("Fetching languages...");

            updateEngineBasedOnPreference();
            return engine.GetSupportedLanguages();
        }

        private void updateEngineBasedOnPreference()
        {
            switch(settings.getEngine())
            {
                case TranslateSettingsModel.Engine.DEEPL:
                    engine = new DEEPLTranslateEngine(settings);
                    break;
                case TranslateSettingsModel.Engine.MYMEMORY:
                    engine = new MyMemoryTranslateEngine(settings);
                    break;
                case TranslateSettingsModel.Engine.BING:
                    engine = new BINGTranslateEngine(settings);
                    break;
            }

            Util.writeInfoLog("Current Engine: " + engine.ToString());
        }

        public Pair getDefaultLanguagePreference()
        {
            return engine.getDefaultLanguagePreference();
        }
    }

}