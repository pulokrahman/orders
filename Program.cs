using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Orders.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Orders
{
    public class Program
    {
        private static readonly Gauge JobsInQueue = Metrics
    .CreateGauge("NumpfOrdersinProcess", "OrderinProcess");
       

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var scope = host.Services.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<OrdersContext>();
            var orderStatus2 = _context.OrderStatus.ToList();
            orderStatus2.RemoveAll(t => t.status != "MAKING");
            int i = orderStatus2.Count;
            JobsInQueue.IncTo(i);
            host.Run();
            var server = new MetricServer(hostname: "localhost", port: 5000);
            server.Start();
            
           


        }
        public static void Increase()
        {
            JobsInQueue.Inc();
        }
        public static void Decrease()
        {
            JobsInQueue.Dec();
        } 

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
