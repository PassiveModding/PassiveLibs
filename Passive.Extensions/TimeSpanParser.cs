using System;
using System.Globalization;
using System.Text;

namespace Passive
{
    // Graciously ripped from JustNirk https://hatebin.com/vgpablqwku
    // Added timespanparseresult for better error checking
    // Fixed negative parsing
    // Added checks for spaces in numbers
    // Added more cases for min/mins etc.
    // Fix issue with whitespace on end of input
    // Catch overflow issues
    // Move state and builder within the method (methods are now static as a result)
    // Fix identifier check for last character returning a result even if it is missing
    public class TimeSpanParser
    {
        public enum TimeSpanParseResult
        {
            Success,

            // If a field is already set ie. 1h1h
            FieldAlreadySet,

            // If a number fails to parse a double check
            NumericParseFailed,

            // If the string is too short, null or whitespace
            InvalidInput,

            // Identifies if the start of a identifier parse fails
            InvalidIdentifierCharacter,

            // Indentifies if the middle of an identifier parse fails.
            InvalidIdentifierString,

            // Identifies if there is no identifier for a specific number
            MissingIdentifier,

            // Indicates spaces between numbers without an identifier separating them.
            NumberDelimitedIncorrectly,

            // Identifies if a number that is too large for a timespan is input
            ValueTooLarge
        }

        private enum State
        {
            None,

            Number,

            Word
        }

