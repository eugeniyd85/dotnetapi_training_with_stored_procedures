using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    // IUserRepository has access to the methods defined in UserRepository.cs and will be used to call those methods in the controllers through dependency injection
    public interface IUserRepository
    {
        // these are calls of appropriate methods from UserRepository.cs which will be made through this interface in the controllers
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToRemove);
        public IEnumerable<User> GetUsers();
        public User GetSingleUser(int userId);
        public UserSalary GetUserSalary(int userId);
        public UserJobInfo GetUserJobInfo(int userId);
    }
}