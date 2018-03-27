using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PrimeApps.Model.Common.Phone;

namespace PrimeApps.App.Helpers
{
    public static class SipProviderHelper
    {
        public static string GetProviderServerConnectionString(string providerKey)
        {
            var providerConnectionString = "";
            var provider = GetProviderInfo(providerKey);
            if (provider.IsWebSocket)
            {
                providerConnectionString = "wss://" + provider.Host + ":" + provider.WebSocketPort;
            }
            else
            {
                providerConnectionString = provider.Host + ":" + provider.Port;
            }
            return providerConnectionString;
        }
        public static string GetProviderSpecificSipUriString(string providerKey, string userCompanyKey)
        {
            var sipUri = "";
            var provider = GetProviderInfo(providerKey);
            switch (providerKey)
            {
                case "verimor":
                    sipUri = userCompanyKey + "." + provider.ProviderDomain;
                    break;

                default:
                    break;
            }
            return sipUri;

        }
        private static SipProvider GetProviderInfo(string provider)
        {
            SipProvider selectedProvider = null;

            switch (provider)
            {
                case "verimor":

                    selectedProvider = new SipProvider()
                    {
                        Host = "api.bulutsantralim.com",
                        Port = "5060",
                        Provider = "verimor",
                        IsWebSocket = true,
                        WebSocketPort = "7443",
                        ProviderDomain = "bulutsantralim.com"
                    };

                    break;
                default:
                    break;
            }

            return selectedProvider;
        }
    }
}