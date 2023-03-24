using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformActivation : MonoBehaviour
{
    public List<RuntimePlatform> SupportedPlatforms;

    private void Awake()
    {
        RuntimePlatform currentPlatform = Application.platform;
#if UNITY_ANDROID
        currentPlatform = RuntimePlatform.Android;
#elif UNITY_IOS
        currentPlatform = RuntimePlatform.IPhonePlayer;
#endif
        if (!SupportedPlatforms.Contains(currentPlatform)) { gameObject.SetActive(false); }
    }
}
