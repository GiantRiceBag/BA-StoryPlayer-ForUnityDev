using System.Text.RegularExpressions;

namespace BAStoryPlayer
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

    public class MasterParser
    {
        const string REG_TITLE = @"#title;([^;\n]+);?([^;\n]+)?;?";
        const string REG_PLACE = @"#place;([^;\n]+);?";
        const string REG_NEXTEPISODE = @"#nextepisode;([^;\n]+);([^;\n]+);?";
        const string REG_CONTINUED = "#continued;?";
        const string REG_NA = @"#na;([^;\n]+);?";
        const string REG_ST = @"#st;(\[-?\d+,-?\d+]);(serial|instant|smooth);(\d+);?";
        const string REG_STM = @"#stm;(\[0,-?\d+]);(serial|instant|smooth);(\d+);([^;\n]+);?";
        const string REG_CLEARST = @"#clearST;?";
        const string REG_WAIT = @"#wait;(\d+);?";
        const string REG_FONTSIZE = @"#fontsize;(\d+);?";
        const string REG_ALL = @"#all;([^;\n]+);?";
        const string REG_HIDEMENU = @"#hidemenu;?";
        const string REG_SHOWMENU = @"#showmenu;?";
        const string REG_ZMC = @"#zmc;(instant|instnat|move);(-?\d+,-?\d+);(\d+);?(\d+)?;?";
        const string REG_BGSHAKE = @"#bgshake;?";
        const string REG_VIDEO = @"#video;([^;\n]+);([^;\n]+);?";
        const string REG_CHARACTER = @"^(?!#)([1-5]);([^;\n]+);([^;\n]+);?([^;\n]+)?";
        const string REG_CHARACTEREFFECT = @"#([1-5]);(((em|fx);([^;\n]+))|\w+);?";
        const string REG_OPTION = @"\[n?s(\d{0,2})?]([^;\n]+)";

        StoryUnit storyUnit;
        BSubParser nextParser;

        public MasterParser()
        {
            // 解析器组装
            BSubParser multimediaLayer = new SubParser_MultimediaLayer(0);
            BSubParser uiLayer = new SubParser_UILayer(4);
            BSubParser characterLayer = new SubParser_CharacterLayer(2);
            BSubParser optionLayer = new SubParser_OptionLayer(3);
            BSubParser systemLayer = new SubParser_SystemLayer(0);

            nextParser = multimediaLayer;
            multimediaLayer.nextParser = uiLayer;
            uiLayer.nextParser = optionLayer;
            optionLayer.nextParser = characterLayer;
            characterLayer.nextParser = systemLayer;
        }

        public System.Collections.Generic.List<StoryUnit> Parse(StoryScript storyScript)
        {
            Regex.CacheSize = 20;

            System.Collections.Generic.List<StoryUnit> storyUnits = new System.Collections.Generic.List<StoryUnit>();

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
                        rawUnit.scriptList.Add(new ScriptData(script, scriptTag));
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

}
