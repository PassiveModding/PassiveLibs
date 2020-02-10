using System;
using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace Disqord.Extensions.Parsers
{
    /// <summary>
    /// Argument parser for timespans, taken from discord.net lib.
    /// </summary>
    public class TimeSpanParser : TypeParser<TimeSpan>
    {
        private static readonly string[] Formats =
        {
            "%d'd'%h'h'%m'm'%s's'", // 4d3h2m1s
            "%d'd'%h'h'%m'm'",      // 4d3h2m
            "%d'd'%h'h'%s's'",      // 4d3h  1s
            "%d'd'%h'h'",           // 4d3h
            "%d'd'%m'm'%s's'",      // 4d  2m1s
            "%d'd'%m'm'",           // 4d  2m
            "%d'd'%s's'",           // 4d    1s
            "%d'd'",                // 4d
            "%h'h'%m'm'%s's'",      //  3h2m1s
            "%h'h'%m'm'",           //  3h2m
            "%h'h'%s's'",           //  3h  1s
            "%h'h'",                //  3h
            "%m'm'%s's'",           //    2m1s
            "%m'm'",                //    2m
            "%s's'",                //      1s
        };

        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string input, CommandContext context)
        {
            return TimeSpan.TryParseExact(input.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, out var timeSpan)
                ? TypeParserResult<TimeSpan>.Successful(timeSpan)
                : TypeParserResult<TimeSpan>.Unsuccessful("Failed to parse TimeSpan");
        }
    }
}