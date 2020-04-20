using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Checks;
using Qmmands;

namespace Passive.Disqord.Checks
{
    public sealed class BotTeamOnlyAttribute : ExtendedCheckAttribute
    {
        public BotTeamOnlyAttribute()
        { }

        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var context = _ as DiscordCommandContext;
            switch (context.Bot.TokenType)
            {
                case TokenType.Bot:
                    {
                        var app = await context.Bot.CurrentApplication.GetAsync().ConfigureAwait(false);
                        if (app.Team != null && app.Team.Members != null)
                        {
                            if (app.Team.Members.ContainsKey(context.User.Id))
                            {
                                return CheckResult.Successful;
                            }
                        }

                        return app.Owner.Id == context.User.Id
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