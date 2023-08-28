using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Spine;

namespace BAStoryPlayer
{
    public enum LoadType
    {
        Prefab = 0, // Legacy
        SkeletonData
    }

    [System.Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("�����Խ�ɫ��������Ϊ��Ҫ������")] public string indexName;
        [HideInInspector] public string familyName;
        public string name;
        [HideInInspector] public string collage;
        public string affiliation;
        [Space]
        public LoadType loadType = LoadType.SkeletonData;
        public string skelUrl;
        [HideInInspector] public string portraitUrl;
        [Space]
        [Tooltip("������������Ϊ 'SkeletonData' ʱ��Ч")] public Vector2 facePosition;
            
        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append($"������ {indexName}\n");
            result.Append($"���� {familyName}{name}\n");
            result.Append($"ѧԺ {collage}\n");
            result.Append($"�������� {affiliation}");
            return result.ToString() ;
        }
    }

    [CreateAssetMenu(menuName = "BAStoryPlayer/��ɫ��Ϣ��",fileName = "CharacterDataTable")]
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
                Debug.LogError($"δ���ڲ�ѯ�����ҵ� ��ɫ [{indexName}] ������");
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
                if (FindObjectOfType<BAStoryPlayerController>() == null)
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

                string[] validSlotName = new string[4]; // Fuck nexon
                validSlotName[0] = "00";
                validSlotName[1] = "00_default";
                validSlotName[2] = "defalt";
                validSlotName[3] = "default";

                foreach (var kvPair in skelData.DefaultSkin.Attachments)
                {
                    SlotData slotData = skelData.Slots.Items[kvPair.Key.SlotIndex];
                    Slot slot = skel.Slots.Items[kvPair.Key.SlotIndex];
                    if (!validSlotName.Any(x=>x == slotData.Name)) // Fuck nexon
                        continue;

                    float boneScaleX = slotData.BoneData.ScaleX * skelScale;
                    float boneScaleY = slotData.BoneData.ScaleY * skelScale;

                    Vector2 offset = rootPos + new Vector2(slotData.BoneData.X * skelScale, slotData.BoneData.Y * skelScale); // Fuck nexon

                    VertexAttachment vertAttachment = kvPair.Value as VertexAttachment; // Fuck nexon
                    if (vertAttachment == null)
                        continue;

                    Vector2[] buffer = new Vector2[vertAttachment.WorldVerticesLength];

                    foreach (var v2 in vertAttachment.GetLocalVertices(slot, buffer))
                    {
                        if (v2 == Vector2.zero)
                            continue;

                        sum += Vector2.Scale(v2,new Vector2(boneScaleX,boneScaleY)) + offset; ; // Fuck nexon
                        count++;
                    }
                }

                if (count == 0)
                    continue;

                chrData.facePosition = sum / count;
            }
        }
    }
}

