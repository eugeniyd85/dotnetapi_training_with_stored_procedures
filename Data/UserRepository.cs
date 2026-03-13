using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
        DataContextEF _entityFramework;

        public UserRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }

        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }

        // public bool AddEntity<T>(T entityToAdd)
        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
                // return true;
            }
            // return false;
        }

        // public bool AddEntity<T>(T entityToAdd)
        public void RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entityFramework.Remove(entityToRemove);
                // return true;
            }
            // return false;
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList();
            return users;
        }

        public User GetSingleUser(int userId)
        {
            User? user = _entityFramework.Users.Where(u => u.UserId == userId).FirstOrDefault();

            if (user != null)
            {
                return user;
            }

            throw new Exception("User not found");
        }

        public UserSalary GetUserSalary(int userId)
        {
            UserSalary? userSalary = _entityFramework.UserSalary.Where(us => us.UserId == userId).FirstOrDefault();

            if (userSalary != null)
            {
                return userSalary;
            }

            throw new Exception("User salary not found");
        }

        public UserJobInfo GetUserJobInfo(int userId)
        {
            UserJobInfo? userJobInfo = _entityFramework.UserJobInfo.Where(uj => uj.UserId == userId).FirstOrDefault();

            if (userJobInfo != null)
            {
                return userJobInfo;
            }

            throw new Exception("User job info not found");
        }
    }
}