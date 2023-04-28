using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : Panel
{
    public MixLevels soundSettings;

    public Toggle subtitleToggle;
    public Toggle closedCaptions;

    private void OnEnable()
    {
        subtitleToggle.SetIsOnWithoutNotify(UserSettingsSystem.ShowSubtitle);
        closedCaptions.SetIsOnWithoutNotify(UserSettingsSystem.ShowClosedCaptions);
    }
}
