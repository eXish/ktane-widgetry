using KModkit;
using System.Collections;
using UnityEngine;

public class WidgetryBatteries : WidgetryWidget
{
    [SerializeField]
    private int _count;

    public void Start()
    {
        if(_count == 1)
            Log("There is 1 battery.");
        else
            Log("There are " + _count + " batteries.");
    }

    public override WidgetryScript.WidgetQuery GetQuery()
    {
        return (a, b) => a != null && a.Equals(KMBombInfo.QUERYKEY_GET_BATTERIES) ? @"{""numbatteries"":" + _count + "}" : string.Empty;
    }
}