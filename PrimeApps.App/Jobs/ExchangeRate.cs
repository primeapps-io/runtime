using PrimeApps.Model.Context;
using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Jobs
{
    public class ExchangeRate
    {
        private IConfiguration _configuration;

        public ExchangeRate(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void DailyRates()
        {
            var todayXml = "http://www.tcmb.gov.tr/kurlar/today.xml";
            var todayXmlDoc = new XmlDocument();
            todayXmlDoc.Load(todayXml);

            var dateAttribute = todayXmlDoc.SelectSingleNode("Tarih_Date/@Date");
            var usdNode = todayXmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteSelling");
            var euroNode = todayXmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteSelling");

            if (dateAttribute == null || usdNode == null || euroNode == null)
                throw new Exception("Merkez Bankası kur bilgisi alınamadı");

            var date = DateTime.Parse(dateAttribute.Value, CultureInfo.CreateSpecificCulture("en-US"));
            var usd = usdNode.InnerXml;
            var euro = euroNode.InnerXml;

            using (var dbContext = new PlatformDBContext(_configuration))
            {
                var exchangeRate = dbContext.ExchangeRates.SingleOrDefault(x => x.Year == date.Year && x.Month == date.Month && x.Day == date.Day);

                if (exchangeRate == null)
                {
                    exchangeRate = new Model.Entities.Platform.ExchangeRate();
                    exchangeRate.Usd = decimal.Parse(usd);
                    exchangeRate.Eur = decimal.Parse(euro);
                    exchangeRate.Date = date;
                    exchangeRate.Year = date.Year;
                    exchangeRate.Month = date.Month;
                    exchangeRate.Day = date.Day;

                    dbContext.ExchangeRates.Add(exchangeRate);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}