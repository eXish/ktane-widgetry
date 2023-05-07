using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WidgetryPortPlate : WidgetryWidget
{
    [SerializeField]
    private PortType[] _ports;

    private Port _combinedPorts;

    public void Awake()
    {
        int count = Random.Range(this is WidgetryModPortPlate ? 1 : 0, _ports.Length + 1);
        for(int i = 0; i < _ports.Length; ++i)
            _ports[i].gameObject.SetActive(false);

        for(int i = 0; i < count; ++i)
        {
            PortType selected = _ports.Where(p => (_combinedPorts & p.Port) == Port.None).PickRandom();
            selected.gameObject.SetActive(true);
            _combinedPorts |= selected.Port;
        }

        if(_combinedPorts == Port.None)
            Log("There is an empty port plate.");
        else
        {
            string data = string.Empty;
            if(count == 1)
                data = "a " + _combinedPorts;
            else if(count == 2)
            {
                foreach(Port p in _allPorts)
                    if((_combinedPorts & p) != Port.None)
                        data += "a " + p.ToString() + (data == string.Empty ? " and " : "");
            }
            else
            {
                Port copy = _combinedPorts;
                foreach(Port p in _allPorts)
                {
                    if((copy & p) != Port.None)
                    {
                        data += (data == string.Empty || data.EndsWith(", and ") ? "" : ", ") + "a " + p;
                        copy ^= p;
                        if(_allPorts.Any(po => po == copy))
                            data += ", and ";
                    }
                }
            }
            Log("There is a port plate with " + data + ".");
        }
    }

    public override WidgetryScript.WidgetQuery GetQuery()
    {
        return (a, b) =>
        {
            if(a != null && a.Equals(KMBombInfo.QUERYKEY_GET_PORTS))
            {
                Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>
                {
                    { "presentPorts", _allPorts.Where(p => (_combinedPorts & p) != Port.None).Select(p => p.ToString()).ToList() }
                };
                return JsonConvert.SerializeObject(dictionary);
            }
            return string.Empty;
        };
    }

    private static readonly Port[] _allPorts = new Port[] { Port.DVI, Port.Parallel, Port.PS2, Port.RJ45, Port.Serial, Port.StereoRCA, Port.HDMI, Port.USB, Port.VGA, Port.ComponentVideo, Port.AC, Port.PCMCIA, Port.CompositeVideo };

    [System.Flags]
    public enum Port
    {
        None = 0,
        Serial = 1,
        Parallel = 2,
        PS2 = 4,
        DVI = 8,
        RJ45 = 16,
        StereoRCA = 32,
        HDMI = 64,
        USB = 128,
        ComponentVideo = 256,
        AC = 512,
        PCMCIA = 1024,
        VGA = 2048,
        CompositeVideo = 4096
    }
}


