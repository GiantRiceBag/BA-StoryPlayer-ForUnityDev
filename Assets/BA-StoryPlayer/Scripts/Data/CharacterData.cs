using UnityEngine;
using Spine.Unity;
using Spine;

namespace BAStoryPlayer
{
    public enum LoadType
    {
        Prefab = 0,
        SkeletonData
    }

    [System.Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("建议以角色罗马音作为主要索引名")]
        public string indexName;
        [HideInInspector] public string familyName;
        public string name;
        [HideInInspector] public string collage;
        public string affiliation;
        [Space]
        public LoadType loadType = LoadType.SkeletonData;
        public string skelUrl;
        [HideInInspector] public string portraitUrl;
        [Space]
        [Tooltip("仅在载入类型为 'SkeletonData' 时有效")]
        public Vector2 facePosition;
            
        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append($"索引名 {indexName}\n");
            result.Append($"姓名 {familyName}{name}\n");
            result.Append($"学院 {collage}\n");
            result.Append($"所属社团 {affiliation}");
            return result.ToString() ;
        }
    }

    [CreateAssetMenu(menuName = "BAStoryPlayer/角色信息表",fileName = "CharacterDataTable")]
    [SerializeField]
    public class CharacterData : ScriptableObject
    {
        [SerializeField]
        System.Collections.Generic.List<CharacterDataUnit> Datas = new System.Collections.Generic.List<CharacterDataUnit>();

        public CharacterDataUnit this[string indexName]
        {
            get
            {
                foreach(var i in Datas)
                {
                    if (i.indexName == indexName)
                        return i;
                }
                Debug.LogError($"未能在查询表中找到 角色 [{indexName}] 的数据");
                return null;
            }
        }

        public void Print()
        {
            foreach(var i in Datas)
            {
                Debug.Log(i.ToString());
            }
        }

        private void OnValidate()
        {
            foreach(var chrData in Datas)
            {
                if (chrData.loadType == LoadType.Prefab)
                    continue;

                SkeletonDataAsset skelDataAsset = Resources.Load<SkeletonDataAsset>(BAStoryPlayerController.Instance.Setting.Path_Prefab
                    + chrData.skelUrl);

                if (skelDataAsset == null) return;
                SkeletonData skelData = skelDataAsset.GetSkeletonData(false);

                if (skelDataAsset == null || skelData == null)
                    continue;

                Skeleton skel = new Skeleton(skelData);

                int count = 0;
                Vector2 sum = Vector2.zero;

                float skelScale = 1f /skelDataAsset.scale;
                BoneData rootBoneData = skel.RootBone.Data;
                Vector2 rootPos = new Vector2(rootBoneData.X, rootBoneData.Y) * skelScale;

                foreach (var kvPair in skelData.DefaultSkin.Attachments)
                {
                    SlotData slotData = skelData.Slots.Items[kvPair.Key.SlotIndex];
                    Slot slot = skel.Slots.Items[kvPair.Key.SlotIndex];
                    if (slotData.Name != ("00") && slotData.Name != ("00_default")) // Fuck nexon
                        continue;

                    float boneScaleX = slotData.BoneData.ScaleX * skelScale;
                    float boneScaleY = slotData.BoneData.ScaleY * skelScale;

                    Vector2 offset = rootPos + new Vector2(slotData.BoneData.X * skelScale, slotData.BoneData.Y * skelScale); // Fuck nexon

                    Vector2[] buffer = new Vector2[((VertexAttachment)kvPair.Value).WorldVerticesLength];

                    foreach (var v2 in ((VertexAttachment)kvPair.Value).GetLocalVertices(slot, buffer))
                    {
                        if (v2 == Vector2.zero)
                            continue;

                        sum += Vector2.Scale(v2,new Vector2(boneScaleX,boneScaleY)) + offset; ; // Fuck nexon
                        count++;
                    }
                }

                chrData.facePosition = count == 0 ? Vector2.zero : sum / count;
            }
        }
    }
}

