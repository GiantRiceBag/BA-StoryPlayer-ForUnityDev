namespace BAStoryPlayer
{
    /*
    处理等待部分
     */
    public class SubParser_SystemLayer : BSubParser
    {
        public SubParser_SystemLayer(int weight) { this.weight = weight; }

        public override StoryUnit Parse(RawStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            for (int i = 0; i< rawStoryUnit.scriptList.Count; i++)
            {
                string[] args = rawStoryUnit.scriptList[i].script.Split(';');

                switch (rawStoryUnit.scriptList[i].tag)
                {
                    case ScriptTag.Wait:
                        {
                            storyUnit.wait = int.Parse(args[1]);
                            break;
                        }
                    default:continue;
                }
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }

}
