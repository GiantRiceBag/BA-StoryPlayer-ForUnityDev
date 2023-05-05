using System.Text.RegularExpressions;

namespace BAStoryPlayer{
    /*
     处理背景音乐,音效等
    脚本处理:Video
     */
    public class SubParser_MultimediaLayer : BSubParser
    {
        public SubParser_MultimediaLayer(int weight)
        {
            this.weight = weight;
        }

        public override StoryUnit Parse(RawStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            // 背景音乐及音效
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
                            // TODO 播放视频 目前暂不需要
                            break;
                        }
                    default: break;
                }
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }
}

