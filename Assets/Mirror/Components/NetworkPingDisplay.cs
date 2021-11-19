using System;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// Component that will display the clients ping in milliseconds
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkPingDisplay")]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-ping-display")]
    public class NetworkPingDisplay : MonoBehaviour
    {
        public Color color = Color.white;
        public int padding = 2;
        int width = 200;
        int height = 40;

        void OnGUI()
        {
            // only while client is active
            if (!NetworkClient.active) return;

            // show rtt in top right corner, right aligned
            GUI.color = color;
            Rect rect = new Rect(Screen.width - width - padding, 0, width, height);
            GUIStyle style = GUI.skin.GetStyle("Label");
            style.fontSize = Screen.height * 2 / 100;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(rect, $"RTT: {Math.Round(NetworkTime.rtt * 1000)}ms", style);
            GUI.color = Color.white;
        }
    }
}
