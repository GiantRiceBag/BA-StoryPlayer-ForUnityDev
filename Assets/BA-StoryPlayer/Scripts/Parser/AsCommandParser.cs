using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.IO;
using BAStoryPlayer.UI;

/*
 * source : https://github.com/Tualin14/ArisStudio
 */

namespace BAStoryPlayer.AsScriptParser
{
    public struct AsCommandContainer
    {
        public string type;
        public string value;
    }

    public class AsCommandParser : ICommandParser
    {
        readonly Dictionary<string, AsCommandContainer> nameIdList = new Dictionary<string, AsCommandContainer>();

        public AsCommandParser() { }

        public List<StoryUnit> Parse(TextAsset storyScript)
        {
            List<StoryUnit> storyUnits = new List<StoryUnit>();
            List<string> asCommandList = new List<string>(storyScript.ToString().Split('\n'));
            asCommandList.RemoveAll(string.IsNullOrEmpty);
            asCommandList = asCommandList.Where(item => !item.StartsWith("//")).ToList();

            StoryUnit storyUnit = new StoryUnit();
            int selectionGroup = 0;

            foreach(var commandLine in asCommandList)
            {
                string textCmd = commandLine;

                if(textCmd.Length == 1)
                {
                    storyUnit = new StoryUnit();
                    storyUnit.selectionGroup = selectionGroup;
                    storyUnits.Add(storyUnit);
                    continue;
                }

                string[] command = AsCommand.Parse(textCmd);
                if (nameIdList.ContainsKey(command[0]))
                    textCmd = $"{nameIdList[command[0]].type} {textCmd}";
                command = AsCommand.Parse(textCmd);

                switch (command[0])
                {
                    case "load":
                        PreLoad(commandLine);
                        break;
                    case "target":
                        // digit only
                        selectionGroup = int.Parse(Regex.Replace(command[1], "[^0-9]", ""));
                        break;
                    case "alias":
                        break;

                    // special commands
                    case "wait":
                        storyUnit.wait = int.Parse(command[1]) * 1000;
                        break;
                    case "targets":
                        break;
                    case "jump":
                        break;
                    case "auto":
                        break;
                    case "switch":
                        break;

                    // select commands
                    case "select":
                    case "button":
                        HandleSelectionCommand(command,storyUnit);
                        break;

                    // dialogue commands
                    case "txt": // Legacy
                    case "t":
                    case "mt":
                    case "middle_text":
                    case "bt":
                    case "bottom_text":
                    case "text":
                    case "tc":
                    case "mtc":
                    case "btc":
                        storyUnit.UpdateType(UnitType.Text);
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetSpeaker(command[1]);
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.PrintMainText(command[3]);
                        break;

                    case "th":
                    case "thc":
                        storyUnit.UpdateType(UnitType.Text);
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetSpeaker(command[1]);
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.PrintMainText(command[3]);
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.Highlight(command[1]);
                        break;

                    // components commands
                    case "label":
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowVenue(command[1]);
                        break;

                    case "banner":
                        storyUnit.UpdateType(UnitType.Title);
                        if (command.Length == 2)
                            storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowTitle("", command[1]);
                        else if (command.Length == 3)
                            storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowTitle(command[1], command[2]);
                        break;

                    // scene commands
                    case "screen": // Legacy
                    case "scene":
                        break;

                    // image commands
                    case "bg":
                        // 仅限bg
                        HandleImageCommand(command, storyUnit);
                        break;
                    case "mg":
                    case "fg":
                    case "image":
                        break;
                    // audio commands
                    case "bgm":
                    case "sfx":
                    case "audio":
                        HandleAudioCommand(command, storyUnit);
                        break;

                    // character commands
                    case "spr":
                    case "char":
                        HandleCharacterCommand(command,storyUnit);
                        break;
                }
            }

            return storyUnits;
        }

        /// <summary>
        /// Run corresponding functions for each command that start with "load".
        /// </summary>
        /// <param name="loadCommand"></param>
        public void PreLoad(string loadCommand)
        {
            /*
            * Examples:
            * load spr hifumi hifumi_spr
            * load bg Classroom Classroom.jpg
            * load bgm MainTheme theme.ogg
            */

            string[] asCommand = AsCommand.Parse(loadCommand);

            // spr, bg, bgm...
            switch (asCommand[1])
            {
                // Character
                case "spr": // default spr 
                    nameIdList.Add(asCommand[2], new AsCommandContainer() { 
                        type = "char",
                        value = Path.GetFileNameWithoutExtension(asCommand[3])
                    });
                    break;
                case "sprC": // Legacy
                case "sprc": // communicate spr
                case "spr_c":
                    nameIdList.Add(asCommand[2], new AsCommandContainer()
                    {
                        type = "char",
                        value = Path.GetFileNameWithoutExtension(asCommand[3])
                    });
                    break;
                // Image
                case "bg":
                case "mg":
                case "fg":
                    nameIdList.Add(asCommand[2], new AsCommandContainer()
                    {
                        type = "bg",
                        value = Path.GetFileNameWithoutExtension(asCommand[3])
                    });
                    break;
                // Audio
                case "bgm":
                    nameIdList.Add(asCommand[2], new AsCommandContainer()
                    {
                        type = "bgm",
                        value = Path.GetFileNameWithoutExtension(asCommand[3])
                    });
                    break;
                case "sfx":
                    nameIdList.Add(asCommand[2], new AsCommandContainer()
                    {
                        type = "sfx",
                        value = Path.GetFileNameWithoutExtension(asCommand[3])
                    });
                    break;
            }
        }

