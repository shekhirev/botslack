using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml;

namespace Threading
{

    public class Wheather
    {
        public static async Task<RootObject> GetWeather(string city)
        {
            HttpClient client = new HttpClient();
            string resBody;

            client.DefaultRequestHeaders.Add("X-Yandex-API-Key", "78b25721-5bc0-464b-8f07-74206ab6d28c");

            string baseAPIURI = "http://api.weather.yandex.ru/v1/informers?";
            string latParams = "lat=" + ((city == "msk") ? "55.752220" : "59.938630");
            string lonParams = "lon=" + ((city == "msk") ? "37.615560" : "30.314130");
                        
            try
            {                
                resBody = await client.GetStringAsync(baseAPIURI + latParams + "&" + lonParams);                
                return JsonConvert.DeserializeObject<RootObject>(resBody);

            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
    public class XMLBilling
    {
        public object FeePercent { get; set; } = null;
        public object data { get; set; } = null;
        public object BillingSchemeId { get; set; } = null;
        public bool Default { get; set; } = true;
    }

    public class BillingSchemeReader
    {
        public static async Task<string> ReadDataAsync(string hostid, XMLBilling billing)
        {
            string connectionString = @"Data Source=DESKTOP-54SB01U;Initial Catalog=Radario;Integrated Security=True";
                                
            string strBilling = "";
            string selBilFromCompany = "SELECT FeePercent, BillingSchemeId FROM [Company] WHERE id=" + hostid;           

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                SqlCommand command = new SqlCommand(selBilFromCompany, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        billing.FeePercent = reader.GetValue(0);
                        if (!reader.IsDBNull(reader.GetOrdinal("BillingSchemeId")))
                        {
                            billing.BillingSchemeId = reader.GetValue(1); ;
                        }
                    }

                    reader.Close();
                }
                else
                {
                    return "Такого хоста нет в базе, или что-то пошло не так.";
                }

                if (billing.BillingSchemeId == null)
                {
                    strBilling = "SELECT data FROM [BillingScheme] WHERE id=1";
                    billing.Default = true;
                }
                else
                {
                    strBilling = "SELECT data FROM [BillingScheme] WHERE GUID='" + billing.BillingSchemeId + "'";
                    billing.Default = false;
                }                

                command = new SqlCommand(strBilling, connection);
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        billing.data = reader.GetValue(0);
                    }

                }

                reader.Close();
            }

            return billing.data.ToString();
        }

        public static async Task<Billing> ReadXML(string xmlBilling)
        {
            Billing bs = new Billing();

            XmlDocument document = new XmlDocument();
            document.LoadXml(xmlBilling);

            XmlElement xmlRoot = document.DocumentElement;
            XmlNodeList channel = xmlRoot.SelectNodes("channel");

            foreach (XmlNode channelNode in channel)
            {
                List<Payment> pay = new List<Payment>();
                foreach (XmlNode paymentNode in channelNode.ChildNodes)
                {
                    Payment payment = new Payment();

                    if (paymentNode.Attributes.Count > 0)
                    {
                        var paymentType = paymentNode.Attributes.GetNamedItem("type").Value;
                        var isFree = paymentType.Contains("Free");
                        var isEntirelyPromoCode = paymentType.Contains("EntirelyPromoCode");

                        if (isFree || isEntirelyPromoCode)
                        {
                            continue;
                        }
                        
                        payment.Type = paymentType;
                    }
                    if (paymentNode.SelectNodes("CommonCommissionProvider").Count > 0)
                    {
                        var cp = paymentNode.SelectSingleNode("CommonCommissionProvider");

                        payment.ps = cp.Attributes.GetNamedItem("ps").Value;
                        payment.fee = cp.Attributes.GetNamedItem("fee").Value;
                        payment.psPayMethod = cp.Attributes.GetNamedItem("psPayMethod").Value;
                    }
                    pay.Add(payment);

                }

                bs.Channel.Add(new Channel {
                    Name = channelNode.SelectSingleNode("@name").Value,
                    Payment = pay
                });
                Console.WriteLine();
            }

            return bs;
        }
    }
}
