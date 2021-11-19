using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugStats : MonoBehaviour
{
    public int avgFrameRate;
    public Text fps_Text;
    public Text rtt_Text;

    public void Update()
    {
        float current = 0;
        current = Time.frameCount / Time.time;
        avgFrameRate = (int)current;
        fps_Text.text = avgFrameRate.ToString();
        rtt_Text.text = Math.Round(NetworkTime.rtt * 1000).ToString() + "ms";
    }
}