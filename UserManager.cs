using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleProject
{
    // TODO: Refactor this class - it's doing too much!
    public class UserManager
    {
        private readonly Dictionary<int, User> _users;
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;
        private readonly IDatabase _database;

        public UserManager(ILogger logger, IEmailService emailService, IDatabase database)
        {
            _users = new Dictionary<int, User>();
            _logger = logger;
            _emailService = emailService;
            _database = database;
        }

        // TODO: Add validation for email format
        public async Task<User> CreateUserAsync(string username, string email, string password, string firstName, string lastName, DateTime birthDate, string address, string city, string country)
        {
            try
            {
                // This is a very long line that will definitely trigger the long line analyzer because it has way more than 100 characters
                var user = new User { Id = GenerateUserId(), Username = username, Email = email, Password = HashPassword(password), FirstName = firstName, LastName = lastName };
                
                _users.Add(user.Id, user);
                await _database.SaveUserAsync(user);
                await _emailService.SendWelcomeEmailAsync(email);
                
                _logger.Log($"User created: {username}");
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create user: {ex.Message}");
                throw;
            }
        }

        public User GetUser(int userId)
        {
            if (_users.ContainsKey(userId))
            {
                return _users[userId];
            }
            return null;
        }

        public List<User> GetAllUsers()
        {
            return _users.Values.ToList();
        }

        public async Task<bool> UpdateUserAsync(int userId, string email, string firstName, string lastName)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            user.Email = email;
            user.FirstName = firstName;
            user.LastName = lastName;

            await _database.UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            _users.Remove(userId);
            await _database.DeleteUserAsync(userId);
            return true;
        }

        private int GenerateUserId()
        {
            return _users.Count + 1;
        }

        private string HashPassword(string password)
        {
            // TODO: Implement proper password hashing with bcrypt or similar
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = _users.Values.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return false;
            }

            var hashedPassword = HashPassword(password);
            return user.Password == hashedPassword;
        }

        public List<User> SearchUsers(string searchTerm)
        {
            return _users.Values
                .Where(u => u.Username.Contains(searchTerm) || u.Email.Contains(searchTerm))
                .ToList();
        }

        public async Task SendEmailToAllUsersAsync(string subject, string body)
        {
            foreach (var user in _users.Values)
            {
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        public Dictionary<string, int> GetUserStatistics()
        {
            var stats = new Dictionary<string, int>();
            stats["TotalUsers"] = _users.Count;
            stats["ActiveUsers"] = _users.Values.Count(u => u.IsActive);
            stats["InactiveUsers"] = _users.Values.Count(u => !u.IsActive);
            return stats;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            var hashedOldPassword = HashPassword(oldPassword);

            if (user.Password != hashedOldPassword)
            {
                return false;
            }

            user.Password = HashPassword(newPassword);
            await _database.UpdateUserAsync(user);
            return true;
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            var activeUsers = _users.Values.Where(u => u.IsActive).ToList();
            return await Task.FromResult(activeUsers);
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            user.IsActive = false;
            await _database.UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            user.IsActive = true;
            await _database.UpdateUserAsync(user);
            return true;
        }

        // TODO: Add role-based permissions
        public bool HasPermission(int userId, string permission)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            return user.Permissions.Contains(permission);
        }

        public async Task<bool> AddPermissionAsync(int userId, string permission)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            if (!user.Permissions.Contains(permission))
            {
                user.Permissions.Add(permission);
                await _database.UpdateUserAsync(user);
            }
            return true;
        }

        public async Task<bool> RemovePermissionAsync(int userId, string permission)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            if (user.Permissions.Contains(permission))
            {
                user.Permissions.Remove(permission);
                await _database.UpdateUserAsync(user);
            }
            return true;
        }

        public List<User> GetUsersCreatedAfter(DateTime date)
        {
            return _users.Values.Where(u => u.CreatedAt > date).ToList();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await Task.FromResult(_users.Count);
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfile profile)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            user.Profile = profile;
            await _database.UpdateUserAsync(user);
            return true;
        }

        public List<User> GetUsersByRole(string role)
        {
            return _users.Values.Where(u => u.Role == role).ToList();
        }

        public async Task<bool> AssignRoleAsync(int userId, string role)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            user.Role = role;
            await _database.UpdateUserAsync(user);
            return true;
        }

        public async Task BulkImportUsersAsync(List<UserImportDto> importData)
        {
            foreach (var dto in importData)
            {
                await CreateUserAsync(dto.Username, dto.Email, dto.Password, dto.FirstName, dto.LastName, dto.BirthDate, dto.Address, dto.City, dto.Country);
            }
        }

        public async Task<byte[]> ExportUsersToCSVAsync()
        {
            // TODO: Implement CSV export functionality
            return await Task.FromResult(new byte[0]);
        }

        public async Task<bool> MergeUsersAsync(int sourceUserId, int targetUserId)
        {
            if (!_users.ContainsKey(sourceUserId) || !_users.ContainsKey(targetUserId))
            {
                return false;
            }

            var sourceUser = _users[sourceUserId];
            var targetUser = _users[targetUserId];

            // Merge permissions
            foreach (var permission in sourceUser.Permissions)
            {
                if (!targetUser.Permissions.Contains(permission))
                {
                    targetUser.Permissions.Add(permission);
                }
            }

            await DeleteUserAsync(sourceUserId);
            await _database.UpdateUserAsync(targetUser);
            return true;
        }

        public async Task CleanupInactiveUsersAsync(int daysInactive)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysInactive);
            var inactiveUsers = _users.Values
                .Where(u => u.LastLoginDate < cutoffDate && !u.IsActive)
                .ToList();

            foreach (var user in inactiveUsers)
            {
                await DeleteUserAsync(user.Id);
            }
        }

        public Dictionary<string, List<User>> GroupUsersByCountry()
        {
            return _users.Values.GroupBy(u => u.Country).ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<bool> VerifyEmailAsync(int userId, string verificationCode)
        {
            if (!_users.ContainsKey(userId))
            {
                return false;
            }

            var user = _users[userId];
            if (user.VerificationCode == verificationCode)
            {
                user.IsEmailVerified = true;
                user.VerificationCode = null;
                await _database.UpdateUserAsync(user);
                return true;
            }
            return false;
        }

        // More methods could be added here...
        // This class is intentionally large to trigger the God Class analyzer
        // In real code, this should be split into multiple services
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginDate { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public string Role { get; set; }
        public UserProfile Profile { get; set; }
        public string Country { get; set; }
        public string VerificationCode { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class UserProfile
    {
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class UserImportDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public interface ILogger
    {
        void Log(string message);
        void LogError(string message);
    }

    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email);
        Task SendEmailAsync(string email, string subject, string body);
    }

    public interface IDatabase
    {
        Task SaveUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
    }
}
