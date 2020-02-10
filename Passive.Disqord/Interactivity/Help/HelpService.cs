using System.Linq;
using System.Text;
using Disqord;
using Qmmands;

namespace Disqord.Extensions.Interactivity.Help
{
    public class HelpService
    {
        public static LocalEmbedBuilder GetModuleHelp(Module module)
        {
            if (module.Commands.Count == 0)
            {
                return new LocalEmbedBuilder()
                    .WithTitle(module.FullAliases.FirstOrDefault() ?? module.Aliases.FirstOrDefault() ?? module.Name);
            }
            else if (module.Commands.Count <= 25)
            {
                var commandInfos = module.Commands.Select(x => GetCommandInfoField(x));
                var builder = new LocalEmbedBuilder();
                foreach (var info in commandInfos)
                {
                    builder.AddField(info);
                }

                // TODO: Split based on max embed length (or x amount of fields + specific max length)
                return builder
                    .WithTitle(module.FullAliases.FirstOrDefault() ?? module.Aliases.FirstOrDefault() ?? module.Name);
            }
            else
            {
                var commandInfos = module.Commands.Select(x => GetCommandHelp(x));

                // TODO: Split based on max embed length (or x amount of fields + specific max length)
                return new LocalEmbedBuilder()
                    .WithDescription(string.Join("\n", commandInfos)).WithTitle(module.FullAliases.First());
            }
        }

        public static string GetCommandHelp(Command command)
        {
            return $"{GetCommandInfo(command)}\n{FormatCommand(command)}";
        }

        private static string FormatCommand(Command command)
        {
            var paramInfo = string.Join(" ", command.Parameters.Select(x => FormatParameter(x)));

            return $"`{command.FullAliases.FirstOrDefault() ?? command.Aliases.FirstOrDefault() ?? command.Name}" +
                $"{(paramInfo.Length > 0 ? " " + paramInfo : "")}`";
        }

        private static string GetCommandInfo(Command command)
        {
            var sb = new StringBuilder();
            sb.Append("**");
            sb.Append(command.Name);
            sb.AppendLine("**");

            if (command.Description != null)
            {
                sb.AppendLine(command.Description);
            }

            if (command.Remarks != null)
            {
                sb.AppendLine("**[**Remarks**]**");
                sb.AppendLine(command.Remarks);
            }

            if (command.FullAliases.Count > 1)
            {
                sb.AppendLine("**[**Aliases**]**");
                sb.AppendJoin(", ", command.FullAliases.Select(x => $"`{x}`"));
                sb.AppendLine();
            }

            if (command.Checks.Count > 0)
            {
                // TODO: Implement custom checkattribute with name and description
                sb.AppendLine("**[**Checks**]**");
                sb.AppendJoin(", ", command.Checks.Select(x => x.GetType().Name));
                sb.AppendLine();
            }

            if (!command.IsEnabled)
            {
                sb.AppendLine("__Command is currently disabled__");
            }

            return sb.ToString();
        }

        private static LocalEmbedFieldBuilder GetCommandInfoField(Command command)
        {
            var field = new LocalEmbedFieldBuilder
            {
                Name = command.Name
            };

            var sb = new StringBuilder();
            sb.AppendLine(FormatCommand(command));

            if (command.Description != null)
            {
                sb.AppendLine(command.Description);
            }

            if (command.Remarks != null)
            {
                sb.AppendLine("**[**Remarks**]**");
                sb.AppendLine(command.Remarks);
            }

            if (command.FullAliases.Count > 1)
            {
                sb.AppendLine("**[**Aliases**]**");
                sb.AppendJoin(", ", command.FullAliases.Select(x => $"`{x}`"));
                sb.AppendLine();
            }

            if (command.Checks.Count > 0)
            {
                // TODO: Implement custom checkattribute with name and description
                sb.AppendLine("**[**Checks**]**");
                sb.AppendJoin(", ", command.Checks.Select(x => x.GetType().Name));
                sb.AppendLine();
            }

            if (!command.IsEnabled)
            {
                sb.AppendLine("__Command is currently disabled__");
            }

            field.Value = sb.ToString();

            return field;
        }

        private static string FormatParameter(Parameter parameter)
        {
            var str = parameter.Name;
            if (parameter.IsMultiple)
            {
                str += "*";
            }

            if (parameter.IsOptional)
            {
                str += "?";
            }

            if (parameter.IsRemainder)
            {
                str += "...";
            }

            if (parameter.Description != null)
            {
                str = str + "(" + parameter.Description + ")";
            }

            if (parameter.Remarks != null)
            {
                str = str + "[" + parameter.Remarks + "]";
            }

            // TODO: DefaultValue & type parsing
            return str;
        }
    }
}