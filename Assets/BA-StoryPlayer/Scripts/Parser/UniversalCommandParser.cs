using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer.UniversalCommandParser
{
    public class UniversalCommandParser : ICommandParser
    {
        private BAStoryPlayer StoryPlayer => BAStoryPlayerController.Instance.StoryPlayer;

        public List<StoryUnit> Parse(TextAsset rawStoryScript)
        {
            List<StoryUnit> storyUnits = new List<StoryUnit>();

            UniversalStoryScript storyScript = JsonUtility.FromJson<UniversalStoryScript>(rawStoryScript.text);

            StoryPlayer.UIModule.SetTitle();
            StoryPlayer.UIModule.SetSynopsis(storyScript.description);

            foreach(var rawStoryUnit in storyScript.content)
            {
                StoryUnit storyUnit = null;
                switch(rawStoryUnit.type)
                {
                    case "title":
                        storyUnit = HandleTitleUnit(rawStoryUnit);
                        break;
                    case "place":
                        storyUnit = HandlePlaceUnit(rawStoryUnit);
                        break;
                    case "bgm":
                        storyUnit = HandleBgmUnit(rawStoryUnit);
                        break;
                    case "text":
                        storyUnit = HandleTextUnit(rawStoryUnit);
                        break;
                    case "select":
                        storyUnit = HandleSelectUnit(rawStoryUnit);
                        break;
                    case "option":
                        storyUnit = HandleOptionUnit(rawStoryUnit);
                        break;
                    case "st":
                        storyUnit = HandleStUnit(rawStoryUnit);
                        break;
                    case "effectOnly":
                        storyUnit = HandleEffectOnlyUnit(rawStoryUnit);
                        break;
                    case "continue":
                        storyUnit = HandleContinueUnit(rawStoryUnit);
                        break;
                    default:
                        storyUnit = new StoryUnit();
                        break;
                }

                HandleUnitCommand(rawStoryUnit, ref storyUnit);
                HandleCommonSetting(rawStoryUnit, ref storyUnit);

                if(storyUnit != null)
                {
                    storyUnits.Add(storyUnit);
                }
            }
            return storyUnits;
        }

        #region Common Unit Handler
        private void HandleCharacterUnit(RawUniversalStoryUnit rawStoryUnit, ref StoryUnit storyUnit)
        {
            foreach (var characterUnit in rawStoryUnit.characters)
            {
                string indexName = characterUnit.name;
                int characterIndex = characterUnit.position - 1;

                storyUnit.action += () => StoryPlayer.CharacterModule.ActivateCharacter(characterIndex, indexName, characterUnit.face);

                if (characterUnit.emotion != null)
                {
                    switch (characterUnit.emotion)
                    {
                        case "h":
                        case "heart":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Heart); };
                            break;
                        case "respond":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Respond); };
                            break;
                        case "m":
                        case "music":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Music); };
                            break;
                        case "k":
                        case "twinkle":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Twinkle); };
                            break;
                        case "u":
                        case "upset":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Upset); };
                            break;
                        case "w":
                        case "sweat":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sweat); };
                            break;
                        case "[...]":
                        case "...":
                        case "dot":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Dot); };
                            break;
                        case "c":
                        case "chat":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Chat); };
                            break;
                        case "[!]":
                        case "!":
                        case "exclaim":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Exclaim); };
                            break;
                        case "[?!]":
                        case "?!":
                        case "[!?]":
                        case "!?":
                        case "surprise":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Surprise); };
                            break;
                        case "[?]":
                        case "?":
                        case "question":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Question); };
                            break;
                        case "[///]":
                        case "///":
                        case "shy":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Shy); };
                            break;
                        case "a":
                        case "angry":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Angry); };
                            break;
                        case "steam":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Steam); };
                            break;
                        case "sigh":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sigh); };
                            break;
                        case "sad":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sad); };
                            break;
                        case "bulb":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Bulb); };
                            break;
                        case "zzz":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Zzz); };
                            break;
                        case "tear":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Tear); };
                            break;
                        case "think":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Think); };
                            break;
                        default:
                            break;
                    }
                }

                if (characterUnit.action != null)
                {
                    switch (characterUnit.action)
                    {
                        case "a":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Appear); };
                            break;
                        case "d":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper); };
                            break;
                        case "dl":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper2Left); };
                            break;
                        case "dr":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper2Right); };
                            break;
                        case "ar":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.AppearL2R, characterUnit.actionArgs - 1); };
                            break;
                        case "al":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.AppearR2L, characterUnit.actionArgs - 1); };
                            break;
                        case "hophop":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Hophop); };
                            break;
                        case "greeting":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Greeting); };
                            break;
                        case "shake":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Shake); };
                            break;
                        case "move":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Move, characterUnit.actionArgs - 1); };
                            break;
                        case "stiff":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Stiff); };
                            break;
                        case "closeup":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Close); };
                            break;
                        case "jump":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Jump); };
                            break;
                        case "falldownL": // TODO 后面动作补齐
                        case "falldownR":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.falldownR); };
                            break;
                        case "hide":
                            storyUnit.action += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Hide); };
                            break;
                        default: 
                            break;
                    }
                }

                if (characterUnit.filter != null)
                {
                    // TODO
                }
            }
        }

        private void HandleUnitCommand(RawUniversalStoryUnit rawStoryUnit,ref StoryUnit storyUnit)
        {
            List<int> args = new List<int>();
            foreach(var rawArg in rawStoryUnit.commandArgs)
            {
                args.Add(int.Parse(rawArg));
            }

            switch (rawStoryUnit.command)
            {
                case "wait":
                    storyUnit.wait = args[0];
                    break;
                case "setFlag":
                    // TODO
                    break;
                case "manipulateFlag":
                    break;
                case "clearST":
                    break;
                case "hideAll":
                    break;
                case "bgShake":
                    break;
                case "popupFile":
                    break;
                case "BG_Filter_Red":
                    break;
                case "BG_Wave_F":
                    break;
                case "BG_Flash":
                    break;
                case "BG_UnderFire_R":
                    break;
                case "BG_Love_L":
                    break;
                case "BG_Rain_L":
                    break;
                case "BG_UnderFire":
                    break;
                case "BG_SandStorm_L":
                    break;
                case "BG_Shining_L":
                    break;
                case "BG_Dust_L":
                    break;
                case "BG_Flash_Sound":
                    break;
                case "BG_FocusLine":
                    break;
                case "BG_Ash_Red":
                    break;
                case "BG_Snow_L":
                    break;
            }
        }

        // 处理一些脚本单元公共配置
        private void HandleCommonSetting(RawUniversalStoryUnit rawStoryUnit, ref StoryUnit storyUnit)
        {
            if(rawStoryUnit.backgroundImage != string.Empty)
            {
                storyUnit.action += () => StoryPlayer.SetBackground(rawStoryUnit.backgroundImage,BackgroundTransistionType.Smooth);
            }
        }
        #endregion

        #region Unit Handler
        private StoryUnit HandleTitleUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();
            storyUnit.UpdateType(UnitType.Title);

            StoryPlayer.UIModule.SetTitle(rawStoryUnit.text);
            storyUnit.action += () => StoryPlayer.UIModule.ShowTitle("", rawStoryUnit.text);

            return storyUnit;
        }

        private StoryUnit HandlePlaceUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            storyUnit.action += () => StoryPlayer.UIModule.ShowVenue(rawStoryUnit.text);

            return storyUnit;
        }

        private StoryUnit HandleBgmUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            storyUnit.action += () => StoryPlayer.AudioModule.PlayBGM(rawStoryUnit.bgmId);

            return storyUnit;
        }

        private StoryUnit HandleTextUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();
            storyUnit.UpdateType(UnitType.Text);

            storyUnit.action += () => StoryPlayer.UIModule.SetSpeaker(rawStoryUnit.speaker, rawStoryUnit.affiliation);
            storyUnit.action += () => StoryPlayer.UIModule.PrintMainText(rawStoryUnit.text);

            if(rawStoryUnit.characters.Count > 0)
            {
                HandleCharacterUnit(rawStoryUnit, ref storyUnit);
            }

            return storyUnit;
        }

        private StoryUnit HandleSelectUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }

        private StoryUnit HandleOptionUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }

        private StoryUnit HandleStUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }

        private StoryUnit HandleEffectOnlyUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }

        private StoryUnit HandleContinueUnit(RawUniversalStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }
        #endregion
    }
}
