using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Services
{
    public interface IUserService
    {
        Task UpsertUserAsync(User user);

        Task<User> GetUserAsync(int userId);

        Task<List<User>> GetUsersAsync();

        Task<bool> ChangeUserTypeAsync(int userId, UserType userType, int editorId);
    }

    public class UserService : IUserService
    {
        private readonly IDbService _dbService;

        public UserService(IDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task UpsertUserAsync(User user)
        {
            using (var db = _dbService.GetConnection())
            {
                await db.ExecuteAsync(@"
Declare @wasReviewer Bit, @wasTrusted Bit;
Select @wasReviewer = IsReviewer, @wasTrusted = IsTrusted From Users Where Id = @Id;

If @wasReviewer Is Null
  -- It's an insert!
  Insert Into Users (Id, DisplayName, IsModerator, IsTrusted, IsReviewer, IsBanned, CreationDate, LastSeenDate)
  Values            (@Id, @DisplayName, @IsModerator, @IsModerator, @IsModerator, 0, @CreationDate, @LastSeenDate)
Else
  -- It's an update
  Update Users
  Set    DisplayName = @DisplayName,
         IsModerator = @IsModerator,
         IsReviewer = Case When @wasReviewer = 1 Then 1 Else @IsModerator End, -- if the user was a reviewer, keep them
                                                                               -- if they're now a mod, make them a reviewer
         IsTrusted = Case When @wasTrusted = 1 Then 1 Else @IsModerator End,
         LastSeenDate = @LastSeenDate
Where    Id = @Id", user).ConfigureAwait(false);
            }
        }

        public async Task<User> GetUserAsync(int userId)
        {
            using (var db = _dbService.GetConnection())
            {
                return await db.QueryFirstOrDefaultAsync<User>(@"
Select *
From   Users
Where  Id = @userId", new
                {
                    userId
                }).ConfigureAwait(false);
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using (var db = _dbService.GetConnection())
            {
                return (await db.QueryAsync<User>("Select * From Users").ConfigureAwait(false)).AsList();
            }
        }

        public async Task<bool> ChangeUserTypeAsync(int userId, UserType userType, int editorId)
        {
            using (var db = _dbService.GetConnection())
            {
                var rows = await db.ExecuteAsync(@"
Insert Into UserHistory
            (UserId, UpdatedById, CreationDate, HistoryTypeId)
Select Id, @editorId, @now,
       Case When @userType = {=TrustedUser} Then {=HistoryMadeTrustedUser}
            When @userType = {=Banned} Then {=HistoryBanned}
            When @userType = {=User} And IsBanned = 1 Then {=HistoryBanLifted}
            When @userType = {=User} And IsTrusted = 1 Then {=HistoryDemotedToRegularUser} End
From   Users
Where  Id = @userId
And    IsModerator = 0
And    IsReviewer = 0;

Update Users
Set    IsTrusted = Case When @userType = {=TrustedUser} Then 1 Else 0 End,
       IsBanned = Case When @userType = {=Banned} Then 1 Else 0 End
Where  Id = @userId
And    IsModerator = 0
And    IsReviewer = 0;", new
                {
                    userId,
                    editorId,
                    userType,
                    UserType.TrustedUser,
                    UserType.Banned,
                    UserType.User,
                    now = DateTime.UtcNow,
                    HistoryMadeTrustedUser = UserHistoryType.MadeTrustedUser,
                    HistoryBanned = UserHistoryType.Banned,
                    HistoryBanLifted = UserHistoryType.BanLifted,
                    HistoryDemotedToRegularUser = UserHistoryType.DemotedToRegularUser
                }).ConfigureAwait(false);
                return rows > 0;
            }
        }
    }
}