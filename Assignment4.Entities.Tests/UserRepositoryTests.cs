using System;
using Assignment4.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assignment4.Entities.Tests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly IUserRepository _repo;
        private readonly KanbanContext _context;

        public UserRepositoryTests()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(connection);
            _context = new KanbanContext(builder.Options);
            _context.Database.EnsureCreated();

            _repo = new UserRepository(_context);

            var testTask = new Task { Title = "Test", Id = 1 };
            var pebn = new User { Name = "Test2", Email = "pebn@itu.dk", tasks = new Task[] { testTask } };

            _context.Add(pebn);
            _context.SaveChanges();
            _repo.Create(new UserCreateDTO { Name = "Test3", Email = "sgun@itu.dk" });
        }

        [Fact]
        public void Creating_a_new_user_should_return_created_and_user_id()
        {
            var (res, createdId) = _repo.Create(new UserCreateDTO { Name = "Test", Email = "emio@itu.dk" });

            Assert.Equal(Response.Created, res);
            Assert.Equal(3, createdId);
        }

        [Fact]
        public void Creating_an_existing_user_should_return_conflict()
        {
            _repo.Create(new UserCreateDTO { Name = "Test", Email = "emio@itu.dk" });
            var (res, createdId) = _repo.Create(new UserCreateDTO { Name = "Test", Email = "emio@itu.dk" });

            Assert.Equal(Response.Conflict, res);
        }

        [Fact]
        public void Can_read_user()
        {
            var (_, id) = _repo.Create(new UserCreateDTO { Name = "Test", Email = "emio@itu.dk" });

            var user = _repo.Read(id);

            Assert.Equal("Test", user.Name);
            Assert.Equal("emio@itu.dk", user.Email);
        }

        [Fact]
        public void Reading_non_existing_user_returns_null()
        {
            var user = _repo.Read(1234);
            Assert.Null(user);
        }

        [Fact]
        public void Reading_all_users_returns_a_list_of_2()
        {
            Assert.Equal(2, _repo.ReadAll().Count);
        }

        [Fact]
        public void Updating_non_existing_user_should_return_not_found()
        {
            var res = _repo.Update(new UserUpdateDTO { Id = 1234, Name = "hey", Email = "emio@itu.dk" });
            Assert.Equal(Response.NotFound, res);
        }

        [Fact]
        public void Updating_user_should_return_updated_and_update_db()
        {
            var newName = "Peter";
            var newEmail = "pebn@itu.dk";

            var res = _repo.Update(new UserUpdateDTO { Id = 1, Name = newName, Email = newEmail });
            var user = _repo.Read(1);

            Assert.Equal(Response.Updated, res);
            Assert.Equal(newName, user.Name);
            Assert.Equal(newEmail, user.Email);
        }

        [Fact]
        public void Deleting_a_user_should_decrease_the_numbers_of_users_in_the_db_by_one_and_return_deleted()
        {
            var numInDb = _repo.ReadAll().Count;

            var res = _repo.Delete(2, false);

            Assert.Equal(Response.Deleted, res);
            Assert.Equal(numInDb - 1, _repo.ReadAll().Count);
        }

        [Fact]
        public void Deleting_non_existing_user_should_return_not_found_and_not_remove_anything()
        {
            var numInDb = _repo.ReadAll().Count;

            var res = _repo.Delete(1324, false);

            Assert.Equal(numInDb, _repo.ReadAll().Count);
            Assert.Equal(Response.NotFound, res);
        }

        [Fact]
        public void Deleting_a_user_in_use_should_result_in_conflict_and_not_remove_anything()
        {
            var numInDb = _repo.ReadAll().Count;

            var res = _repo.Delete(1, false);

            Assert.Equal(numInDb, _repo.ReadAll().Count);
            Assert.Equal(Response.Conflict, res);
        }

        [Fact]
        public void Can_delete_a_user_in_use_using_the_force()
        {
            var numInDb = _repo.ReadAll().Count;

            var res = _repo.Delete(1, true);

            Assert.Equal(numInDb - 1, _repo.ReadAll().Count);
            Assert.Equal(Response.Deleted, res);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
