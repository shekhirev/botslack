using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Threading
{
    public class Result
    {
        public string text { get; set; }      
        public string response_type { get; set; }
        public List<Attachments> attachments { get; set; }
        public decimal feePercent { get; set; }

        public Result()
        {

        }

        public Result(string hostid, XMLBilling billing)
        {
            feePercent = (decimal)billing.FeePercent;
            text = "*Биллинг схема хоста № " + hostid + " *";
            text += (billing.Default) ? " `(используется дефолтная схема)`" : "" ;           
            
            attachments = new List<Attachments>();
        }

        public void PrepareSlackResponse(Billing bilObject)
        {
            string ExtraOrderMention = "В стоимость заказа дополнительно будет включаться комиссия ПС";
            string IncludedInHostFeeMention = "Величина ПС включена в комиссию компании";
            string ExtraHostFeeMention = "Компания дополнительно оплачивает комиссию ПС, т.е. помимо указанной fee с компании удерживается еще и сумма ПС";
            string ExtraOrderWithHostFeeMention = "Пользователь оплачивает комиссию организатора (идет в нашу прибыль) + комиссию ПС. С компании дополнительно ничего не удерживается";

            foreach (var channel in bilObject.Channel)
            {                
                string resText = "";
               
                foreach (var pt in channel.Payment)
                {

                    resText = "*Комиссия ПС = *" + pt.ps;
                    pt.fee = pt.fee.Contains("%") ? pt.fee : 
                                pt.fee.Contains("*") ? (decimal.Parse(pt.fee.Replace("*",""), CultureInfo.InvariantCulture) * feePercent).ToString("0.00") : pt.fee;
                    resText += "\n*Комиссия с орга* = " + pt.fee;
                    resText += "\n*Описание комиссии:* ";
                   
                    switch (pt.psPayMethod)
                    {
                        case "ExtraOrder":
                            resText += ExtraOrderMention;
                            break;
                        case "IncludedInHostFee":
                            resText += IncludedInHostFeeMention;
                            break;
                        case "ExtraHostFee":
                            resText += ExtraHostFeeMention;
                            break;
                        case "ExtraOrderWithHostFee":
                            resText += ExtraOrderWithHostFeeMention;
                            break;

                    }
                }
                
                string[] arrChannel = channel.Name.Split(',');   

                for (int i = 0; i < arrChannel.Length; i++)
                {
                    switch (arrChannel[i].Trim())
                    {
                        case "RadarioWebSite":
                            arrChannel[i] = "Сайт Радарио";
                            break;
                        case "MobileApp":
                            arrChannel[i] = "Мобильная версия Радарио";
                            break;
                        case "FiveMinutesSite":
                            arrChannel[i] = "Сайт пятиминутка";
                            break;
                        case "MobileWidget":
                            arrChannel[i] = "Мобильный виджет";
                            break;
                        case "Widget":
                            arrChannel[i] = "Виджет на сайте";
                            break;
                        case "VkApp":
                            arrChannel[i] = "Вконтакте";
                            break;
                        case "FbApp":
                            arrChannel[i] = "Фейсбук";
                            break;
                        case "OkApp":
                            arrChannel[i] = "Одноклассники";
                            break;
                        case "BookingOffice":
                            arrChannel[i] = "Касса";
                            break;
                        case "TicketDesk":
                            arrChannel[i] = "Билетный стол";
                            break;
                        case "CompanyContractor":
                            arrChannel[i] = "Контрагенты";
                            break;
                        case "Delivery":
                            arrChannel[i] = "Доставка";
                            break;
                        case "Api":
                            arrChannel[i] = "API";
                            break;
                        default:                            
                            break;
                    }
                }            
                

                attachments.Add(new Attachments {
                    pretext = "Каналы: " + string.Join(", ", arrChannel),
                    color = "#ff0000",
                    text = resText
                });
            }
        }
    }

    public class Attachments
    {
        public string text { get; set; }
        public string pretext { get; set; }
        public string color { get; set; }
        public string image_url { get; set; }
    }

    public class Info
    {        
        public string url { get; set; }
    }
    public class Fact
    {
        public int temp { get; set; }
        public string icon { get; set; }
        public string condition { get; set; }
    }

    public class Part
    {
        public string part_name { get; set; }
        public int temp_avg { get; set; }
        public string icon { get; set; }
        public string condition { get; set; }
    }

    public class Forecast
    {       
        public List<Part> parts { get; set; }
    }

    public class RootObject
    {
        public Info info { get; set; }
        public Fact fact { get; set; }
        public Forecast forecast { get; set; }
    }
}
