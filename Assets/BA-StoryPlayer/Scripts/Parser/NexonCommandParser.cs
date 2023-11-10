using System.Collections.Generic;
using System.Text.RegularExpressions;
using BAStoryPlayer.UI;
using UnityEngine;

namespace BAStoryPlayer.NexonScriptParser
{
    public enum ScriptTag
    {
        Undefined = 0,
        Title,
        Place,
        NextEpisode,
        Continued,
        Na,
        St,
        Stm,
        ClearSt,
        Wait,
        FontSize,
        All,
        HideMenu,
        ShowMenu,
        Zmc,
        BgShake,
        Video,
        Character,
        CharacterEffect,
        Option
    }

    public class NexonCommandParser : ICommandParser
    {
        private const string REG_TITLE = @"#title;([^;\n]+);?([^;\n]+)?;?";
        private const string REG_PLACE = @"#place;([^;\n]+);?";
        private const string REG_NEXTEPISODE = @"#nextepisode;([^;\n]+);([^;\n]+);?";
        private const string REG_CONTINUED = "#continued;?";
        private const string REG_NA = @"#na;([^;\n]+);?";
        private const string REG_ST = @"#st;(\[-?\d+,-?\d+]);(serial|instant|smooth);(\d+);?";
        private const string REG_STM = @"#stm;(\[0,-?\d+]);(serial|instant|smooth);(\d+);([^;\n]+);?";
        private const string REG_CLEARST = @"#clearST;?";
        private const string REG_WAIT = @"#wait;(\d+);?";
        private const string REG_FONTSIZE = @"#fontsize;(\d+);?";
        private const string REG_ALL = @"#all;([^;\n]+);?";
        private const string REG_HIDEMENU = @"#hidemenu;?";
        private const string REG_SHOWMENU = @"#showmenu;?";
        private const string REG_ZMC = @"#zmc;(instant|instnat|move);(-?\d+,-?\d+);(\d+);?(\d+)?;?";
        private const string REG_BGSHAKE = @"#bgshake;?";
        private const string REG_VIDEO = @"#video;([^;\n]+);([^;\n]+);?";
        private const string REG_CHARACTER = @"^(?!#)([1-5]);([^;\n]+);([^;\n]+);?([^;\n]+)?";
        private const string REG_CHARACTEREFFECT = @"#([1-5]);(((em|fx);([^;\n]+))|\w+);?";
        private const string REG_OPTION = @"\[n?s(\d{0,2})?]([^;\n]+)";

        private BNexonSubParser nextParser;

        public NexonCommandParser()
        {
            BNexonSubParser multimediaLayer = new NexonSubParser_MultimediaLayer(0);
            BNexonSubParser uiLayer = new NexonSubParser_UILayer(4);
            BNexonSubParser characterLayer = new NexonSubParser_CharacterLayer(2);
            BNexonSubParser optionLayer = new NexonSubParser_OptionLayer(3);
            BNexonSubParser systemLayer = new NexonSubParser_SystemLayer(0);

            nextParser = multimediaLayer;
            multimediaLayer.nextParser = uiLayer;
            uiLayer.nextParser = optionLayer;
            optionLayer.nextParser = characterLayer;
            characterLayer.nextParser = systemLayer;
        }

        public List<StoryUnit> Parse(TextAsset rawStoryScript)
        {
            Regex.CacheSize = 20;

            NexonStoryScript storyScript = JsonUtility.FromJson(rawStoryScript.ToString(), typeof(NexonStoryScript)) as NexonStoryScript;
            List<StoryUnit> storyUnits = new List<StoryUnit>();

            // 单元转换
            foreach (var rawUnit in storyScript.content)
            {
                StoryUnit unit = new StoryUnit();
                unit.selectionGroup = rawUnit.selectionGroup;

                // 拆分并标记类型
                foreach(var script in rawUnit.scripts.Split('\n'))
                {
                    var scriptTag = GetScriptTag(script);
                    if (scriptTag != ScriptTag.Undefined)
                    {
                        rawUnit.scriptList.Add(new NexonScriptData(script, scriptTag));
                    }

                }
                rawUnit.scripts = null;
                // 脚本解析为一系列Action
                storyUnits.Add(nextParser.Parse(rawUnit, unit));
            }

            return storyUnits;
        }

