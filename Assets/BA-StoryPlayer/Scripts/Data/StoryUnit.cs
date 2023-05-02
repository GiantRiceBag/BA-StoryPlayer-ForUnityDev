using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    public enum UnitType
    {
        Text = 0,
        Command,
        Option,
        Title,
    }

    public class StoryUnit
    {
        public UnitType type = UnitType.Text;
        public Action action;
        public int wait = 0;

        public void Execute()
        {
            action();
        }
    }

}
