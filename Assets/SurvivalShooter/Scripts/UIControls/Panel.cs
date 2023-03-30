using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Panel : MonoBehaviour
{
    public GameObject selectedObjectOnEnable;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(selectedObjectOnEnable);
    }
}