        void HandleCharacterCommand(string[] command,StoryUnit storyUnit)
        {
            string indexName = command[1];

            switch (command[2])
            {
                // Character state
                case "show":
                    storyUnit.action += ()=> BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(indexName, CharacterAction.Appear);
                    break;
                case "hide":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(indexName, CharacterAction.Disapper);
                    break;
                case "showD": // legacy
                case "appear":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.ActivateCharacter(indexName);
                    break;
                case "hideD": // legacy
                case "disappear":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(indexName, CharacterAction.Hide);
                    break;

                case "hl":
                case "highlight":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.Highlight(indexName);
                    break;

                case "fade":
                    // TODO: character fade
                    break;

                case "state":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAnimation(indexName, command[3]);
                    break;
                case "skin":
                    break;
                case "emo":
                case "emotion":
                    HandleEmotion(storyUnit, indexName, command[3]);
                    break;

                case "anim":
                case "animation":
                    break;

                // Character Movement
                // 目前只接受 -10 -5 0 5 10这5个参数 对应原槽位 0-4 的位置
                case "x":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.MoveCharacterTo(indexName, (int.Parse(command[3]) + 10) / 5);
                    break;
                case "y":
                    break;
                case "z":
                    break;
                case "p":
                case "pos":
                case "position":
                    break;

                case "moveX": // legacy
                case "xm":
                case "move":
                case "move_x":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(indexName,CharacterAction.Move, (int.Parse(command[3]) + 10) / 5);
                    break;
                case "moveY": // legacy
                case "ym":
                case "move_y":
                    break;
                case "pm":
                case "move_pos":
                case "move_position":
                    break;

                case "shakeX": // legacy
                case "xs":
                case "shake_x":
                    break;
                case "shakeY": // legacy
                case "ys":
                case "shake_y":
                    break;
                case "shake":
                    break;

                case "scale":
                    break;

                case "close":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(indexName, CharacterAction.Close);
                    break;

                case "back":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetAction(indexName, CharacterAction.Back);
                    break;
            }
        }
        void HandleImageCommand(string[] command,StoryUnit storyUnit)
        {
            string imgUrl = nameIdList[command[1]].value;

            switch (command[2])
            {
                // Image State
                case "show":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.SetBackground(imgUrl,BackgroundTransistionType.Smooth);
                    break;
                case "hide":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.SetBackground(null, BackgroundTransistionType.Smooth);
                    break;
                case "showD": // Legacy
                case "appear":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.SetBackground(imgUrl, BackgroundTransistionType.Instant);
                    break;
                case "hideD": // Legacy
                case "disappear":
                    storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.SetBackground(null, BackgroundTransistionType.Instant);
                    break;
                case "hl":
                case "highlight":
                case "fade":
                case "x":
                case "y":
                case "z":
                case "p":
                case "pos":
                case "position":
                case "moveX": // legacy
                case "xm":
                case "move":
                case "move_x":
                case "moveY": // legacy
                case "ym":
                case "move_y":
                case "pm":
                case "move_pos":
                case "move_position":
                case "shakeX": // legacy
                case "xs":
                case "shake_x":
                case "shakeY": // legacy
                case "ys":
                case "shake_y":
                    break;
                case "shake":
                    break;

                case "scale":
                    break;
            }
        }
        void HandleAudioCommand(string[] command,StoryUnit storyUnit)
        {
            string url = nameIdList[command[1]].value;
            if(command[0] == "bgm")
                switch (command[2])
                {
                    case "play":
                        storyUnit.action += ()=> BAStoryPlayerController.Instance.StoryPlayer.AudioModule.PlayBGM(url);
                        break;
                    case "pause":
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.AudioModule.PauseBGM();
                        break;
                    case "stop":
                        break;

                    case "v":
                    case "volume":
                        break;

                    case "fade":
                        break;

                    case "loop":
                        break;
                    case "once":
                        break;
                }
            else
                switch (command[2])
                {
                    case "play":
                        storyUnit.action += () => BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(url);
                        break;
                    case "pause":
                        break;
                    case "stop":
                        break;

                    case "v":
                    case "volume":
                        break;

                    case "fade":
                        break;

                    case "loop":
                        break;
                    case "once":
                        break;
                }
        }
        void HandleSelectionCommand(string[] command,StoryUnit storyUnit)
        {
            List<OptionData> datas = new System.Collections.Generic.List<OptionData>();

            for(int i = 1; i + 1 < command.Length; i+=2)
            {
                OptionData data = new OptionData();
                data.text = command[i];
                data.optionID = int.Parse(Regex.Replace(command[i + 1], "[^0-9]", ""));
                datas.Add(data);
            }

            if (datas.Count != 0)
            {
                storyUnit.UpdateType(UnitType.Option);
                storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.UIModule.ShowOption(datas); };
            }
        }

