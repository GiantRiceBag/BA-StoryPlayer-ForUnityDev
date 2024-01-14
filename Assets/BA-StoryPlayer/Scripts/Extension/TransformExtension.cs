using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    public static class TransformExtension
    {
        public static void ClearAllChild(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static bool IsDescendantOf(this Transform transform, Transform target)
        {
            Transform current = transform.parent;
            while(current != null)
            {
                if(current == target)
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }
    }
}
