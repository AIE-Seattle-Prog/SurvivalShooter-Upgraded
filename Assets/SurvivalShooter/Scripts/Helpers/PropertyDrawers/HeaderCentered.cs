using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class HeaderCenteredAttribute : PropertyAttribute
{
    public readonly string data;

    public HeaderCenteredAttribute(string data)
    {
        this.data = data;
    }
}
