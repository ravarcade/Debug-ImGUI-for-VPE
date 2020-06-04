using System.Collections.Generic;
using ImGuiNET;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VisualPinball.Engine.Common;
using VisualPinball.Engine.Unity.ImgGUI.Tools;
using VisualPinball.Unity.Game;
using VisualPinball.Unity.Physics.DebugUI;
using VisualPinball.Unity.Physics.Engine;
using VisualPinball.Unity.VPT.Table;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualPinball.Engine.Unity.ImgGUI
{
    public class DebugUIClient : IDebugUI
    {
        const int _MaxSolenoidAnglesDataLength = 500;
        public int numFramesOnChart = 200;

        private DebugOverlay debugOverlay = new DebugOverlay();
        public bool showOverlayWindow = true;
        public bool showDebugWindow = true;
        public bool showDemoWindow = false;


        public FPSHelper _fps = new FPSHelper(true, 0, 100, "n1");
        public FPSHelper _physicsTicks = new FPSHelper(true, 0, 1500, "n0", 300);
        public ChartFloat _physicsTimes = new ChartFloat(100, 0.0f, 20.0f, 50);
        public float _phyTimeAccu = 0;
        public class FlipperDebugData
        {
            public List<float> onAngles = new List<float>();
            public List<float> offAngles = new List<float>();
            public bool solenoid = false;
            public int onDuration = 0;
            public int offDuration = 0;
            public int duration = 0;
        }

        public Dictionary<string, Entity> _flippers = new Dictionary<string, Entity>();
        public Dictionary<Entity, string> _flipperNames = new Dictionary<Entity, string>();
        public Dictionary<Entity, int> _flipperToIdx = new Dictionary<Entity, int>();
        public List<FlipperDebugData> _flippersDebugData = new List<FlipperDebugData>();

        public int _ballCounter = 0;
        public bool _enableManualBallRoller = false;

        private Player _player = null;
        public Player player
        {
            get
            {
                if (_player == null)
                {
                    var players = GameObject.FindObjectsOfType<Player>();
                    _player = players?[0];
                }
                return _player;
            }
        }

        public string Name => "ImgGUI";

        public void Init(TableBehavior tableBehavior)
        {
            // add component if not already added in editor
            var guiHook = tableBehavior.gameObject.GetComponent<DebugGuiHook>();
            if (guiHook == null) {
                guiHook = tableBehavior.gameObject.AddComponent<DebugGuiHook>();
            }
            guiHook.debugUI = this;
        }

        private void ProcessDataOncePerFrame()
        {
            _fps.Tick();
            _physicsTimes.Add(_phyTimeAccu);
            _phyTimeAccu = 0.0f;
        }

        public bool Draw()
        {
            ProcessDataOncePerFrame();

            if (showOverlayWindow)
                debugOverlay.Draw(this);

            if (showDemoWindow)
                ImGui.ShowDemoWindow(ref showDemoWindow);

            if (showDebugWindow)
            {
                OnDebug();
            }

            return showDebugWindow;
        }

        void _DrawFlipperState(string name, DebugFlipperState fs, ref FlipperDebugData fdd)
        {
            if (ImGui.TreeNode(name))
            {
                if (fs.Solenoid)
                {
                    ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Angle: " + fs.Angle.ToString("n1"));
                }
                else
                {
                    ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Angle: " + fs.Angle.ToString("n1"));
                }

                ImGui.PushItemWidth(-1);
                _DrawFlipperAngles(fdd.onAngles.ToArray(), true, 10 * math.PI / 180.0f, "Rotation speed, On after " + fdd.offDuration + " ms" );
                _DrawFlipperAngles(fdd.offAngles.ToArray(), true, 10 * math.PI / 180.0f, "Rotation speed, Off after " + fdd.onDuration + " ms");
                _DrawFlipperAngles(fdd.onAngles.ToArray(), false, 1, "Angle, On after " + fdd.offDuration + " ms");
                _DrawFlipperAngles(fdd.offAngles.ToArray(), false, 1, "Angle, Off after " + fdd.onDuration + " ms");
                ImGui.PopItemWidth();
            }
            ImGui.TreePop();
            ImGui.Separator();
        }

        void _OnDebugFlippers()
        {
            if (ImGui.TreeNode("Flippers"))
            {
                var sliders = EngineProvider<IPhysicsEngine>.Get().FlipperGetDebugSliders();
                foreach (var slider in sliders) {
                    ImGui_SliderFloat(slider.Label, slider.Param, slider.MinValue, slider.MaxValue);
                }

                if (sliders.Length > 0)
                {
                    ImGui.Separator();
                }

                var allFdds = _flippersDebugData.ToArray();
                var allFss = EngineProvider<IPhysicsEngine>.Get().FlipperGetDebugStates();
                foreach (var fs in allFss) {
                    int fidx = _flipperToIdx[fs.Entity];
                    var name = _flipperNames[fs.Entity];
                    _DrawFlipperState(name, fs, ref allFdds[fidx]);
                }
                //ImGui.SliderInt("Num frames on chart", ref numFramesOnChart, 10, 500);
                //				player.physicsEngine?.OnDebugDraw();
                ImGui.TreePop();
                ImGui.Separator();
            }
        }

        private void OnDebug()
        {
            ImGui.SetNextWindowPos(new Vector2(30, 20), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(350, 100), ImGuiCond.FirstUseEver);

            ImGui.Begin("Debug");
            ImGui.Text("Balls on table: " + _ballCounter.ToString("n0"));
            ImGui.Checkbox("ManualBallRoller", ref _enableManualBallRoller);
            if (_enableManualBallRoller && Input.GetMouseButton(0))
                ManualBallRoller();

            _OnDebugFlippers();

            if (ImGui.Button("Add Ball"))
            {
                player?.CreateBall(new DebugBallCreator());
            }

#if UNITY_EDITOR
			if (ImGui.Button("Add Ball & Pause"))
			{
				player?.CreateBall(new DebugBallCreator());
				EditorApplication.isPaused = true;
			}
#endif
            if (ImGui.Button("Exit"))
            {
                Application.Quit();
            }
        }

        Camera camera = null;

        private void ManualBallRoller()
        {
            if (camera == null)
            {
                camera = GameObject.FindObjectOfType<Camera>();
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            const float epsilon = 0.0001f;
            float dist = math.abs(ray.direction.y) > epsilon ? ray.origin.y / ray.direction.y : 0.0f;
            if (dist < epsilon)
            {
                var pointOnPlayfieldSurface = ray.origin - ray.direction * dist;
                EngineProvider<IPhysicsEngine>.Get().BallManualRoll(_lastCreatedBallEntityForManualBallRoller, pointOnPlayfieldSurface);
            }
        }

        // ================================================================ IDebugUI ===
        public void OnRegisterFlipper(Entity entity, string name)
        {
            _flippers[name] = entity;
            _flipperNames[entity] = name;
            _flipperToIdx[entity] = _flippersDebugData.Count;
            _flippersDebugData.Add(new FlipperDebugData());
        }

        public void OnPhysicsUpdate(int numSteps, float processingTime)
        {
            _physicsTicks.Tick(numSteps);
            _phyTimeAccu += processingTime;
            var _allFdd = _flippersDebugData.ToArray();

            // read flipper states and create charts
            var flippers = EngineProvider<IPhysicsEngine>.Get().FlipperGetDebugStates();
            foreach (var fs in flippers) {
                ref FlipperDebugData fdd = ref _allFdd[_flipperToIdx[fs.Entity]];

                if (fs.Solenoid)
                {
                    if (fdd.solenoid != fs.Solenoid)
                    {
                        fdd.onAngles.Clear();
                        fdd.offDuration = fdd.duration;
                        fdd.duration = 0;
                    }

                    if (fdd.onAngles.Count < _MaxSolenoidAnglesDataLength)
                        fdd.onAngles.Add(fs.Angle);
                }
                else
                {
                    if (fdd.solenoid != fs.Solenoid)
                    {
                        fdd.offAngles.Clear();
                        fdd.onDuration = fdd.duration;
                        fdd.duration = 0;
                    }

                    if (fdd.offAngles.Count < _MaxSolenoidAnglesDataLength)
                        fdd.offAngles.Add(fs.Angle);
                }

                fdd.solenoid = fs.Solenoid;
                ++fdd.duration;
            }
        }

        Entity _lastCreatedBallEntityForManualBallRoller = Entity.Null;
        public void OnCreateBall(Entity entity)
        {
            if (entity == Entity.Null || entity.Index < 0) // not valid entity!
                return;
            _lastCreatedBallEntityForManualBallRoller = entity;
            ++_ballCounter;
        }

        // ================================================================== Helpers ===
        void ImGui_SliderFloat(string label, DebugFlipperSliderParam param, float min, float max)
        {
            var engine = EngineProvider<IPhysicsEngine>.Get();
            float val = engine.GetFlipperDebugValue(param);
            if (ImGui.SliderFloat(label, ref val, min, max))
            {
                engine.SetFlipperDebugValue(param, val);
            }
        }

        private void _DrawFlipperAngles(float[] arr, bool drawSpeed, float scale, string overlay_text = "")
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
            float scale_min = 3.402823466e+38F;
            float scale_max = 3.402823466e+38F;
            int len = math.min(numFramesOnChart, arr.Length);
            ImGui.PlotLines("", ref arr[0], len, 0, overlay_text, scale_min, scale_max, new System.Numerics.Vector2(0, 50.0f));
        }

    }
}