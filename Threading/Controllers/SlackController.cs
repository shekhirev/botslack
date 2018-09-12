using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Threading;

namespace Threading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlackController : ControllerBase
    {

        // POST: api/Slack
        [HttpPost]
        public async void PostGetWheather([FromForm]string text, [FromForm]string response_url)
        {
            string city = text;
            RootObject obj = await Wheather.GetWeather(city);

            city = (city == "msk")? "Москве":"Санкт-Петербурге";

            Result res = new Result
            {
                text = $"Сейчас в {city} *{obj.fact.temp}* (°C), {obj.fact.condition}\n" +
                $"Подробнее на Яндексе -> {obj.info.url} "
            };

            var client = new HttpClient();

            await client.PostAsync(response_url, new StringContent(
                JsonConvert.SerializeObject(res),
                Encoding.UTF8,
                "application/json"));
        }

        [HttpPost]
        [Route("loginashost")]
        public async Task<string> LoginAsHost([FromForm]string text)
        {
            try
            {
                string hostid = text;
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://radario.ru/internal/");
                client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("124214:123213")));
                return await client.GetStringAsync("login-uri?hostid=" + hostid);
            }
            catch (HttpRequestException ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("getBilling")]
        public async void Get([FromForm]string text, [FromForm]string response_url)
        {
            string hostid = text;
            XMLBilling billing = new XMLBilling();
            string xmlData = await BillingSchemeReader.ReadDataAsync(hostid, billing);

            var client = new HttpClient();

            if (xmlData.Contains("billing"))
            {
                Billing bs = await BillingSchemeReader.ReadXML(xmlData);
                Result res = new Result(hostid, billing);
                res.PrepareSlackResponse(bs);                

                await client.PostAsync(response_url, new StringContent(
                    JsonConvert.SerializeObject(res),
                    Encoding.UTF8,
                    "application/json"));
            }
            else
            {
                Result res = new Result
                {
                    text = xmlData
                };

                await client.PostAsync(response_url, new StringContent(
                    JsonConvert.SerializeObject(res),
                    Encoding.UTF8,
                    "application/json"));
            }
        }

        [HttpGet]
        [Route("get")]
        public async Task<string> GetQ([FromQuery]string hostid)
        {
            XMLBilling billing = new XMLBilling();
            string xmlData = await BillingSchemeReader.ReadDataAsync(hostid, billing);

            if (xmlData.Contains("billing"))
            {
                Billing bs = await BillingSchemeReader.ReadXML(xmlData);

                Result res = new Result(hostid, billing);
                res.PrepareSlackResponse(bs);

                return JsonConvert.SerializeObject(res);
            }            
            else
            {
                return xmlData;
            }

        }
    }
}
