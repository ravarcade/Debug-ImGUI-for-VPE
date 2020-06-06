using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Physics.Engine;
using VisualPinball.Engine.Unity.ImgGUI.Tools;
using ImGuiNET;

using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VisualPinball.Engine.Unity.ImgGUI
{

	internal class DebugWindow
    {
		DebugUI _debugUI;
        bool _enableManualBallRoller;
        int _numFramesOnChart = 200;

        public DebugWindow(DebugUI debugUI) 
		{
            _debugUI = debugUI;
            _enableManualBallRoller = false;
        }

        public void Draw()
        {
            ImGui.SetNextWindowPos(new Vector2(30, 20), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(350, 100), ImGuiCond.FirstUseEver);

            ImGui.Begin("Debug");
            ImGui.Text("Balls on table: " + _debugUI.Balls.Counter.ToString("n0"));
            ImGui.Checkbox("ManualBallRoller", ref _enableManualBallRoller);
            if (_enableManualBallRoller && Input.GetMouseButton(0))
                _debugUI.Balls.ManualBallRoller();

            _OnDebugFlippers();

            if (ImGui.Button("Add Ball"))
            {
                _debugUI.VPE.CreateBall();
            }

#if UNITY_EDITOR
            if (ImGui.Button("Add Ball & Pause"))
            {
                _debugUI.VPE.CreateBall();
                EditorApplication.isPaused = true;
            }
#endif
            if (ImGui.Button("Exit"))
            {
                Application.Quit();
            }

            ImGui.End();
        }

        void _OnDebugFlippers()
        {
            if (ImGui.TreeNode("Flippers"))
            {
                var sliders = EngineProvider<IPhysicsEngine>.Get().FlipperGetDebugSliders();
                foreach (var slider in sliders) {
                    ImGuiExt.SliderFloat(slider.Label, slider.Param, slider.MinValue, slider.MaxValue);
                }

                if (sliders.Length > 0)
                {
                    ImGui.Separator();
                }

                foreach (var flipperEntity in _debugUI.Flippers.Entities) 
                {
                    _DrawFlipperState(flipperEntity);
                }

                ImGui.TreePop();
                ImGui.Separator();
            }
        }

        void _DrawFlipperState(Entity flipperEntity )
        {
            var fdd = _debugUI.Flippers[flipperEntity];

            if (ImGui.TreeNode(fdd.Name))
            {
                if (fdd.solenoid)
                {
                    ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Angle: " + fdd.Angle.ToString("n1"));
                }
                else
                {
                    ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Angle: " + fdd.Angle.ToString("n1"));
                }

                ImGui.PushItemWidth(-1);
                _DrawFlipperAngles(fdd.onAngles.ToArray(), true, 10 * math.PI / 180.0f, "Rotation speed, On after " + fdd.offDuration + " ms");
                _DrawFlipperAngles(fdd.offAngles.ToArray(), true, 10 * math.PI / 180.0f, "Rotation speed, Off after " + fdd.onDuration + " ms");
                _DrawFlipperAngles(fdd.onAngles.ToArray(), false, 1, "Angle, On after " + fdd.offDuration + " ms");
                _DrawFlipperAngles(fdd.offAngles.ToArray(), false, 1, "Angle, Off after " + fdd.onDuration + " ms");
                ImGui.PopItemWidth();
                ImGui.TreePop();
            }
        }

        void _DrawFlipperAngles(float[] arr, bool drawSpeed, float scale, string overlay_text = "")
        {
            if (arr.Length < 3)
                arr = new float[3] { 0, 0, 0 };

            if (drawSpeed)
            {
                float[] speed = new float[arr.Length - 1];
                for (int i = 0; i < speed.Length; ++i)
                    speed[i] = (arr[i + 1] - arr[i]) * scale;
                arr = speed;
            }
            float scale_min = float.MaxValue;
            float scale_max = float.MaxValue;
            int len = math.min(_numFramesOnChart, arr.Length);
            ImGui.PlotLines("", ref arr[0], len, 0, overlay_text, scale_min, scale_max, new Vector2(0, 50.0f));
        }

    }

}