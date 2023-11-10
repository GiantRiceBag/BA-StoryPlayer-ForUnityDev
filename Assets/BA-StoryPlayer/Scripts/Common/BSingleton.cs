using UnityEngine;

namespace BAStoryPlayer
{
    public class BSingleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;
    protected bool _enable;

    public static T Instance
    {
        get
        {
            _instance = FindObjectOfType<T>();
            if(_instance == null)
            {
                GameObject go = new GameObject(typeof(T).ToString());
                _instance = go.AddComponent<T>();

                    if (typeof(T).Name.Equals("BAStoryPlayerController"))
                    {
                        go.AddComponent<RectTransform>();
                        go.AddComponent<Canvas>();
                        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
                        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1920, 1080);
                        scaler.matchWidthOrHeight = 1;
                    }
                }

             return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if(_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            _enable = true;
        }
        else
        {
            if (this != _instance)
                Destroy(gameObject);
        }
    }
}

}
