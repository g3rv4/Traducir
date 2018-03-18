using System.Threading.Tasks;
using Dapper;
using Traducir.Core.Models;

namespace Traducir.Core.Services
{
    public interface IUserService
    {
        Task UpsertUser(User user);
    }
    public class UserService : IUserService
    {
        private IDbService _dbService { get; }
        public UserService(IDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task UpsertUser(User user)
        {
            using(var db = _dbService.GetConnection())
            {
                await db.ExecuteAsync(@"
Declare @hasVote;
Select @hasVote = HasVote From Users Where Id = @Id;

If @hasVote Is Null
  -- It's an insert!
  Insert Into Users (Id, DisplayName, IsModerator, HasVote, CreationDate, LastSeenDate)
  Values            (@Id, @DisplayName, @IsModerator, @IsModerator, @CreationDate, @LastSeenDate)
Else
  -- It's an update
  Update Users
  Set    DisplayName = @DisplayName,
         IsModerator = @IsModerator,
         HasVote = Case When @hasVote = 1 Then 1 Else @IsModerator, -- if the user already had voting rights, keep them
                                                                    -- if they're now a mod, give them voting rights
         LastSeenDate = @LastSeenDate", user);
            }
        }
    }
}