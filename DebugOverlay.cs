using UnityEngine;
using ImGuiNET;
using Unity.Entities.UniversalDelegates;

namespace VisualPinball.Engine.Unity.ImgGUI
{

    internal class DebugOverlay
    {
        DebugUI _debugUI;
        int _corner = 0;
        bool _enableManualBallRoller = false;

        public DebugOverlay(DebugUI debugUI)
        {
            _debugUI = debugUI;
        }

        public void Draw()
        {
            _ProcessKeyboardInput();
            const float DISTANCE = 10.0f;
            var io = ImGui.GetIO();
            var window_pos = new System.Numerics.Vector2((_corner & 1) != 0 ? io.DisplaySize.X - DISTANCE : DISTANCE, (_corner & 2) != 0 ? io.DisplaySize.Y - DISTANCE : DISTANCE);
            var window_pos_pivot = new System.Numerics.Vector2((_corner & 1) != 0 ? 1.0f : 0.0f, (_corner & 2) != 0 ? 1.0f : 0.0f);
            ImGui.SetNextWindowPos(window_pos, ImGuiCond.Always, window_pos_pivot);
            ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background


            if (ImGui.Begin("Simple overlay", ref _debugUI.showOverlayWindow, (_corner != -1 ? ImGuiWindowFlags.NoMove : 0) | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav))
            {
                var pm = _debugUI.Performance;

                ImGui.PushItemWidth(-1);    // remove space saved for label
                pm.Fps.Draw("FPS: ");
                pm.PhysicsTicks.Draw("Physics: ");
                pm.PhysicsTimes.Draw("", "Physics time: ", "n1");
                ImGui.PopItemWidth();

                if (ImGuiExt.Button("[F6]", "Press F6 to spawn ball at cursor"))
                    _debugUI.VPE.CreateBall();

                if (ImGuiExt.Button("[F7]", "Press F7 to enable manual ball roller", _enableManualBallRoller ? 0.35f : 0.0f))
                    _enableManualBallRoller = !_enableManualBallRoller;

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
                if (ImGui.MenuItem("Top-left", null, _corner == 0)) _corner = 0;
                if (ImGui.MenuItem("Top-right", null, _corner == 1)) _corner = 1;
                if (ImGui.MenuItem("Bottom-left", null, _corner == 2)) _corner = 2;
                if (ImGui.MenuItem("Bottom-right", null, _corner == 3)) _corner = 3;
                ImGui.Separator();

                if (ImGui.MenuItem("Hide Ovarlay")) _debugUI.showOverlayWindow = false;
                if (ImGui.MenuItem("Show Debug Window", null, _debugUI.showDebugWindow)) _debugUI.showDebugWindow = !_debugUI.showDebugWindow;
                if (ImGui.MenuItem("Show ImGUI Demo Window", null, _debugUI.showDemoWindow)) _debugUI.showDemoWindow = !_debugUI.showDemoWindow;
                ImGui.Separator();

                if (ImGui.MenuItem("Exit")) Application.Quit();
                ImGui.EndPopup();
            }
        }

        void _ProcessKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.F6))
                _debugUI.VPE.CreateBallAtClick();

            if (Input.GetKeyDown(KeyCode.F7))
                _enableManualBallRoller = !_enableManualBallRoller;

            if (_enableManualBallRoller && Input.GetMouseButton(0))
                _debugUI.Balls.ManualBallRoller();

        }
    }

}