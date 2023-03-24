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
        toggles[QualitySettings.GetQualityLevel()].SetIsOnWithoutNotify(true);
    }

    public void SetQualitySetting(int index)
    {
        QualitySettings.SetQualityLevel(index); 
    }
}
