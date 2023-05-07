using BAStoryPlayer.UI;
namespace BAStoryPlayer
{
    public class SubParser_OptionLayer : BSubParser
    {
        /*
         处理选项部分
         */
       public SubParser_OptionLayer(int weight) { this.weight = weight; }

        public override StoryUnit Parse(RawStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            System.Collections.Generic.List<OptionData> datas = new System.Collections.Generic.List<OptionData>();

            for (int i = 0; i < rawStoryUnit.scriptList.Count; i++)
            {
                if(rawStoryUnit.scriptList[i].tag == ScriptTag.Option)
                {
                    string[] args = rawStoryUnit.scriptList[i].script.Substring(1).Split(']');
                    int optionIndex = 0;
                    // 有编号选项
                    if (args[0].Length == 2)
                        optionIndex = int.Parse((args[0][1]).ToString());
                    else
                    {
                        // 无编号选项即只有一个选项则不在读取后面的指令
                        datas.Add(new OptionData(optionIndex, args[1]));
                        break;
                    }

                }
            }

            if(datas.Count != 0)
            {
                storyUnit.UpdateType(weight, UnitType.Option);
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowOption(datas); };
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }

}
