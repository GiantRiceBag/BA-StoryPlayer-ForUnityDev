using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    public abstract class CommandParser
    {
        protected BAStoryPlayer storyPlayer;
        public BAStoryPlayer StoryPlayer => storyPlayer;

        public CommandParser(BAStoryPlayer storyPlayer) 
        {
            this.storyPlayer = storyPlayer;
        }

        public virtual List<StoryUnit> Parse(TextAsset rawStoryScriptTextAsset)
        {
            return Parse(rawStoryScriptTextAsset.text);
        }
        public abstract List<StoryUnit> Parse(string rawStoryScriptJson);
    }
}
