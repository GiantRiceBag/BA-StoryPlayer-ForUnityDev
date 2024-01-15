using System;
using System.Collections.Generic;
using UnityEngine;
namespace BAStoryPlayer.Event
{
    #region Events
    public struct OnStartPlayingStory : IEvent { }
    public struct OnPlayerSelected : IEvent 
    {
        [Obsolete] public int scriptGourpID;
        [Obsolete] public int selectionGroup;
        public string script;
        public List<StoryUnit> storyUnits;
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
