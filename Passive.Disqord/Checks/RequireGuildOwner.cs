using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Disqord.Extensions.Checks
{
    public sealed class RequireGuildOwnerAttribute : GuildOnlyAttribute
    {
        public RequireGuildOwnerAttribute()
        {
            Description = $"Requires the current user is the server owner.";
            Name = "Server Owner Check.";
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var baseResult = base.CheckAsync(_).Result;
            if (!baseResult.IsSuccessful)
                return baseResult;

            var context = _ as DiscordCommandContext;

            if (context.Guild.OwnerId == context.Member.Id)
            {
                return CheckResult.Successful;
            }
            else
            {
                return CheckResult.Unsuccessful("This command can only be executed by the server owner.");
            }
        }
    }
}