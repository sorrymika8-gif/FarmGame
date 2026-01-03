using UnityEngine;

public class test1 : MonoBehaviour
{
    // 在这里定义 npc 变量
    // [SerializeField] 会让这个变量出现在 Unity 的检查器（Inspector）面板中
    [SerializeField] public GameObject npc; 

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0)) 
        {
            // 确保你已经把 NPC 拖进了变量框，否则会报空引用错误
            if (npc != null)
            {
                // 获取鼠标在世界坐标中的位置
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // 尝试获取 NPC 上的 FastBrain2D 脚本并调用 OnHit
                FastBrain2D fastBrain = npc.GetComponent<FastBrain2D>();
                
                if (fastBrain != null)
                {
                    Debug.Log("点击了鼠标，触发快思考！");
                    fastBrain.OnHit(mousePos, 5f);
                }
                else
                {
                    Debug.LogError("NPC 物体上没有挂载 FastBrain2D 脚本！");
                }
            }
            else
            {
                Debug.LogError("请在 Inspector 面板中将 NPC 物体拖入 test1 脚本的 Npc 槽位中！");
            }
        }
    }
}