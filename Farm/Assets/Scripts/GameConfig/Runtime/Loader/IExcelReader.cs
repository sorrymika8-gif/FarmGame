// ==========================================================
// 自动生成配置系统 - Excel 读取接口
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// Excel 读取器接口
    /// </summary>
    public interface IExcelReader : IDisposable
    {
        /// <summary>
        /// 打开 Excel 文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        void Open(string filePath);

        /// <summary>
        /// 关闭文件
        /// </summary>
        void Close();

        /// <summary>
        /// 获取单元格值
        /// </summary>
        /// <param name="row">行索引（从0开始）</param>
        /// <param name="col">列索引（从0开始）</param>
        /// <returns>单元格字符串值</returns>
        string GetCellValue(int row, int col);

        /// <summary>
        /// 获取总行数
        /// </summary>
        int GetRowCount();

        /// <summary>
        /// 获取总列数
        /// </summary>
        int GetColumnCount();

        /// <summary>
        /// 检查工作表是否存在
        /// </summary>
        /// <param name="sheetName">工作表名称</param>
        bool HasSheet(string sheetName);

        /// <summary>
        /// 设置当前工作表（按名称）
        /// </summary>
        /// <param name="sheetName">工作表名称</param>
        void SetActiveSheet(string sheetName);

        /// <summary>
        /// 设置当前工作表（按索引）
        /// </summary>
        /// <param name="sheetIndex">工作表索引（从0开始）</param>
        void SetActiveSheet(int sheetIndex);

        /// <summary>
        /// 获取所有工作表名称
        /// </summary>
        IEnumerable<string> GetSheetNames();
    }

    /// <summary>
    /// Excel 读取器工厂
    /// </summary>
    public interface IExcelReaderFactory
    {
        /// <summary>
        /// 创建 Excel 读取器
        /// </summary>
        IExcelReader Create();
    }
}
