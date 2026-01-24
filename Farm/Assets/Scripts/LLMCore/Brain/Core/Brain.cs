using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FarmGame.GameLLM;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 大脑核心
    /// 协调 提示词构建 -> LLM 调用 -> 指令解析 的流程
    /// </summary>
    public class Brain : IDisposable
    {
        private readonly PromptBuilderRegistry mPromptBuilders;
        private readonly CommandParserRegistry mCommandParsers;
        private readonly CommandExecutorRegistry mCommandExecutors;
        private readonly CommandQueue mCommandQueue;

        public Brain()
        {
            mPromptBuilders = new PromptBuilderRegistry();
            mCommandParsers = new CommandParserRegistry();
            mCommandExecutors = new CommandExecutorRegistry();
            mCommandQueue = new CommandQueue(mCommandExecutors);
        }

        #region 注册方法

        /// <summary>注册提示词构建器</summary>
        public void RegisterPromptBuilder(IPromptBuilder builder)
        {
            mPromptBuilders.Register(builder);
        }

        /// <summary>注册指令解析器</summary>
        public void RegisterCommandParser(ICommandParser parser)
        {
            mCommandParsers.Register(parser);
        }

        /// <summary>注册指令执行器</summary>
        public void RegisterCommandExecutor(ICommandExecutor executor)
        {
            mCommandExecutors.Register(executor);
        }

        #endregion

        #region 核心流程

        /// <summary>
        /// 执行决策（构建提示词 -> LLM -> 解析指令）
        /// </summary>
        public async UniTask<DecisionResult> DecideAsync(
            DecisionContext context,
            CancellationToken cancellationToken = default)
        {
            var startTime = Time.realtimeSinceStartup;
            var result = new DecisionResult();

            try
            {
                // 1. 获取提示词构建器
                var promptBuilder = mPromptBuilders.Get(context.DecisionType);
                if (promptBuilder == null)
                {
                    result.Success = false;
                    result.ErrorMessage = $"未找到决策类型 '{context.DecisionType}' 的提示词构建器";
                    return result;
                }

                // 2. 构建提示词
                string prompt = promptBuilder.Build(context);

                // 3. 调用 LLM
                // 注意：LLMService.Client 是静态属性，直接访问，不要通过 Instance
                if (LLMService.Client == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "LLMService Client 未初始化";
                    return result;
                }

                // 创建请求，这里我们将构建好的 prompt 作为 User 消息发送
                // PromptBuilder 应该在 prompt 中包含所有的上下文和指令格式要求
                var llmRequest = new LLMRequest().AddUser(prompt);
                
                var llmResponse = await LLMService.Client.SendAsync(llmRequest, cancellationToken);

                if (!llmResponse.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = llmResponse.ErrorMessage;
                    return result;
                }

                result.RawOutput = llmResponse.Content;

                // 4. 获取指令解析器
                var commandParser = mCommandParsers.Get(context.DecisionType);
                if (commandParser == null)
                {
                    result.Success = false;
                    result.ErrorMessage = $"未找到决策类型 '{context.DecisionType}' 的指令解析器";
                    return result;
                }

                // 5. 解析指令
                var commands = commandParser.Parse(llmResponse.Content);
                result.Commands.AddRange(commands);
                result.Success = true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Debug.LogError($"[Brain] Decision failed: {ex.Message}");
            }
            finally
            {
                result.ProcessingTime = Time.realtimeSinceStartup - startTime;
            }

            return result;
        }

        /// <summary>
        /// 执行决策并将指令加入队列
        /// </summary>
        public async UniTask<DecisionResult> DecideAndEnqueueAsync(
            DecisionContext context,
            CancellationToken cancellationToken = default)
        {
            var result = await DecideAsync(context, cancellationToken);

            if (result.Success && result.Commands.Count > 0)
            {
                mCommandQueue.EnqueueRange(result.Commands, context);
            }

            return result;
        }

        /// <summary>
        /// 处理指令队列中的所有指令
        /// </summary>
        public void ProcessCommandQueue()
        {
            mCommandQueue.ProcessAll();
        }

        /// <summary>
        /// 处理指令队列中的下一个指令
        /// </summary>
        public bool ProcessNextCommand()
        {
            return mCommandQueue.ProcessNext();
        }

        /// <summary>
        /// 指令队列中的指令数量
        /// </summary>
        public int PendingCommandCount => mCommandQueue.Count;

        #endregion

        public void Dispose()
        {
            // 清理资源 (如果需要)
            mCommandQueue.Clear();
        }
    }
}
