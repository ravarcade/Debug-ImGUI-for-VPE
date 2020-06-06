using System;
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

namespace VisualPinball.Engine.Unity.ImgGUI
{
    public class PerformanceMonitor
    {
        public FPSHelper Fps = new FPSHelper(true, 0, 100, "n1");
        public FPSHelper PhysicsTicks = new FPSHelper(true, 0, 1500, "F0", 300);
        public ChartFloat PhysicsTimes = new ChartFloat(100, 0.0f, 20.0f, 50);

        float _phyTimeAccu = 0;

        public void OnPhysicsUpdate(double physicClockMilliseconds, int numSteps, float processingTimeMilliseconds)
        {
            PhysicsTicks.Tick(numSteps);
            _phyTimeAccu += processingTimeMilliseconds;
        }

        public void OnUpdateBeforeDraw()
        {
            Fps.Tick();
            PhysicsTimes.Add(_phyTimeAccu);
            _phyTimeAccu = 0.0f;
        }
    }
}