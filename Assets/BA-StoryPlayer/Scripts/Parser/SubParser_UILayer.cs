namespace BAStoryPlayer
{
    /*
     �������任�Լ�UI����
    �ű����� : Title,Place,NextEpisode,Continued,Show/HideMenu,BgShake,ClearSt
     */
    public class SubParser_UILayer : BSubParser
    {
        public SubParser_UILayer(int weight)
        {
            this.weight = weight;
        }

        public override StoryUnit Parse(RawStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            if (rawStoryUnit.backgroundURL != string.Empty)
            {
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.SetBackground(rawStoryUnit.backgroundURL); };
            }

            for(int i = 0; i < rawStoryUnit.scriptList.Count; i++) {
                string[] args = rawStoryUnit.scriptList[i].script.Split(';');

                switch (rawStoryUnit.scriptList[i].tag)
                {
                    case ScriptTag.Title:
                        {
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowTitle(args[1], args[2]); };
                            storyUnit.UpdateType(weight, UnitType.Title);
                            break;
                        }
                    case ScriptTag.Place:
                        {
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowVenue(args[1]); };
                            break;
                        }
                    case ScriptTag.NextEpisode:
                        {
                            //TODO
                            UnityEngine.Debug.Log("û��");
                            break;
                        }
                    case ScriptTag.Continued:
                        {
                            //TODO
                            UnityEngine.Debug.Log("û��");
                            break;
                        }
                    case ScriptTag.ShowMenu:
                        {
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActive_UI_Button(true); };
                            break;
                        }
                    case ScriptTag.HideMenu:
                        {
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActive_UI_Button(false); };
                            break;
                        }
                    case ScriptTag.BgShake:
                        {
                            //TODO
                            UnityEngine.Debug.Log("û��");
                            break;
                        }
                    case ScriptTag.ClearSt:
                        {
                            BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActive_UI_TextArea(false);
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActive_UI_Button(false); };
                            break;
                        }
                    default:continue;
                }
            }


            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }

}
