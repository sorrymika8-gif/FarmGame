using UnityEngine;
using FarmGame.Core;
using FarmGame.UI;
using Cysharp.Threading.Tasks;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// 对话系统组件
    /// 挂载在 NPC 上，负责管理与该 NPC 的通过 UI 的交互
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        private NPCEntity mEntity;
        private DialogueUIPanel mDialoguePanel;

        /// <summary>
        /// 绑定实体
        /// </summary>
        public void Bind(NPCEntity entity)
        {
            mEntity = entity;
        }

        /// <summary>
        /// 开始对话
        /// </summary>
        public void StartDialogue()
        {
            if (mEntity == null) return;

            // 打开对话面板（面板内已整合立绘显示）
            mDialoguePanel = UIManager.Instance.OpenPanel<DialogueUIPanel>(
                "UI/DialogUI/DiaLogUiPab", 
                new DialogueUIData(mEntity)
            );
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        public void EndDialogue()
        {
            // 对话面板自己管理关闭，这里只清理引用
            mDialoguePanel = null;
        }

        /// <summary>
        /// 接收用户输入（通常由 UI 调用）
        /// </summary>
        public void ReceiveInput(string content)
        {
            if (mEntity == null) return;
            mEntity.ReceiveChatAsync(content).Forget();
        }
    }
}
