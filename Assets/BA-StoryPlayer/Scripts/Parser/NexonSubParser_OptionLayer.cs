using BAStoryPlayer.UI;
namespace BAStoryPlayer.NexonCommandParser
{
    public class NexonSubParser_OptionLayer : BNexonSubParser
    {
        /*
         ����ѡ���
         */
       public NexonSubParser_OptionLayer(int weight) { this.weight = weight; }

        public override StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
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
                    // �б��ѡ��
                    if (args[0].Length == 2)
                    {
                        optionIndex = int.Parse((args[0][1]).ToString()); // TODO ������ʱֻ֧��0-9��ѡ����
                        datas.Add(new OptionData(optionIndex, args[1]));
                    }
                    else
                    {
                        // �ޱ��ѡ�ֻ��һ��ѡ�����ڶ�ȡ�����ָ��
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
