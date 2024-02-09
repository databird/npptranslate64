using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Windows.Forms;
using nppTranslateCS.BingTranslate;
using System.Web.UI;

namespace nppTranslateCS
{
    class MyMemoryTranslateEngine : ITranslateEngine
    {
        TranslateSettingsModel settings = null;

        public MyMemoryTranslateEngine(TranslateSettingsModel settings)
        {
            this.settings = settings;
        }

        public Pair getDefaultLanguagePreference()
        {
            return new Pair("","");
        }

        public string Translate(string from, string to, string text)
        {
            String myMemoryBaseUrl = "http://api.mymemory.translated.net/get?";
            String getParams = String.Format("q={0}&langpair={1}|{2}", HttpUtility.UrlEncode(text), HttpUtility.UrlEncode(from), HttpUtility.UrlEncode(to));

            String optionalEmailParam = "";

            if((settings.email != null) && (settings.email.Length>0))
            {
                optionalEmailParam = "&de=" + HttpUtility.UrlEncode(settings.email);   
            }
            
            String finalUrl = myMemoryBaseUrl + getParams + optionalEmailParam;

            MyMemoryResponseModel response  = makeGETRequest(finalUrl);

            if(response.responseStatus.Equals("200"))
            {
                return response.responseData.translatedText;
            }
            else
            {
                throw new Exception(response.responseData.translatedText);
            }
        }

        public List<Pair> GetSupportedLanguages()
        {
            List<Pair> langList = new List<Pair>();
            langList.Add(new Pair("ar", "Arabic"));
            langList.Add(new Pair("bg", "Bulgarian"));
            langList.Add(new Pair("ca", "Catalan"));
            langList.Add(new Pair("zh-CHS", "Chinese Simplified"));
            langList.Add(new Pair("zh-CHT", "Chinese Traditional"));
            langList.Add(new Pair("cs", "Czech"));
            langList.Add(new Pair("da", "Danish"));
            langList.Add(new Pair("nl", "Dutch"));
            langList.Add(new Pair("en", "English"));
            langList.Add(new Pair("et", "Estonian"));
            langList.Add(new Pair("fi", "Finnish"));
            langList.Add(new Pair("fr", "French"));
            langList.Add(new Pair("de", "German"));
            langList.Add(new Pair("el", "Greek"));
            langList.Add(new Pair("ht", "Haitian Creole"));
            langList.Add(new Pair("he", "Hebrew"));
            langList.Add(new Pair("hi", "Hindi"));
            langList.Add(new Pair("mww", "Hmong Daw"));
            langList.Add(new Pair("hu", "Hungarian"));
            langList.Add(new Pair("id", "Indonesian"));
            langList.Add(new Pair("it", "Italian"));
            langList.Add(new Pair("ja", "Japanese"));
            langList.Add(new Pair("tlh", "Klingon"));
            langList.Add(new Pair("tlh-Qaak", "Klingon (pIqaD)"));
            langList.Add(new Pair("ko", "Korean"));
            langList.Add(new Pair("lv", "Latvian"));
            langList.Add(new Pair("lt", "Lithuanian"));
            langList.Add(new Pair("ms", "Malay"));
            langList.Add(new Pair("mt", "Maltese"));
            langList.Add(new Pair("no", "Norwegian"));
            langList.Add(new Pair("fa", "Persian"));
            langList.Add(new Pair("pl", "Polish"));
            langList.Add(new Pair("pt", "Portuguese"));
            langList.Add(new Pair("ro", "Romanian"));
            langList.Add(new Pair("ru", "Russian"));
            langList.Add(new Pair("sk", "Slovak"));
            langList.Add(new Pair("sl", "Slovenian"));
            langList.Add(new Pair("es", "Spanish"));
            langList.Add(new Pair("sv", "Swedish"));
            langList.Add(new Pair("th", "Thai"));
            langList.Add(new Pair("tr", "Turkish"));
            langList.Add(new Pair("uk", "Ukrainian"));
            langList.Add(new Pair("ur", "Urdu"));
            langList.Add(new Pair("vi", "Vietnamese"));
            langList.Add(new Pair("cy", "Welsh"));

            return langList;

        }


        private MyMemoryResponseModel makeGETRequest(String url)
        {
            Util.writeInfoLog(url);
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "GET";
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MyMemoryResponseModel));
                //Get deserialized object from JSON stream
                MyMemoryResponseModel mymemoryResp = (MyMemoryResponseModel)serializer.ReadObject(webResponse.GetResponseStream());
                return mymemoryResp;
            }
        }
    }

    
}