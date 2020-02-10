using Qmmands;

namespace Disqord.Extensions.Checks
{
    public abstract class ExtendedCheckAttribute : CheckAttribute
    {
        public string Description { get; internal set; }

        public string Name { get; internal set; }
    }
}