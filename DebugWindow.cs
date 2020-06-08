using UnityEngine;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;


namespace VisualPinball.Engine.Unity.ImgGUI
{

    public class DebugWindow
    {
        DebugUI _debugUI;
        int _numFramesOnChart = 200;

        public VPEUtilities VPE { get => _debugUI.VPE; }

        public DebugWindow(DebugUI debugUI)
        {
            _debugUI = debugUI;
        }

        public void Draw()
        {
            ImGui.SetNextWindowPos(new Vector2(30, 20), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(350, 100), ImGuiCond.FirstUseEver);

            ImGui.Begin("Debug");

            _debugUI.Balls.OnDebugWindow(this);
            _debugUI.Flippers.OnDebugWindow(this);

            if (ImGui.Button("Exit"))
            {
                Application.Quit();
            }

            ImGui.End();
        }

    }

}