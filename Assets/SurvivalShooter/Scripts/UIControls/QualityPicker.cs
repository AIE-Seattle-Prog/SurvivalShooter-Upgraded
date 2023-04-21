using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QualityPicker : MonoBehaviour
{
    public List<Toggle> toggles = new List<Toggle>();

    private void OnEnable()
    {
        int qualityLevel = QualitySettings.GetQualityLevel();
        if (qualityLevel <= toggles.Count)
        {
            toggles[qualityLevel].SetIsOnWithoutNotify(true);
        }
        else
        {
            Debug.LogWarning("Unknown quality level - is this for a specific platform?");
        }
    }

    public void SetQualitySetting(int index)
    {
        QualitySettings.SetQualityLevel(index);
        UserSettingsSystem.GraphicsPreset = index;
    }
}
