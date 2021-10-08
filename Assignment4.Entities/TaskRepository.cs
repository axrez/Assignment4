using Assignment4.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Assignment4;

namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        KanbanContext context { get; set; }

        public TaskRepository(KanbanContext context) => this.context = context;

        public IReadOnlyCollection<TaskDTO> All()
        {
            var tasks = from t in context.Tasks
                        select new TaskDTO
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Description = t.Description,
                            Tags = t.tags.Select(tag => tag.Name).ToList(),
                            State = t.state,
                        };

            return tasks.ToList();
        }

        public int Create(TaskDTO task)
        {
            User userToAssignTo;

            if (task.AssignedToId != null)
            {
                userToAssignTo = (from u in context.Users
                                  where u.Id == task.AssignedToId
                                  select new User
                                  {
                                      Email = u.Email,
                                      Name = u.Name,
                                      Id = u.Id,
                                      tasks = u.tasks
                                  }).FirstOrDefault();
            }


            var taskToInsert = new Task
            {
                Id = task.Id,
                Description = task.Description,
                Title = task.Title,
                state = task.State,
                tags = task.Tags.Select(tag => new Tag
                {
                    Name = tag,
                    tasks = new Task[1]
                }).ToList(),
            };

            context.Tasks.Add(taskToInsert);

            return 0;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public TaskDetailsDTO FindById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(TaskDTO task)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
