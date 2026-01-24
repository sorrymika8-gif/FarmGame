// ==========================================================
// 自动生成配置系统 - 数据加载器
// ==========================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 配置数据加载器
    /// 从 Excel 读取数据行，填充到容器
    /// </summary>
    public class ConfigLoader
    {
        private readonly ConfigSchemaParser mSchemaParser;

        public ConfigLoader()
        {
            mSchemaParser = new ConfigSchemaParser();
        }

        /// <summary>
        /// 加载配置到容器
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="reader">Excel 读取器</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>配置容器</returns>
        public IConfigContainer Load<T>(IExcelReader reader, string filePath) where T : class, new()
        {
            // 1. 解析 Schema
            var schema = mSchemaParser.Parse(reader, filePath);

            // 2. 验证 Schema
            if (!schema.Validate(out var errors))
            {
                throw new InvalidOperationException(
                    $"配置表 '{filePath}' Schema 验证失败:\n{string.Join("\n", errors)}");
            }

            // 3. 创建容器
            var container = CreateContainer<T>(schema);

            // 4. 加载数据
            LoadDataRows(reader, schema, container, typeof(T));

            return container;
        }

        /// <summary>
        /// 加载配置到容器（通过反射创建配置类实例）
        /// </summary>
        /// <param name="reader">Excel 读取器</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="configType">配置类型</param>
        /// <returns>配置容器</returns>
        public IConfigContainer Load(IExcelReader reader, string filePath, Type configType)
        {
            // 1. 解析 Schema
            var schema = mSchemaParser.Parse(reader, filePath);

            // 2. 验证 Schema
            if (!schema.Validate(out var errors))
            {
                throw new InvalidOperationException(
                    $"配置表 '{filePath}' Schema 验证失败:\n{string.Join("\n", errors)}");
            }

            // 3. 创建容器
            var container = CreateContainerByType(schema, configType);

            // 4. 加载数据
            LoadDataRows(reader, schema, container, configType);

            return container;
        }

        /// <summary>
        /// 根据 Schema 创建对应的容器
        /// </summary>
        private IConfigContainer CreateContainer<T>(ConfigSchema schema) where T : class, new()
        {
            var keyDepth = schema.KeyDepth;
            var keyTypes = GetKeyTypes(schema);

            return schema.Format switch
            {
                ConfigFormat.List => new ListContainer<T>(),

                ConfigFormat.Map when keyDepth == 1 =>
                    CreateMapContainer1<T>(keyTypes[0]),

                ConfigFormat.Map when keyDepth == 2 =>
                    CreateMapContainer2<T>(keyTypes[0], keyTypes[1]),

                ConfigFormat.Map when keyDepth == 3 =>
                    CreateMapContainer3<T>(keyTypes[0], keyTypes[1], keyTypes[2]),

                ConfigFormat.GroupMap when keyDepth == 1 =>
                    CreateGroupMapContainer1<T>(keyTypes[0]),

                ConfigFormat.GroupMap when keyDepth == 2 =>
                    CreateGroupMapContainer2<T>(keyTypes[0], keyTypes[1]),

                ConfigFormat.GroupMap when keyDepth == 3 =>
                    CreateGroupMapContainer3<T>(keyTypes[0], keyTypes[1], keyTypes[2]),

                _ => throw new NotSupportedException(
                    $"不支持的配置格式: Format={schema.Format}, KeyDepth={keyDepth}")
            };
        }

        /// <summary>
        /// 通过反射创建容器
        /// </summary>
        private IConfigContainer CreateContainerByType(ConfigSchema schema, Type configType)
        {
            var keyDepth = schema.KeyDepth;
            var keyTypes = GetKeyTypes(schema);

            Type containerType;

            switch (schema.Format)
            {
                case ConfigFormat.List:
                    containerType = typeof(ListContainer<>).MakeGenericType(configType);
                    break;

                case ConfigFormat.Map when keyDepth == 1:
                    containerType = typeof(MapContainer<,>).MakeGenericType(keyTypes[0], configType);
                    break;

                case ConfigFormat.Map when keyDepth == 2:
                    containerType = typeof(MapContainer<,,>).MakeGenericType(keyTypes[0], keyTypes[1], configType);
                    break;

                case ConfigFormat.Map when keyDepth == 3:
                    containerType = typeof(MapContainer<,,,>).MakeGenericType(keyTypes[0], keyTypes[1], keyTypes[2], configType);
                    break;

                case ConfigFormat.GroupMap when keyDepth == 1:
                    containerType = typeof(GroupMapContainer<,>).MakeGenericType(keyTypes[0], configType);
                    break;

                case ConfigFormat.GroupMap when keyDepth == 2:
                    containerType = typeof(GroupMapContainer<,,>).MakeGenericType(keyTypes[0], keyTypes[1], configType);
                    break;

                case ConfigFormat.GroupMap when keyDepth == 3:
                    containerType = typeof(GroupMapContainer<,,,>).MakeGenericType(keyTypes[0], keyTypes[1], keyTypes[2], configType);
                    break;

                default:
                    throw new NotSupportedException(
                        $"不支持的配置格式: Format={schema.Format}, KeyDepth={keyDepth}");
            }

            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        /// <summary>
        /// 获取主键字段的类型列表
        /// </summary>
        private Type[] GetKeyTypes(ConfigSchema schema)
        {
            var keyFields = schema.GetKeyFields();
            var types = new Type[keyFields.Count];
            for (int i = 0; i < keyFields.Count; i++)
            {
                types[i] = SupportedTypes.GetSystemType(keyFields[i].Type);
            }
            return types;
        }

        /// <summary>
        /// 加载数据行到容器
        /// </summary>
        private void LoadDataRows(IExcelReader reader, ConfigSchema schema, IConfigContainer container, Type configType)
        {
            var rowCount = reader.GetRowCount();
            var startRow = schema.DataStartRow;

            // 获取配置类的字段信息
            var fieldInfos = new Dictionary<string, FieldInfo>();
            foreach (var field in schema.Fields)
            {
                var fieldInfo = configType.GetField(field.Name,
                    BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    fieldInfos[field.Name] = fieldInfo;
                }
            }

            // 获取容器的 Add 方法
            var addMethod = GetAddMethod(container, schema);

            // 遍历数据行
            for (int row = startRow; row < rowCount; row++)
            {
                try
                {
                    // 检查是否为空行（第一列为空则跳过）
                    var firstCell = reader.GetCellValue(row, 0);
                    if (string.IsNullOrWhiteSpace(firstCell))
                    {
                        continue;
                    }

                    // 创建配置实例
                    var instance = Activator.CreateInstance(configType);

                    // 填充字段值
                    foreach (var field in schema.Fields)
                    {
                        var cellValue = reader.GetCellValue(row, field.ColumnIndex);
                        var parsedValue = TypeParser.Parse(cellValue, field.Type);

                        if (fieldInfos.TryGetValue(field.Name, out var fieldInfo))
                        {
                            fieldInfo.SetValue(instance, parsedValue);
                        }
                    }

                    // 添加到容器
                    AddToContainer(container, schema, instance, addMethod);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"加载配置失败: 文件={schema.SourceFilePath}, 行={row + 1}\n{ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 获取容器的 Add 方法
        /// </summary>
        private MethodInfo GetAddMethod(IConfigContainer container, ConfigSchema schema)
        {
            var containerType = container.GetType();
            var methods = containerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                if (method.Name == "Add")
                {
                    var parameters = method.GetParameters();
                    var expectedParamCount = schema.Format == ConfigFormat.List ? 1 : schema.KeyDepth + 1;

                    if (parameters.Length == expectedParamCount)
                    {
                        return method;
                    }
                }
            }

            throw new InvalidOperationException($"无法找到容器的 Add 方法");
        }

        /// <summary>
        /// 添加实例到容器
        /// </summary>
        private void AddToContainer(IConfigContainer container, ConfigSchema schema, object instance, MethodInfo addMethod)
        {
            var keyFields = schema.GetKeyFields();
            var keyValues = new object[keyFields.Count];

            // 获取主键值
            var instanceType = instance.GetType();
            for (int i = 0; i < keyFields.Count; i++)
            {
                var fieldInfo = instanceType.GetField(keyFields[i].Name,
                    BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    keyValues[i] = fieldInfo.GetValue(instance);
                }
            }

            // 构建参数列表
            object[] args;
            if (schema.Format == ConfigFormat.List)
            {
                args = new[] { instance };
            }
            else
            {
                args = new object[keyValues.Length + 1];
                Array.Copy(keyValues, args, keyValues.Length);
                args[keyValues.Length] = instance;
            }

            // 调用 Add 方法
            addMethod.Invoke(container, args);
        }

        #region 容器创建辅助方法

        private IConfigContainer CreateMapContainer1<T>(Type keyType)
        {
            var containerType = typeof(MapContainer<,>).MakeGenericType(keyType, typeof(T));
            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        private IConfigContainer CreateMapContainer2<T>(Type keyType1, Type keyType2)
        {
            var containerType = typeof(MapContainer<,,>).MakeGenericType(keyType1, keyType2, typeof(T));
            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        private IConfigContainer CreateMapContainer3<T>(Type keyType1, Type keyType2, Type keyType3)
        {
            var containerType = typeof(MapContainer<,,,>).MakeGenericType(keyType1, keyType2, keyType3, typeof(T));
            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        private IConfigContainer CreateGroupMapContainer1<T>(Type keyType)
        {
            var containerType = typeof(GroupMapContainer<,>).MakeGenericType(keyType, typeof(T));
            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        private IConfigContainer CreateGroupMapContainer2<T>(Type keyType1, Type keyType2)
        {
            var containerType = typeof(GroupMapContainer<,,>).MakeGenericType(keyType1, keyType2, typeof(T));
            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        private IConfigContainer CreateGroupMapContainer3<T>(Type keyType1, Type keyType2, Type keyType3)
        {
            var containerType = typeof(GroupMapContainer<,,,>).MakeGenericType(keyType1, keyType2, keyType3, typeof(T));
            return (IConfigContainer)Activator.CreateInstance(containerType);
        }

        #endregion
    }
}
