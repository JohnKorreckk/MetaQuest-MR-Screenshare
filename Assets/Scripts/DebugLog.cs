using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugLogger : MonoBehaviour
{
    public Text debugText; // or TextMeshProUGUI for TMP

    private int maxLines = 20;
    private readonly System.Collections.Generic.Queue<string> messages = new System.Collections.Generic.Queue<string>();

    public void Log(string message)
    {
        if (messages.Count >= maxLines)
        {
            messages.Dequeue();
        }
        messages.Enqueue(message);

        debugText.text = string.Join("\n", messages);
    }
}
