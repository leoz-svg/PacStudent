using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class GameplayPanels : MonoBehaviour
{
    public GameSystem game;
    public StarRatingView resultStars;
    public TextMeshProUGUI ratingDetails,taskSummary;
    public GameObject pauseOverlay, resultsOverlay;
    public UnityEngine.UI.Button resumeButton, restartButton, menuButton, retryButton, resultMenuButton;
    public UnityEngine.UI.Toggle musicToggle;
    public UnityEngine.UI.Slider musicSlider;
    public TextMeshProUGUI musicState, volumeText, resultTitle, resultSubtitle, resultScore, resultTime, resultStats, recordText;
    void Awake()
    {
        pauseOverlay.SetActive(false); resultsOverlay.SetActive(false);
        resumeButton.onClick.AddListener(game.TogglePause);
        restartButton.onClick.AddListener(game.RestartRound); retryButton.onClick.AddListener(game.RestartRound);
        menuButton.onClick.AddListener(game.ReturnToMenu); resultMenuButton.onClick.AddListener(game.ReturnToMenu);
        musicToggle.onValueChanged.AddListener(MusicSettings.SetEnabled);
        musicSlider.onValueChanged.AddListener(MusicSettings.SetVolume);
        RefreshMusic();
    }
    void OnEnable() { MusicSettings.Changed+=RefreshMusic; }
    void OnDisable() { MusicSettings.Changed-=RefreshMusic; MusicSettings.Save(); }
    void RefreshMusic()
    {
        musicToggle.SetIsOnWithoutNotify(MusicSettings.Enabled); musicSlider.SetValueWithoutNotify(MusicSettings.Volume);
        if(musicToggle.graphic) musicToggle.graphic.enabled=MusicSettings.Enabled;
        musicState.text=MusicSettings.Enabled?"SOUND ON":"SOUND OFF";
        volumeText.text=Mathf.RoundToInt(MusicSettings.Volume*100)+"%";
    }
    public void ShowPause(bool show)
    {
        RefreshMusic(); pauseOverlay.SetActive(show);
        if(show) pauseOverlay.transform.SetAsLastSibling(); else MusicSettings.Save();
        if(EventSystem.current) EventSystem.current.SetSelectedGameObject(show?resumeButton.gameObject:null);
    }
    public void ShowResults(GameSystem gs,bool completed,bool record)
    {
        pauseOverlay.SetActive(false); resultsOverlay.SetActive(true); resultsOverlay.transform.SetAsLastSibling();
        resultTitle.text=completed?"GAME COMPLETE!":"GAME OVER";
        resultTitle.color=completed?new Color(.4f,1,.75f):new Color(1,.4f,.48f);
        resultSubtitle.text=completed?"CONGRATULATIONS!  /  MAZE CLEARED":"ONE MORE RUN?  /  FIND YOUR NEXT ROUTE";
        resultStars.Show(gs.Stars);
        ratingDetails.text=(completed?"[OK]":"[--]")+" CLEAR   |   "+(completed&&gs.lives>=2?"[OK]":"[--]")+" 2+ LIVES   |   "+(completed&&RoundRating.Advanced(gs.levelIndex,gs.runTime,gs.Challenges==null?0:gs.Challenges.CompletedCount)?"[OK]":"[--]")+" ADVANCED\nADVANCED: "+(gs.levelIndex==2?"ALL 3 TASKS + WITHIN 4:00":"FINISH WITHIN 3:00");
        taskSummary.text=gs.Challenges==null?"CLASSIC RUN\nNo bonus tasks in this level":"TASK BONUS  +"+gs.Challenges.Bonus+" (INCLUDED)\n";
        if(gs.Challenges!=null)foreach(var task in gs.Challenges.Tasks)taskSummary.text+=(task.Complete?"[OK] ":"[--] ")+task.Label+"  "+task.Progress+"/"+task.Target+"\n";
        resultScore.text=gs.score.ToString("D6"); resultTime.text=HUDController.FormatTime(gs.runTime);
        resultStats.text="PELLETS COLLECTED     "+gs.FoodEaten+"\nGHOSTS CAUGHT          "+gs.GhostsCaught+"\n"+
            (gs.levelIndex==2?"BEST CHAIN                  "+gs.BestStreak+"\nPULSES / HITS               "+gs.PulsesUsed+" / "+gs.PulseHits:"LIVES REMAINING          "+gs.lives);
        recordText.text=record?"NEW HIGH SCORE / BEST TIME AT THIS SCORE":"BEST SCORE  "+PlayerPrefs.GetInt("PacStudent.Level"+gs.levelIndex+".Score",0).ToString("D6");
        if(EventSystem.current) EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }
    void OnDestroy()
    {
        if(resumeButton) resumeButton.onClick.RemoveListener(game.TogglePause);
        if(restartButton) restartButton.onClick.RemoveListener(game.RestartRound);
        if(retryButton) retryButton.onClick.RemoveListener(game.RestartRound);
        if(menuButton) menuButton.onClick.RemoveListener(game.ReturnToMenu);
        if(resultMenuButton) resultMenuButton.onClick.RemoveListener(game.ReturnToMenu);
        if(musicToggle) musicToggle.onValueChanged.RemoveListener(MusicSettings.SetEnabled);
        if(musicSlider) musicSlider.onValueChanged.RemoveListener(MusicSettings.SetVolume);
    }
}

