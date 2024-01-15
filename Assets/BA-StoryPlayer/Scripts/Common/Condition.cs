using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    public enum RelationalOperators
    {
        Less,
        LessEqual,
        Equal,
        NotEqual,
        Greater,
        GreaterEqual
    }

    public class Condition
    {
        public string flag;
        public RelationalOperators operation;
        public int value;

        public Condition(string flag, RelationalOperators operation, int value)
        {
            this.flag = flag;
            this.operation = operation;
            this.value = value;
        }

        public bool Validate(Dictionary<string, int> flagTable)
        {
            if (flagTable == null || !flagTable.ContainsKey(flag))
            {
                return false;
            }

            switch (operation)
            {
                case RelationalOperators.Less:
                    return flagTable[flag] < value;
                case RelationalOperators.LessEqual:
                    return flagTable[flag] <= value;
                case RelationalOperators.Equal:
                    return flagTable[flag] == value;
                case RelationalOperators.NotEqual:
                    return flagTable[flag] != value;
                case RelationalOperators.Greater:
                    return flagTable[flag] > value;
                case RelationalOperators.GreaterEqual:
                    return flagTable[flag] >= value;
            }

            return false;
        }
    }
}