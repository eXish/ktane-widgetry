using KModkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WidgetryEncIndicator : WidgetryIndicator
{
    private static readonly char[] chars = "ใɮʖฬนÞฏѨԈԒดลЖ".ToCharArray();
    private static readonly Dictionary<char, int[]> values = new Dictionary<char, int[]>()
    {
        { 'ใ', new int[] { 5, 0, 4 } },
        { 'ɮ', new int[] { 4, 0, 5 } },
        { 'ʖ', new int[] { 0, -1, 4 } },
        { 'ฬ', new int[] { 0, 2, 5 } },
        { 'น', new int[] { 2, 1, 2 } },
        { 'Þ', new int[] { -2, 5, 5 } },
        { 'ฏ', new int[] { 4, 1, 2 } },
        { 'Ѩ', new int[] { 3, 5, 4 } },
        { 'Ԉ', new int[] { 4, 4, 2 } },
        { 'Ԓ', new int[] { 3, 2, 3 } },
        { 'ด', new int[] { -1, 3, 4 } },
        { 'ล', new int[] { -1, -2, 4 } },
        { 'Ж', new int[] { 5, 0, 5 } },
    };

    public override void Awake()
    {
        tryagain:
        List<char> selections = chars.ToList();

        int total = 0;
        _label.text = string.Empty;

        for(int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, selections.Count);
            char selection = selections[index];
            total += values[selection][i];
            _label.text += selection;
            selections.Remove(selection);
        }

        if(total < 0 || total >= 11)
            goto tryagain;

        _ind = (Indicator)total;
        string[] labels = new string[] { "CLR", "IND", "TRN", "FRK", "CAR", "FRQ", "NSA", "SIG", "MSA", "SND", "BOB" };
        _state = Random.Range(0, 2) == 1;

        Log("There is an " + (_state ? "" : "un") + "lit encrypted " + _ind + " indicator. (" + _label.text + ")");
    }
}