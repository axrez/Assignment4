using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Assignment4.Entities;

namespace Assignment4
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = LoadConfiguration();
            var connectionString = configuration.GetConnectionString("Kanban");

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>().UseSqlServer(connectionString);
            using var context = new KanbanContext(optionsBuilder.Options);





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
