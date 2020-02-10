using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace Disqord.Extensions.Checks
{
    public sealed class RequireMemberGuildPermissionsAttribute : GuildOnlyAttribute
    {
        public GuildPermissions Permissions { get; }

        public RequireMemberGuildPermissionsAttribute(Permission permissions)
        {
            Permissions = permissions;
            Description = $"Requires the current user has `{permissions}` permissions in the guild the command is being executed in.";
            Name = "Guild Permission Check.";
        }

        public RequireMemberGuildPermissionsAttribute(params Permission[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            Permissions = permissions.Aggregate(Permission.None, (total, permission) => total | permission);
            Description = $"Requires the user has {string.Join(' ', permissions.Select(x => $"`{x}`"))} permissions in the guild the command is being executed in.";
            Name = "Guild Permission Check.";
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var baseResult = base.CheckAsync(_).Result;
            if (!baseResult.IsSuccessful)
                return baseResult;

            var context = _ as DiscordCommandContext;
            var permissions = context.Member.Permissions;
            return permissions.Has(Permissions)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"You lack the necessary guild permissions ({Permissions - permissions}) to execute this.");
        }
    }
}