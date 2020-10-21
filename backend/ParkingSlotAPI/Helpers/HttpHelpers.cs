using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Helpers
{
    public class HttpHelpers
    {

        public static async Task<string> GetResourceTwoHeaders(string uriString, string headerKey1 = "", string headerValue1 = "", string headerKey2 = "", string headerValue2 = "")
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uriString);
                    client.DefaultRequestHeaders.Add(headerKey1, headerValue1);
                    client.DefaultRequestHeaders.Add(headerKey2, headerValue2);

                    HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nHttpRequest couldn't be fulfilled.");
                Console.WriteLine("Message: {0} ", e.Message);
                return null;
            }
        }


        public static async Task<string> GetResource(string uriString, string headerKey = "", string headerValue = "")
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uriString);
                    client.DefaultRequestHeaders.Add(headerKey, headerValue);

                    HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nHttpRequest couldn't be fulfilled.");
                Console.WriteLine("Message: {0} ", e.Message);
                return null;
            }
        }

        public static async Task<string> GetResourceNoHeader(string uriString)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uriString);

                    HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nHttpRequest couldn't be fulfilled.");
                Console.WriteLine("Message: {0} ", e.Message);
                return null;
            }
        }
    }
}
