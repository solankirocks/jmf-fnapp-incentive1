using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
	public static class CustomConfiguration
	{
		public static IConfigurationRoot config;

		public static void BuildConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();
			config = builder.Build();
		}
	}
	public static class AzureConfigurationManager
	{
		public static IConfiguration Configuration { get; set; } = CustomConfiguration.config;

		public static string? GetSetting(string name)
		{
			return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
		}

		public static string? GetConnection(string name)
		{
			
			string connectionStringValue;
			connectionStringValue = CustomConfiguration.config.GetConnectionString(name) ?? string.Empty;
			
			return connectionStringValue;
		}
	}

}
