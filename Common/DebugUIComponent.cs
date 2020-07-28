using ImGuiNET;
using UnityEngine;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    [AddComponentMenu("Visual Pinball/DebugUI ImGUI")]
    [DisallowMultipleComponent]
    public class DebugUIComponent : MonoBehaviour
    {      
        public DebugUI debugUI = null;

        void OnEnable()
        {
            ImGuiUn.Layout += OnLayout;
        }

        void OnDisable()
        {
            ImGuiUn.Layout -= OnLayout;
        }

        void OnLayout()
        {
            debugUI.OnDraw();
        }

    }
}