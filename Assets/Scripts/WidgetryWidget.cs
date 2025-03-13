using System;
using UnityEngine;

public abstract class WidgetryWidget : MonoBehaviour
{
    public abstract string Id { get; }

    public abstract WidgetryScript.WidgetQuery GetQuery();

    protected Action<string> Log { get; private set; }

    public WidgetryWidget()
    {
        Log = s => (Log = GetComponentInParent<WidgetryScript>().ComponentLog)(s);
    }
}