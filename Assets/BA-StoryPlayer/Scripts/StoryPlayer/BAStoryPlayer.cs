using UnityEngine;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        public bool Auto;

        [SerializeField]
        CharacterManager _characterManager;
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

        // Start is called before the first frame update
        void Start()
        {
            // TODO Test
            //CharacterManager.ActivateCharacter(2, "hoshino", "00",TransistionType.Smooth);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
