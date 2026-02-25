using System;
using System.Collections.Generic;
using UnityEngine;
using FarmGame.Core;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC表情图片服务
    /// 负责管理和加载NPC的表情差分图资源
    /// 采用资源文件夹约定：Resources/prefabs/npcs/{npc_name}/portraits/{expression}.png
    /// </summary>
    public class PortraitService
    {
        #region 单例
        private static PortraitService mInstance;
        public static PortraitService Instance => mInstance ??= new PortraitService();
        private PortraitService() { }
        #endregion

        #region 常量
        /// <summary>表情资源路径模板</summary>
        private const string PORTRAIT_PATH_TEMPLATE = "prefabs/npcs/{0}/portraits/{1}";
        
        /// <summary>默认表情</summary>
        public const string DEFAULT_EXPRESSION = "default";
        #endregion

        #region 缓存
        /// <summary>NPC可用表情列表缓存 (npcId -> expressions[])</summary>
        private Dictionary<string, string[]> mAvailableExpressionsCache = new Dictionary<string, string[]>();
        
        /// <summary>立绘Sprite缓存 (path -> Sprite)</summary>
        private Dictionary<string, Sprite> mSpriteCache = new Dictionary<string, Sprite>();
        #endregion

        #region 公共接口

        /// <summary>
        /// 获取NPC的可用表情列表
        /// </summary>
        /// <param name="npcId">NPC的ID或model_name</param>
        /// <returns>可用表情ID数组</returns>
        public string[] GetAvailableExpressions(string npcId)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                return Array.Empty<string>();
            }

            // 检查缓存
            if (mAvailableExpressionsCache.TryGetValue(npcId, out var cached))
            {
                return cached;
            }

            // 扫描资源文件夹获取可用表情
            var expressions = ScanAvailableExpressions(npcId);
            mAvailableExpressionsCache[npcId] = expressions;
            
            return expressions;
        }

        /// <summary>
        /// 检查NPC是否支持指定表情
        /// </summary>
        public bool HasExpression(string npcId, string expression)
        {
            var expressions = GetAvailableExpressions(npcId);
            return Array.Exists(expressions, e => e.Equals(expression, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 获取NPC指定表情的立绘Sprite
        /// </summary>
        /// <param name="npcId">NPC的ID或model_name</param>
        /// <param name="expression">表情ID，如 "happy", "sad"</param>
        /// <returns>立绘Sprite，加载失败返回null</returns>
        public Sprite GetPortraitSprite(string npcId, string expression)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                Debug.LogWarning("[PortraitService] npcId为空");
                return null;
            }

            // 如果表情为空，使用默认表情
            if (string.IsNullOrEmpty(expression))
            {
                expression = DEFAULT_EXPRESSION;
            }

            string path = GetPortraitPath(npcId, expression);

            // 检查缓存
            if (mSpriteCache.TryGetValue(path, out var cached))
            {
                return cached;
            }

            // 加载资源
            var sprite = LoadPortraitSprite(path);
            
            // 如果指定表情加载失败，尝试加载默认表情
            if (sprite == null && expression != DEFAULT_EXPRESSION)
            {
                Debug.LogWarning($"[PortraitService] 表情 '{expression}' 加载失败，尝试使用默认表情");
                string defaultPath = GetPortraitPath(npcId, DEFAULT_EXPRESSION);
                sprite = LoadPortraitSprite(defaultPath);
                
                if (sprite != null)
                {
                    // 将请求的表情映射到默认sprite，避免重复尝试加载
                    mSpriteCache[path] = sprite;
                }
            }

            return sprite;
        }

        /// <summary>
        /// 预加载NPC的所有表情立绘
        /// </summary>
        public void PreloadPortraits(string npcId)
        {
            var expressions = GetAvailableExpressions(npcId);
            foreach (var expression in expressions)
            {
                GetPortraitSprite(npcId, expression);
            }
            Debug.Log($"[PortraitService] 预加载NPC '{npcId}' 的 {expressions.Length} 个表情完成");
        }

        /// <summary>
        /// 清除指定NPC的立绘缓存
        /// </summary>
        public void ClearCache(string npcId)
        {
            mAvailableExpressionsCache.Remove(npcId);
            
            // 清除该NPC的所有Sprite缓存
            var keysToRemove = new List<string>();
            string prefix = $"prefabs/npcs/{npcId}/portraits/";
            
            foreach (var key in mSpriteCache.Keys)
            {
                if (key.StartsWith(prefix))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                mSpriteCache.Remove(key);
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearAllCache()
        {
            mAvailableExpressionsCache.Clear();
            mSpriteCache.Clear();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取立绘资源路径
        /// </summary>
        private string GetPortraitPath(string npcId, string expression)
        {
            return string.Format(PORTRAIT_PATH_TEMPLATE, npcId, expression);
        }

        /// <summary>
        /// 扫描NPC可用的表情列表
        /// 通过尝试加载常用表情来判断哪些表情可用
        /// </summary>
        private string[] ScanAvailableExpressions(string npcId)
        {
            // 常见表情列表（包含你可能使用的所有表情）
            string[] commonExpressions = new string[]
            {
                "default",
                "neutral",
                "happy",
                "sad",
                "angry",
                "surprised",
                "shy",
                "confused",
                "excited",
                "tired",
                "scared",
                "love",
                "smug",
                "thinking"
            };

            var availableList = new List<string>();

            foreach (var expression in commonExpressions)
            {
                string path = GetPortraitPath(npcId, expression);
                var sprite = ResourceManager.Instance.Load<Sprite>(path);
                
                if (sprite != null)
                {
                    availableList.Add(expression);
                    // 顺便缓存加载的Sprite
                    mSpriteCache[path] = sprite;
                }
            }

            if (availableList.Count == 0)
            {
                Debug.LogWarning($"[PortraitService] NPC '{npcId}' 没有找到任何可用的表情立绘");
            }
            else
            {
                Debug.Log($"[PortraitService] NPC '{npcId}' 发现 {availableList.Count} 个可用表情: {string.Join(", ", availableList)}");
            }

            return availableList.ToArray();
        }

        /// <summary>
        /// 加载立绘Sprite
        /// </summary>
        private Sprite LoadPortraitSprite(string path)
        {
            try
            {
                var sprite = ResourceManager.Instance.Load<Sprite>(path);
                if (sprite != null)
                {
                    mSpriteCache[path] = sprite;
                }
                return sprite;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PortraitService] 加载立绘失败 '{path}': {e.Message}");
                return null;
            }
        }

        #endregion
    }
}
