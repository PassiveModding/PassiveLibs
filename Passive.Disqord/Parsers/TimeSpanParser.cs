using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static readonly string TimeSpanExtendedCheck = @"[0-9dhms\-. ]";

        private static readonly string TimeSpanDelimiter = @"(?<=[dhms])";

        private static readonly string TimeSpanDelimitedCheck = @"^-?\d*\.?(\d*)+[dhms]$";

        public ValueTask<TypeParserResult<TimeSpan>> ParseAsync(string input)
        {
            if (TimeSpan.TryParseExact(input.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, out var timeSpan))
            {
                return TypeParserResult<TimeSpan>.Successful(timeSpan);
            }
            else
            {
                input = input.ToLower();
                if (input.Contains("ms"))
                {
                    return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (`ms` is not supported)");
                }

                // Attempt to parse values above what timespan regularly parses, ie. 24h => 1d is not normally possible.
                if (!Regex.IsMatch(input, TimeSpanExtendedCheck, RegexOptions.Compiled))
                {
                    return TypeParserResult<TimeSpan>.Unsuccessful("Failed to parse TimeSpan");
                }

                // remove spaces.
                input = input.Replace(" ", "");

                // Split string into identified values ie. "6d24h" => { "6d", "24h" }
                var splits = Regex.Split(input, TimeSpanDelimiter, RegexOptions.Compiled);

                // Somehow no results found.
                if (splits.Length == 0)
                {
                    return TypeParserResult<TimeSpan>.Unsuccessful("Failed to parse TimeSpan");
                }
                else
                {
                    // Faster than setting flags.
                    bool d = false;
                    bool h = false;
                    bool m = false;
                    bool s = false;

                    TimeSpan response = new TimeSpan();
                    foreach (var split in splits)
                    {
                        if (split == string.Empty)
                        {
                            continue;
                        }

                        // Split is only the delimiting character
                        if (!Regex.IsMatch(split, TimeSpanDelimitedCheck, RegexOptions.Compiled))
                        {
                            return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (`{split}` is invalid.)");
                        }

                        // Last char should always be a valid character, not sure if this check is necessary.
                        var lastChar = split.Substring(split.Length - 1);

                        // Get the number without the trailing character.
                        var num = split[0..^1];

                        if (!double.TryParse(num, out var result))
                        {
                            return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (`{num}` is not of type `double`)");
                        }

                        switch (lastChar)
                        {
                            case "d":
                                if (d)
                                {
                                    return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (multiple day setters)");
                                }
                                d = true;
                                response += TimeSpan.FromDays(result);
                                break;

                            case "h":
                                if (h)
                                {
                                    return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (multiple hour setters)");
                                }
                                h = true;
                                response += TimeSpan.FromHours(result);
                                break;

                            case "m":
                                if (m)
                                {
                                    return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (multiple minute setters)");
                                }
                                m = true;
                                response += TimeSpan.FromMinutes(result);
                                break;

                            case "s":
                                if (s)
                                {
                                    return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (multiple seconds setters)");
                                }
                                s = true;
                                response += TimeSpan.FromSeconds(result);
                                break;
                        }
                    }

                    // Check to see if any of the methods were hit.
                    if (d || h || m || s)
                    {
                        return TypeParserResult<TimeSpan>.Successful(response);
                    }

                    return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (Timespan has no modifying arguments)");
                }
            }
        }

        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string input, CommandContext context)
        {
            return ParseAsync(input);
        }
    }
}