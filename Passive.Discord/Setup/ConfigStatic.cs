using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Newtonsoft.Json;

namespace Passive.Discord.Setup
{
    public partial class Config
    {
        public static string FilesPath = Path.Combine(AppContext.BaseDirectory, "files");

        private static string savePath = Path.Combine(FilesPath, "config.json");

        public enum Defaults
        {
            Token,

            Prefix,

            ShardCount
        }

        public static Config LoadFromFile(string path)
        {
            Config config;
            if (path == null)
            {
                path = savePath;
            }

            EnsureFilesDirectoryCreated();

            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<Config>(file);
                savePath = path;
            }
            else
            {
                config = GenerateAndSaveDefaultConfig(path);
            }

            return config;
        }

        public static Config ParseArguments(string[] args = null)
        {
            Config config = null;
            if (args != null)
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o =>
                    {
                        config = LoadFromFile(o.ConfigPath);
                    });
            }
            else
            {
                EnsureFilesDirectoryCreated();
            }

            config ??= LoadFromFile(null);
            return config;
        }

        private static void EnsureFilesDirectoryCreated()
        {
            if (!Directory.Exists(FilesPath))
            {
                Directory.CreateDirectory(FilesPath);
            }
        }

        private static Config GenerateAndSaveDefaultConfig(string path)
        {
            savePath = path;
            var config = new Config();
            File.WriteAllText(path, JsonConvert.SerializeObject(config));
            return config;
        }
    }
}