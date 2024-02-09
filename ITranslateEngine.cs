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
    interface ITranslateEngine
    {
        string Translate(string from, string to, string text);

        List<Pair> GetSupportedLanguages();

        Pair getDefaultLanguagePreference();
    }

    
}