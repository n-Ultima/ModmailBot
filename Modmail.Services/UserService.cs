﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modmail.Data;
using Modmail.Data.Models;
using Remora.Discord.Core;
using Remora.Results;

namespace Modmail.Services
{
    public class UserService : ModmailService
    {
        public UserService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {}

        public async Task<IEnumerable<Snowflake>> FetchBlacklistedUsersAsync()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var modmailContext = scope.ServiceProvider.GetRequiredService<ModmailContext>();
                return await modmailContext.BlockedUsers
                    .Select(x => x.Id)
                    .ToListAsync();
            }
        }

        public async Task<Result> BlacklistUserAsync(Snowflake userId)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var modmailContext = scope.ServiceProvider.GetRequiredService<ModmailContext>();
                var currentBlacklists = await FetchBlacklistedUsersAsync();
                var check = await modmailContext.BlockedUsers.FindAsync(userId);
                if (check != null)
                    return Result.FromError(new ExceptionError(new Exception("The user provided is already blacklisted.")));

                modmailContext.BlockedUsers.Add(new BlockedUser
                {
                    Id = userId
                });
                await modmailContext.SaveChangesAsync();
                return Result.FromSuccess();
            }
        }
    }
}