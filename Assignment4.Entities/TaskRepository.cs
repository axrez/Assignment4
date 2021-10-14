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
        private readonly DateTime? _utcTime;
        private DateTime UtcTime {
            get => _utcTime ?? DateTime.UtcNow;
        } 

		public TaskRepository(KanbanContext context)
		{
			_context = context;
		}

        public TaskRepository(KanbanContext context, DateTime utcTime) {
            _context = context;
            _utcTime = utcTime;
        }

		#region Create
		public (Response Response, int TaskId) Create(TaskCreateDTO task)
		{
            var assignedUser = (
                from u in _context.Users
                where u.Id == task.AssignedToId
                select u
            ).FirstOrDefault();

            if (assignedUser == null)
                return (Response.BadRequest, 0);

            var tags = (
                from t in _context.Tags.ToList()
                join s in task.Tags on t.Name equals s
                select t
            ).ToList();

			if (tags.Count() != task.Tags.Count())
				tags = tags.Concat(CreateMissingTags(tags, task.Tags)).ToList();
            
            var utcNow = UtcTime;

			var newTask = new Task
			{
                Title = task.Title,
                Description = task.Description,
                Created = utcNow,
                AssignedToName = assignedUser.Name,
                state = State.New,
                tags = tags,
                StateUpdated = utcNow
			};

            _context.Tasks.Add(newTask);

			assignedUser.tasks.Add(newTask);

            _context.SaveChanges();

            return (Response.Created, newTask.Id);
		}
		#endregion

		#region Delete
		public Response Delete(int taskId)
		{
            var task = _context.Tasks.Find(taskId);
            if (task == null)
                return Response.NotFound;
            switch (task.state)
            {
                case State.New:
                    _context.Tasks.Remove(task);
                    break;
				case State.Active:
					task.state = State.Removed;
					break;
				case State.Resolved:
					return Response.Conflict;
				case State.Closed:
					return Response.Conflict;
				case State.Removed:
					return Response.Conflict;
                default:
                    break;
            }
            _context.SaveChanges();
			return Response.Deleted;
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
            var taskToUpdate = _context.Tasks.Find(task.Id);
            if (taskToUpdate == null)
                return Response.NotFound;
            taskToUpdate.state = task.State;
            taskToUpdate.StateUpdated = UtcTime;
            _context.SaveChanges();
			return Response.Updated;
		}
		#endregion

		private IEnumerable<Tag> CreateMissingTags(IEnumerable<Tag> existing, IEnumerable<string> requested)
		{
			var existingStrings = from t in existing select t.Name;
			var missingTags = new List<Tag>();
			foreach (var str in requested)
			{
				if (existingStrings.Contains(str)) continue;
				else
				{
					var newTag = new Tag { Name = str };
					missingTags.Add(newTag);
				}
			}
			return missingTags;
		}
	}
}
