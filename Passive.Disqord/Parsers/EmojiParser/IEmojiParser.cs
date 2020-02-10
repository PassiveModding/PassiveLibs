using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Extensions.Parsers.EmojiParser;
using Newtonsoft.Json;
using Qmmands;

namespace Disqord.Extensions.Parsers
{
    /// <summary>
    /// IEmoji parser, allows parsing of LocalEmoji and LocalCustomEmoji for commands.
    /// </summary>
    public sealed class IEmojiParser : TypeParser<IEmoji>
    {
        private const string EmojiMap = "https://static.emzi0767.com/misc/discordEmojiMap.json";
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);

        private static EmojiDefinition[] emojis = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="IEmojiParser"/> class.
        /// </summary>
        /// <param name="client">HttpClient used for initial fetch of emojis on the first parse call.</param>
        public IEmojiParser(HttpClient client)
        {
            this.Client = client;
        }

        private HttpClient Client { get; }

        /// <inheritdoc/>
        public override async ValueTask<TypeParserResult<IEmoji>>
            ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            if (LocalCustomEmoji.TryParse(value, out var localCustomEmoji))
            {
                return TypeParserResult<IEmoji>.Successful(localCustomEmoji);
            }

            if (emojis == null)
            {
                await Locker.WaitAsync();
                try
                {
                    await GetEmojis();
                }
                finally
                {
                    Locker.Release();
                }
            }

            var match = emojis.FirstOrDefault(x => x.Surrogates == value ||
                            x.NamesWithColons.Any(n => n.Equals(value, System.StringComparison.OrdinalIgnoreCase)) ||
                            x.Names.Any(n => n.Equals(value, System.StringComparison.OrdinalIgnoreCase)));

            if (match != null)
            {
                var localEmoji = new LocalEmoji(match.Surrogates);
                return TypeParserResult<IEmoji>.Successful(localEmoji);
            }

            return TypeParserResult<IEmoji>.Unsuccessful("Invalid custom emoji format.");
        }

        private async Task GetEmojis()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, EmojiMap))
            {
                using (var response = await this.Client.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    emojis = JsonConvert.DeserializeObject<EmojiVersion>(content).EmojiDefinitions;
                }
            }
        }
    }
}