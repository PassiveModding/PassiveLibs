using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Passive
{
    public static class Extensions
    {
        /// <summary>
        /// Trims a string to a given length or otherwise returns it.
        /// </summary>
        /// <param name="value">The specified string to be shortened.</param>
        /// <param name="length">The desired length.</param>
        /// <returns>A string that is under the specified length.</returns>
        public static string FixLength(this string value, int length = 1023)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("Length must be a positive integer.");
            }

            if (value.Length > length)
            {
                if (length <= 3)
                {
                    value = value.Substring(0, length);
                }
                else
                {
                    // This may cause some weird formatting if the string is very short.
                    value = value.Substring(0, length - 3) + "...";
                }
            }

            return value;
        }

        public static string GetReadableLength(this TimeSpan length)
        {
            int days = (int)length.TotalDays;
            int hours = (int)length.TotalHours - days * 24;
            int minutes = (int)length.TotalMinutes - days * 24 * 60 - hours * 60;
            int seconds = (int)length.TotalSeconds - days * 24 * 60 * 60 - hours * 60 * 60 - minutes * 60;

            return $"{(days > 0 ? $"{days} Day(s) " : "")}{(hours > 0 ? $"{hours} Hour(s) " : "")}{(minutes > 0 ? $"{minutes} Minute(s) " : "")}{(seconds > 0 ? $"{seconds} Second(s)" : "")}";
        }

        /// <summary>
        /// Searches the specified assembly for all classes marked with the <see cref="Services.ServiceAttribute"/> attribute.
        /// </summary>
        /// <param name="assembly">The specified assembly.</param>
        /// <returns>an enumerable of all classes marked with the <see cref="Services.ServiceAttribute"/> attribute.</returns>
        public static IEnumerable<Type> GetServices(this Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ServiceAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Converts a color code from hexadecimal to <see cref="Disqord.Color"/>.
        /// </summary>
        /// <param name="colorHex">A hexadecimal color code, with or without a # prefix.</param>
        /// <returns>A <see cref="Color"/>.</returns>
        public static Color FromHex(this string colorHex)
        {
            if (!colorHex.StartsWith('#'))
            {
                colorHex = "#" + colorHex;
            }

            if (colorHex.Length != 7)
            {
                throw new Exception("this method only accepts 6 character color codes, the prefixed # is optional.");
            }

            var sysColor = ColorTranslator.FromHtml(colorHex);
            var color = Color.FromArgb(sysColor.R, sysColor.G, sysColor.B);
            return color;
        }

        public static TimeSpanParser.TimeSpanParseResult ParseAsTimeSpan(this string input, out TimeSpan output)
        {
            return TimeSpanParser.TryParse(input, out output);
        }

        public static IEnumerable<IEnumerable<T>> SplitList<T>(this IEnumerable<T> list, int groupSize = 30)
        {
            var splitList = new List<List<T>>();
            for (var i = 0; i < list.Count(); i += groupSize)
            {
                splitList.Add(list.Skip(i).Take(groupSize).ToList());

                //yield return list.Skip(i).Take(groupSize);
            }

            return splitList;
        }

        public static IEnumerable<IEnumerable<T>> SplitList<T>(this IEnumerable<T> list, Func<T, int> sumComparator, int maxGroupSum)
        {
            var subList = new List<T>();
            int currentSum = 0;

            foreach (var item in list)
            {
                //Get the size of the current item.
                var addedValue = sumComparator(item);

                //Ensure that the current item will fit in a group
                if (addedValue > maxGroupSum)
                {
                    //TODO: add options to skip fields that exceed the length or add them as a solo group rather than just error out
                    throw new InvalidOperationException("A fields value is greater than the maximum group value size.");
                }

                //Add group to splitlist if the new item will exceed the given size.
                if (currentSum + addedValue > maxGroupSum)
                {
                    //splitList.Append(subList);
                    yield return subList;

                    //Clear the current sum and the subList
                    currentSum = 0;
                    subList = new List<T>();
                }

                subList.Add(item);
                currentSum += addedValue;
            }

            //Return any remaining elements
            if (subList.Count != 0)
            {
                yield return subList;
            }
        }

        public static string AsOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";

                case 2:
                    return num + "nd";

                case 3:
                    return num + "rd";

                default:
                    return num + "th";
            }
        }
    }
}