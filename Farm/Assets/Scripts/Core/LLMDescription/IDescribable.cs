namespace FarmGame.Core.LLMDescription
{
    /// <summary>
    /// 可描述对象接口
    /// 实现此接口的对象可以通过 LLMDescriptionService 生成叙事风格的描述
    /// </summary>
    public interface IDescribable
    {
        /// <summary>
        /// 描述类型标识
        /// 用于匹配对应的提示词模板（如 "Crop", "NPC", "Building"）
        /// </summary>
        string DescriptionType { get; }

        /// <summary>
        /// 获取对象的显示名称
        /// </summary>
        /// <returns>显示名称</returns>
        string GetDisplayName();

        /// <summary>
        /// 获取用于缓存的唯一标识
        /// 当此值改变时，会重新生成描述
        /// </summary>
        /// <returns>缓存键（可包含状态信息，如 "crop_1_stage_2"）</returns>
        string GetCacheKey();
    }
}
