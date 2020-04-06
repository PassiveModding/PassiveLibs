using Disqord.Rest;
using Passive.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disqord.Extensions.Passive
{
    public static class Extensions
    {
        /// <summary>
        /// Converts from a System.Drawing.Color to a Disqord.Color
        /// </summary>
        /// <param name="color">The System color</param>
        /// <returns>a Disqord Color</returns>
        public static Color ToDisqord(this System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B);
        }

        public static Task<RestUserMessage> EmbedMessageAsync(this ICachedMessageChannel channel, string message, Color? color = null)
        {
            var embed = new LocalEmbedBuilder().WithDescription(message).WithColor(color ?? Color.Lavender);
            return channel.SendMessageAsync("", false, embed.Build());
        }

        public static Task<RestUserMessage> EmbedMessageAsync(this ICachedMessageChannel channel, LocalEmbedBuilder builder)
        {
            if (builder.Color == null) builder.Color = Color.Lavender;
            return channel.SendMessageAsync("", false, builder.Build());
        }

        public static Task<RestUserMessage> EmbedMessageAsync(this ITextChannel channel, LocalEmbedBuilder builder)
        {
            if (builder.Color == null) builder.Color = Color.Lavender;
            return channel.SendMessageAsync("", false, builder.Build());
        }

        /// <summary>
        /// Gets a users max position within their guild, fetching the guild
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<int> GetPositionAsync(this RestMember user)
        {
            var guild = await user.Guild.FetchAsync();
            return GetPosition(user, guild);
        }

        /// <summary>
        /// Gets a users max position within a guild
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static int GetPosition(this RestMember user, RestGuild guild)
        {
            if (guild == null) return -1;
            if (guild.OwnerId == user.Id) return int.MaxValue;

            return GetPosition(user, guild.Roles);
        }

        /// <summary>
        /// Gets a users max position within a list of roles.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static int GetPosition(this RestMember user, IReadOnlyDictionary<Snowflake, RestRole> roles)
        {
            int maxPos = 0;
            foreach (var roleId in user.RoleIds)
            {
                if (roles.TryGetValue(roleId, out var role))
                {
                    if (role.Position > maxPos)
                    {
                        maxPos = role.Position;
                    }
                }
            }
            return maxPos;
        }

        public static async Task<GuildPermissions> GetPermissions(this RestMember user)
        {
            var guild = await user.Guild.FetchAsync();
            return GetPermissions(user, guild);
        }

        public static GuildPermissions GetPermissions(this RestMember user, RestGuild guild)
        {
            if (user.Id == guild.OwnerId) return GuildPermissions.All;

            return GetPermissions(user, guild.Roles);
        }

        public static GuildPermissions GetPermissions(this RestMember user, IReadOnlyDictionary<Snowflake, RestRole> roles)
        {
            GuildPermissions basePerms = GuildPermissions.None;
            foreach (var role in roles.OrderBy(x => x.Value.Position))
            {
                if (!user.RoleIds.Contains(role.Key)) continue;
                basePerms += role.Value.Permissions;
            }
            return basePerms;
        }

        public static Logger.LogLevel GetLevel(this Disqord.Logging.LogMessageSeverity severity)
        {
            switch (severity)
            {
                case Disqord.Logging.LogMessageSeverity.Trace:
                    return Logger.LogLevel.Verbose;

                case Disqord.Logging.LogMessageSeverity.Debug:
                    return Logger.LogLevel.Debug;

                case Disqord.Logging.LogMessageSeverity.Information:
                    return Logger.LogLevel.Info;

                case Disqord.Logging.LogMessageSeverity.Warning:
                    return Logger.LogLevel.Warn;

                case Disqord.Logging.LogMessageSeverity.Error:
                    return Logger.LogLevel.Error;

                case Disqord.Logging.LogMessageSeverity.Critical:
                    return Logger.LogLevel.Error;

                default:
                    return Logger.LogLevel.Error;
            }
        }
    }
}