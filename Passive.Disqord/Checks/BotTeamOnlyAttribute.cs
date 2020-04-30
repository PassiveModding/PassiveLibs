using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Checks;
using Disqord.Rest;
using Qmmands;

namespace Passive.Disqord.Checks
{
    public sealed class BotTeamOnlyAttribute : ExtendedCheckAttribute
    {
        private static RestApplication application;

        public BotTeamOnlyAttribute()
        { }

        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var context = _ as DiscordCommandContext;
            switch (context.Bot.TokenType)
            {
                case TokenType.Bot:
                    {
                        if (application == null)
                        {
                            application = await context.Bot.GetCurrentApplicationAsync().ConfigureAwait(false);
                        }

                        if (application.Team != null && application.Team.Members != null)
                        {
                            if (application.Team.Members.ContainsKey(context.User.Id))
                            {
                                return CheckResult.Successful;
                            }
                        }

                        return application.Owner.Id == context.User.Id
                            ? CheckResult.Successful
                            : CheckResult.Unsuccessful("This can only be executed by the bot's owner or team members.");
                    }

                case TokenType.Bearer:
                case TokenType.User:
                    {
                        return context.Bot.CurrentUser.Id == context.User.Id
                            ? CheckResult.Successful
                            : CheckResult.Unsuccessful("This can only be executed by the currently logged in user.");
                    }

                default:
                    throw new InvalidOperationException("Invalid token type.");
            }
        }
    }
}