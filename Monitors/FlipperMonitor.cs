using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Physics.Engine;
using ImGuiNET;

using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    public class FlipperDebugData
    {
        public List<float> onAngles = new List<float>();
        public List<float> offAngles = new List<float>();
        public bool solenoid = false;
        public int onDuration = 0;
        public int offDuration = 0;
        public int currentSolenoidStateDuration = 0;
        public string Name;
        public float Angle = 0;

        public FlipperDebugData(string name)
        {
            Name = name;
        }
    }

    public class FlipperMonitor : IMonitor
    {
        const int _MaxSolenoidAnglesDataLength = 500;
        int _numFramesOnChart = 200;

        Dictionary<Entity, int> _flipperToIdx = new Dictionary<Entity, int>();
        FlipperDebugData[] _flipperDebugData = new FlipperDebugData[0];
        public int Count { get => _flipperDebugData.Length; }

        public void Register(Entity entity, string name)
        {
            _flipperToIdx.Add(entity, _flipperDebugData.Length);
            Array.Resize(ref _flipperDebugData, _flipperDebugData.Length + 1);
            _flipperDebugData[_flipperDebugData.Length - 1] = new FlipperDebugData(name);
        }

        public void OnPhysicsUpdate(double physicClockMilliseconds, int numSteps, float processingTimeMilliseconds)
        {
            // read flipper states and create charts
            var flippers = EngineProvider<IPhysicsEngine>.Get().FlipperGetDebugStates();

            foreach (var fs in flippers)
            {
                var fdd = GetFlipperDebugData(fs.Entity);

                if (fs.Solenoid)
                {
                    if (fdd.solenoid != fs.Solenoid)
                    {
                        fdd.onAngles.Clear();
                        fdd.offDuration = fdd.currentSolenoidStateDuration;
                        fdd.currentSolenoidStateDuration = 0;
                    }

                    if (fdd.onAngles.Count < _MaxSolenoidAnglesDataLength)
                        fdd.onAngles.Add(fs.Angle);
                }
                else
                {
                    if (fdd.solenoid != fs.Solenoid)
                    {
                        fdd.offAngles.Clear();
                        fdd.onDuration = fdd.currentSolenoidStateDuration;
                        fdd.currentSolenoidStateDuration = 0;
                    }

                    if (fdd.offAngles.Count < _MaxSolenoidAnglesDataLength)
                        fdd.offAngles.Add(fs.Angle);
                }

                fdd.solenoid = fs.Solenoid;
                fdd.Angle = fs.Angle;
                ++fdd.currentSolenoidStateDuration;
            }
        }

        public Entity[] Entities
        {
            get { return _flipperToIdx.Keys.ToArray(); }
        }

        public ref FlipperDebugData this[Entity entity]
        {
            get { return ref GetFlipperDebugData(entity); }
        }

        public ref FlipperDebugData GetFlipperDebugData(Entity flipperEntity)
        {
            Assert.IsTrue(_flipperToIdx.ContainsKey(flipperEntity));
            return ref _flipperDebugData[_flipperToIdx[flipperEntity]];
        }

        // ==================================================================== Draw in Debug Window ===

        public void OnDebugWindow(DebugWindow dw)
        {
            if (ImGui.TreeNode("FlipperMonitor", string.Format("Flippers ({0})", Count)))
            {
                var sliders = EngineProvider<IPhysicsEngine>.Get().FlipperGetDebugSliders();
                foreach (var slider in sliders)
                {
                    ImGuiExt.SliderFloat(slider.Label, slider.Param, slider.MinValue, slider.MaxValue);
                }

                if (sliders.Length > 0)
                {
                    ImGui.Separator();
                }

                foreach (var flipperEntity in Entities)
                {
                    _DrawFlipperState(flipperEntity);
                }

                ImGui.TreePop();
                ImGui.Separator();
            }
        }

        void _DrawFlipperState(Entity flipperEntity)
        {
            var fdd = GetFlipperDebugData(flipperEntity);

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