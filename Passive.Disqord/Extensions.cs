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
    }
}