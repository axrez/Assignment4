using System.Collections.Generic;
using Assignment4.Core;
using System.Linq;

namespace Assignment4.Entities
{
    public class UserRepository : IUserRepository
    {
        private readonly KanbanContext _context;

        public UserRepository(KanbanContext context)
        {
            _context = context;
        }

        public (Response Response, int UserId) Create(UserCreateDTO user)
        {
            var existingUsersWithConflictingEmails = from u in _context.Users
                                                     where u.Email == user.Email
                                                     select u.Id;

            if (existingUsersWithConflictingEmails.ToList().Count > 0)
            {
                return (Response.Conflict, -1);
            }

            var userToInsert = new User { Email = user.Email, Name = user.Name };

            _context.Add(userToInsert);

            _context.SaveChanges();

            return (Response.Created, userToInsert.Id);
        }

        public Response Delete(int userId, bool force = false)
        {
            var user = _context.Users.Where(u => u.Id == userId).FirstOrDefault();

            if (user == null)
            {
                return Response.NotFound;
            }

            if (user.tasks != null && user.tasks.Count != 0 && !force)
            {
                return Response.Conflict;
            }

            _context.Remove(user);
            _context.SaveChanges();

            return Response.Deleted;
        }

        public UserDTO Read(int userId)
        {
            var users = from u in _context.Users
                        where u.Id == userId
                        select new UserDTO(u.Id, u.Name, u.Email);

            return users.FirstOrDefault();
        }

        public IReadOnlyCollection<UserDTO> ReadAll() => (from u in _context.Users
                                                          select new UserDTO(u.Id, u.Name, u.Email)).ToList();

        public Response Update(UserUpdateDTO user)
        {
            var readUser = _context.Users.Where(u => u.Id == user.Id).FirstOrDefault();

            if (readUser == null)
            {
                return Response.NotFound;
            }

            readUser.Email = user.Email;
            readUser.Name = user.Name;

            _context.SaveChanges();
            return Response.Updated;
        }
    }
}
