using UnityEngine;
using TMPro;

public class LevelTwoHUD : MonoBehaviour
{
    public TextMeshProUGUI waveText, comboText, foodText, noticeText;
    public UnityEngine.UI.Image energyFill, comboFill;
    float noticeLeft;
    public void Notify(string text, Color color)
    {
        noticeText.text = text; noticeText.color = color; noticeLeft = 2.5f;
    }
    public void Refresh(LevelTwoRules rules, int remaining, float delta)
    {
        var phase = rules.Phase;
        Color accent = phase == LevelTwoRules.Wave.Hunt ? new Color(1,.35f,.45f) : phase == LevelTwoRules.Wave.Warning ? new Color(1,.8f,.3f) : new Color(.4f,1,.75f);
        waveText.color = accent;
        waveText.text = (phase == LevelTwoRules.Wave.Hunt ? "HUNT!" : phase == LevelTwoRules.Wave.Warning ? "GET READY" : "RECHARGE") + "\n<size=20>" + Mathf.CeilToInt(rules.PhaseLeft) + "s</size>";
        comboText.text = "COMBO  x" + rules.Multiplier + "\n<size=18>" + rules.Streak + " PELLETS" + (rules.Multiplier < 3 ? "\n" + (rules.Multiplier == 1 ? 10 : 25) + " FOR x" + (rules.Multiplier + 1) : "\nMAX MULTIPLIER") + "</size>";
        foodText.text = "PELLETS LEFT\n<size=38>" + remaining + "</size>";
        energyFill.fillAmount = rules.Energy / 100f;
        energyFill.color = rules.Energy == 100 ? new Color(1,.8f,.3f) : new Color(.4f,1,.75f);
        comboFill.fillAmount = rules.ComboLeft / 3f;
        noticeLeft = Mathf.Max(0, noticeLeft - delta);
        if (noticeLeft == 0)
        {
            noticeText.color = accent;
            noticeText.text = phase == LevelTwoRules.Wave.Hunt ? "USE YOUR PULSE\nTO TURN THE HUNT" : "PLAN YOUR ROUTE\nKEEP THE COMBO";
        }
    }
}
