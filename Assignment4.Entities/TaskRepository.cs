using System;
using System.Collections.Generic;
using System.Linq;
using Assignment4.Core;
using static Assignment4.Core.Response;

namespace Assignment4.Entities
{
	public class TaskRepository : ITaskRepository
	{
        private readonly KanbanContext _context;

		public TaskRepository(KanbanContext context)
		{
			_context = context;
		}

		#region Create
		public (Response Response, int TaskId) Create(TaskCreateDTO task)
		{
			throw new System.NotImplementedException();
		}
        #endregion

        #region Delete
		public Response Delete(int taskId)
		{
			throw new System.NotImplementedException();
		}
        #endregion

        #region Read
		public TaskDetailsDTO Read(int taskId)
		{
			var tasks =
                from t in _context.Tasks
                where t.Id == taskId
                select new TaskDetailsDTO(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Created,
                    t.AssignedToName,
                    t.tags.Select(x => x.Name).ToArray(),
                    t.state,
                    t.StateUpdated
                );
            
            return tasks.FirstOrDefault();
		}

		public IReadOnlyCollection<TaskDTO> ReadAll()
		{
			var tasks =
                from t in _context.Tasks
                select new TaskDTO(
                    t.Id,
                    t.Title,
                    t.AssignedToName,
                    t.tags.Select(x => x.Name).ToArray(),
                    t.state
                );
            
            return tasks.ToArray();
		}

		public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
		{
			var tasks =
                from t in _context.Tasks
                where t.state == state
                select new TaskDTO(
                    t.Id,
                    t.Title,
                    t.AssignedToName,
                    t.tags.Select(x => x.Name).ToArray(),
                    t.state
                );
            
            return tasks.ToArray();
		}

		public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
		{
			var tasks =
                from t in _context.Tasks
                where t.tags.Select(x => x.Name).Contains(tag)
                select new TaskDTO(
                    t.Id,
                    t.Title,
                    t.AssignedToName,
                    t.tags.Select(x => x.Name).ToArray(),
                    t.state
                );

            return tasks.ToArray();
		}

		public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
		{
            var user = (
                from u in _context.Users
                where u.Id == userId
                select u
            ).FirstOrDefault();

            var tasks =
                from t in user.tasks
                select new TaskDTO(
                    t.Id,
                    t.Title,
                    t.AssignedToName,
                    t.tags.Select(x => x.Name).ToArray(),
                    t.state
                );

            return tasks.ToArray();
		}

		public IReadOnlyCollection<TaskDTO> ReadAllRemoved()
		{
			return ReadAllByState(State.Removed);
		}
        #endregion
        
        #region  Update
		public Response Update(TaskUpdateDTO task)
		{
			throw new System.NotImplementedException();
		}
        #endregion
	}
}
