using UnityEngine;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        public bool Auto;

        [SerializeField]
        CharacterManager _characterManager;
        [SerializeField]
        UIManager _UIManager;
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

        void Start()
        {
            // TODO Test
            CharacterManager.ActivateCharacter(0, "hoshino", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(1, "aru", "00", TransistionType.Smooth);
            CharacterManager.ActivateCharacter(2, "serika_shibaseki", "00");
            //CharacterManager.ActivateCharacter(3, "shiroko", "00", TransistionType.Smooth);
            CharacterManager.ActivateCharacter(4, "kayoko", "00", TransistionType.Smooth);
        }


    }

}