        ScriptTag GetScriptTag(string script)
        {
            if (Regex.IsMatch(script, REG_TITLE))
                return ScriptTag.Title;
            if (Regex.IsMatch(script, REG_PLACE))
                return ScriptTag.Place;
            if (Regex.IsMatch(script, REG_NEXTEPISODE))
                return ScriptTag.NextEpisode;
            if (Regex.IsMatch(script, REG_CONTINUED))
                return ScriptTag.NextEpisode;
            if (Regex.IsMatch(script, REG_NA))
                return ScriptTag.Na;
            if (Regex.IsMatch(script, REG_ST))
                return ScriptTag.St;
            if (Regex.IsMatch(script, REG_STM))
                return ScriptTag.Stm;
            if (Regex.IsMatch(script, REG_CLEARST))
                return ScriptTag.ClearSt;
            if (Regex.IsMatch(script, REG_WAIT))
                return ScriptTag.Wait;
            if (Regex.IsMatch(script, REG_FONTSIZE))
                return ScriptTag.FontSize;
            if (Regex.IsMatch(script, REG_ALL))
                return ScriptTag.All;
            if (Regex.IsMatch(script, REG_HIDEMENU))
                return ScriptTag.HideMenu;
            if (Regex.IsMatch(script, REG_SHOWMENU))
                return ScriptTag.ShowMenu;
            if (Regex.IsMatch(script, REG_ZMC))
                return ScriptTag.Zmc;
            if (Regex.IsMatch(script, REG_BGSHAKE))
                return ScriptTag.BgShake;
            if (Regex.IsMatch(script, REG_VIDEO))
                return ScriptTag.Video;
            if (Regex.IsMatch(script, REG_CHARACTER))
                return ScriptTag.Character;
            if (Regex.IsMatch(script, REG_CHARACTEREFFECT))
                return ScriptTag.CharacterEffect;
            if (Regex.IsMatch(script, REG_OPTION))
                return ScriptTag.Option;
            return ScriptTag.Undefined;
        }
    }

