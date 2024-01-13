using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        public List<RawUniversalStoryUnit> content;

#if UNITY_EDITOR
        public override string ToString() 
        { 
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

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

        public void Print()
        {
            Debug.Log(ToString());
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
    public class RawUniversalStoryUnit
    {
        public long id;
        public string type;
        public string backgroundImage;
        public string bgmId;
        public string speaker;
        public string affiliation;
        public string text;
        public List<RawUniversalStoryCharacterUnit> characters;
        public string command;
        public List<string> commandArgs;

        public int selectionGroup;
        public List<string> condition; // TODO 适配BranchTable
        // public List<RawUniversalStoryUnit> content; // 选项组子内容 TODO 要重构

#if UNITY_EDITOR
        public override string ToString()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            stringBuilder.AppendLine($"id   [{id}]");
            stringBuilder.AppendLine($"type   [{type}]");
            stringBuilder.AppendLine($"backgroundIamge   [{backgroundImage}]");
            stringBuilder.AppendLine($"bgmId   [{bgmId}]");
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
    public class RawUniversalStoryCharacterUnit
    {
        public string uuid;
        public string name;
        public int position;
        public string face;
        public string emotion;
        public string action;
        public int actionArgs;
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
            stringBuilder.AppendLine($"filter   [{filter}]");

            return stringBuilder.ToString();
        }
#endif
    }
}

