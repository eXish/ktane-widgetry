using KModkit;
using System.Collections;
using UnityEngine;

public class Widgetry2FA : WidgetryWidget
{
    [SerializeField]
    private TextMesh _text, _timer;
    [SerializeField]
    private float _changeTime;
    [SerializeField]
    private AudioClip _clip;
    [SerializeField]
    private KMAudio _audio;

    private float _timeStarted;

    public int Key { get; private set; }

    private void Start()
    {
        StartCoroutine(Loop());
    }

    private void Update()
    {
        _timer.text = ((int)Mathf.Clamp(_timeStarted + _changeTime - Time.time, 0, 99)).ToString() + ".";
    }

    private IEnumerator Loop()
    {
        while(true)
        {
            Key = Random.Range(0, 1000000);
            _text.text = Key.ToString() + ".";
            _timeStarted = Time.time;
            Log("2FA code has changed to " + Key + ".");
            yield return new WaitUntil(() => Time.time > _timeStarted + _changeTime);
            _audio.PlaySoundAtTransform(_clip.name, transform);
        }
    }

    public override WidgetryScript.WidgetQuery GetQuery()
    {
        return (a, b) => a != null && a.Equals(KMBombInfoExtensions.WidgetQueryTwofactor) ? @"{""twofactor_key"":" + Key + "}" : string.Empty;
    }
}