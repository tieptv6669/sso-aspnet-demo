using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using TestEfCore.Models;

namespace TestEfCore
{
    public class Program
    {
        public static ILoggerFactory LoggerFactory { get; private set; }
        public static IConfigurationRoot Configuration { get; private set; }

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

            Console.WriteLine("Environment: {0}", environment);

            var services = new ServiceCollection();

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            if (environment == "Development")
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = Path.Combine(currentDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{environment}.json");
                builder.AddJsonFile(path, optional: true);
            }
            else
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: false);
            }

            Configuration = builder.Build();

            string connectionString = Configuration.GetSection("ConnectionStrings").Value;

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<ApplicationContext>(o => o.UseSqlServer(connectionString), ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();

        }
    }
}
