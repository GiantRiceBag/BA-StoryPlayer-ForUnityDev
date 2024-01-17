using System;
using System.Collections.Generic;
using UnityEngine;

using BAStoryPlayer.UI;
using BAStoryPlayer.Utility;

namespace BAStoryPlayer.Parser.UniversaScriptParser
{
    public class UniversalCommandParser : CommandParser
    {
        public UniversalCommandParser(BAStoryPlayer storyPlayer) : base(storyPlayer) { }

        public override List<StoryUnit> Parse(TextAsset rawStoryScript)
        {
            List<StoryUnit> storyUnits = new List<StoryUnit>();
            UniversalStoryScript storyScript = JsonUtility.FromJson<UniversalStoryScript>(rawStoryScript.text);

            StoryPlayer.UIModule.SetTitle();
            StoryPlayer.UIModule.SetSynopsis(storyScript.description);

            foreach(var rawStoryUnit in storyScript.content)
            {
                StoryUnit storyUnit = ProcessRawStoryUnit(rawStoryUnit);

                if(storyUnit != null)
                {
                    storyUnits.Add(storyUnit);
                }
            }
            return storyUnits;
        }
        private StoryUnit ProcessRawStoryUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = null;

            switch (rawStoryUnit.type)
            {
                case "title":
                    storyUnit = HandleTitleUnit(rawStoryUnit);
                    break;
                case "place":
                    storyUnit = HandlePlaceUnit(rawStoryUnit);
                    break;
                case "text":
                    storyUnit = HandleTextUnit(rawStoryUnit);
                    break;
                case "select":
                    storyUnit = HandleSelectUnit(rawStoryUnit);
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

            storyUnit.scripts = rawStoryUnit.script;

            HandleUnitCommand(rawStoryUnit, storyUnit);
            HandleCommonSetting(rawStoryUnit, storyUnit);

            return storyUnit;
        }
        private StoryUnit ProcessRawStoryUnit(RawStoryUnitBase rawStoryUnitBase)
        {
            return ProcessRawStoryUnit(rawStoryUnitBase.AsRawStoryUnit());
        }

