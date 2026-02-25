using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Player;
using FarmGame.Item;

namespace FarmGame.Shop
{
    /// <summary>
    /// 商店类型枚举
    /// </summary>
    public enum ShopType
    {
        /// <summary>无</summary>
        None = 0,
        /// <summary>种子店</summary>
        SeedShop = 1,
        /// <summary>工具店</summary>
        ToolShop = 2,
        /// <summary>杂货店</summary>
        GeneralShop = 3
    }

    /// <summary>
    /// 商店商品数据（运行时）
    /// </summary>
    public class ShopItemData
    {
        /// <summary>商店配置</summary>
        public ShopConfig ShopConfig { get; set; }

        /// <summary>物品配置</summary>
        public ItemConfig ItemConfig { get; set; }

        /// <summary>购买价格</summary>
        public int BuyPrice => ShopConfig?.buy_price ?? 0;

        /// <summary>出售价格</summary>
        public int SellPrice => ItemConfig?.sell_price ?? 0;
    }

    /// <summary>
    /// 商店管理器
    /// 负责商品的买卖逻辑
    /// </summary>
    public class ShopManager : MonoSingleton<ShopManager>
    {
        #region 私有字段

        private bool mIsInitialized;
        private Dictionary<int, List<ShopConfig>> mShopItemsByType;

        #endregion

        #region 事件

        /// <summary>
        /// 购买成功事件
        /// 参数：物品ID, 数量, 花费金币
        /// </summary>
        public event Action<int, int, int> OnItemBought;

        /// <summary>
        /// 出售成功事件
        /// 参数：物品ID, 数量, 获得金币
        /// </summary>
        public event Action<int, int, int> OnItemSold;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化商店管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 按商店类型分组商品配置
            mShopItemsByType = new Dictionary<int, List<ShopConfig>>();

