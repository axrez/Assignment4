using System.IO;
using Assignment4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Assignment4
{
    public class KanbanContextFactory : IDesignTimeDbContextFactory<KanbanContext>
    {
        public KanbanContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<KanbanContext>()
                .Build();

            var connectionString = configuration.GetConnectionString("Kanban");

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>()
                .UseSqlServer(connectionString);

            return new KanbanContext(optionsBuilder.Options);
        }

        public static void Seed(KanbanContext context)
        {
            context.Database.ExecuteSqlRaw("DELETE dbo.TagTask");
            context.Database.ExecuteSqlRaw("DELETE dbo.Tasks");
            context.Database.ExecuteSqlRaw("DELETE dbo.Tags");
            context.Database.ExecuteSqlRaw("DELETE dbo.Users");

            var easyTag = new Tag { Name = "Easy" };
            var mediumTag = new Tag { Name = "Medium" };
            var hardTag = new Tag { Name = "Hard" };
            var cumbersomeTag = new Tag { Name = "Cumbersome" };

            var learnTask = new Task { Title = "Learn how to use ef framework", state = Core.State.Active, tags = new[] { hardTag, cumbersomeTag } };
            var useTask = new Task { Title = "Create code with ef framework", state = Core.State.New, tags = new[] { easyTag } };

            var emil = new User { Email = "emio@itu.dk", Name = "Emil", tasks = new[] { learnTask, useTask } };

            context.Users.Add(
                emil
            );

            context.SaveChanges();
        }
    }
}