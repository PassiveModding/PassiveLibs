using System;
using Disqord.Extensions.Passive;
using Passive;

namespace Disqord.Extensions.Interactivity.Help
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HelpMetadataAttribute : Attribute
    {
        public HelpMetadataAttribute(string buttonCode, string colorHex = null)
        {
            this.ButtonCode = buttonCode;
            Color = colorHex == null ? Color.Aquamarine : colorHex.FromHex().ToDisqord();
        }

        public string ButtonCode { get; }

        public Color Color { get; }
    }
}