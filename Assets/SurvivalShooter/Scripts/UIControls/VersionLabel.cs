using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionLabel : MonoBehaviour
{
    public Text label;
    public string formatter = "ver. {0}";

    private void Start()
    {
        label.text = string.Format(formatter, Application.version);
    }
}
