using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSlider : MonoBehaviour
{
    public Slider slider;
    public Text valueText;
    public string valueTextFormatter;
    // TODO - support a text field too!

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(SetValueWithoutNotify);
        valueText.text = slider.value.ToString(valueTextFormatter);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(SetValueWithoutNotify);
    }

    public void SetValueWithoutNotify(float newValue)
    {
        slider.SetValueWithoutNotify(newValue);
        valueText.text = newValue.ToString(valueTextFormatter);
    }

    public float Value
    {
        get => slider.value;
        set
        {
            slider.value = value;
            valueText.text = value.ToString(valueTextFormatter);
        }
    }
}
