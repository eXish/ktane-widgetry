using System;
using UnityEngine;

public abstract class WidgetryWidget : MonoBehaviour
{
    public abstract WidgetryScript.WidgetQuery GetQuery();

    protected readonly Action<string> Log;

    public WidgetryWidget()
    {
        Log = s => GetComponentInParent<WidgetryScript>().ComponentLog(s);
    }
}