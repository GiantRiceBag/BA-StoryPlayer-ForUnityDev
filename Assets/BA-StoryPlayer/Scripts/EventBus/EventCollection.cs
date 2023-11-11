using UnityEngine;
namespace BAStoryPlayer.Event
{
    #region Events
    public struct OnPlayerSelectedBranch : IEvent 
    {
        public int scriptGourpID;
        public int selectionGroup;
    }
    public struct OnPlayerCanceledAuto : IEvent { }

    public struct OnClosedStoryPlayer : IEvent { }
    public struct OnUnlockedPlayerInput : IEvent { }

    public struct OnAnimatedCharacter : IEvent
    {
        public float time;
    }

    public struct OnPrintingLine : IEvent { }
    public struct OnPrintedLine : IEvent { }
    #endregion
}