        #region Common Unit Handler
        private void HandleCharacterUnit(RawStoryUnit rawStoryUnit, StoryUnit storyUnit)
        {
            foreach (var characterUnit in rawStoryUnit.characters)
            {
                string indexName = characterUnit.name;
                int characterIndex = characterUnit.position - 1;

                storyUnit.actions += () => StoryPlayer.CharacterModule.ActivateCharacter(characterIndex, indexName, characterUnit.face);

                if (characterUnit.emotion != null)
                {
                    switch (characterUnit.emotion)
                    {
                        case "h":
                        case "heart":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Heart); };
                            break;
                        case "respond":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Respond); };
                            break;
                        case "m":
                        case "music":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Music); };
                            break;
                        case "k":
                        case "twinkle":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Twinkle); };
                            break;
                        case "u":
                        case "upset":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Upset); };
                            break;
                        case "w":
                        case "sweat":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sweat); };
                            break;
                        case "[...]":
                        case "...":
                        case "dot":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Dot); };
                            break;
                        case "c":
                        case "chat":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Chat); };
                            break;
                        case "[!]":
                        case "!":
                        case "exclaim":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Exclaim); };
                            break;
                        case "[?!]":
                        case "?!":
                        case "[!?]":
                        case "!?":
                        case "surprise":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Surprise); };
                            break;
                        case "[?]":
                        case "?":
                        case "question":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Question); };
                            break;
                        case "[///]":
                        case "///":
                        case "shy":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Shy); };
                            break;
                        case "a":
                        case "angry":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Angry); };
                            break;
                        case "steam":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Steam); };
                            break;
                        case "sigh":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sigh); };
                            break;
                        case "sad":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Sad); };
                            break;
                        case "bulb":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Bulb); };
                            break;
                        case "zzz":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Zzz); };
                            break;
                        case "tear":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Tear); };
                            break;
                        case "think":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetEmotion(characterIndex, CharacterEmotion.Think); };
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
                        case "appear":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Appear); };
                            break;
                        case "d":
                        case "disappear":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper); };
                            break;
                        case "dl":
                        case "disappearLeft":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper2Left); };
                            break;
                        case "dr":
                        case "disappearRight":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Disapper2Right); };
                            break;
                        case "ar":
                        case "appearToRight":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.AppearL2R, characterIndex); };
                            break;
                        case "al":
                        case "appearToLeft":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.AppearR2L, characterIndex); };
                            break;
                        case "hophop":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Hophop); };
                            break;
                        case "greeting":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Greeting); };
                            break;
                        case "shake":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Shake); };
                            break;
                        case "move":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Move, characterUnit.actionArgs - 1); };
                            break;
                        case "stiff":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Stiff); };
                            break;
                        case "closeup":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Close); };
                            break;
                        case "jump":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Jump); };
                            break;
                        case "falldownL":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.falldownL); };
                            break;
                        case "falldownR":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.falldownR); };
                            break;
                        case "hide":
                            storyUnit.actions += () => { StoryPlayer.CharacterModule.SetAction(characterIndex, CharacterAction.Hide); };
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
        private void HandleUnitCommand(RawStoryUnit rawStoryUnit, StoryUnit storyUnit)
        {
            switch (rawStoryUnit.command)
            {
                case "wait":
                    {
                        if(ArgsParser.TryParse(rawStoryUnit.commandArgs,out int waitTime))
                        {
                            storyUnit.wait = waitTime;
                        }
                        break;
                    }
                case "setFlag":
                    {
                        if(ArgsParser.TryParse(rawStoryUnit.commandArgs, out string flagName,out FlagOperator flagOperator,out int value))
                        {
                            if (FlagOperation.Handle(StoryPlayer.FlagTable, flagName, flagOperator, value))
                            {
                                if (!StoryPlayer.ModifiedFlagTable.ContainsKey(flagName))
                                {
                                    StoryPlayer.ModifiedFlagTable.Add(flagName, value);
                                }
                                else
                                {
                                    StoryPlayer.ModifiedFlagTable[flagName] = value;
                                }
                            }
                        }
                        break;
                    }
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
        //  处理一些脚本单元公共配置 音频及背景等
        private void HandleCommonSetting(RawStoryUnit rawStoryUnit, StoryUnit storyUnit)
        {
            if(rawStoryUnit.backgroundImage != string.Empty)
            {
                storyUnit.actions += () => StoryPlayer.BackgroundModule.SetBackground(rawStoryUnit.backgroundImage,TransistionType.Fade);
            }
            if(rawStoryUnit.bgm != string.Empty) 
            {
                storyUnit.actions += () => StoryPlayer.AudioModule.PlayBGM(rawStoryUnit.bgm,false);
            }
        }
        #endregion

        #region Unit Handler
        private StoryUnit HandleTitleUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();
            storyUnit.UpdateType(UnitType.Title);

            string[] titles = rawStoryUnit.text.Split(';');

            if (titles.Length == 2)
            {
                StoryPlayer.UIModule.SetTitle(titles[1]);
                storyUnit.actions += () => StoryPlayer.UIModule.ShowTitle(titles[0], titles[1]);
            }
            else
            {
                StoryPlayer.UIModule.SetTitle(rawStoryUnit.text);
                storyUnit.actions += () => StoryPlayer.UIModule.ShowTitle("", rawStoryUnit.text);
            }

            return storyUnit;
        }
        private StoryUnit HandlePlaceUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            storyUnit.actions += () => StoryPlayer.UIModule.ShowVenue(rawStoryUnit.text);

            return storyUnit;
        }
        private StoryUnit HandleTextUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();
            storyUnit.UpdateType(UnitType.Text);

            storyUnit.actions += () => StoryPlayer.UIModule.SetSpeaker(rawStoryUnit.speaker, rawStoryUnit.affiliation);
            storyUnit.actions += () => StoryPlayer.UIModule.PrintMainText(rawStoryUnit.text);

            if(rawStoryUnit.characters.Count > 0)
            {
                HandleCharacterUnit(rawStoryUnit, storyUnit);
            }

            return storyUnit;
        }
        private StoryUnit HandleSelectUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();
            storyUnit.UpdateType(UnitType.Option);

            List<OptionData> options = new();

            foreach (RawSelectionGroup selection in rawStoryUnit.selectionGroups)
            {
                List<StoryUnit> optionStoryUnits = new();
                List<Condition> optionConditions = new();

                foreach(RawStoryUnitBase selectionStoryUnit in selection.content)
                {
                    optionStoryUnits.Add(ProcessRawStoryUnit(selectionStoryUnit));
                }
                foreach(RawCondition optionCondition in selection.conditions)
                {
                    if (Enum.TryParse(typeof(RelationalOperators), optionCondition.operation,out var operRes))
                    {
                        optionConditions.Add(
                            new Condition(
                                optionCondition.flag, 
                                (RelationalOperators)operRes,
                                optionCondition.value)
                            );
                    }
                }

                options.Add(new OptionData(selection.text,selection.script, optionStoryUnits,optionConditions));
            }

            if(options.Count > 0)
            {
                storyUnit.actions += () =>
                {
                    StoryPlayer.UIModule.ShowOptions(options);
                };
            }

            return storyUnit;
        }

        private StoryUnit HandleStUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }
        private StoryUnit HandleEffectOnlyUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }
        private StoryUnit HandleContinueUnit(RawStoryUnit rawStoryUnit)
        {
            StoryUnit storyUnit = new StoryUnit();

            // TODO

            return storyUnit;
        }
        #endregion
    }
}
