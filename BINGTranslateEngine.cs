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
    class BINGTranslateEngine : ITranslateEngine
    {
        private TranslateSettingsModel settings;

        public BINGTranslateEngine(TranslateSettingsModel inParam)
        {
            settings = inParam;

        }

        public Pair getDefaultLanguagePreference()
        {
            return new Pair("AUTO","en");
        }

        private void validateClientCredentials()
        {
            Pair cc  = settings.getClientCredentials();

            String clientId = (String)cc.First;
            String clientSecret = (string)cc.Second;
            Util.writeInfoLog("ClientID: " + maskString(clientId));
            Util.writeInfoLog("clientSecret: " + maskString(clientSecret));

            if( (((string)cc.First).Length == 0) || (((string)cc.First).Length == 0))
            {
                throw new BINGClientCredentialException();
            }
            
        }

        private string GetAccessToken()
        {

            AdmAccessToken admToken;
            string headerValue = "";
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication((string)settings.getClientCredentials().First, (string)settings.getClientCredentials().Second);
            
            try
            {
                admToken = admAuth.GetAccessToken();
                DateTime tokenReceived = DateTime.Now;
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return headerValue;
        }

        public string Translate(string from, string to, string text)
        {
            
            BingTranslate.LanguageServiceClient client = CreateWebServiceInstance();

            
            // Creates a block within which an OperationContext object is in scope.
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = PrepareClientForPOST();
                String result = client.Translate("", text, from.Equals("AUTO")?"":from, to, "text/plain", "general");   
                
                return result;
            }
        }

        public List<Pair> GetSupportedLanguages()
        {
            
            BingTranslate.LanguageServiceClient client = CreateWebServiceInstance();

            string[] languageCodes = null;
            string[] languageNames = null;

            // Creates a block within which an OperationContext object is in scope.
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = PrepareClientForPOST();
                //Keep appId parameter blank as we are sending access token in authorization header.
                languageCodes = client.GetLanguagesForTranslate("");
         
            }

            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = PrepareClientForPOST();
                //Keep appId parameter blank as we are sending access token in authorization header.
                languageNames = client.GetLanguageNames("", "en", languageCodes, false);
         
            }

            List<Pair> langList = new List<Pair>();

            for (int i = 0; i < languageNames.Length; i++)
            {
                langList.Add(new Pair(languageCodes[i], languageNames[i]));
            }
            langList.Add(new Pair("AUTO", "AUTO"));
            return langList;
        }

        internal HttpRequestMessageProperty PrepareClientForPOST()
        {
            String authToken = GetAccessToken();

            // Add TranslatorService as a service reference, Address:http://api.microsofttranslator.com/V2/Soap.svc
            //Set Authorization header before sending the request
            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Method = "POST";
            httpRequestProperty.Headers.Add("Authorization", authToken);

            return httpRequestProperty;

        }


        internal LanguageServiceClient CreateWebServiceInstance()
        {
            validateClientCredentials();

            BasicHttpBinding binding = new BasicHttpBinding();
            // I think most (or all) of these are defaults--I just copied them from app.config:
            binding.SendTimeout = TimeSpan.FromMinutes(1);
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.AllowCookies = false;
            binding.BypassProxyOnLocal = false;
            binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.TextEncoding = System.Text.Encoding.UTF8;
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            return new LanguageServiceClient(binding, new EndpointAddress("http://api.microsofttranslator.com/V2/soap.svc"));
        }
        private String maskString(String str)
        {
            StringBuilder sb = new StringBuilder();

            int index = 0;
            foreach (char c in str)
            {
                if (index % 2 == 0)
                    sb.Append(c);
                else
                    sb.Append("*");
                index++;
            }

            return sb.ToString();

        }
    }

    [DataContract]
    public class AdmAccessToken
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string scope { get; set; }
    }

    public class AdmAuthentication
    {
        public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientId;
        private string cientSecret;
        private string request;

        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.cientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
        }

        public AdmAccessToken GetAccessToken()
        {
            return HttpPost(DatamarketAccessUri, this.request);
        }

        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }
        }

        

    }


    public class BINGClientCredentialException: ArgumentException
    {
        public BINGClientCredentialException()
        {
        }
        
    }

}