using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Threading
{
    public class Result
    {
        public string text { get; set; }      
        public string response_type { get; set; }
        public List<Attachments> attachments { get; set; }
        public string feePercent { get; set; }

        public Result()
        {

        }

        public Result(string hostid, XMLBilling billing)
        {
            feePercent = billing.FeePercent.ToString();
            text = "*Биллинг схема хоста № " + hostid + " *";
            text += (billing.Default) ? " `(используется дефолтная схема)`" : "" ;
            text += "\n`Примечание`:";
            text += "\nExtraOrder, // В стоимость заказа дополнительно будет включаться комиссия ПС";
            text += "\nIncludedInHostFee, // Величина ПС включена в комиссию компании";
            text += "\nExtraHostFee, // Компания дополнительно оплачивает комиссию ПС, т.е. помимо указанной fee с компании удерживается еще и сумма ПС";
            text += "\nExtraOrderWithHostFee // Пользователь оплачивает комиссию организатора (идет в нашу прибыль) + комиссию ПС. С компании дополнительно ничего не удерживается";
            
            attachments = new List<Attachments>();
        }

        public void PrepareSlackResponse(Billing bilObject)
        {
            
            foreach (var channel in bilObject.Channel)
            {                
                string resText = "";
               
                foreach (var pt in channel.Payment)
                {
                    resText = "*Комиссия ПС = *" + pt.ps;
                    pt.fee = pt.fee.Contains("%") ? pt.fee : feePercent;
                    resText += "\n*Комиссия с орга* = " + pt.fee;
                    resText += "\n*Комиссия на ком* = " + pt.psPayMethod;
                }
                attachments.Add(new Attachments {
                    pretext = "Каналы: " + channel.Name,
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
