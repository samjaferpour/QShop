// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Text;






var client = new HttpClient();
var bodyString = "{\"name\":\"string 1000\"}";
var requestJson = JsonConvert.SerializeObject(bodyString);
var queryString = new StringContent(requestJson, Encoding.UTF8, "application/json");
var proxyResponse = client.PostAsync("http://localhost:5009/api/Category/AddNewCategory", queryString).Result;

Console.WriteLine("Hello, World!");
