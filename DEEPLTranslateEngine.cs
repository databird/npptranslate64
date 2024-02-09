using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.UI;

namespace nppTranslateCS
{
    [DataContract]
    public class DeeplResult
    {
        public class Translation
        {
            public string detected_source_language { get; set; }
            public string text { get; set; }
        }

        [DataMember]
        public Translation[] translations { get; set; }
    }


    class DEEPLTranslateEngine : ITranslateEngine
    {
        TranslateSettingsModel settings = null;

        public DEEPLTranslateEngine(TranslateSettingsModel settings)
        {
            this.settings = settings;
        }

        public Pair getDefaultLanguagePreference()
        {
            return new Pair("", "AUTO");
        }

        public string Translate(string from, string to, string text)
        {
            String deeplBaseUrl = "https://api-free.deepl.com/v2/translate";

            String getParams = "{\"text\": [\"" + text + "\"],\"source_lang\":\"" + from + "\",\"target_lang\":\"" + to + "\",\"tag_handling\":\"html\"}";

            DeeplResult response = makePOSTRequest(deeplBaseUrl, getParams);

            if (response.translations.Length > 0)
                return response.translations[0].text;
            else
                throw new Exception("Error with DEEPL query");
        }

        public List<Pair> GetSupportedLanguages()
        {
            List<Pair> langList = new List<Pair>();

            langList.Add(new Pair("BG", "Bulgarian"));
            langList.Add(new Pair("CS", "Czech"));
            langList.Add(new Pair("DA", "Danish"));
            langList.Add(new Pair("DE", "German"));
            langList.Add(new Pair("EL", "Greek"));
            langList.Add(new Pair("EN", "English"));
            langList.Add(new Pair("ES", "Spanish"));
            langList.Add(new Pair("ET", "Estonian"));
            langList.Add(new Pair("FI", "Finnish"));
            langList.Add(new Pair("FR", "French"));
            langList.Add(new Pair("HU", "Hungarian"));
            langList.Add(new Pair("ID", "Indonesian"));
            langList.Add(new Pair("IT", "Italian"));
            langList.Add(new Pair("JA", "Japanese"));
            langList.Add(new Pair("KO", "Korean"));
            langList.Add(new Pair("LT", "Lithuanian"));
            langList.Add(new Pair("LV", "Latvian"));
            langList.Add(new Pair("NB", "Norwegian"));
            langList.Add(new Pair("NL", "Dutch"));
            langList.Add(new Pair("PL", "Polish"));
            langList.Add(new Pair("PT", "Portuguese"));
            langList.Add(new Pair("RO", "Romanian"));
            langList.Add(new Pair("RU", "Russian"));
            langList.Add(new Pair("SK", "Slovak"));
            langList.Add(new Pair("SL", "Slovenian"));
            langList.Add(new Pair("SV", "Swedish"));
            langList.Add(new Pair("TR", "Turkish"));
            langList.Add(new Pair("UK", "Ukrainian"));
            langList.Add(new Pair("ZH", "Chinese"));

            return langList;
        }

        private DeeplResult makePOSTRequest(String url, String _params)
        {
            Util.writeInfoLog(url);

            //Prepare API request
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            WebRequest webRequest = WebRequest.Create(url);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.Headers.Add("Authorization: DeepL-Auth-Key " + (string)settings.deeplApiKey);
            webRequest.Method = "POST";
            byte[] bytes = Encoding.UTF8.GetBytes(_params);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DeeplResult));
                //Get deserialized object from JSON stream
                DeeplResult deeplResp = (DeeplResult)serializer.ReadObject(webResponse.GetResponseStream());
                return deeplResp;
            }
        }
    }


}