using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using Jotunn.Managers;
using ListEmAll;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Logger = Jotunn.Logger;

namespace CreatureLister {
    public class HumanoidModel {
        public HumanoidModel(string internalName, Character.Faction faction, string group, float health,
            HitData.DamageModifiers damageModifiers, string defeatSetGlobalKey,
            Dictionary<string, ItemModel> defaultItems,
            Dictionary<string, ItemModel> randomWeapons) {
            InternalName = internalName;
            Faction = faction;
            Group = group;
            Health = health;
            DamageModifiers = damageModifiers;
            DefeatSetGlobalKey = defeatSetGlobalKey;
            var allItems = defaultItems.Concat(randomWeapons).ToList();
            List<KeyValuePair<string, ItemModel>> allWeaponsWithDamage =
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                allItems.Where(pair => pair.Value.DamageTypes.GetTotalDamage() > 0).ToList();
            AverageDamageTypes = AggregateWeaponDamage(allWeaponsWithDamage);
            AverageTotalDamage = AverageDamageTypes?.GetTotalDamage() ?? 0f;
            AverageTotalDamageToPlayer = AverageDamageTypes?.GetTotalBlockableDamage() ?? 0f;
            AllItemNamesContributingToDamage = allWeaponsWithDamage.Select(pair => pair.Key).ToList();
            DefaultItemAndRandomWeaponsNames = allItems.Select(pair => pair.Key).ToList();
        }

        [UsedImplicitly] public readonly string InternalName;
        [UsedImplicitly] public readonly Character.Faction Faction;
        [UsedImplicitly] public readonly string Group;
        [UsedImplicitly] public readonly float Health;
        [UsedImplicitly] public readonly HitData.DamageModifiers DamageModifiers;
        [UsedImplicitly] public readonly string DefeatSetGlobalKey;
        [UsedImplicitly] public readonly HitData.DamageTypes? AverageDamageTypes;
        [UsedImplicitly] public readonly float AverageTotalDamage;
        [UsedImplicitly] public readonly float AverageTotalDamageToPlayer;
        [UsedImplicitly] public readonly List<string> AllItemNamesContributingToDamage;
        [UsedImplicitly] public readonly List<string> DefaultItemAndRandomWeaponsNames;

        private static HitData.DamageTypes? AggregateWeaponDamage(List<KeyValuePair<string, ItemModel>> allWeapons) {
            int numberWeaponsWithDamage = allWeapons.Count;
            if (numberWeaponsWithDamage == 0) return null;
            var weightedDamage = allWeapons.Select(pair => {
                var pureDmg = pair.Value.DamageTypes;
                pureDmg.Modify(pair.Value.Weight / (float) numberWeaponsWithDamage);
                return pureDmg;
            }).ToList();
            return weightedDamage.Aggregate((a, b) => {
                a.Add(b);
                return a;
            });
        }
    }

    public static class HumanoidLister {
        private const string CloneString = "(Clone)";
        private const string PlayerString = "Player";
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ListEmAllPlugin.PluginGuid}.defaults.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        public static void WriteData() {
            var yamlContent = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Serialize(ListHumanoids());
            File.WriteAllText(DefaultFile, yamlContent);
            Logger.LogInfo($"wrote yaml content to file '{DefaultFile}'");
        }

        private static Dictionary<string, HumanoidModel> ListHumanoids() {
            Dictionary<string, Humanoid> characters = PrefabManager.Cache.GetPrefabs(typeof(Humanoid))
                .Where(kv => !kv.Key.Contains(CloneString))
                .Where(kv => !kv.Key.Contains(PlayerString))
                .ToDictionary(pair => pair.Key, pair => (Humanoid) pair.Value);

            Dictionary<string, HumanoidModel> output = characters.ToDictionary(pair => pair.Key, pair => {
                string internalName = pair.Value.m_name;
                Character.Faction faction = pair.Value.m_faction;
                string group = pair.Value.m_group;
                float health = pair.Value.m_health;
                HitData.DamageModifiers damageModifiers = pair.Value.m_damageModifiers;
                string defeatSetGlobalKey = pair.Value.m_defeatSetGlobalKey;
                Dictionary<string, ItemModel> defaultItems = ExtractItemList(pair.Value.m_defaultItems);
                Dictionary<string, ItemModel> randomWeapons = ExtractItemList(pair.Value.m_randomWeapon);
                return new HumanoidModel(internalName, faction, group, health, damageModifiers,
                    defeatSetGlobalKey, defaultItems, randomWeapons);
            });
            return output;
        }

        private static Dictionary<string, ItemModel> ExtractItemList(GameObject[] items) {
            return items.Where(item => item != null)
                .GroupBy(item => item.name)
                .Select(group => new {Name = group.Key, Count = group.Count()})
                .ToDictionary(item => item.Name,
                    item => {
                        if (!ItemLister.Items.ContainsKey(item.Name)) {
                            Logger.LogWarning(
                                $"item '{item.Name}' wasn't found, probably custom creature is configured with " +
                                $"an item that doesn't provide 'ItemDrop' component");
                            return null;
                        }

                        var resolvedItem = ItemLister.Items[item.Name];
                        resolvedItem.Weight = item.Count;
                        return resolvedItem;
                    })
                .Where(pair => pair.Value != null)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}