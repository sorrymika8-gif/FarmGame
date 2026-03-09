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

        /// <summary>物品配置信息（统一封装）</summary>
        public ItemConfigInfo ItemConfigInfo { get; set; }

        /// <summary>购买价格</summary>
        public int BuyPrice => ShopConfig?.buy_price ?? 0;

        /// <summary>物品ID</summary>
        public int ItemId => ItemConfigInfo.ClassId;

        /// <summary>物品名称</summary>
        public string ItemName => ItemConfigInfo.Name;

        /// <summary>物品图标</summary>
        public string ItemIcon => ItemConfigInfo.Icon;

        /// <summary>物品描述</summary>
        public string ItemDescription => ItemConfigInfo.Description;
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

            // 诊断：打印所有已加载的配置类型
            var allTypes = ConfigManager.Instance.GetAllConfigTypes();
            Debug.Log($"[ShopManager] ConfigManager 已加载的配置类型: {string.Join(", ", allTypes.Select(t => t.Name))}");

            try
            {
                // 优先使用 GetList 获取 List 格式的配置
                var listContainer = ConfigManager.Instance.GetList<ShopConfig>();
                if (listContainer != null)
                {
                    var allConfigs = listContainer.GetAll();
                    Debug.Log($"[ShopManager] 成功获取 ListContainer，读取到 {allConfigs.Count} 条商店配置");
                    
                    foreach (var config in allConfigs)
                    {
                        if (!mShopItemsByType.ContainsKey(config.shop_type))
                        {
                            mShopItemsByType[config.shop_type] = new List<ShopConfig>();
                        }
                        mShopItemsByType[config.shop_type].Add(config);
                        Debug.Log($"[ShopManager] 加载商品: id={config.id}, shop_type={config.shop_type}, item_id={config.item_id}, price={config.buy_price}");
                    }
                }
                else
                {
                    Debug.LogWarning("[ShopManager] GetList<ShopConfig> 返回 null，配置可能不是 List 格式");
                }
            }
            catch (KeyNotFoundException)
            {
                Debug.LogWarning("[ShopManager] ShopConfig 未加载，请检查 shop.xlsx 文件是否存在");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ShopManager] 初始化失败: {ex}");
            }

            mIsInitialized = true;
            
            // 输出详细的商店类型信息
            foreach (var kvp in mShopItemsByType)
            {
                Debug.Log($"[ShopManager] 商店类型 {kvp.Key} 包含 {kvp.Value.Count} 个商品");
            }
            
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
            if (!ValidateInitialized()) 
            {
                Debug.LogWarning("[ShopManager] GetShopItems called but not initialized");
                return new List<ShopItemData>();
            }

            Debug.Log($"[ShopManager] GetShopItems called with shopType={shopType}, 已加载的商店类型: {string.Join(", ", mShopItemsByType.Keys)}");

            var result = new List<ShopItemData>();

            if (mShopItemsByType.TryGetValue(shopType, out var shopConfigs))
            {
                Debug.Log($"[ShopManager] 找到商店类型 {shopType}，包含 {shopConfigs.Count} 个配置");
                
                foreach (var shopConfig in shopConfigs)
                {
                    // 使用 ItemConfigHelper 从多个配置表中查找物品
                    var configInfo = ItemConfigHelper.GetConfigInfo(shopConfig.item_id);
                    if (configInfo.IsValid)
                    {
                        result.Add(new ShopItemData
                        {
                            ShopConfig = shopConfig,
                            ItemConfigInfo = configInfo
                        });
                        Debug.Log($"[ShopManager] 添加商品: item_id={shopConfig.item_id}, name={configInfo.Name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ShopManager] 物品配置未找到 item_id={shopConfig.item_id}，请检查 item.xlsx 或 seed.xlsx");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ShopManager] 未找到商店类型 {shopType}");
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
