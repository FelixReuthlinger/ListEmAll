using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using Jotunn;
using Jotunn.Managers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ListEmAll
{
    public class PrefabNameTranslationModel
    {
        public PrefabNameTranslationModel(string name, string translationNameToken, string translationDescriptionToken,
            string translatedName, string translatedDescription)
        {
            Name = name;
            TranslationNameToken = translationNameToken;
            TranslationDescriptionToken = translationDescriptionToken;
            TranslatedName = translatedName;
            TranslatedDescription = translatedDescription;
        }

        [UsedImplicitly] public readonly string Name;
        [UsedImplicitly] public readonly string TranslationNameToken;
        [UsedImplicitly] public readonly string TranslationDescriptionToken;
        [UsedImplicitly] public readonly string TranslatedName;
        [UsedImplicitly] public readonly string TranslatedDescription;
    }

    public static class PrefabLister
    {
        private static readonly Dictionary<string, PrefabNameTranslationModel> Translations = ListAllTranslations();
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ListEmAllPlugin.PluginGuid}.defaults.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        public static void WriteData()
        {
            var yamlContent = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Serialize(Translations);
            File.WriteAllText(DefaultFile, yamlContent);
            Logger.LogInfo($"wrote yaml content to file '{DefaultFile}'");
        }

        private static Dictionary<string, PrefabNameTranslationModel> ListAllTranslations()
        {
            Dictionary<string, ItemDrop> items = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop))
                .ToDictionary(pair => pair.Key, pair => (ItemDrop)pair.Value);
            return items.ToDictionary(pair => pair.Key, pair =>
            {
                string prefabName = pair.Value.name;
                string prefabTranslationNameToken = pair.Value.m_itemData.m_shared.m_name;
                string prefabTranslationDescriptionToken = pair.Value.m_itemData.m_shared.m_description;
                string prefabTranslatedName = Localization.instance.Localize(prefabTranslationNameToken);
                string prefabTranslatedDescription = Localization.instance.Localize(prefabTranslationDescriptionToken);
                PrefabNameTranslationModel result =
                    new(prefabName,
                        prefabTranslationNameToken,
                        prefabTranslationDescriptionToken,
                        prefabTranslatedName,
                        prefabTranslatedDescription
                    );
                return result;
            }).Where(pair => pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}