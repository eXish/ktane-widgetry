using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using wawa.DDL;

#if !UNITY_EDITOR
using System.Reflection;
#endif

public class WidgetryScript : MonoBehaviour
{
    private static readonly Harmony _harmony = new Harmony("anon.widgetry");

    private static bool _harmonized, _mbharmonized;
    public delegate string WidgetQuery(string queryKey, string queryInfo);
    private static readonly Dictionary<object, List<WidgetQuery>> _widgets = new Dictionary<object, List<WidgetQuery>>();
    private static int _idc = 1;
    private int _id;

    [SerializeField]
    private GameObject[] _slots;

    [SerializeField]
    private KMBombInfo _info;

    [SerializeField]
    private KMSelectable _buttonWhatsit, _buttonDoodad;

    private bool _solved;

    private static object _key = new object();
    private Action _destroy = () => { };

    private void Awake()
    {
        _id = _idc++;
#if !UNITY_EDITOR
        //object bomb = GetComponentInParent(ReflectionHelper.FindTypeInGame("Bomb"));
        //if(bomb == null)
        //    bomb = _key;
        //_key = bomb;

        if(!_widgets.ContainsKey(_key))
            _widgets.Add(_key, new List<WidgetQuery>());
        foreach(WidgetQuery q in GenerateWidgets())
            _widgets[_key].Add(q);

        _destroy += () => {  _widgets[_key] = new List<WidgetQuery>(); };
#else  
        if (!_widgets.ContainsKey(_key))
            _widgets.Add(_key, new List<WidgetQuery>());
        foreach (WidgetQuery q in GenerateWidgets())
            _widgets[_key].Add(q);
#endif

#if !UNITY_EDITOR
        HookMultipleBombs();
        if(_harmonized)
            return;
        Debug.Log("[Widgetry] Modifying base game code...");

        MethodInfo orig = ReflectionHelper.FindTypeInGame("WidgetManager").Method("GetWidgetQueryResponses");
        MethodInfo postfix = GetType().Method("QueryPostfix");
        _harmony.Patch(orig, postfix: new HarmonyMethod(postfix));
        _harmonized = true;
    }

    private void HookMultipleBombs()
    {
        if(_mbharmonized)
            return;
        Type bip = ReflectionHelper.FindType("MultipleBombsAssembly.BombInfoProvider");
        if(bip == null)
            return;
        Debug.Log("[Widgetry] Multiple Bombs detected! Modifying...");
        MethodInfo orig = bip.Method("GetWidgetQueryResponses");
        MethodInfo postfix = GetType().Method("MBQueryPostfix");

        _harmony.Patch(orig, postfix: new HarmonyMethod(postfix));
        _mbharmonized = true;
#endif
    }

    private void Start()
    {
        _buttonWhatsit.OnInteract += () => { Whatsit(); return false; };
        _buttonDoodad.OnInteract += () => { Doodad(); return false; };
    }

    private void Whatsit()
    {
        _buttonWhatsit.AddInteractionPunch(1f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, _buttonWhatsit.transform);

        if (_solved)
            return;

        if (GetComponentsInChildren<WidgetryIndicator>().Length > 0)
            Solve();
        else
            Strike();
    }

    private void Doodad()
    {
        _buttonDoodad.AddInteractionPunch(1f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, _buttonDoodad.transform);

        if (_solved)
            return;

        if (GetComponentsInChildren<WidgetryIndicator>().Length == 0)
            Solve();
        else
            Strike();
    }

