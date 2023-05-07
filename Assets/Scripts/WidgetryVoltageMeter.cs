using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class WidgetryVoltageMeter : WidgetryWidget
{
    public GameObject squareModel;
    public GameObject circleModel;
    public GameObject pointer;
    public GameObject circlePointer;

    readonly private string QUERY_KEY = "volt";
    private bool activated;
    private bool solved;
    private bool _isCircular;
    private double[] possibleVoltages = new double[] { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10 };
    static int _chosenVoltageInternal;
    static int chosenVoltage
    {
        get
        {
            try
            {
                Type t = ReflectionHelper.FindType("VoltageMeterScript");
                return t.Field<int>("chosenVoltage", null);
            }
            catch(Exception)
            {
                return _chosenVoltageInternal;
            }
        }
        set
        {
            _chosenVoltageInternal = value;
            try
            {
                Type t = ReflectionHelper.FindType("VoltageMeterScript");
                t.SetField("chosenVoltage", null, value);
            }
            catch(Exception) { }
        }
    }

    static int _widgetIdCounterInternal;
    static int widgetIdCounter
    {
        get
        {
            try
            {
                Type t = ReflectionHelper.FindType("VoltageMeterScript");
                return t.Field<int>("widgetIdCounter", null);
            }
            catch(Exception)
            {
                return _widgetIdCounterInternal;
            }
        }
        set
        {
            _widgetIdCounterInternal = value;
            try
            {
                Type t = ReflectionHelper.FindType("VoltageMeterScript");
                t.SetField("widgetIdCounter", null, value);
            }
            catch(Exception) { }
        }
    }
    int widgetId;

    void Awake()
    {
        widgetId = widgetIdCounter++;
        GetComponentInParent<KMBombInfo>().OnBombSolved += Solve;
        GetComponentInParent<KMBombInfo>().OnBombExploded += Explode;

        _isCircular = UnityEngine.Random.Range(0, 2) == 0;
        Log("There is a " + (_isCircular ? "circular" : "square") + " voltage meter");
        if(_isCircular)
            squareModel.SetActive(false);
        else
            circleModel.SetActive(false);

        if(widgetId == 1)
        {
            chosenVoltage = UnityEngine.Random.Range(0, possibleVoltages.Length);
            Debug.LogFormat("[Voltage Meter] Voltage: {0}V", possibleVoltages[chosenVoltage]);
        }

        Activate();
    }

    void Activate()
    {
        if(!solved)
        {
            StartCoroutine(MovePointer(true));
            activated = true;
        }
    }

    void Solve()
    {
        if(activated)
            StartCoroutine(MovePointer(false));
        solved = true;
        widgetIdCounter = 1;
    }

    void Explode()
    {
        widgetIdCounter = 1;
    }

    public string GetQueryResponse(string queryKey, string queryInfo)
    {
        if(queryKey == QUERY_KEY)
        {
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                { "voltage", possibleVoltages[chosenVoltage].ToString() }
            };
            string responseStr = JsonConvert.SerializeObject(response);
            return responseStr;
        }
        return "";
    }

    private IEnumerator MovePointer(bool startup)
    {
        float t = 0f;
        while(t < 1f)
        {
            if(!_isCircular)
            {
                if(startup)
                    pointer.transform.localPosition = Vector3.Lerp(new Vector3(-0.0525f, -0.0123f, -0.0061f), new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f), t);
                else
                    pointer.transform.localPosition = Vector3.Lerp(new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f), new Vector3(-0.0525f, -0.0123f, -0.0061f), t);
            }
            else
            {
                if(startup)
                    circlePointer.transform.localEulerAngles = Vector3.Lerp(new Vector3(0f, -76.5f, 0f), new Vector3(0f, ((float)possibleVoltages[chosenVoltage] - 5f) * 15.3f, 0f), t);
                else
                    circlePointer.transform.localEulerAngles = Vector3.Lerp(new Vector3(0f, ((float)possibleVoltages[chosenVoltage] - 5f) * 15.3f, 0f), new Vector3(0f, -76.5f, 0f), t);
            }
            t += Time.deltaTime * 2f;
            yield return null;
        }
        if(!_isCircular)
        {
            if(startup)
                pointer.transform.localPosition = new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f);
            else
                pointer.transform.localPosition = new Vector3(-0.0525f, -0.0123f, -0.0061f);
        }
        else
        {
            if(startup)
                circlePointer.transform.localEulerAngles = new Vector3(0f, ((float)possibleVoltages[chosenVoltage] - 5f) * 15.3f, 0f);
            else
                circlePointer.transform.localEulerAngles = new Vector3(0f, -76.5f, 0f);
        }
    }

    public override WidgetryScript.WidgetQuery GetQuery()
    {
        return (a, b) => GetQueryResponse(a, b);
    }
}
