using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Qmmands;

namespace Disqord.Extensions.Interactivity.Help
{
    public class HelpMenu : MenuBase
    {
        private readonly Dictionary<string, LocalEmbed> pages = new Dictionary<string, LocalEmbed>();
        private LocalEmbed homePage;

        public HelpMenu(CommandService commandService)
        {
            CommandService = commandService;
        }

        public CommandService CommandService { get; }

        protected override async Task<IUserMessage> InitialiseAsync()
        {
            var tempDict = new Dictionary<string, (Module Module, LocalEmbedBuilder Embed, string Title)>();
            foreach (var module in CommandService.GetAllModules())
            {
                if (module.Attributes
                    .FirstOrDefault(x => x.GetType()
                    .Equals(typeof(HelpMetadataAttribute))) is HelpMetadataAttribute attMatch)
                {
                    var title = $"{attMatch.ButtonCode} {module.Name}";
                    if (!tempDict.TryAdd(
                        attMatch.ButtonCode,
                        (module, HelpService.GetModuleHelp(module).WithColor(attMatch.Color),
                        title)))
                    {
                        // TODO: warn about module being ignored due to duplicate button.
                    }
                }
            }

            var footerTitles = new List<string>
            {
                "❓ Help"
            };

            footerTitles.AddRange(tempDict.Values.Select(x => x.Title));
            var footerContent = string.Join(", ", footerTitles);
            var homeBuilder = new LocalEmbedBuilder()
                .WithTitle("Modules")
                .WithColor(Color.Aqua)
                .WithFooter(footerContent);

            foreach (var dPage in tempDict)
            {
                if (dPage.Value.Module.Commands.Count == 0)
                {
                    homeBuilder.AddField(new LocalEmbedFieldBuilder
                    {
                        Name = dPage.Value.Title,
                        Value = "No Commands."
                    });
                }
                else
                {
                    homeBuilder.AddField(new LocalEmbedFieldBuilder
                    {
                        Name = dPage.Value.Title,
                        Value = string.Join(", ", dPage.Value.Module.Commands.Select(c => '`' + c.Name + '`'))
                    });

                    pages.Add(dPage.Key, dPage.Value.Embed.WithFooter(footerContent).Build());
                }
            }

            homePage = homeBuilder.Build();

            var message = await Channel.SendMessageAsync("", false, homePage);
            await AddButtonAsync(new Button(new LocalEmoji("❓"), x =>
            {
                return Message.ModifyAsync(m => m.Embed = homePage);
            }));

            foreach (var page in pages)
            {
                await AddButtonAsync(new Button(new LocalEmoji(page.Key), x =>
                {
                    return Message.ModifyAsync(m => m.Embed = page.Value);
                }));
            }

            return message;
        }
    }
}