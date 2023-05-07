using UnityEngine;

public class WidgetryIndicator : WidgetryWidget
{
    [SerializeField]
    protected TextMesh _label;
    [SerializeField]
    private Renderer _bulb;
    [SerializeField]
    private Material _onMat, _offMat;

    protected Indicator _ind;
    protected bool _state;

    public virtual void Awake()
    {
        _ind = (Indicator)Random.Range(0, 11);
        _state = Random.Range(0, 2) == 1;
        _bulb.sharedMaterial = _state ? _onMat : _offMat;
        _label.text = _ind.ToString();
        Log("There is a " + (_state ? "" : "un") + "lit " + _ind + " indicator.");
    }

    public override WidgetryScript.WidgetQuery GetQuery()
    {
        return (a, b) => a != null && a.Equals(KMBombInfo.QUERYKEY_GET_INDICATOR) ? @"{""label"":""" + _ind + @""",""on"":""" + _state + @"""}" : string.Empty;
    }

    protected enum Indicator
    {
        CLR,
        IND,
        TRN,
        FRK,
        CAR,
        FRQ,
        NSA,
        SIG,
        MSA,
        SND,
        BOB,
        NLL
    }
}