    private void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
        _solved = true;
        GetComponent<KMAudio>().PlaySoundAtTransform("Solve", _buttonDoodad.transform);
    }

    private void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }

    private void OnDestroy()
    {
        if (_destroy != null)
            _destroy();
        if (_configNext.Count != 0)
        {
            ComponentLog("There were extra configurations in the mission's settings.");
            _configNext.Clear();
        }
        _inMission = false;
    }

    private static bool _inMission;
    private static readonly Queue<string[][]> _configNext = new Queue<string[][]>();
    private IEnumerable<WidgetQuery> GenerateWidgets()
    {
    mission:
        if (_inMission && _configNext.Count > 0)
        {
            ComponentLog("Using the mission's settings.");
            return GenerateWidgets(_configNext.Dequeue());
        }

        // One Widgetry specifier applies to one Widgetry module, with extra modules using the default settings.
        // Example:
        // [Widgetry] [Indicator, Indicator, Indicator, VoltageMeter] [TwoFactor, ModdedPortPlate, PortPlate, EncryptedIndicator] [BatteryHolder]
        // [Widgetry] [TwoFactor, BatteryHolder, EncryptedIndicator, Indicator, ModdedPortPlate, PortPlate, VoltageMeter] [TwoFactor, BatteryHolder, EncryptedIndicator, Indicator, ModdedPortPlate, PortPlate, VoltageMeter] [TwoFactor, BatteryHolder, EncryptedIndicator, Indicator, ModdedPortPlate, PortPlate, VoltageMeter]
        if (!_inMission && Missions.Description.IsSome)
        {
            var desc = Missions.Description.Value;
            var lines = desc.Split('\n').Where(l => Regex.IsMatch(l, @"^\s*\[Widgetry\]")).ToArray();
            if (lines.Length == 0)
                goto standard;
            var ids = "(?:TwoFactor|BatteryHolder|(?:Encrypted)?Indicator|(?:Modded)?PortPlate|VoltageMeter)";
            var matches = lines.Select(l => Regex.Match(l, @"^\s*\[Widgetry\]((?:\s+\[" + ids + @"(?:,\s+" + ids + @")*\s*\]){3})\s*$")).ToArray();
            if (matches.Any(m => !m.Success))
            {
                ComponentLog("The mission description has a section for Widgetry, but it isn't valid.");
                goto standard;
            }
            foreach (var m in matches)
            {
                var line = Regex.Replace(m.Groups[1].Value, "\\s", "");
                line = line.Substring(1, line.Length - 2);
                var slots = line.Split(new[] { "][" }, StringSplitOptions.None);
                var config = slots.Select(sl => sl.Split(',')).ToArray();
                _configNext.Enqueue(config);
            }
            _inMission = true;
            goto mission;
        }

    standard:
        ComponentLog("Using the default settings.");
        return GenerateWidgets(
            new[] { "TwoFactor" },
            new[] { "EncryptedIndicator", "ModdedPortPlate", "VoltageMeter" },
            new[] { "BatteryHolder", "PortPlate", "Indicator" });
    }
    private IEnumerable<WidgetQuery> GenerateWidgets(params string[][] config)
    {
        if (config.Length != 3)
            throw new ArgumentException("Expected config for 3 slots, got " + config.Length);
        ComponentLog("Widget A can be: " + config[0].Join(", "));
        ComponentLog("Widget B can be: " + config[1].Join(", "));
        ComponentLog("Widget C can be: " + config[2].Join(", "));
        var ids = config.Select(a => a.PickRandom()).ToArray();
        for (int i = 0; i < 3; i++)
        {
            var children = _slots[i].GetComponentsInChildren<WidgetryWidget>(true);
            foreach (var ch in children)
                ch.gameObject.SetActive(false);
            var widget = children.Where(w => w.Id == ids[i]).PickRandom();
            widget.gameObject.SetActive(true);
            yield return widget.GetQuery();
        }
    }

    private static List<string> QueryPostfix(List<string> output, string queryKey, string queryInfo/*, object __instance*/)
    {
        if (_widgets.ContainsKey(_key))
            output.AddRange(_widgets[_key].Select(f => f(queryKey, queryInfo)).Where(s => s != null && !s.Equals(string.Empty)));
        return output;
    }

    private static List<string> MBQueryPostfix(List<string> output, object bomb, string queryKey, string queryInfo/*, object __instance*/)
    {
        if (_widgets.ContainsKey(_key))
            output.AddRange(_widgets[_key].Select(f => f(queryKey, queryInfo)).Where(s => s != null && !s.Equals(string.Empty)));
        return output;
    }

    public void ComponentLog(string obj)
    {
        Debug.LogFormat("[Widgetry #{0}] {1}", _id, obj);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"""!{0} W"" to press the button labeled W. ""!{0} D"" to press the button labeled D.";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command.ToLowerInvariant(), @"\s*w\s*"))
        {
            yield return null;
            _buttonWhatsit.OnInteract();
        }
        else if (Regex.IsMatch(command.ToLowerInvariant(), @"\s*d\s*"))
        {
            yield return null;
            _buttonDoodad.OnInteract();
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (GetComponentsInChildren<WidgetryIndicator>().Length == 0)
            _buttonDoodad.OnInteract();
        else
            _buttonWhatsit.OnInteract();
        yield break;
    }
}
