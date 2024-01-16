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
    public struct OnCanceledAuto : IEvent { }

    public struct OnClosedStoryPlayer : IEvent { }
    public struct OnUnlockedPlayerInput : IEvent { }

    public struct OnSetCharacterAction : IEvent
    {
        public float time;
    }

    public struct OnStartPrintingMainText : IEvent { }
    public struct OnFinishedPrintingMainText : IEvent { }
    #endregion
}
