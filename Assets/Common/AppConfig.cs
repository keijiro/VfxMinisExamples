using UnityEngine;

public sealed class AppConfig : MonoBehaviour
{
    void Start()
    {
#if !UNITY_EDITOR && UNITY_IOS
        Application.targetFrameRate = 60;
#endif
    }
}