        public static TimeSpanParseResult TryParse(string input, out TimeSpan output)
        {
            output = default;

            // Null/empty strings should not parse. the minimum length is 2 as you must have a number and identifier.
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return TimeSpanParseResult.InvalidInput;

            var builder = new StringBuilder();
            var state = State.None;
            var result = TimeSpan.Zero;
            var placeholder = 0d;
            var daySet = false;
            var hourSet = false;
            var minuteSet = false;
            var secondSet = false;
            var msSet = false;
            var negative = false;
            var whitespaceSkipped = false;

            // Standardise input, commas should be treated as decimal points.
            input = input.Replace(',', '.');

            try
            {
                for (var index = 0; index < input.Length; index++)
                {
                    var c = char.ToLower(input[index]);
                    var prevState = state;

                    // Last character of string.
                    if (index == input.Length - 1)
                    {
                        state = GetState(c);

                        if (state == State.Word)
                        {
                            // As prev state is number, the last character must be a valid singe character identifier.
                            if (prevState == State.Number)
                            {
                                if (!(daySet || hourSet || minuteSet || secondSet || msSet))
                                {
                                    // Leading sign is only allowed for the first parsed number.
                                    if (!double.TryParse(builder.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, null, out placeholder))
                                    {
                                        return TimeSpanParseResult.NumericParseFailed;
                                    }
                                    else
                                    {
                                        // Negative was parsed for first value so rather than only using the single value as negative, treat the whole operation as negative.
                                        if (placeholder < 0)
                                        {
                                            negative = true;
                                            placeholder = -placeholder;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!double.TryParse(builder.ToString(), NumberStyles.AllowDecimalPoint, null, out placeholder))
                                        return TimeSpanParseResult.NumericParseFailed;
                                }

                                switch (c)
                                {
                                    case 'd':
                                        if (daySet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromDays(placeholder);
                                        daySet = true;
                                        break;

                                    case 'h':
                                        if (hourSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromHours(placeholder);
                                        hourSet = true;
                                        break;

                                    case 'm':
                                        if (minuteSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromMinutes(placeholder);
                                        minuteSet = true;
                                        break;

                                    case 's':
                                        if (secondSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromSeconds(placeholder);
                                        secondSet = true;
                                        break;

                                    default:
                                        return TimeSpanParseResult.InvalidIdentifierCharacter;
                                }
                            }
                            else
                            {
                                // Prev state is not numeric so attempt to parse word.

                                // Ensure the last character is not whitespace and the parse is performed
                                // on the last identifying character
                                if (!char.IsWhiteSpace(c))
                                {
                                    builder.Append(c);
                                }
                                var suffix = builder.ToString();

                                switch (suffix)
                                {
                                    case "d":
                                    case "day":
                                    case "days":
                                        if (daySet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromDays(placeholder);
                                        daySet = true;
                                        break;

                                    case "h":
                                    case "hr":
                                    case "hrs":
                                    case "hour":
                                    case "hours":
                                        if (hourSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromHours(placeholder);
                                        hourSet = true;
                                        break;

                                    case "m":
                                    case "min":
                                    case "mins":
                                    case "minute":
                                    case "minutes":
                                        if (minuteSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromMinutes(placeholder);
                                        minuteSet = true;
                                        break;

                                    case "s":
                                    case "sec":
                                    case "secs":
                                    case "second":
                                    case "seconds":
                                        if (secondSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromSeconds(placeholder);
                                        secondSet = true;
                                        break;

                                    case "ms":
                                        if (msSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromSeconds(placeholder);
                                        msSet = true;
                                        break;

                                    default:
                                        return TimeSpanParseResult.InvalidIdentifierCharacter;
                                }
                            }
                        }
                        else
                        {
                            // The last character should always be a word character
                            return TimeSpanParseResult.MissingIdentifier;
                        }

                        continue;
                    }

                    if (char.IsWhiteSpace(c))
                    {
                        // Indicate that whitespace characters have been skipped
                        whitespaceSkipped = true;
                        continue;
                    }

                    state = GetState(c);
                    if (whitespaceSkipped)
                    {
                        // If the previous state and current state are both number
                        // and whitespace has been skipped
                        // that means there is a number such as "12 34" which should fail the parse.
                        if (prevState == State.Number && state == State.Number)
                        {
                            return TimeSpanParseResult.NumberDelimitedIncorrectly;
                        }

                        whitespaceSkipped = false;
                    }

                    if (prevState == State.None || prevState == state)
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        switch (prevState)
                        {
                            case State.Number:

                                if (!(daySet || hourSet || minuteSet || secondSet || msSet))
                                {
                                    // Leading sign is only allowed for the first parsed number.
                                    if (!double.TryParse(builder.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, null, out placeholder))
                                        return TimeSpanParseResult.NumericParseFailed;
                                    else
                                    {
                                        // Negative was parsed for first value so rather than only using the single value as negative, treat the whole operation as negative.
                                        if (placeholder < 0)
                                        {
                                            negative = true;
                                            placeholder = -placeholder;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!double.TryParse(builder.ToString(), NumberStyles.AllowDecimalPoint, null, out placeholder))
                                        return TimeSpanParseResult.NumericParseFailed;
                                }
                                break;

                            case State.Word:
                                var suffix = builder.ToString();

                                switch (suffix)
                                {
                                    case "d":
                                    case "day":
                                    case "days":
                                        if (daySet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromDays(placeholder);
                                        daySet = true;
                                        break;

                                    case "h":
                                    case "hr":
                                    case "hrs":
                                    case "hour":
                                    case "hours":
                                        if (hourSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromHours(placeholder);
                                        hourSet = true;
                                        break;

                                    case "m":
                                    case "min":
                                    case "mins":
                                    case "minute":
                                    case "minutes":
                                        if (minuteSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromMinutes(placeholder);
                                        minuteSet = true;
                                        break;

                                    case "s":
                                    case "sec":
                                    case "secs":
                                    case "second":
                                    case "seconds":
                                        if (secondSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromSeconds(placeholder);
                                        secondSet = true;
                                        break;

                                    case "ms":
                                        if (msSet)
                                            return TimeSpanParseResult.FieldAlreadySet;

                                        result += TimeSpan.FromMilliseconds(placeholder);
                                        msSet = true;
                                        break;

                                    default:
                                        return TimeSpanParseResult.InvalidIdentifierString;
                                }
                                break;

                            default:
                                throw new Exception("this shouldn't happen");
                        }

                        builder.Clear().Append(c);
                    }
                }
            }
            catch (OverflowException)
            {
                // When adding a value to the timespan, a large double can be converted to a much larger amount of ticks
                // this resulting in an overflow.
                return TimeSpanParseResult.ValueTooLarge;
            }

            output = negative ? -result : result;
            return TimeSpanParseResult.Success;
        }

        private static bool IsNumber(char c)
            => c >= '0' && c <= '9' || c == '-' || c == '.';

        private static State GetState(char c)
            => IsNumber(c)
            ? State.Number
            : State.Word;
    }
}