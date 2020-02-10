using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Disqord.Extensions.Checks
{
    public class RequireBotChannelPermissionsAttribute : GuildOnlyAttribute
    {
        public RequireBotChannelPermissionsAttribute(Permission permissions)
        {
            Permissions = permissions;
            Description = $"Requires the bot has `{permissions}` permissions in the channel the command is being executed in.";
            Name = "Channel Permission Check.";
        }

        public RequireBotChannelPermissionsAttribute(params Permission[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            Permissions = permissions.Aggregate(Permission.None, (total, permission) => total | permission);
            Description = $"Requires the bot has {string.Join(' ', permissions.Select(x => $"`{x}`"))} permissions in the channel the command is being executed in.";
            Name = "Channel Permission Check.";
        }

        public ChannelPermissions Permissions { get; }

        public override ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var baseResult = base.CheckAsync(_).Result;
            if (!baseResult.IsSuccessful)
                return baseResult;

            var context = _ as DiscordCommandContext;
            var permissions = context.Guild.CurrentMember.GetPermissionsFor(context.Channel as IGuildChannel);
            return permissions.Has(Permissions)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The bot lacks the necessary channel permissions ({Permissions - permissions}) to execute this.");
        }
    }
}