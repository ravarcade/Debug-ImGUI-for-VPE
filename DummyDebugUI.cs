using Unity.Entities;
using VisualPinball.Unity.Physics.DebugUI;
using VisualPinball.Unity.VPT.Table;


namespace VisualPinball.Engine.Unity.ImgGUI
{

    /// <summary>
    /// Dummy DebugUI implementation used to disable DebugUI.
    /// </summary>
    public class DummyDebugUI : IDebugUI
    {
        public string Name => "- Disabled -";
        public void Init(TableBehavior tableBehavior) { }
        public void OnPhysicsUpdate(double physicClockMilliseconds, int numSteps, float processingTimeMilliseconds) { }
        public void OnRegisterFlipper(Entity entity, string name) { }
        public void OnCreateBall(Entity entity) { }

        public int AddProperty<T>(int parentIdx, string name, T currentValue, string tip) { return 0; }
        public bool GetProperty<T>(int propIdx, ref T val) { return false; }
        public void SetProperty<T>(int propIdx, T value) { }
    }

}