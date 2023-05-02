using UnityEngine;
using UnityEngine.Events;
using System;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        public bool Auto;

        [SerializeField]
        CharacterManager _characterManager;
        [SerializeField]
        UIManager _UIManager;
        int currentGroupID = -1;

        public CharacterManager CharacterManager
        {
            get
            {
                if (_characterManager == null)
                    _characterManager = transform.GetComponentInChildren<CharacterManager>();
                return _characterManager;
            }
        }
        public UIManager UIManager
        {
            get
            {
                if (_UIManager == null)
                    _UIManager = transform.GetComponentInChildren<UIManager>();
                return _UIManager;
            }
        }
        public int GroupID
        {
            get
            {
                return currentGroupID;
            }
        }

        public UnityEvent<int,int> OnPlayerSelect;


        

        void Start()
        {
            // TODO Test
            //CharacterManager.ActivateCharacter(0, "hoshino", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(1, "aru", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(2, "serika_shibaseki", "00");
            //CharacterManager.ActivateCharacter(3, "shiroko", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(4, "kayoko", "00", TransistionType.Smooth);

            OnPlayerSelect.AddListener ((id,groupID) =>
            {
                Debug.Log($"玩家进行了选择{id},组ID{groupID}");
            });
        }
    }

}
