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

        // TODO: Compile these expressions
        private static readonly string TimeSpanExtendedCheck = @"[0-9dhms DHMS]";

        private static readonly string TimeSpanDelimiter = @"(?<=[dhmsDHMS])";

        private static readonly string TimeSpanDelimitedCheck = @"^[0-9]+[dhmsDHMS]$";

        public ValueTask<TypeParserResult<TimeSpan>> ParseAsync(string input)
        {
            if (TimeSpan.TryParseExact(input.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, out var timeSpan))
            {
                return TypeParserResult<TimeSpan>.Successful(timeSpan);
            }
            else
            {
                // Attempt to parse values above what timespan regularly parses, ie. 24h => 1d is not normally possible.
                if (!Regex.IsMatch(input, TimeSpanExtendedCheck))
                {
                    return TypeParserResult<TimeSpan>.Unsuccessful("Failed to parse TimeSpan");
                }

                // remove spaces.
                input = input.Replace(" ", "");

                // Split string into identified values ie. "6d24h" => { "6d", "24h" }, removing empty entries
                var splits = Regex.Split(input, TimeSpanDelimiter).Where(x => x != string.Empty).ToArray();

                // Somehow no results found.
                if (splits.Length == 0)
                {
                    return TypeParserResult<TimeSpan>.Unsuccessful("Failed to parse TimeSpan");
                }
                else
                {
                    TimeSpan response = new TimeSpan();
                    foreach (var split in splits)
                    {
                        // Split is only the delimiting character
                        if (!Regex.IsMatch(split, TimeSpanDelimitedCheck))
                        {
                            return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (`{split}` is invalid.)");
                        }

                        var lastChar = split.Substring(split.Length - 1);
                        var num = split.Substring(0, split.Length - 1);

                        if (!double.TryParse(num, out var result))
                        {
                            return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (`{num}` is not of type `double`)");
                        }
                        switch (lastChar.ToLower())
                        {
                            case "d":
                                response += TimeSpan.FromDays(result);
                                break;

                            case "h":
                                response += TimeSpan.FromHours(result);
                                break;

                            case "m":
                                response += TimeSpan.FromMinutes(result);
                                break;

                            case "s":
                                response += TimeSpan.FromSeconds(result);
                                break;

                            default:
                                return TypeParserResult<TimeSpan>.Unsuccessful($"Failed to parse TimeSpan (`{lastChar}` is not a valid timespan character.)");
                        }
                    }

                    return TypeParserResult<TimeSpan>.Successful(response);
                }
            }
        }

        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string input, CommandContext context)
        {
            return ParseAsync(input);
        }
    }
}