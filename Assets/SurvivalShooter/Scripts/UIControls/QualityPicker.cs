using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QualityPicker : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown dropdown;
    [SerializeField]
    private QualityDescriptor[] qualityOptions;

    [System.Serializable]
    public struct QualityDescriptor
    {
        public int qualityIndex;
        public string name;
    }

    private void Start()
    {
        dropdown.ClearOptions();
        List<string> tempOptions = new();
        foreach (var desc in qualityOptions)
        {
            tempOptions.Add(desc.name);
        }
        dropdown.AddOptions(tempOptions);

        // set current level
        dropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
    }

    public void SetQualitySetting(int index)
    {
        int realIndex = qualityOptions[index].qualityIndex;
        QualitySettings.SetQualityLevel(realIndex);
        dropdown.SetValueWithoutNotify(realIndex);
    }
}
