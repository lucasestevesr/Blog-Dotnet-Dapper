using BlogLucas.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BlogLucas.Repositories
{
    public class UserRepository : Repository<User>
    {
        private readonly SqlConnection _connection;
        public UserRepository(SqlConnection connection)
        : base(connection)
            => _connection = connection;

        public List<User> GetWithRoles()
        {
            var query = @"SELECT 
                            [User].*,
                            [Role].*
                        FROM
                            [User]
                            LEFT JOIN [UserRole] ON [UserRole].[UserId] = [User].[Id]
                            LEFT JOIN [Role] ON [UserRole].[RoleId] = [Role].[Id]";
            
            var users = new List<User>();

            var items = _connection.Query<User, Role, User>(
                query,
                (user, role) =>
                {
                    var usr = users.FirstOrDefault(x => x.Id == user.Id);
                    if(usr == null)
                    {
                        usr = user;
                        if (role != null)
                            usr.Roles.Add(role);
                        users.Add(usr);
                    }
                    else
                        user.Roles.Add(role);
                    
                    return user;
                }, splitOn: "Id");        
                
            return users;
        }

        public void BindUserRole(int userId, int roleId)
        {
            var insertSql = @"INSERT INTO [UserRole] VALUES(@userId, @roleId)";

            _connection.Execute(insertSql, new
            {
                userId,
                roleId
            });
        }

        public void UnbindUserRole(int userId, int roleId)
        {
            var deleteSql = @"DELETE FROM [UserRole] WHERE [UserId] = @userId and [RoleId] = @roleId";

            _connection.Execute(deleteSql, new
            {
                userId,
                roleId
            });
        }
    }
}