using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Assignment4.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assignment4.Entities.Tests
{
	/*
	There is an issue inserting tags into the Sqlite database. I couldn't fix it, so i just ignore tags for now.
	*/
	public class TaskRepositoryTests : IDisposable
	{
		#region Setup
		private readonly KanbanContext _context;
		private readonly TaskRepository _repository;
		private readonly DateTime _utcTime;

		#region Tags
		private readonly Tag assignment = new Tag { Name = "Assignment" };
		private readonly Tag test = new Tag { Name = "Test" };
		private readonly Tag extra = new Tag { Name = "Extra" };
		private readonly Tag confusing = new Tag { Name = "Confusing" };
		#endregion

		public TaskRepositoryTests()
		{
			// Create Sqlite database and connect to it
			var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			var builder = new DbContextOptionsBuilder<KanbanContext>();
			builder.UseSqlite(connection);
			builder.EnableSensitiveDataLogging(true);
			var context = new KanbanContext(builder.Options);
			context.Database.EnsureCreated();

			// Create test data
			#region Tasks
			var implementTaskRepo = new Task
			{
				Id = 1,
				Title = "Implement TaskRepository",
				Description = "Implement ITaskRepository in the TaskRepository class.",
				AssignedToName = "Magnus",
				state = State.Active,
				tags = new[] { assignment, test }
			};
			var cryOverErrors = new Task
			{
				Id = 2,
				Title = "Cry over errors",
				Description = "WHY DOES IT NOT JUST WORK!?!?!?",
				AssignedToName = "Magnus",
				state = State.Resolved,
				tags = new[] { confusing }
			};
			var addTheThing = new Task
			{
				Id = 3,
				Title = "Add the thing",
				Description = "Sorry nvm.",
				AssignedToName = "Emil",
				state = State.New,
				tags = new[] { extra, confusing }
			};
			var setupStuff = new Task
			{
				Id = 4,
				Title = "Setup stuff",
				Description = "The stuff needs to be set up.",
				AssignedToName = "Emil",
				state = State.Closed,
				tags = new[] { confusing, test }
			};
			var badIdea = new Task
			{
				Id = 5,
				Title = "Bad idea",
				Description = "Trust me, it'll be great!",
				AssignedToName = null,
				state = State.Removed,
				tags = new Tag[] { }
			};
			#endregion

			#region Users
			var magnus = new User
			{
				Id = 1,
				Email = "daml@itu.dk",
				Name = "Magnus",
				tasks = new List<Task> { implementTaskRepo, cryOverErrors }
			};
			var emil = new User
			{
				Id = 2,
				Email = "emio@itu.dk",
				Name = "Emil",
				tasks = new List<Task> { addTheThing, setupStuff }
			};
			#endregion

			context.Users.AddRange(
				magnus,
				emil
			);
			context.Tasks.Add(badIdea);

			context.SaveChanges();

			_utcTime = DateTime.UtcNow;
			_context = context;
			_repository = new TaskRepository(_context, _utcTime);
		}

		public void Dispose()
		{
			_context.Dispose();
		}
		#endregion

		#region Read
		[Fact]
		public void Read_Nonexisting_Returns_Null()
		{
			// Arrange
			var id = 6;
			// Act
			var task = _repository.Read(id);
			// Assert
			Assert.Null(task);
		}

		[Fact]
		public void Read_Existing_Returns_Task_With_Id()
		{
			// Arrange
			var id = 1;
			// Act
			var task = _repository.Read(id);
			// Assert
			Assert.Equal(id, task.Id);
			Assert.Equal("Implement TaskRepository", task.Title);
			Assert.Equal("Implement ITaskRepository in the TaskRepository class.", task.Description);
			Assert.Equal(new DateTime(), task.Created);
			Assert.Equal("Magnus", task.AssignedToName);
			Assert.Equal(State.Active, task.State);
			Assert.Equal(new DateTime(), task.StateUpdated);
		}

		[Fact]
		public void Read_Add_Returns_All()
		{
			// Arrange
			var expected = new[] {
				new TaskDTO(
					1,
					"Implement TaskRepository",
					"Magnus",
					new[] { assignment.Name, test.Name },
					State.Active
				),
				new TaskDTO(
					2,
					"Cry over errors",
					"Magnus",
					new[] { confusing.Name },
					State.Resolved
				),
				new TaskDTO(
					3,
					"Add the thing",
					"Emil",
					new[] { extra.Name, confusing.Name },
					State.New
				),
				new TaskDTO(
					4,
					"Setup stuff",
					"Emil",
					new[] { confusing.Name, test.Name },
					State.Closed
				),
				new TaskDTO(
					5,
					"Bad idea",
					null,
					new string[] { },
					State.Removed
				)
			};
			// Act
			var tasks = _repository.ReadAll();
			var comparer = new TaskDTOComparer();
			// Assert
			Assert.Equal(expected, tasks, comparer);
		}

		[Fact]
		public void ReadAllByState_Returns_All_With_State()
		{
			// Arrange
			var expected = new[] {
				new TaskDTO(
					1,
					"Implement TaskRepository",
					"Magnus",
					new[] { assignment.Name, test.Name },
					State.Active
				)
			};
			// Act
			var tasks = _repository.ReadAllByState(State.Active);
			var comparer = new TaskDTOComparer();
			// Assert
			Assert.Equal(expected, tasks, comparer);
		}

		[Fact]
		public void ReadAllRemoved_Returns_All_Removed()
		{
			// Arrange
			var expected = new[] {
				new TaskDTO(
					5,
					"Bad idea",
					null,
					new string[] { },
					State.Removed
				)
			};
			// Act
			var tasks = _repository.ReadAllRemoved();
			var comparer = new TaskDTOComparer();
			// Assert
			Assert.Equal(expected, tasks, comparer);
		}

		[Fact]
		public void ReadAllByTag_Returns_All_With_Tag()
		{
			// Arrange
			var expected = new[] {
				new TaskDTO(
					2,
					"Cry over errors",
					"Magnus",
					new[] { confusing.Name },
					State.Resolved
				),
				new TaskDTO(
					3,
					"Add the thing",
					"Emil",
					new[] { extra.Name, confusing.Name },
					State.New
				),
				new TaskDTO(
					4,
					"Setup stuff",
					"Emil",
					new[] { confusing.Name, test.Name },
					State.Closed
				)
			};
			// Act
			var tasks = _repository.ReadAllByTag("Confusing");
			var comparer = new TaskDTOComparer();
			// Assert
			Assert.Equal(expected, tasks, comparer);
		}

		[Fact]
		public void ReadAllByUser_Returns_All_With_User()
		{
			// Arrange
			var expected = new[] {
				new TaskDTO(
					3,
					"Add the thing",
					"Emil",
					new[] { extra.Name, confusing.Name },
					State.New
				),
				new TaskDTO(
					4,
					"Setup stuff",
					"Emil",
					new[] { confusing.Name, test.Name },
					State.Closed
				)
			};
			// Act
			var tasks = _repository.ReadAllByUser(2);
			var comparer = new TaskDTOComparer();
			// Assert
			Assert.Equal(expected, tasks, comparer);
		}
		#endregion
		
		#region Create
		[Fact]
		public void Created_Returns_Response_Created_And_Id_6()
		{
			// Arrange
			var newTask = new TaskCreateDTO {
				Title = "New Task",
				AssignedToId = 1,
				Description = "Unimaginative description.",
				Tags = new[] { assignment.Name, confusing.Name }
			};
			// Act
			var result = _repository.Create(newTask);
			// Assert
			Assert.Equal((Response.Created, 6), result);
		}

		[Fact]
		public void Created_With_Nonexisting_User_Returns_Bad_Request()
		{
			// Arrange
			var newTask = new TaskCreateDTO {
				Title = "New Task",
				AssignedToId = 3,
				Description = "Unimaginative description.",
				Tags = new[] { assignment.Name, confusing.Name }
			};
			// Act
			var result = _repository.Create(newTask);
			// Assert
			Assert.Equal((Response.BadRequest, 0), result);
		}

		[Fact]
		public void Created_Has_Correct_Created_And_State_Updated()
		{
			// Arrange
			var newTask = new TaskCreateDTO {
				Title = "New Task",
				AssignedToId = 1,
				Description = "Unimaginative description.",
				Tags = new[] { assignment.Name, confusing.Name }
			};
			// Act
			var result = _repository.Create(newTask);
			// Assert
			var createdTask = _repository.Read(result.TaskId);
			Assert.Equal(_utcTime, createdTask.Created);
			Assert.Equal(_utcTime, createdTask.StateUpdated);
		}

		[Fact]
		public void Create_With_Nonexisting_Tag_Creates_Tag()
		{
			// Arrange
			var newTask = new TaskCreateDTO {
				Title = "New Task",
				AssignedToId = 1,
				Description = "Unimaginative description",
				Tags = new[] { confusing.Name, "Super Secret Tag"}
			};
			// Act
			_repository.Create(newTask);
			// Assert
			Assert.NotNull((
				from t in _context.Tags
				where t.Name == "Super Secret Tag"
				select t
			).FirstOrDefault());
		}
		
		[Fact]
		public void Create_Adds_Task_To_User()
		{
			// Arrange
			var newTask = new TaskCreateDTO {
				Title = "New Task",
				AssignedToId = 1,
				Description = "Unimaginative description",
				Tags = new[] { assignment.Name, confusing.Name }
			};
			// Act
			var result = _repository.Create(newTask);
			// Assert
			var user = (
				from u in _context.Users
				where u.Id == newTask.AssignedToId
				select u
			).FirstOrDefault();
			Assert.Contains(result.TaskId, from t in user.tasks select t.Id);
		}
		#endregion

		#region Update
		[Fact]
		public void Update_Returns_Response_Updated()
		{
			// Arrange
			var update = new TaskUpdateDTO
			{
				Id = 1,
				State = State.Resolved
			};
			// Act
			var result = _repository.Update(update);
			// Assert
			Assert.Equal(Response.Updated, result);
		}

		[Fact]
		public void Update_Nonexsisting_Returns_Respones_NotFound()
		{
			// Arrange
			var update = new TaskUpdateDTO
			{
				Id = 6,
				State = State.Resolved
			};
			// Act
			var result = _repository.Update(update);
			// Assert
			Assert.Equal(Response.NotFound, result);
		}

		[Fact]
		public void Update_Changes_State()
		{
			// Arrange
			var update = new TaskUpdateDTO
			{
				Id = 1,
				State = State.Resolved
			};
			// Act
			var result = _repository.Update(update);
			// Assert
			Assert.Equal(State.Resolved, _repository.Read(1).State);
		}

		[Fact]
		public void Update_Changes_Time()
		{
			// Arrange
			var update = new TaskUpdateDTO
			{
				Id = 1,
				State = State.Resolved
			};
			// Act
			var result = _repository.Update(update);
			// Assert
			Assert.Equal(_utcTime, _repository.Read(1).StateUpdated);
		}
		#endregion

		#region Delete
		[Fact]
		public void Delete_Existing_Responds_Deleted()
		{
			// Arrange
			var deleteId = 1;
			// Act
			var result = _repository.Delete(deleteId);
			// Assert
			Assert.Equal(Response.Deleted, result);
		}

		[Fact]
		public void Delete_Nonexisting_Responds_NotFound()
		{
			// Arrange
			var deleteId = 6;
			// Act
			var result = _repository.Delete(deleteId);
			// Assert
			Assert.Equal(Response.NotFound, result);
		}

		[Fact]
		public void Delete_New_Removes_From_Database()
		{
			// Arrange
			var deleteId = 3;
			// Act
			_repository.Delete(deleteId);
			// Assert
			Assert.Null(_repository.Read(deleteId));
		}

		[Theory]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(5)]
		public void Delete_Resolved_Closed_Or_Removed_Responds_Conflict(int deleteId)
		{
			// Act
			var results = _repository.Delete(deleteId);
			// Assert
			Assert.Equal(Response.Conflict, results);
		}

		[Fact]
		public void Delete_Active_Makes_It_Removed()
		{
			// Arrange
			var deleteId = 1;
			// Act
			_repository.Delete(deleteId);
			// Assert
			Assert.Equal(State.Removed, _repository.Read(deleteId).State);
		}
		#endregion

		private class TaskDetailsDTOComparer : IEqualityComparer<TaskDetailsDTO>
		{
			public bool Equals(TaskDetailsDTO x, TaskDetailsDTO y)
			{
				var isEqual = x.Id == y.Id
					       && x.Title == y.Title
					       && x.Description == y.Description
					       && x.Created == y.Created
					       && x.AssignedToName == y.AssignedToName
					       && x.State == y.State
					       && x.StateUpdated == y.StateUpdated;
				if (isEqual == false) return false;
				if (x.Tags == null)
					return y.Tags == null;
				else
					return x.Tags.OrderBy(s => s).SequenceEqual(y.Tags.OrderBy(s => s));
			}

			public int GetHashCode([DisallowNull] TaskDetailsDTO obj)
			{
				return obj.GetHashCode();
			}
		}

		private class TaskDTOComparer : IEqualityComparer<TaskDTO>
		{
			public bool Equals(TaskDTO x, TaskDTO y)
			{
				var isEqual = x.Id == y.Id
					       && x.Title == y.Title
					       && x.AssignedToName == y.AssignedToName
					       && x.State == y.State;
				if (isEqual == false) return false;
				if (x.Tags == null)
					return y.Tags == null;
				else
					return x.Tags.OrderBy(s => s).SequenceEqual(y.Tags.OrderBy(s => s));
			}

			public int GetHashCode([DisallowNull] TaskDTO obj)
			{
				return obj.GetHashCode();
			}
		}
	}
}
