using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer.Utility
{
    public static class ArgsParser
    {
        public static bool TryParse<T>(string arg, out T arg1) where T : IComparable
        {
            arg1 = default(T);
            if (!TryConvert(arg, out T res1))
            {
                return false;
            }

            arg1 = res1;
            return true;
        }
        public static bool TryParse<T>(List<string> args,out T arg1) where T : IComparable
        {
            arg1 = default(T);

            if (args == null || args.Count < 1)
            {
                return false;
            }

            if (!TryConvert(args[0],out T res1))
            {
                return false;
            }

            arg1 = res1;
            return true;
        }
        public static bool TryParse<T,K>(List<string> args, out T arg1,out K arg2) where T : IComparable where K : IComparable
        {
            arg1 = default(T);
            arg2 = default(K);

            if (args == null || args.Count < 2)
            {
                return false;
            }

            if (!TryConvert(args[0], out T res1))
            {
                return false;
            }
            if (!TryConvert(args[1], out K res2))
            {
                return false;
            }

            arg1 = res1;
            arg2 = res2;
            return true;
        }
        public static bool TryParse<T,K,U>(List<string> args, out T arg1, out K arg2,out U arg3) where T : IComparable where K: IComparable where U : IComparable
        {
            arg1 = default(T);
            arg2 = default(K);
            arg3 = default(U);

            if (args == null || args.Count < 3)
            {
                return false;
            }

            if (!TryConvert(args[0], out T res1))
            {
                return false;
            }
            if (!TryConvert(args[0], out K res2))
            {
                return false;
            }
            if (!TryConvert(args[0], out U res3))
            {
                return false;
            }

            arg1 = res1;
            arg2 = res2;
            arg3 = res3;
            return true;
        }

        private static bool TryConvert<T>(string arg,out T res) where T : IComparable
        {
            try
            {
                if(typeof(T).IsEnum)
                {
                    res = (T)Enum.Parse(typeof(T), arg);
                }
                else
                {
                    res = (T)Convert.ChangeType(arg, typeof(T));
                }
                return true;
            }
            catch
            {
                res = default(T);
                return false;
            }
        }
    }
}
