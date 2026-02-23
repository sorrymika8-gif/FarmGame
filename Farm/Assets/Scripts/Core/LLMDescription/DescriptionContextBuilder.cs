using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FarmGame.Core.LLMDescription
{
    /// <summary>
    /// 描述上下文构建器
    /// 使用反射自动收集带有 [Describable] 特性的属性
    /// </summary>
    public class DescriptionContextBuilder
    {
        #region 缓存

        /// <summary>
        /// 类型属性元数据缓存
        /// Key: 类型, Value: 属性信息列表
        /// </summary>
        private static readonly Dictionary<Type, List<PropertyMeta>> sTypeMetaCache = new();

        /// <summary>
        /// 属性元数据
        /// </summary>
        private class PropertyMeta
        {
            public MemberInfo Member;
            public DescribableAttribute Attribute;
            public List<DescribableNestedAttribute> NestedAttributes;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从可描述对象构建上下文
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <returns>描述上下文</returns>
        public DescriptionContext Build(IDescribable target)
        {
            if (target == null)
            {
                Debug.LogWarning("[DescriptionContextBuilder] Target is null");
                return null;
            }

            var context = new DescriptionContext
            {
                Type = target.DescriptionType,
                DisplayName = target.GetDisplayName(),
                CacheKey = target.GetCacheKey()
            };

            // 获取或创建类型的属性元数据
            var type = target.GetType();
            var metas = GetOrCreateTypeMeta(type);

            // 遍历所有标记的属性，收集值
            foreach (var meta in metas)
            {
                try
                {
                    // 收集直接属性
                    if (meta.Attribute != null && meta.Attribute.IncludeInDescription)
                    {
                        var value = GetMemberValue(meta.Member, target);
                        context.AddProperty(meta.Attribute.Key, value);
                    }

                    // 收集嵌套属性
                    if (meta.NestedAttributes != null)
                    {
                        foreach (var nestedAttr in meta.NestedAttributes)
                        {
                            var value = GetNestedValue(target, nestedAttr.PropertyPath);
                            context.AddProperty(nestedAttr.Key, value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[DescriptionContextBuilder] Failed to get value for {meta.Member.Name}: {e.Message}");
                }
            }

            return context;
        }

        /// <summary>
        /// 清除类型元数据缓存
        /// </summary>
        public static void ClearCache()
        {
            sTypeMetaCache.Clear();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取或创建类型的属性元数据
        /// </summary>
        private List<PropertyMeta> GetOrCreateTypeMeta(Type type)
        {
            if (sTypeMetaCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var metas = new List<PropertyMeta>();

            // 扫描属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var describable = prop.GetCustomAttribute<DescribableAttribute>();
                var nestedAttrs = prop.GetCustomAttributes<DescribableNestedAttribute>();
                var nestedList = nestedAttrs != null ? new List<DescribableNestedAttribute>(nestedAttrs) : null;

                if (describable != null || (nestedList != null && nestedList.Count > 0))
                {
                    metas.Add(new PropertyMeta
                    {
                        Member = prop,
                        Attribute = describable,
                        NestedAttributes = nestedList?.Count > 0 ? nestedList : null
                    });
                }
            }

            // 扫描字段
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var describable = field.GetCustomAttribute<DescribableAttribute>();
                var nestedAttrs = field.GetCustomAttributes<DescribableNestedAttribute>();
                var nestedList = nestedAttrs != null ? new List<DescribableNestedAttribute>(nestedAttrs) : null;

                if (describable != null || (nestedList != null && nestedList.Count > 0))
                {
                    metas.Add(new PropertyMeta
                    {
                        Member = field,
                        Attribute = describable,
                        NestedAttributes = nestedList?.Count > 0 ? nestedList : null
                    });
                }
            }

            sTypeMetaCache[type] = metas;
            return metas;
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        private object GetMemberValue(MemberInfo member, object target)
        {
            return member switch
            {
                PropertyInfo prop => prop.GetValue(target),
                FieldInfo field => field.GetValue(target),
                _ => null
            };
        }

        /// <summary>
        /// 获取嵌套属性值
        /// 支持路径格式如 "PlantData.name"
        /// </summary>
        private object GetNestedValue(object target, string propertyPath)
        {
            if (target == null || string.IsNullOrEmpty(propertyPath))
                return null;

            var parts = propertyPath.Split('.');
            var current = target;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                var type = current.GetType();

                // 尝试获取属性
                var prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    current = prop.GetValue(current);
                    continue;
                }

                // 尝试获取字段
                var field = type.GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }

                // 找不到成员
                Debug.LogWarning($"[DescriptionContextBuilder] Cannot find member '{part}' in path '{propertyPath}'");
                return null;
            }

            return current;
        }

        #endregion
    }
}
