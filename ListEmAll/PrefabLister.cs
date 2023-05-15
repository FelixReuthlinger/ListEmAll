using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using Jotunn;
using Jotunn.Managers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ListEmAll {
    public class PrefabNameTranslationModel {
        public PrefabNameTranslationModel(string name, string translatedName, string translatedDescription)
        {
            Name = name;
            TranslatedName = translatedName;
            TranslatedDescription = translatedDescription;
        }

        [UsedImplicitly] public readonly string Name;
        [UsedImplicitly] public readonly string TranslatedName;
        [UsedImplicitly] public readonly string TranslatedDescription;
    }

    public static class PrefabLister {
        private static readonly Dictionary<string, PrefabNameTranslationModel> Translations = ListAllTranslations();
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ListEmAllPlugin.PluginGuid}.defaults.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        public static void WriteData() {
            var yamlContent = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Serialize(Translations);
            File.WriteAllText(DefaultFile, yamlContent);
            Logger.LogInfo($"wrote yaml content to file '{DefaultFile}'");
        }
        
        private static Dictionary<string, PrefabNameTranslationModel> ListAllTranslations() {
            Dictionary<string, Hoverable> items = PrefabManager.Cache.GetPrefabs(typeof(Hoverable))
                .ToDictionary(pair => pair.Key, pair => (Hoverable) pair.Value);
            return items.ToDictionary(pair => pair.Key, pair => {
                string prefabName = pair.Key;
                string prefabTranslatedName = pair.Value.GetHoverName();
                string prefabTranslatedDescription = pair.Value.GetHoverText();
                PrefabNameTranslationModel result = new(prefabName, prefabTranslatedName, prefabTranslatedDescription);
                return result;
            }).Where(pair => pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}