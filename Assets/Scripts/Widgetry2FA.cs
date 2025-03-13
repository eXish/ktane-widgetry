using KModkit;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
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
        ReadSettings();
        StartCoroutine(Loop());
    }

    private void ReadSettings()
    {
        float time;
        if (TryReadTFASettings(out time))
        {
            _changeTime = time;
            Log("Using Two-Factor's configured duration.");
        }
        else if (TryReadMWSettings(out time))
        {
            _changeTime = time;
            Log("Using Multiple Widgets's configured two-factor duration.");
        }
        else
            Log("Using the default two-factor duration.");
        Log(string.Format("Two-factor duration is {0} seconds.", _changeTime));
    }
    private class TFASettings
    {
        public int SettingsVersion;
        public int TwoFactorTimerLength;
    }
    private bool TryReadTFASettings(out float time)
    {
        time = 0f;
        Log("Reading Two-Factor's mod settings...");
        try
        {
            var path = Path.Combine(Path.Combine(Application.persistentDataPath, "Modsettings"), "twofactor-settings.txt");
            var settings = JsonConvert.DeserializeObject<TFASettings>(File.ReadAllText(path));
            if (settings.SettingsVersion != 0)
                Log("Expected 2FA settings version 0. Not reading settings.");
            else if (settings.TwoFactorTimerLength <= 0)
                Log("2FA settings specifies a time of zero or less. Not using it.");
            else
            {
                time = settings.TwoFactorTimerLength;
                return true;
            }
        }
        catch (FileNotFoundException) { Log("Failed to read 2FA settings because they don't exist."); }
        catch (System.Exception e) { Log("Failed to read 2FA settings. Inner exception:"); Debug.LogException(e, this); }
        return false;
    }
    private class MWSettings
    {
        public int SettingsVersion;
        public int TwoFactorDuration = 60;
    }
    private bool TryReadMWSettings(out float time)
    {
        time = 0f;
        Log("Reading Multiple Widgets's mod settings...");
        try
        {
            var path = Path.Combine(Path.Combine(Application.persistentDataPath, "Modsettings"), "MultipleWidgets-settings.txt");
            var settings = JsonConvert.DeserializeObject<MWSettings>(File.ReadAllText(path));
            if (settings.SettingsVersion != 4)
                Log("Expected MW settings version 4. Not reading settings.");
            else if (settings.TwoFactorDuration <= 0)
                Log("MW settings specifies a time of zero or less. Not using it.");
            else
            {
                time = settings.TwoFactorDuration;
                return true;
            }
        }
        catch (FileNotFoundException) { Log("Failed to read MW settings because they don't exist."); }
        catch (System.Exception e) { Log("Failed to read MW settings. Inner exception:"); Debug.LogException(e, this); }
        return false;
    }


    private void Update()
    {
        _timer.text = ((int)Mathf.Clamp(_timeStarted + _changeTime - Time.time, 0, 99)).ToString() + ".";
    }

    private IEnumerator Loop()
    {
        while (true)
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