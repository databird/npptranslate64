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

    [DataContract]
    public class ResponseData
    {
        [DataMember]
        public String translatedText { get; set; }
        [DataMember]
        public string match { get; set; }
    }

    [DataContract]
    public class MyMemoryResponseModel
    {
        [DataMember]
        public ResponseData responseData { get; set; }
        [DataMember]
        public string responseDetails { get; set; }
        [DataMember]
        public string responseStatus { get; set; }
        [DataMember]
        public string responderId { get; set; }
    }

}