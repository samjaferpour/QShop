using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System;
using System.Text;
using System.Reflection.Metadata;
using System.Runtime.Serialization;

namespace QShop
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            this._httpClientFactory = httpClientFactory;
        }
        public class CategoryRequest
        {
            public string? Name { get; set; }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory()
                {
                    HostName= "localhost",
                    VirtualHost= "/",
                    UserName= "guest",
                    Password= "guest",
                    Port = 5672
                };
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: "shop.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false);
                channel.QueueDeclare(queue: "shop_queue", durable: true, exclusive: false, autoDelete: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received +=  async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var bodyString = Encoding.UTF8.GetString(body);
                    var request = JsonConvert.DeserializeObject<CategoryRequest>(bodyString);
                    


                    var client = _httpClientFactory.CreateClient("shop");
                    var proxyRequest = new HttpRequestMessage(HttpMethod.Post, "api/Category/AddNewCategory");                  
                    proxyRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    try
                    {
                        var proxyResponse = await client.SendAsync(proxyRequest);
                        if (!proxyResponse.IsSuccessStatusCode)
                        {
                            channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                        }
                        //var proxyString = JsonConvert.DeserializeObject<string>(proxyResponse);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    
                };
                channel.BasicConsume(queue: "shop_queue", autoAck: false, consumer: consumer);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //await Task.Delay(1000, stoppingToken);

            }
        }
    }
}