        void HandleEmotion(StoryUnit storyUnit, string indexName, string emotionName)
        {
            switch (emotionName)
            {
                case "Heart":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Heart); };
                        break;
                case "Respond":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Respond); };
                        break;
                case "Note":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Music); };
                        break;
                case "Twinkle":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Twinkle); };
                        break;
                case "Anxiety ":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Upset); };
                        break;
                case "Sweat":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Sweat); };
                        break;
                case "Idea":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Dot); };
                        break;
                case "Chat":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Chat); };
                        break;
                case "E":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Exclaim); };
                        break;
                case "EQ":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Surprise); };
                        break;
                case "Q":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Question); };
                        break;
                case "Shy":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Shy); };
                        break;
                case "Aggro":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Angry); };
                        break;
                case "Steam":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Steam); };
                        break;
                case "Sigh":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Sigh); };
                        break;
                case "Sad":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Sad); };
                        break;
                case "Bulb":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Bulb); };
                        break;
                case "Zzz":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Zzz); };
                        break;
                case "Tear":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Tear); };
                        break;
                case "Think":
                        storyUnit.action += () => { BAStoryPlayerController.Instance.StoryPlayer.CharacterModule.SetEmotion(indexName, CharacterEmotion.Think); };
                        break;
                default: return;
            }
        }
    }

    public static class AsCommand
    {
        /// <summary>
        /// Parse a string of command.
        /// </summary>
        /// <param name="textCommand"></param>
        /// <returns>An array of string of those command.</returns>
        public static string[] Parse(string textCommand)
        {
            /*
            * Split command with space delimiter then return it as array.
            *
            * You can write a word without using single or double quote.
            * Example: txt Arona Hello
            *
            * But, you must use either single or double quote if
            * you write a sentence that have a 'Space' character.
            * Example: txt Arona 'Hello, Sensei.'
            *
            * You can write a single ( ' ) or double ( " ) quote character
            * in a word or a sentence by escaping the character...
            * Example: txt Arona 'It\'s yummy.'
            * or use the double quote as delimiter...
            * Example: txt Arona "It's yummy."
            * and vice versa.
            *
            * Source: txt 'Arona' "Arona" "I'm, Arona." 'Say, "Hello"' 'You\'re'
            * Result: txt | 'Arona' | "Arona" | "I'm, Arona." | 'Say, "Hello"' | 'You\'re'
            */

            const string pattern = @"('[^'\\]*(?:\\.[^'\\]*)*')|(\""[^""\\]*(?:\\.[^""\\]*)*"")|(\S+)";
            String[] splittedCommand = Regex
                .Matches(textCommand, pattern)
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();

            /*
            * Normalize splitted text by removing either single or
            * double quote at the start and the end of the word.
            * Also Unescape escaped character.
            *
            * Using string.Trim() resulting in some unexpected result,
            * so we reconstruct it using string.Substring().
            *
            * Source: txt 'Arona' "Arona" "I'm, Arona." 'Say, "Hello"' 'You\'re'
            * Result: txt | Arona | Arona | I'm, Arona. | Say, "Hello" | You're
            */

            List<string> finalCommand = new List<string>();

            foreach (String item in splittedCommand)
            {
                String cmd = item; // Clone the text first, so we can do other operations

                // If the current text is "// (comment)", exit iteration.
                if (cmd.Trim() == "//")
                    break;

                if ((item.StartsWith("'") && item.EndsWith("'")) || (item.StartsWith("\"") && item.EndsWith("\"")))
                {
                    cmd = item.Substring(1, item.Length - 2); // Reconstruct the text
                    cmd = Regex.Unescape(cmd); // Unescape escaped character in text
                }

                finalCommand.Add(cmd);
            }

            return finalCommand.ToArray();
        }
    }
}

