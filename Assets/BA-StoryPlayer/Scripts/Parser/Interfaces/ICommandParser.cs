using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    public interface ICommandParser
    {
        public List<StoryUnit> Parse(TextAsset rawStoryScript);
    }
}
