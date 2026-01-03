using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

// 定义符合 Gemini API 要求的 JSON 结构类
[Serializable]
public class GeminiRequest
{
    public Content[] contents;
}

[Serializable]
public class Content
{
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}

public class SlowBrain : MonoBehaviour
{
    [SerializeField] private string apiKey = "AIzaSyAxDAro4_nC9Xl5JSIHMPvSvWYTKifTZi4"; 
    private string apiURL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private bool isThinking = false;
    public string npcPersonality = "你是一个2D游戏里的哥布林，性格贪婪胆小。";

    public void DecideNextBigAction(string eventDescription)
    {
        if (isThinking) return;
        StartCoroutine(AskGemini(eventDescription));
    }

    IEnumerator AskGemini(string eventDescription)
    {
        isThinking = true;
        Debug.Log("慢思考：正在通过 Gemini 思考对策...");

        // 1. 构建对象并转化为标准的 JSON，这样会自动处理换行符和引号
        string fullPrompt = $"{npcPersonality}\n当前局势：{eventDescription}\n请给出你的下一步行动，字数简短。";
        
        GeminiRequest requestData = new GeminiRequest {
            contents = new Content[] {
                new Content {
                    parts = new Part[] {
                        new Part { text = fullPrompt }
                    }
                }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);

        // 2. 发送请求
        using (UnityWebRequest request = new UnityWebRequest($"{apiURL}?key={apiKey}", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Gemini 响应成功: " + request.downloadHandler.text);
                ExecutePlan(request.downloadHandler.text);
            }
            else
            {
                // 如果还是 400，这里会打印出服务器给出的具体错误原因
                Debug.LogError($"Gemini 连接失败 ({request.responseCode}): " + request.error);
                Debug.LogError("服务器详情: " + request.downloadHandler.text);
            }
        }

        isThinking = false;
    }

    void ExecutePlan(string responseJson)
    {
        // 这里后续可以写 JSON 解析代码提取对话
    }
}