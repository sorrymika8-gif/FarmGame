using System.Collections.Generic;
using UnityEngine;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Spawn
{
    /// <summary>
    /// 生成队列 - 环形缓冲区实现
    /// 零 GC、固定内存、溢出时覆盖最旧请求
    /// </summary>
    public class SpawnQueue
    {
        #region 单例

        private static SpawnQueue mInstance;

        /// <summary>单例实例</summary>
        public static SpawnQueue Instance
        {
            get
            {
                mInstance ??= new SpawnQueue(AtomConstants.SpawnQueueCapacity);
                return mInstance;
            }
        }

        #endregion

        #region 私有字段

        private readonly SpawnRequest[] mBuffer;
        private int mHead;
        private int mTail;
        private int mCount;
        private readonly int mCapacity;

        // 延迟请求单独存储（环形缓冲区不支持跳过中间元素）
        private readonly List<SpawnRequest> mDelayedList;

        #endregion

        #region 公共属性

        /// <summary>当前队列中的请求数量（不含延迟队列）</summary>
        public int Count => mCount;

        /// <summary>延迟队列中的请求数量</summary>
        public int DelayedCount => mDelayedList.Count;

        /// <summary>总请求数量</summary>
        public int TotalCount => mCount + mDelayedList.Count;

        /// <summary>队列容量</summary>
        public int Capacity => mCapacity;

        /// <summary>是否为空</summary>
        public bool IsEmpty => mCount == 0;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建生成队列
        /// </summary>
        /// <param name="capacity">容量</param>
        public SpawnQueue(int capacity)
        {
            mCapacity = capacity;
            mBuffer = new SpawnRequest[capacity];
            mHead = 0;
            mTail = 0;
            mCount = 0;
            mDelayedList = new List<SpawnRequest>();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 入队请求
        /// </summary>
        /// <param name="request">生成请求</param>
        public void Enqueue(SpawnRequest request)
        {
            // 如果有延迟，放入延迟队列
            if (request.TimeRemaining > 0.01f)
            {
                mDelayedList.Add(request);
                return;
            }

            // 立即请求放入环形缓冲区
            mBuffer[mTail] = request;
            mTail = (mTail + 1) % mCapacity;

            if (mCount == mCapacity)
            {
                // 缓冲区满，覆盖最旧的请求
                mHead = (mHead + 1) % mCapacity;
                Debug.LogWarning("[SpawnQueue] Buffer full, oldest request discarded");
            }
            else
            {
                mCount++;
            }
        }

        /// <summary>
        /// 查看队首请求（不移除）
        /// </summary>
        /// <param name="request">输出请求</param>
        /// <returns>是否成功</returns>
        public bool TryPeek(out SpawnRequest request)
        {
            if (mCount == 0)
            {
                request = default;
                return false;
            }

            request = mBuffer[mHead];
            return true;
        }

        /// <summary>
        /// 出队请求
        /// </summary>
        /// <returns>队首请求</returns>
        public SpawnRequest Dequeue()
        {
            if (mCount == 0)
            {
                Debug.LogError("[SpawnQueue] Dequeue from empty queue");
                return default;
            }

            var request = mBuffer[mHead];
            mHead = (mHead + 1) % mCapacity;
            mCount--;

            return request;
        }

        /// <summary>
        /// 尝试出队请求
        /// </summary>
        /// <param name="request">输出请求</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(out SpawnRequest request)
        {
            if (mCount == 0)
            {
                request = default;
                return false;
            }

            request = Dequeue();
            return true;
        }

        /// <summary>
        /// 处理延迟队列，将到期的请求转入主队列
        /// 应在每帧调用
        /// </summary>
        public void ProcessDelayedRequests()
        {
            for (int i = mDelayedList.Count - 1; i >= 0; i--)
            {
                var request = mDelayedList[i];
                if (request.IsReady)
                {
                    mDelayedList.RemoveAt(i);
                    Enqueue(request);  // 重新入队（此时 delay 已到期，会进入主队列）
                }
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            mHead = 0;
            mTail = 0;
            mCount = 0;
            mDelayedList.Clear();
        }

        /// <summary>
        /// 重置单例（用于场景切换等）
        /// </summary>
        public static void ResetInstance()
        {
            mInstance?.Clear();
            mInstance = null;
        }

        #endregion
    }
}
