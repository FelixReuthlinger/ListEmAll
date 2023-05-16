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
    public class TranslationModel
    {

        public TranslationModel(ItemDrop fromItemDrop)
        {
            Name = fromItemDrop.name;
            TranslationNameToken = fromItemDrop.m_itemData.m_shared.m_name;
            TranslationDescriptionToken = fromItemDrop.m_itemData.m_shared.m_description;
            TranslatedName = Localization.instance.Localize(TranslationNameToken);
            TranslatedDescription = Localization.instance.Localize(TranslationDescriptionToken);
        }
        
        public TranslationModel(Piece fromPiece)
        {
            Name = fromPiece.name;
            TranslationNameToken = fromPiece.m_name;
            TranslationDescriptionToken = fromPiece.m_description;
            TranslatedName = Localization.instance.Localize(TranslationNameToken);
            TranslatedDescription = Localization.instance.Localize(TranslationDescriptionToken);
        }

        [UsedImplicitly] public readonly string Name;
        [UsedImplicitly] public readonly string TranslationNameToken;
        [UsedImplicitly] public readonly string TranslationDescriptionToken;
        [UsedImplicitly] public readonly string TranslatedName;
        [UsedImplicitly] public readonly string TranslatedDescription;
    }

    public static class TranslationsPrinter
    {
        private static readonly Dictionary<string, TranslationModel> Translations = ListAllTranslations();
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ListEmAllPlugin.PluginGuid}.defaults.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        public static void WriteData(string prefabNamePrefixFilter)
        {
            Dictionary<string, TranslationModel> filteredTranslations = Translations
                .Where(pair => pair.Key.StartsWith(prefabNamePrefixFilter))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            int prefabCount = filteredTranslations.Count;
            if (prefabCount > 0)
            {
                Logger.LogInfo(
                    $"filtering prefab name using the prefix {prefabNamePrefixFilter} " +
                    $"did yield {filteredTranslations.Count} found prefabs that will be printed");
                WriteData(filteredTranslations,
                    Path.Combine(DefaultConfigRootPath, $"{ListEmAllPlugin.PluginGuid}.{prefabNamePrefixFilter}.yaml"));
            }
            else
            {
                Logger.LogWarning(
                    $"filtering prefab name using the prefix {prefabNamePrefixFilter} " +
                    $"did NOT yield any prefabs, skipping to write file!");
            }
        }

        public static void WriteData()
        {
            WriteData(Translations, DefaultFile);
        }

        private static void WriteData(Dictionary<string, TranslationModel> translations, string filPath)
        {
            var yamlContent = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Serialize(translations);
            File.WriteAllText(filPath, yamlContent);
            Logger.LogInfo($"wrote yaml content to file '{DefaultFile}'");
        }

        private static Dictionary<string, TranslationModel> ListAllTranslations()
        {
            List<Dictionary<string, TranslationModel>> results = new()
            {
                PrefabManager.Cache.GetPrefabs(typeof(ItemDrop))
                    .ToDictionary(pair => pair.Key, pair => new TranslationModel((ItemDrop)pair.Value)),
                PrefabManager.Cache.GetPrefabs(typeof(Piece))
                    .ToDictionary(pair => pair.Key, pair => new TranslationModel((Piece)pair.Value)),
            };
            return results.SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(group => group.Key, group => group.First());
        }
    }
}