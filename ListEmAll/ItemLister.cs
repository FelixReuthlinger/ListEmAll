using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Jotunn.Managers;

namespace CreatureLister {
    public class ItemModel {
        public ItemModel(HitData.DamageTypes damageTypes) {
            DamageTypes = damageTypes;
        }

        [UsedImplicitly] public readonly HitData.DamageTypes DamageTypes;
        public int Weight;
    }

    public static class ItemLister {
        public static readonly Dictionary<string, ItemModel> Items = ListItems();

        private static Dictionary<string, ItemModel> ListItems() {
            Dictionary<string, ItemDrop> items = PrefabManager.Cache.GetPrefabs(typeof(ItemDrop))
                .ToDictionary(pair => pair.Key, pair => (ItemDrop) pair.Value);

            return items.ToDictionary(pair => pair.Key, pair => {
                ItemDrop.ItemData.SharedData itemCommon = pair.Value.m_itemData.m_shared;
                ItemModel result = null;
                switch (itemCommon.m_itemType) {
                    // does damage
                    case ItemDrop.ItemData.ItemType.Bow:
                    case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                    case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                    case ItemDrop.ItemData.ItemType.Torch:
                    case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                    // does protect
                    case ItemDrop.ItemData.ItemType.Chest:
                    case ItemDrop.ItemData.ItemType.Hands:
                    case ItemDrop.ItemData.ItemType.Helmet:
                    case ItemDrop.ItemData.ItemType.Legs:
                    case ItemDrop.ItemData.ItemType.Shoulder:
                    case ItemDrop.ItemData.ItemType.Utility:
                    // does block
                    case ItemDrop.ItemData.ItemType.Shield:
                        result = new ItemModel(itemCommon.m_damages);
                        break;
                    // ignore the rest
                    case ItemDrop.ItemData.ItemType.Tool:
                    case ItemDrop.ItemData.ItemType.Ammo:
                    case ItemDrop.ItemData.ItemType.Consumable:
                    case ItemDrop.ItemData.ItemType.Customization:
                    case ItemDrop.ItemData.ItemType.Material:
                    case ItemDrop.ItemData.ItemType.Misc:
                    case ItemDrop.ItemData.ItemType.None:
                    case ItemDrop.ItemData.ItemType.Trophie:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"item type '{itemCommon.m_itemType}' not supported");
                }

                return result;
            }).Where(pair => pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}