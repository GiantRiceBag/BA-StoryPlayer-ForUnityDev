using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    public enum FlagOperator
    {
        add,
        subtract,
        setOpposite,
        assign
    }

    public static class FlagOperation
    {
        public static bool Handle(Dictionary<string,int> flagTable,string flagName,FlagOperator oper,int value)
        {
            if(flagTable == null || !flagTable.ContainsKey(flagName))
            {
                return false;
            }

            switch (oper)
            {
                case FlagOperator.add:
                    flagTable[flagName] += value;
                    break;
                case FlagOperator.subtract:
                    flagTable[flagName] -= value;
                    break;
                case FlagOperator.setOpposite:
                    flagTable[flagName] = -flagTable[flagName];
                    break;
                case FlagOperator.assign:
                    flagTable[flagName] = value;
                    break;
            }

            return true;
        }
    }
}