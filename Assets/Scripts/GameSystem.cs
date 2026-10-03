using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameSystem : MonoBehaviour
{
 public static GameSystem I {get;private set;}
 public static GameSystem Instance=>I;
 public int levelIndex=1,startLives=3,score,lives;
 public float scaredDuration=10,runTime;
 public HUDController hud;
 public LevelGenerator level;
 public PacStudentController player;
 public GhostController[] ghosts;
 public CherryController cherry;
 public MusicManager music;
 public GameObject blockingPanel;
 public TextMeshProUGUI message,abilityText;
 public UnityEngine.UI.Button exitButton;
 public ParticleSystem pulse;
 public GameplayPanels panels;
 public bool Paused {get;private set;}
 public int BestStreak {get;private set;}
 public int GhostsCaught {get;private set;}
 public int PulsesUsed {get;private set;}
 public int PulseHits {get;private set;}
 public int FoodEaten {get;private set;}
 public int BestPulseHits {get;private set;}
 public int SuperPulses {get;private set;}
 public int PowerPellets {get;private set;}
 public int HuntPellets {get;private set;}
 public int Stars {get;private set;}
 public RoundChallenges Challenges {get;private set;}
 public bool Playing {get;private set;}
 public float ScareTimer {get;private set;}
 public bool IsScared=>ScareTimer>0;
 public LevelTwoHUD huntHud;
 public LevelTwoRules Hunt {get;private set;}
 public bool HuntActive => levelIndex==2 && Hunt!=null && Hunt.Phase==LevelTwoRules.Wave.Hunt;
 public int Energy => Hunt==null?0:Hunt.Energy;
 public int Remaining => remaining;
 int remaining; bool ending;
 void Awake() { I=this; }
 void OnDestroy() { if(I==this) { I=null; Time.timeScale=1; } }
 IEnumerator Start()
 {
  score=0; lives=startLives; runTime=0; remaining=level.CountFood();
  if(levelIndex==2) { Hunt=new LevelTwoRules(); Challenges=new RoundChallenges(UnityEngine.Random.Range(0,int.MaxValue)); }
  hud.SetLevel(levelIndex); hud.SetScore(0); hud.SetLives(lives); hud.SetTime(0); hud.SetScaredTime(0);
  UpdateAbility(); blockingPanel.SetActive(true); exitButton.interactable=false; music.PlayIntro();
  foreach(var text in new[]{"3","2","1","GO!"}) { message.text=text+(levelIndex==2?"\n<size=24>CHAIN PELLETS  /  CHARGE ENERGY\nSPACE: PULSE  /  100 ENERGY: SUPER PULSE</size>":""); yield return new WaitForSeconds(1); }
  blockingPanel.SetActive(false); exitButton.interactable=true; Playing=true;
 }
 void Update()
 {
  if(Input.GetKeyDown(KeyCode.Escape)) TogglePause();
  if(!Playing) return;
  runTime+=Time.deltaTime; ScareTimer=Mathf.Max(0,ScareTimer-Time.deltaTime);
  if(Hunt!=null)
  {
   var before=Hunt.Phase; Hunt.Tick(Time.deltaTime);
   if(before!=Hunt.Phase && huntHud) huntHud.Notify(Hunt.Phase==LevelTwoRules.Wave.Warning?"HUNT IN 3s!\nSAVE A PULSE":HuntActive?"ALL GHOSTS\nARE HUNTING!":"HUNT OVER\nRECHARGE",HuntActive?new Color(1,.35f,.45f):Color.white);
   UpdateAbility();
  }
  hud.SetTime(runTime); hud.SetScaredTime(ScareTimer);
  bool dead=false,scared=IsScared;
  foreach(var g in ghosts)
  {
   dead|=g.State==GhostController.GhostState.Dead;
   scared|=g.State==GhostController.GhostState.Scared||g.State==GhostController.GhostState.Recovering;
   if(Vector3.Distance(g.transform.position,player.transform.position)>.7f || g.State==GhostController.GhostState.Dead) continue;
   if(IsScared||g.localScared>0||g.State!=GhostController.GhostState.Normal) { g.Kill(); GhostsCaught++; AddScore(300); UpdateChallenges(); SfxManager.I?.PlayCherry(); }
   else { LoseLife(); break; }
  }
  music.SetMood(dead,scared);
 }
 public void Consume(Vector3Int cell)
 {
  if(!Playing) return;
  bool power=level.powerPellets.HasTile(cell),pellet=level.pellets.HasTile(cell);
  if(!power&&!pellet) return;
  level.powerPellets.SetTile(cell,null); level.pellets.SetTile(cell,null);
  AddScore(Hunt!=null?Hunt.Collect(power):(power?50:10)); remaining--;
  FoodEaten++; if(power) PowerPellets++; if(HuntActive) HuntPellets++; if(Hunt!=null) BestStreak=Mathf.Max(BestStreak,Hunt.Streak);
  if(power) { StartScared(); SfxManager.I?.PlayCherry(); }
  else SfxManager.I?.PlayPellet();
  if(Hunt!=null) UpdateAbility();
  UpdateChallenges();
  if(remaining<=0) StartCoroutine(EndRound(true));
 }
 public void AddScore(int value) { score+=value; hud.SetScore(score); }
 public void StartScared() { AddScare(10); }
 public void AddScare(float seconds) { ScareTimer=Mathf.Max(ScareTimer,seconds); }
 public void LoseLife() { if(Playing&&!ending) StartCoroutine(DeathSequence()); }
 IEnumerator DeathSequence()
 {
  Playing=false; Hunt?.ResetAfterHit(); UpdateAbility(); lives=Mathf.Max(0,lives-1); hud.SetLives(lives); player.Die(); SfxManager.I?.PlayPacDeath();
  yield return new WaitForSeconds(1.6f);
  if(lives==0) { yield return EndRound(false); yield break; }
  ScareTimer=0; hud.SetScaredTime(0); player.ResetPlayer();
  foreach(var ghost in ghosts) ghost.ResetGhost();
  cherry.Clear(); Playing=true;
 }
 IEnumerator EndRound(bool completed)
 {
  if(ending) yield break; ending=true; Playing=false; exitButton.interactable=false;
  Paused=false; Time.timeScale=0;
  player.animator.speed=0; SfxManager.I?.StopMoveLoop(); blockingPanel.SetActive(false);
  string key="PacStudent.Level"+levelIndex;
  bool record=score>0 && (score>PlayerPrefs.GetInt(key+".Score",0) || score==PlayerPrefs.GetInt(key+".Score",0)&&runTime<PlayerPrefs.GetFloat(key+".Time",float.MaxValue));
  Stars=RoundRating.Evaluate(completed,levelIndex,lives,runTime,Challenges==null?0:Challenges.CompletedCount);
  PlayerPrefs.SetInt(RoundRating.Key(levelIndex),Mathf.Max(Stars,PlayerPrefs.GetInt(RoundRating.Key(levelIndex),0)));
  PlayerPrefs.Save();
  SaveRecord(levelIndex,score,runTime);
  panels.ShowResults(this,completed,record);
  yield break;
 }
 public void TogglePause()
 {
  if(ending||(!Playing&&!Paused)||!panels) return;
  Paused=!Paused; Playing=!Paused; Time.timeScale=Paused?0:1;
  if(Paused) { SfxManager.I?.StopMoveLoop(); player.animator.speed=0; }
  panels.ShowPause(Paused);
 }
 public void RestartRound() { Time.timeScale=1; Paused=false; Playing=false; MusicSettings.Save(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
 public void ReturnToMenu() { Time.timeScale=1; Paused=false; Playing=false; MusicSettings.Save(); SceneManager.LoadScene("StartScene"); }
 public static void SaveRecord(int level,int value,float time)
 {
  string key="PacStudent.Level"+level;
  int best=PlayerPrefs.GetInt(key+".Score",0); float fastest=PlayerPrefs.GetFloat(key+".Time",float.MaxValue);
  if(value>best || value==best && time<fastest)
  { PlayerPrefs.SetInt(key+".Score",value); PlayerPrefs.SetFloat(key+".Time",time); PlayerPrefs.Save(); }
 }
 public void TryPulse()
 {
  if(!Playing||Hunt==null) return;
  if(!Hunt.TryPulse(out bool super))
  {
   if(huntHud) huntHud.Notify(Hunt.Cooldown>0?"PULSE RECHARGING":"NEED 50 ENERGY\nKEEP COLLECTING",new Color(1,.8f,.3f));
   return;
  }
  float radius=super?10:6; int hits=0;
  foreach(var ghost in ghosts) if(ghost.State!=GhostController.GhostState.Dead && Vector3.Distance(ghost.transform.position,player.transform.position)<=radius) { ghost.Scare(super?7:5); hits++; }
  PulsesUsed++; PulseHits+=hits; BestPulseHits=Mathf.Max(BestPulseHits,hits); if(super) SuperPulses++;
  if(pulse) { pulse.transform.position=player.transform.position; pulse.transform.localScale=Vector3.one*(super?1.67f:1); pulse.Play(); }
  if(huntHud) huntHud.Notify((super?"SUPER PULSE!":"PULSE!")+"\n"+hits+" GHOSTS HIT",super?new Color(1,.8f,.3f):Color.cyan);
  SfxManager.I?.PlayCherry(); UpdateAbility(); UpdateChallenges();
 }
 void UpdateChallenges()
 {
  if(Challenges==null)return;
  int reward=Challenges.Observe(BestStreak,BestPulseHits,SuperPulses,GhostsCaught,PowerPellets,HuntPellets);
  if(reward>0){AddScore(reward);if(huntHud)huntHud.Notify("TASK COMPLETE!\n+"+reward+" POINTS",new Color(1,.8f,.3f));}
 }
 void UpdateAbility()
 {
  if(!abilityText) return; abilityText.gameObject.SetActive(levelIndex==2);
  abilityText.text="ENERGY "+Energy+"/100   |   "+(Hunt!=null&&Hunt.Cooldown>0?"PULSE COOLDOWN: "+Hunt.Cooldown.ToString("0.0")+"s":Energy==100?"SPACE: SUPER PULSE (100)":Energy>=50?"SPACE: PULSE READY (50)":"SPACE: PULSE (50)");
  if(Hunt!=null && huntHud) huntHud.Refresh(Hunt,remaining,Playing?Time.deltaTime:0);
 }
}
