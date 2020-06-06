using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Assertions;
using Unity.Entities;
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Physics.Engine;

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

        public void Register(Entity entity, string name)
        {
            _flipperToIdx.Add(entity, _flipperDebugData.Length);
            Array.Resize(ref _flipperDebugData, _flipperDebugData.Length + 1);
            _flipperDebugData[_flipperDebugData.Length-1] = new FlipperDebugData(name);            
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

    }
}