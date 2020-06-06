using UnityEngine;
using ImGuiNET;


namespace VisualPinball.Engine.Unity.ImgGUI
{

    internal class DebugOverlay
    {
        DebugUI _debugUI;
        private int corner = 0;

        public DebugOverlay(DebugUI debugUI)
        {
            _debugUI = debugUI;
        }

        public void Draw()
        {
            const float DISTANCE = 10.0f;
            var io = ImGui.GetIO();
            var window_pos = new System.Numerics.Vector2((corner & 1) != 0 ? io.DisplaySize.X - DISTANCE : DISTANCE, (corner & 2) != 0 ? io.DisplaySize.Y - DISTANCE : DISTANCE);
            var window_pos_pivot = new System.Numerics.Vector2((corner & 1) != 0 ? 1.0f : 0.0f, (corner & 2) != 0 ? 1.0f : 0.0f);
            ImGui.SetNextWindowPos(window_pos, ImGuiCond.Always, window_pos_pivot);
            ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background


            if (ImGui.Begin("Simple overlay", ref _debugUI.showOverlayWindow, (corner != -1 ? ImGuiWindowFlags.NoMove : 0) | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav))
            {
                var pm = _debugUI.Performance;

                ImGui.PushItemWidth(-1);    // remove space saved for label
                pm.Fps.Draw("FPS: ");
                pm.PhysicsTicks.Draw("Physics: ");
                pm.PhysicsTimes.Draw("", "Physics time: ", "n1");
                ImGui.PopItemWidth();

                if (ImGui.IsMousePosValid())
                    ImGui.Text("Mouse Position: (" + io.MousePos.X.ToString("n1") + ", " + io.MousePos.Y.ToString("n1") + ")");
                else
                    ImGui.Text("Mouse Position: <invalid>");

                ImGui.Separator();
                ImGui.Text("(right-click to change position)");

                _RightButtonMenu();
            }
            ImGui.End();
        }

        void _RightButtonMenu()
        {
            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Top-left", null, corner == 0)) corner = 0;
                if (ImGui.MenuItem("Top-right", null, corner == 1)) corner = 1;
                if (ImGui.MenuItem("Bottom-left", null, corner == 2)) corner = 2;
                if (ImGui.MenuItem("Bottom-right", null, corner == 3)) corner = 3;
                ImGui.Separator();

                if (ImGui.MenuItem("Hide Ovarlay")) _debugUI.showOverlayWindow = false;
                if (ImGui.MenuItem("Show Debug Window", null, _debugUI.showDebugWindow)) _debugUI.showDebugWindow = !_debugUI.showDebugWindow;
                if (ImGui.MenuItem("Show ImGUI Demo Window", null, _debugUI.showDemoWindow)) _debugUI.showDemoWindow = !_debugUI.showDemoWindow;
                ImGui.Separator();

                if (ImGui.MenuItem("Exit")) Application.Quit();
                ImGui.EndPopup();
            }

        }
    }

}