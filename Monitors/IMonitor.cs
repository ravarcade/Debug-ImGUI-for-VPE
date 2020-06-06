using Unity.Entities;


/// <summary>
/// Generic interface to monitor objects.
/// </summary>
internal interface IMonitor
{
    void Register(Entity entity, string name);
    void OnPhysicsUpdate(double physicClockMilliseconds, int numSteps, float processingTimeMilliseconds);
}