using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthWidget : PlayerWidget
{

    [Header("Settings")]
    [SerializeField]
    private float activeAlpha = 1.0f;
    [SerializeField]
    private float inactiveAlpha = 0.0125f;

    [Header("Elements")]
    public Slider healthSlider;
    public Image healthIcon;
    public CanvasGroup widgetGroup;

    protected override void OnBindToPlayer(PlayerController player)
    {
        base.OnBindToPlayer(player);

        widgetGroup.alpha = activeAlpha;

        OwningPlayer.Character.health.OnHealthChanged.AddListener(HandlePlayerHealthChanged);
    }

    private void HandlePlayerHealthChanged(int arg0)
    {
        healthSlider.value = arg0;
    }

    protected override void OnUnbindFromPlayer()
    {
        OwningPlayer.Character.health.OnHealthChanged.RemoveListener(HandlePlayerHealthChanged);

        widgetGroup.alpha = inactiveAlpha;

        base.OnUnbindFromPlayer();
    }
}
