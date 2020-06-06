using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Physics.Engine;


namespace VisualPinball.Engine.Unity.ImgGUI
{
    public class BallMonitor : IMonitor
    {
        List<Entity> _balls = new List<Entity>();
        Entity _lastCreatedBallEntityForManualBallRoller = Entity.Null;
        Camera camera = null;

        public int Counter {  get { return _balls.Count; } }

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
    }
}