namespace BAStoryPlayer.NexonCommandParser
{
    /*
     ����������,��Ч��
    �ű�����:Video
     */
    public class NexonSubParser_MultimediaLayer : BNexonSubParser
    {
        public NexonSubParser_MultimediaLayer(int weight)
        {
            this.weight = weight;
        }

        public override StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            // �������ּ���Ч
            if (rawStoryUnit.bgmURL != string.Empty)
            {
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.AudioModule.PlayBGM(rawStoryUnit.bgmURL); };
            }

            if (rawStoryUnit.soundURL != string.Empty)
            {
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(rawStoryUnit.soundURL); };
            }

            for(int i =  0; i < rawStoryUnit.scriptList.Count; i++)
            {
                switch (rawStoryUnit.scriptList[i].tag)
                {
                    case ScriptTag.Video:
                        {
                            // TODO ������Ƶ Ŀǰ�ݲ���Ҫ
                            break;
                        }
                    default: break;
                }
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }
}

