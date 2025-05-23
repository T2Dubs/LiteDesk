using System.Windows;
using LiteDesk.Core;
using LiteDesk.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace LiteDesk
{
    public partial class App : Application
    {
        public static IServiceProvider? Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var dbConfig = config.GetSection("Database").Get<AppDbConfig>()
                ?? throw new InvalidOperationException("Missing or invalid 'Database' config in appsettings.json");
            serviceCollection.AddSingleton(dbConfig);

            switch (dbConfig.Type)
            {
                case "Sqlite":
                    serviceCollection.AddSingleton<IDatabaseService, SqliteService>();
                    break;
                case "SqlServer":
                    serviceCollection.AddSingleton<IDatabaseService, SqlServerService>();
                    break;
                case "Postgres":
                    serviceCollection.AddSingleton<IDatabaseService, PostgresService>();
                    break;
                case "MySql":
                    serviceCollection.AddSingleton<IDatabaseService, MySqlService>();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported DB type: {dbConfig.Type}");
            }

            Services = serviceCollection.BuildServiceProvider();
            base.OnStartup(e);
        }
    }
}