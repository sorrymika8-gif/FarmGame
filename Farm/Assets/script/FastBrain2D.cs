using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class FastBrain2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private SlowBrain slowBrain;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        slowBrain = GetComponent<SlowBrain>();
    }

    // 被攻击时的快思考反应
    public void OnHit(Vector2 damageSource, float force)
    {
        // 1. 物理反馈：立即弹开 (无延迟)
        Vector2 pushDirection = ((Vector2)transform.position - damageSource).normalized;
        rb.AddForce(pushDirection * force, ForceMode2D.Impulse);

        // 2. 视觉反馈：闪红 (模拟疼痛直觉)
        StartCoroutine(FlashRed());

        // 3. 逻辑触发：将事件上报给“慢大脑”
        // 告诉慢大脑：谁在什么位置打了我也，我现在的血量
        string context = $"我被攻击了，攻击者位置：{damageSource}。我现在很痛，心情变得很差。";
        slowBrain.DecideNextBigAction(context);
    }

    IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }
}