using System;
using System.Threading.Tasks;
using Passive;
using Qmmands;
using static Passive.TimeSpanParser;

namespace Disqord.Extensions.Parsers
{
    public class TimeSpanParser : TypeParser<TimeSpan>
    {
        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string input, CommandContext context)
        {
            var result = input.ParseAsTimeSpan(out var timespan);
            if (result == TimeSpanParseResult.Success) return TypeParserResult<TimeSpan>.Successful(timespan);
            return TypeParserResult<TimeSpan>.Unsuccessful(result.ToString());
        }
    }
}