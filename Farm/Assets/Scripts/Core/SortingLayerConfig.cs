using UnityEngine;

namespace FarmGame.Core
{
    /// <summary>
    /// 渲染排序层级定义
    /// 层级优先级（从低到高）：Background < MapObjects < Characters < WorldUI
    /// 注意：Screen Space UI 不需要 SortingLayer，Canvas 会自动在最上层渲染
    /// </summary>
    public static class SortingLayerConfig
    {
        #region 层级名称常量

        /// <summary>
        /// 默认层级
        /// </summary>
        public const string Default = "Default";

        /// <summary>
        /// 背景层 - 地图背景、地形等
        /// </summary>
        public const string Background = "Background";

        /// <summary>
        /// 地图物件层 - 地图上的预制体、建筑、装饰物等
        /// </summary>
        public const string MapObjects = "MapObjects";

        /// <summary>
        /// 角色层 - NPC 和玩家（同一层级，按 Y 轴排序）
        /// </summary>
        public const string Characters = "Characters";

        /// <summary>
        /// 世界空间 UI 层 - 气泡、血条、名字标签等
        /// </summary>
        public const string WorldUI = "WorldUI";

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取 SortingLayer 的 ID
        /// </summary>
        /// <param name="layerName">层级名称</param>
        /// <returns>层级 ID，如果不存在返回 0（Default）</returns>
        public static int GetLayerID(string layerName)
        {
            return SortingLayer.NameToID(layerName);
        }

        /// <summary>
        /// 设置 SpriteRenderer 的排序层级
        /// </summary>
        /// <param name="renderer">SpriteRenderer 组件</param>
        /// <param name="layerName">层级名称</param>
        /// <param name="orderInLayer">层内排序值（可选）</param>
        public static void SetSortingLayer(SpriteRenderer renderer, string layerName, int? orderInLayer = null)
        {
            if (renderer == null) return;

            renderer.sortingLayerName = layerName;
            if (orderInLayer.HasValue)
            {
                renderer.sortingOrder = orderInLayer.Value;
            }
        }

        /// <summary>
        /// 设置 Renderer 的排序层级（通用）
        /// </summary>
        /// <param name="renderer">Renderer 组件</param>
        /// <param name="layerName">层级名称</param>
        /// <param name="orderInLayer">层内排序值（可选）</param>
        public static void SetSortingLayer(Renderer renderer, string layerName, int? orderInLayer = null)
        {
            if (renderer == null) return;

            renderer.sortingLayerName = layerName;
            if (orderInLayer.HasValue)
            {
                renderer.sortingOrder = orderInLayer.Value;
            }
        }

        #endregion
    }
}
