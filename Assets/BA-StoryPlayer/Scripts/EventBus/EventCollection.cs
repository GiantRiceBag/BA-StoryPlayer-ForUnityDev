using UnityEngine;
namespace BAStoryPlayer.Event
{
    #region Events
    public struct OnPlayerSelect : IEvent 
    {
        public int scriptGourpID;
        public int selectionGroup;
    }
    public struct OnPlayerCancelAuto : IEvent { }
    public struct OnCloseStoryPlayer : IEvent { }

    public struct OnAnimateCharacter : IEvent
    {
        public float time;
    }

    public struct OnStartPrintLine : IEvent { }
    public struct OnFinishPrintLine : IEvent { }
    #endregion

    static class EventCollection { }
}
