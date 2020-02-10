using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Disqord.Extensions.Checks
{
    public class GuildOnlyAttribute : ExtendedCheckAttribute
    {
        public GuildOnlyAttribute()
        {
            Description = "Requires the command be executed from within a Discord server.";
            Name = "Guild Only Check.";
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var context = _ as DiscordCommandContext;
            return context.Guild != null
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("This can only be executed in a guild.");
        }
    }
}