    #region SubParser
    public class BNexonSubParser
    {
        protected int weight = 0; // 解析权重
        public BNexonSubParser nextParser;
        public virtual StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null) { return null; }
        public void SetNextParser(BNexonSubParser next) { nextParser = next; }
    }

    /*
     * 处理角色操作以及文本对话部分
     * 脚本:character,na,hide
     */
    public class NexonSubParser_CharacterLayer : BNexonSubParser
    {
        public NexonSubParser_CharacterLayer(int weight) { this.weight = weight; }

        public override StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            // 防止多人说话
            bool isOccupied = false;

            for (int i = 0; i < rawStoryUnit.scriptList.Count; i++)
            {
                string[] args = rawStoryUnit.scriptList[i].script.Split(';');

                switch (rawStoryUnit.scriptList[i].tag)
                {
                    case ScriptTag.Character:
                        // 注意下标减1
                        if (args.Length == 3) // 无台词
                            storyUnit.action += () => {
                                BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(int.Parse(args[0]) - 1, args[1], args[2]);
                            };
                        else if (args.Length == 4) // 有台词
                        {
                            if (isOccupied) continue;
                            storyUnit.UpdateType(UnitType.Text);
                            storyUnit.action += () => {
                                BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(int.Parse(args[0]) - 1, args[1], args[2], args[3]);
                            };
                            isOccupied = true;
                        }
                        break;
                    case ScriptTag.CharacterEffect:
                        switch (args[1])
                        {
                            case "em":
                                HandleEmotion(storyUnit, int.Parse(args[0][1].ToString()) - 1, args[2]);
                                break;
                            case "fx":
                                // TODO
                                break;
                            default:
                                HandleAction(storyUnit, int.Parse(args[0][1].ToString()) - 1, args[1]);
                                break;
                        }
                        break;
                    case ScriptTag.Na:
                        if (isOccupied) continue;
                        storyUnit.action += () => {
                            BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetSpeaker();
                            BAStoryPlayerController.Instance.StoryPlayer.UIModule.PrintMainText(args[1]);
                        };
                        isOccupied = true;
                        storyUnit.UpdateType(UnitType.Text);
                        break;
                    case ScriptTag.All:
                        switch (args[1])
                        {
                            case "hide":
                                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.HideAll(); };
                                break;
                            default: break;
                        }
                        break;
                    default: continue;
                }
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }


        private void HandleEmotion(StoryUnit storyUnit, int characterIndex, string emotionName)
        {
            switch (emotionName)
            {
                case "h":
                case "heart":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Heart); };
                    break;
                case "respond":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Respond); };
                    break;
                case "m":
                case "music":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Music); };
                    break;
                case "k":
                case "twinkle":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Twinkle); };
                    break;
                case "u":
                case "upset":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Upset); };
                    break;
                case "w":
                case "sweat":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sweat); };
                    break;
                case "[...]":
                case "...":
                case "dot":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Dot); };
                    break;
                case "c":
                case "chat":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Chat); };
                    break;
                case "[!]":
                case "!":
                case "exclaim":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Exclaim); };
                    break;
                case "[?!]":
                case "?!":
                case "[!?]":
                case "!?":
                case "surprise":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Surprise); };
                    break;
                case "[?]":
                case "?":
                case "question":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Question); };
                    break;
                case "[///]":
                case "///":
                case "shy":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Shy); };
                    break;
                case "a":
                case "angry":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Angry); };
                    break;
                case "steam":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Steam); };
                    break;
                case "sigh":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sigh); };
                    break;
                case "sad":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sad); };
                    break;
                case "bulb":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Bulb); };
                    break;
                case "zzz":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Zzz); };
                    break;
                case "tear":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Tear); };
                    break;
                case "think":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Think); };
                    break;
                default: return;
            }
        }
        private void HandleAction(StoryUnit storyUnit, int characterIndex, string actionName)
        {
            switch (actionName)
            {
                case "a":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Appear); };
                    break;
                case "d":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper); };
                    break;
                case "dl":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper2Left); };
                    break;
                case "dr":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper2Right); };
                    break;
                case "ar":// 注意实际-1
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.AppearL2R, characterIndex); };
                    break;
                case "al":// 注意实际-1
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.AppearR2L, characterIndex); };
                    break;
                case "hophop":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Hophop); };
                    break;
                case "greeting":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Greeting); };
                    break;
                case "shake":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Shake); };
                    break;
                case "m1": // 注意实际-1
                case "m2":
                case "m3":
                case "m4":
                case "m5":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Move, int.Parse(actionName[1].ToString()) - 1); };
                    break;
                case "stiff":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Stiff); };
                    break;
                case "closeup":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Close); };
                    break;
                case "jump":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Jump); };
                    break;
                case "falldownR":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.falldownR); };
                    break;
                case "hide":
                    storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Hide); };
                    break;
                default: return;
            }
        }
    }

    /*
     * 处理背景音乐,音效等
     * 脚本处理:Video
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

            // 背景音乐及音效
            if (rawStoryUnit.bgmURL != string.Empty)
            {
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.AudioModule.PlayBGM(rawStoryUnit.bgmURL); };
            }

            if (rawStoryUnit.soundURL != string.Empty)
            {
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(rawStoryUnit.soundURL); };
            }

            for (int i = 0; i < rawStoryUnit.scriptList.Count; i++)
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


    /*
     * 处理选项
     */
    public class NexonSubParser_OptionLayer : BNexonSubParser
    {
        /*
         处理选项部分
         */
        public NexonSubParser_OptionLayer(int weight) { this.weight = weight; }

        public override StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            System.Collections.Generic.List<OptionData> datas = new System.Collections.Generic.List<OptionData>();

            for (int i = 0; i < rawStoryUnit.scriptList.Count; i++)
            {
                if (rawStoryUnit.scriptList[i].tag == ScriptTag.Option)
                {
                    string[] args = rawStoryUnit.scriptList[i].script.Substring(1).Split(']');
                    int optionIndex = 0;
                    // 有编号选项
                    if (args[0].Length == 2)
                    {
                        optionIndex = int.Parse((args[0][1]).ToString()); // TODO 这里暂时只支持0-9的选项编号
                        datas.Add(new OptionData(optionIndex, args[1]));
                    }
                    else
                    {
                        // 无编号选项即只有一个选项则不在读取后面的指令
                        datas.Add(new OptionData(optionIndex, args[1]));
                        break;
                    }

                }
            }

            if (datas.Count != 0)
            {
                storyUnit.UpdateType(UnitType.Option);
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowOption(datas); };
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }

    /*
     * 处理等待部分
     */
    public class NexonSubParser_SystemLayer : BNexonSubParser
    {
        public NexonSubParser_SystemLayer(int weight) { this.weight = weight; }

        public override StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            for (int i = 0; i < rawStoryUnit.scriptList.Count; i++)
            {
                string[] args = rawStoryUnit.scriptList[i].script.Split(';');

                switch (rawStoryUnit.scriptList[i].tag)
                {
                    case ScriptTag.Wait:
                        {
                            storyUnit.wait = int.Parse(args[1]);
                            break;
                        }
                    default: continue;
                }
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }

    /*
     * 处理背景变换以及UI部分
     * 脚本处理 : Title,Place,NextEpisode,Continued,Show/HideMenu,BgShake,ClearSt
     */
    public class NexonSubParser_UILayer : BNexonSubParser
    {
        public NexonSubParser_UILayer(int weight)
        {
            this.weight = weight;
        }

        public override StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null)
        {
            if (storyUnit == null)
                storyUnit = new StoryUnit();

            if (rawStoryUnit.backgroundURL != string.Empty)
            {
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.SetBackground(rawStoryUnit.backgroundURL, BackgroundTransistionType.Smooth); };
            }

            for (int i = 0; i < rawStoryUnit.scriptList.Count; i++)
            {
                string[] args = rawStoryUnit.scriptList[i].script.Split(';');

                switch (rawStoryUnit.scriptList[i].tag)
                {
                    case ScriptTag.Title:
                        {
                            if (args.Length == 3)
                                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowTitle(args[1], args[2]); };
                            else
                                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowTitle("", args[1]); };
                            storyUnit.UpdateType(UnitType.Title);
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
                            Debug.Log("没做");
                            break;
                        }
                    case ScriptTag.Continued:
                        {
                            //TODO
                            Debug.Log("没做");
                            break;
                        }
                    case ScriptTag.ShowMenu:
                        {
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActiveButton(true); };
                            break;
                        }
                    case ScriptTag.HideMenu:
                        {
                            storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActiveButton(false); };
                            break;
                        }
                    case ScriptTag.BgShake:
                        {
                            //TODO
                            Debug.Log("没做");
                            break;
                        }
                    case ScriptTag.ClearSt:
                        {
                            storyUnit.action += () => {
                                BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetActiveTextArea(false);
                            };
                            break;
                        }
                    default: continue;
                }
            }

            return nextParser == null ? storyUnit : nextParser.Parse(rawStoryUnit, storyUnit);
        }
    }
    #endregion
}
