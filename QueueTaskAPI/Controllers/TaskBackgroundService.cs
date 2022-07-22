using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
//using System.Net.Http;
using QueueTaskAPI.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using System.Net.Http;


namespace QueueTaskAPI.Controllers
{
    public class TaskBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private ConnectionFactory factory;
        public TaskBackgroundService(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<TaskBackgroundService>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = "host.docker.internal", Port = 31672 };
            // localhost
            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _channel.QueueDeclare("task-processed", true, false, false, null);
            // _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            // _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message  
                HandleMessage(content);
                // _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("task-processed", false, consumer);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message  
                HandleMessage(content);
                // _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("task-processed", false, consumer);
            return Task.CompletedTask;
        }

        private async void HandleMessage(string contentinput)
        {
           

            var objTask = Newtonsoft.Json.JsonConvert.DeserializeObject<QueueTask>(contentinput);
            string suiId = objTask.TaskId.ToString();
             //var apiurl = "http://localhost:9697/api/QueueTasks/"+ suiId;
              var apiurl = "http://host.docker.internal:31845/api/QueueTasks/"+ suiId;
            if (objTask != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiurl);
                    HttpResponseMessage result;

                    try
                    {

                        var json = JsonConvert.SerializeObject(objTask);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        //Console.WriteLine("In PostAsync: before PostAsync:"+json.Length );
                        result = await client.PutAsync("", content);
                        result.EnsureSuccessStatusCode();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in PostAsync:" + e.Message);
                        if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                    }



                }

            }

        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
