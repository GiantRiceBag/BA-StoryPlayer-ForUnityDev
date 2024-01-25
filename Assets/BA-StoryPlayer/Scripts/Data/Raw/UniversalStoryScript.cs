using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BAStoryPlayer
{
    /*
     * 伟大的头给出的新版格式 这里为了纪念头要🔒一下它的🐂🐂
     */
    [Serializable]
    public class UniversalStoryScript
    {
        public string serial;
        public string uuid;
        public string description;
        public List<RawStoryUnit> content;

#if UNITY_EDITOR
        public override string ToString() 
        { 
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"serial [{serial}]");
            stringBuilder.AppendLine($"uuid [{uuid}]");
            stringBuilder.AppendLine($"description [{description}]");
            stringBuilder.AppendLine($"*****unit*****");
            foreach (var unit in content)
            {
                stringBuilder.AppendLine(unit.ToString());
            }

            return stringBuilder.ToString();
        }
        public void ToJsonFile(string url)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.dataPath + $"/{url}");
            sw.Write(ToJson());
            sw.Close();
        }
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
#endif
    }

    [Serializable]
    public class RawStoryUnitBase
    {
        public long id;
        public string type;
        public string backgroundImage;
        public string bgm;
        public string speaker;
        public string affiliation;
        public string text;
        public string script;

        public List<RawStoryCharacterUnit> characters;
        public string command;
        public List<string> commandArgs;

        public RawStoryUnit AsRawStoryUnit()
        {
            return new RawStoryUnit()
            {
                id = this.id,
                type = this.type,
                backgroundImage = this.backgroundImage,
                bgm = this.bgm,
                speaker = this.speaker,
                affiliation = this.affiliation,
                text = this.text,
                characters = this.characters,
                command = this.command,
                commandArgs = this.commandArgs,
                script = this.script,
            };
        }
    }

    [Serializable]
    public class RawStoryUnit : RawStoryUnitBase
    {
        public List<RawSelectionGroup> selectionGroups;

#if UNITY_EDITOR
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"id   [{id}]");
            stringBuilder.AppendLine($"type   [{type}]");
            stringBuilder.AppendLine($"backgroundIamge   [{backgroundImage}]");
            stringBuilder.AppendLine($"bgmId   [{bgm}]");
            stringBuilder.AppendLine($"speaker   [{speaker}]");
            stringBuilder.AppendLine($"affiliation   [{affiliation}]");
            stringBuilder.AppendLine($"text   [{text}]");
            foreach(var chr in characters)
            {
                stringBuilder.AppendLine($"*****character {id}*****");
                stringBuilder.Append(chr.ToString());
            }

            return stringBuilder.ToString();
        }
#endif
    }

    [Serializable]
    public class RawStoryCharacterUnit
    {
        public string uuid;
        public string name;
        public int position;
        public string face;
        public string emotion;
        public string action;
        public int actionArgs;
        public bool highlight;
        public string filter;

#if UNITY_EDITOR
        public override string ToString()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            stringBuilder.AppendLine($"uuid   [{uuid}]");
            stringBuilder.AppendLine($"name   [{name}]");
            stringBuilder.AppendLine($"position   [{position}]");
            stringBuilder.AppendLine($"face   [{face}]");
            stringBuilder.AppendLine($"emotion   [{emotion}]");
            stringBuilder.AppendLine($"action   [{action}]");
            stringBuilder.AppendLine($"actionArgs   [{actionArgs}]");
            stringBuilder.AppendLine($"highlight [{highlight}]");
            stringBuilder.AppendLine($"filter   [{filter}]");

            return stringBuilder.ToString();
        }
#endif
    }

    [Serializable]
    public class RawSelectionGroup
    {
        public long id;
        public string type;
        public string text;
        public bool isConditional;
        public List<RawCondition> conditions;
        [HideInInspector] public List<RawStoryUnitBase> content;
        public string script;
    }

    [Serializable]
    public class RawCondition
    {
        public string flag;
        public string operation;
        public int value;
    }
}