using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Extensions.Checks;
using Qmmands;

namespace Passive.Disqord.Checks
{
    public sealed class RatelimitAttribute : ExtendedCheckAttribute
    {
        private static readonly Dictionary<(ulong, ulong?), CommandTimeout> invokeTracker = new Dictionary<(ulong, ulong?), CommandTimeout>();

        private readonly int maxCalls;

        private readonly RateLimitFlags flags;

        private readonly TimeSpan rateLimitLength;

        private readonly bool applyPerGuild;

        private readonly bool applyPerChannel;

        public RatelimitAttribute(int maxCalls, int seconds, RateLimitFlags flags = RateLimitFlags.None, string bucket = null)
        {
            rateLimitLength = new TimeSpan(0, 0, 0, seconds);
            this.maxCalls = maxCalls;
            this.flags = flags;
            applyPerGuild = (flags & RateLimitFlags.ApplyPerGuild) == RateLimitFlags.ApplyPerGuild;
            applyPerChannel = (flags & RateLimitFlags.ApplyPerChannel) == RateLimitFlags.ApplyPerChannel;
        }

        public RatelimitAttribute(int maxCalls, int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0, RateLimitFlags flags = RateLimitFlags.None, string bucket = null)
        {
            rateLimitLength = new TimeSpan(days, hours, minutes, seconds, minutes);
            this.maxCalls = maxCalls;
            this.flags = flags;
            applyPerGuild = (flags & RateLimitFlags.ApplyPerGuild) == RateLimitFlags.ApplyPerGuild;
            applyPerChannel = (flags & RateLimitFlags.ApplyPerChannel) == RateLimitFlags.ApplyPerChannel;
        }

        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            var context = _ as DiscordCommandContext;

            var now = DateTime.UtcNow;
            var key = applyPerGuild ? (context.User.Id.RawValue, context.Guild?.Id.RawValue) : (applyPerChannel ? (context.User.Id.RawValue, context.Channel?.Id.RawValue) : (context.User.Id.RawValue, null));

            var timeout = (invokeTracker.TryGetValue(key, out var t)) && ((now - t.FirstInvoke) < rateLimitLength) ? t : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= maxCalls)
            {
                invokeTracker[key] = timeout;
                return CheckResult.Successful;
            }

            var remainingTimeout = timeout.FirstInvoke - now + rateLimitLength;

            return CheckResult.Unsuccessful($"You are currently in Timeout for {remainingTimeout.GetReadableLength()}");
        }

        private sealed class CommandTimeout
        {
            public DateTime FirstInvoke;

            public uint TimesInvoked;

            public CommandTimeout(DateTime init)
            {
                FirstInvoke = init;
            }
        }

        public enum RateLimitFlags
        {
            None = 0,

            ApplyPerChannel = 1 << 0,

            ApplyPerGuild = 1 << 1
        }
    }
}