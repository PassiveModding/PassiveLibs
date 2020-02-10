using System;
using Newtonsoft.Json;

namespace Disqord.Extensions.Parsers.EmojiParser
{
    public class EmojiDefinition
    {
        [JsonProperty("primaryName")]
        public string PrimaryName { get; set; }

        [JsonProperty("primaryNameWithColons")]
        public string PrimaryNameWithColons { get; set; }

        [JsonProperty("names")]
        public string[] Names { get; set; }

        [JsonProperty("namesWithColons")]
        public string[] NamesWithColons { get; set; }

        [JsonProperty("surrogates")]
        public string Surrogates { get; set; }

        [JsonProperty("utf32codepoints")]
        public long[] Utf32Codepoints { get; set; }

        [JsonProperty("assetFileName")]
        public string AssetFileName { get; set; }

        [JsonProperty("assetUrl")]
        public Uri AssetUrl { get; set; }
    }
}