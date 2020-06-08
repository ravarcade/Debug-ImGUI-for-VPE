using Unity.Entities;
using VisualPinball.Unity.Physics.DebugUI;
using VisualPinball.Unity.VPT.Table;
using ImGuiNET;
using UnityEngine;
using Unity.Mathematics;

namespace VisualPinball.Engine.Unity.ImgGUI
{

    public class DebugUI : IDebugUI
    {
        public string Name => "ImgGUI";

        public bool showOverlayWindow = true;
        public bool showDebugWindow = true;
        public bool showDemoWindow = false;

        FlipperMonitor _flippers = new FlipperMonitor();
        BallMonitor _balls = null;
        PerformanceMonitor _performance = new PerformanceMonitor();
        VPEUtilities _VPEUtilities;


        public FlipperMonitor Flippers { get => _flippers; }
        public BallMonitor Balls { get => _balls; }
        public PerformanceMonitor Performance { get => _performance; }
        public VPEUtilities VPE { get => _VPEUtilities; }

        DebugOverlay _debugOverlay;
        DebugWindow _debugWindow;

        // ==================================================================== IDebugUI ===

        public void Init(TableBehavior tableBehavior)
        {
            // add component if not already added in editor
            var debugUIComponent = tableBehavior.gameObject.GetComponent<DebugUIComponent>();
            if (debugUIComponent == null)
            {
                debugUIComponent = tableBehavior.gameObject.AddComponent<DebugUIComponent>();
            }
            debugUIComponent.debugUI = this;
            _balls = new BallMonitor(this);
            _debugOverlay = new DebugOverlay(this);
            _debugWindow = new DebugWindow(this);
            _VPEUtilities = new VPEUtilities(this, tableBehavior);
        }

        public void OnPhysicsUpdate(double physicClockMilliseconds, int numSteps, float processingTimeMilliseconds)
        {
            _performance.OnPhysicsUpdate(physicClockMilliseconds, numSteps, processingTimeMilliseconds);
            _flippers.OnPhysicsUpdate(physicClockMilliseconds, numSteps, processingTimeMilliseconds);
            _balls.OnPhysicsUpdate(physicClockMilliseconds, numSteps, processingTimeMilliseconds);
        }

        public void OnRegisterFlipper(Entity entity, string name)
        {
            _flippers.Register(entity, name);
        }

        public void OnCreateBall(Entity entity)
        {
            _balls.Register(entity, null);
        }

        // ==================================================================== DebugUI ===

        public void OnDraw()
        {
            _performance.OnUpdateBeforeDraw();

            if (showOverlayWindow)
                _debugOverlay.Draw();

            if (showDemoWindow)
                ImGui.ShowDemoWindow(ref showDemoWindow);

            if (showDebugWindow)
                _debugWindow.Draw();
        }
    }

}