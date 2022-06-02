namespace Raspberry_Lib.Components
{
    /// <summary>
    /// Interface that when added to a component lets SceneBase know that it wants OnBeginPlay called before the first Update is called
    /// </summary>
    internal interface IBeginPlay
    {
        int BeginPlayOrder { get; }
        void OnBeginPlay();
    }
}
