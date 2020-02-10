using System;
using System.Collections.Generic;
using System.Drawing;
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
    }
}