            try
            {
                var shopList = ConfigManager.Instance.GetList<ShopConfig>();
                if (shopList != null)
                {
                    foreach (var config in shopList.GetAll())
                    {
                        if (!mShopItemsByType.ContainsKey(config.shop_type))
                        {
                            mShopItemsByType[config.shop_type] = new List<ShopConfig>();
                        }
                        mShopItemsByType[config.shop_type].Add(config);
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                // 配置表尚未生成或为空，使用空数据
                Debug.LogWarning("[ShopManager] ShopConfig not found, shop will be empty");
            }

            mIsInitialized = true;
            Debug.Log($"[ShopManager] Initialized with {mShopItemsByType.Count} shop types");
        }

        #endregion

        #region 公共接口 - 查询

        /// <summary>
        /// 获取指定商店的商品列表
        /// </summary>
        /// <param name="shopType">商店类型</param>
        /// <returns>商品数据列表</returns>
        public List<ShopItemData> GetShopItems(int shopType)
        {
            if (!ValidateInitialized()) return new List<ShopItemData>();

            var result = new List<ShopItemData>();

            if (mShopItemsByType.TryGetValue(shopType, out var shopConfigs))
            {
                foreach (var shopConfig in shopConfigs)
                {
                    var itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(shopConfig.item_id);
                    if (itemConfig != null)
                    {
                        result.Add(new ShopItemData
                        {
                            ShopConfig = shopConfig,
                            ItemConfig = itemConfig
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取指定商店的商品列表
        /// </summary>
        /// <param name="shopType">商店类型</param>
        /// <returns>商品数据列表</returns>
        public List<ShopItemData> GetShopItems(ShopType shopType)
        {
            return GetShopItems((int)shopType);
        }

        /// <summary>
        /// 获取物品在指定商店的购买价格
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="shopType">商店类型</param>
        /// <returns>购买价格，-1表示该商店不出售此物品</returns>
        public int GetBuyPrice(int itemId, int shopType)
        {
            if (!ValidateInitialized()) return -1;

            if (mShopItemsByType.TryGetValue(shopType, out var shopConfigs))
            {
                var config = shopConfigs.FirstOrDefault(c => c.item_id == itemId);
                if (config != null)
                {
                    return config.buy_price;
                }
            }

            return -1;
        }

        /// <summary>
        /// 获取物品的出售价格（从物品配置读取）
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>出售价格，0表示不可出售</returns>
        public int GetSellPrice(int itemId)
        {
            if (!ValidateInitialized()) return 0;

            var itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(itemId);
            return itemConfig?.sell_price ?? 0;
        }

        #endregion

        #region 公共接口 - 交易

        /// <summary>
        /// 检查是否能购买物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">数量</param>
        /// <param name="shopType">商店类型</param>
        /// <returns>是否能购买</returns>
        public bool CanBuy(int itemId, int count, int shopType)
        {
            if (!ValidateInitialized()) return false;
            if (count <= 0) return false;

            int buyPrice = GetBuyPrice(itemId, shopType);
            if (buyPrice < 0) return false; // 该商店不出售此物品

            int totalCost = buyPrice * count;
            return PlayerManager.Instance.HasEnoughGold(totalCost);
        }

        /// <summary>
        /// 检查是否能出售物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否能出售</returns>
        public bool CanSell(int itemId, int count)
        {
            if (!ValidateInitialized()) return false;
            if (count <= 0) return false;

            int sellPrice = GetSellPrice(itemId);
            if (sellPrice <= 0) return false; // 不可出售

            // 检查玩家是否拥有足够物品
            var inventory = GetPlayerInventory();
            if (inventory == null) return false;

            return inventory.HasItem(itemId, count);
        }

        /// <summary>
        /// 购买物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">数量</param>
        /// <param name="shopType">商店类型</param>
        /// <returns>是否购买成功</returns>
        public bool BuyItem(int itemId, int count, int shopType)
        {
            if (!CanBuy(itemId, count, shopType))
            {
                Debug.LogWarning($"[ShopManager] Cannot buy item {itemId} x{count} from shop {shopType}");
                return false;
            }

            int buyPrice = GetBuyPrice(itemId, shopType);
            int totalCost = buyPrice * count;

            // 扣除金币
            if (!PlayerManager.Instance.SpendGold(totalCost))
            {
                return false;
            }

            // 添加物品到背包
            var inventory = GetPlayerInventory();
            if (inventory != null)
            {
                inventory.AddItem(itemId, count);
            }

            Debug.Log($"[ShopManager] Bought item {itemId} x{count} for {totalCost} gold");
            OnItemBought?.Invoke(itemId, count, totalCost);
            return true;
        }

        /// <summary>
        /// 出售物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否出售成功</returns>
        public bool SellItem(int itemId, int count)
        {
            if (!CanSell(itemId, count))
            {
                Debug.LogWarning($"[ShopManager] Cannot sell item {itemId} x{count}");
                return false;
            }

            int sellPrice = GetSellPrice(itemId);
            int totalEarning = sellPrice * count;

            // 从背包移除物品
            var inventory = GetPlayerInventory();
            if (inventory == null || !inventory.RemoveItem(itemId, count))
            {
                return false;
            }

            // 增加金币
            PlayerManager.Instance.AddGold(totalEarning);

            Debug.Log($"[ShopManager] Sold item {itemId} x{count} for {totalEarning} gold");
            OnItemSold?.Invoke(itemId, count, totalEarning);
            return true;
        }

        #endregion

        #region 私有方法

        private bool ValidateInitialized()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[ShopManager] Not initialized. Call Initialize() first.");
                return false;
            }
            return true;
        }

        private InventoryComponent GetPlayerInventory()
        {
            var player = PlayerManager.Instance.Player;
            if (player == null)
            {
                Debug.LogWarning("[ShopManager] Player not found");
                return null;
            }
            return player.Inventory;
        }

        #endregion
    }
}
