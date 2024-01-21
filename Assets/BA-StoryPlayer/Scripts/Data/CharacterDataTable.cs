using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Spine.Unity;
using Spine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BAStoryPlayer
{
    public enum LoadType
    {
        SkeletonData,
        Prefab
    }

    public interface ICharacterDataTableInternal
    {
        public List<CharacterDataUnit> List {  get; }
        public Dictionary<string,CharacterDataUnit> Dictionary {  get; }


#if UNITY_EDITOR
        public void AddEditor(CharacterDataUnit data);
        public void ClearEditor();
        public void Reflash();
#endif
    }

    [Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("建议以角色罗马音作为主要索引名")] public string indexName;
        [HideInInspector, Obsolete] public string familyName;
        [HideInInspector, Obsolete] public string firstName;
        [HideInInspector, Obsolete] public string collage;
        [HideInInspector, Obsolete] public string affiliation;
        [Space]
        public LoadType loadType;
        public string skeletonDataUrl;
        [HideInInspector, Obsolete] public string portraitUrl;
        [Space]
        [Tooltip("仅在载入类型为 'SkeletonData' 时有效")] public Vector2 facePosition;
    }

    [CreateAssetMenu(menuName = "BAStoryPlayer/角色信息表",fileName = "CharacterDataTable")]
    [Serializable]
    public class CharacterDataTable : ScriptableObject,IEnumerable<CharacterDataUnit>, ICharacterDataTableInternal
    {
        [SerializeField] private List<CharacterDataUnit> _list = new List<CharacterDataUnit>();
        private Dictionary<string, CharacterDataUnit> _dictionary = new();
        private List<CharacterDataUnit> _runtimeUnits = new();

        public IReadOnlyList<CharacterDataUnit> List => _list;
        public IReadOnlyDictionary<string, CharacterDataUnit> Dictionary => _dictionary;

        List<CharacterDataUnit> ICharacterDataTableInternal.List => _list;
        Dictionary<string, CharacterDataUnit> ICharacterDataTableInternal.Dictionary => _dictionary;

        public CharacterDataUnit this[int index]
        {
            get => List[index];
        }
        public CharacterDataUnit this[string indexName]
        {
            get
            {
                if (!Dictionary.ContainsKey(indexName))
                {
                    TryReloadDictionary();
                }
                try
                {
                    return Dictionary[indexName];
                }
                catch
                {
                    Debug.LogError($"未能在查询表中找到 角色 [{indexName}] 的数据");
                    return null;
                }
            }
        }

        public IEnumerator<CharacterDataUnit> GetEnumerator()
        {
            return List.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnEnable()
        {
            TryReloadDictionary();
        }

        private void TryReloadDictionary()
        {
            _dictionary.Clear();
            foreach (var chrData in List)
            {
                _dictionary.Add(chrData.indexName, chrData);
            }
        }

        public bool ContainsKey(string key) => Dictionary.ContainsKey(key);

        public void AddRuntimeUnit(string indexName,SkeletonGraphic skeletonGraphic)
        {
            if (Dictionary.ContainsKey(indexName))
            {
                return;
            }

            CharacterDataUnit unit = new()
            {
                indexName = indexName,
                loadType = LoadType.SkeletonData,
                facePosition = GetFacePosition(skeletonGraphic.SkeletonDataAsset)
            };

            _list.Add(unit);
            _runtimeUnits.Add(unit);
            _dictionary.Add(indexName, unit);
        }

        public void ClearRuntimeUnits()
        {
            foreach (var unit in _runtimeUnits)
            {
                _list.Remove(unit);
                _dictionary.Remove(unit.indexName);
            }
            _runtimeUnits.Clear();
        }

        public Vector2 GetFacePosition(SkeletonDataAsset skeletonDataAsset)
        {
            SkeletonData skelData = skeletonDataAsset.GetSkeletonData(false);
            if (skelData == null)
            {
                return Vector2.zero;
            }

            Skeleton skel = new Skeleton(skelData);

            int count = 0;
            Vector2 sum = Vector2.zero;

            float skelScale = 1f / skeletonDataAsset.scale;
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
                if (!validSlotName.Any(x => x == slotData.Name)) // Fuck nexon
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

                    sum += Vector2.Scale(v2, new Vector2(boneScaleX, boneScaleY)) + offset; ; // Fuck nexon
                    count++;
                }
            }

            if (count == 0)
                return Vector2.zero;

            return sum / count;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _dictionary.Clear();

            foreach (var chrData in _list)
            {
                _dictionary.Add(chrData.indexName, chrData);

                if (chrData.loadType == LoadType.Prefab || chrData.skeletonDataUrl == string.Empty)
                {
                    continue;
                }

                SkeletonDataAsset skelDataAsset = null;

                string[] guids = AssetDatabase.FindAssets(chrData.skeletonDataUrl.Split('/').Last());
                if(guids.Length > 0)
                {
                    skelDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }

                if (skelDataAsset == null) 
                {
                    continue;
                }

                SkeletonData skelData = skelDataAsset.GetSkeletonData(false);
                if (skelData == null)
                {
                    continue;
                }

                Vector2 facePosition = GetFacePosition(skelDataAsset);
                chrData.facePosition = facePosition;
            }
        }

        public void ClearEditor()
        {
            _list.Clear();
            _dictionary.Clear();
        }
        public void AddEditor(CharacterDataUnit data)
        {
            if (!_dictionary.ContainsKey(data.indexName))
            {
                _list.Add(data);
                _dictionary.Add(data.indexName, data);
            }
        }
        public void Reflash()
        {
            OnValidate();
        }
#endif
    }
}

