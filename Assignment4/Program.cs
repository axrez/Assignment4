using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Assignment4
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = LoadConfiguration();
            var connectionString = configuration.GetConnectionString("Kanban");

            using var connection = new SqlConnection(connectionString);
            var cmdText = "SELECT * FROM Task";
            using var command = new SqlCommand(cmdText, connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var test = new
                {
                    Id = reader.GetInt64(0),
                    Title = reader.GetString(1)
                };
                Console.WriteLine(test);
            }
        }

        static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>();

            return builder.Build();
        }
    }
}
