using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Physics.Engine;
using ImGuiNET;

using Vector2 = System.Numerics.Vector2;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualPinball.Engine.Unity.ImgGUI
{
    public class BallMonitor : IMonitor
    {
        DebugUI _debugUI;
        List<Entity> _balls = new List<Entity>();
        Entity _lastCreatedBallEntityForManualBallRoller = Entity.Null;
        public int Counter { get { return _balls.Count; } }

        public void Register(Entity entity, string name)
        {
            _balls.Add(entity);
            _lastCreatedBallEntityForManualBallRoller = entity;
        }

        public void OnPhysicsUpdate(double physicClockMilliseconds, int numSteps, float processingTimeMilliseconds)
        {

        }

        public void ManualBallRoller()
        {
            float3 p;
            if (_debugUI.VPE.GetClickCoords(out p))
                EngineProvider<IPhysicsEngine>.Get().BallManualRoll(_lastCreatedBallEntityForManualBallRoller, p);            
        }

        public BallMonitor(DebugUI debugUI)
        {
            _debugUI = debugUI;
        }

        // ==================================================================== Draw in Debug Window ===

        public void OnDebugWindow(DebugWindow dw)
        {
            if (ImGui.TreeNode("BallMonitor", string.Format("Balls ({0})", Counter)))
            {
                if (ImGui.Button("Add ball"))
                {
                    dw.VPE.CreateBall();
                }

#if UNITY_EDITOR
                if (ImGuiExt.InlineButton("Add ball & Pause"))
                {
                    dw.VPE.CreateBall();
                    EditorApplication.isPaused = true;
                }
#endif
                if (ImGuiExt.InlineButton("Add ball in lane"))
                {
                    dw.VPE.CreateBall(900, 1200);
                }
                ImGui.TreePop();
            }
        }
    }
    
}