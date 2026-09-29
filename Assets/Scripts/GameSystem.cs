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
 public bool Playing {get;private set;}
 public float ScareTimer {get;private set;}
 public bool IsScared=>ScareTimer>0;
 public int Energy {get;private set;}
 int remaining; bool ending;
 void Awake() { I=this; }
 void OnDestroy() { if(I==this) I=null; }
 IEnumerator Start()
 {
  score=0; lives=startLives; runTime=0; remaining=level.CountFood();
  hud.SetLevel(levelIndex); hud.SetScore(0); hud.SetLives(lives); hud.SetTime(0); hud.SetScaredTime(0);
  UpdateAbility(); blockingPanel.SetActive(true); exitButton.interactable=false; music.PlayIntro();
  foreach(var text in new[]{"3","2","1","GO!"}) { message.text=text; yield return new WaitForSeconds(1); }
  blockingPanel.SetActive(false); exitButton.interactable=true; Playing=true;
 }
 void Update()
 {
  if(!Playing) return;
  runTime+=Time.deltaTime; ScareTimer=Mathf.Max(0,ScareTimer-Time.deltaTime);
  hud.SetTime(runTime); hud.SetScaredTime(ScareTimer);
  bool dead=false,scared=IsScared;
  foreach(var g in ghosts)
  {
   dead|=g.State==GhostController.GhostState.Dead;
   scared|=g.State==GhostController.GhostState.Scared||g.State==GhostController.GhostState.Recovering;
   if(Vector3.Distance(g.transform.position,player.transform.position)>.7f || g.State==GhostController.GhostState.Dead) continue;
   if(IsScared||g.localScared>0||g.State!=GhostController.GhostState.Normal) { g.Kill(); AddScore(300); SfxManager.I?.PlayCherry(); }
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
  AddScore(power?50:10); remaining--;
  if(power) { StartScared(); SfxManager.I?.PlayCherry(); }
  if(levelIndex==2) { Energy=Mathf.Min(100,Energy+(power?25:5)); UpdateAbility(); }
  if(remaining<=0) StartCoroutine(EndRound(true));
 }
 public void AddScore(int value) { score+=value; hud.SetScore(score); }
 public void StartScared() { AddScare(10); }
 public void AddScare(float seconds) { ScareTimer=Mathf.Max(ScareTimer,seconds); }
 public void LoseLife() { if(Playing&&!ending) StartCoroutine(DeathSequence()); }
 IEnumerator DeathSequence()
 {
  Playing=false; lives=Mathf.Max(0,lives-1); hud.SetLives(lives); player.Die(); SfxManager.I?.PlayPacDeath();
  yield return new WaitForSeconds(1.6f);
  if(lives==0) { yield return EndRound(false); yield break; }
  ScareTimer=0; hud.SetScaredTime(0); player.ResetPlayer();
  foreach(var ghost in ghosts) ghost.ResetGhost();
  cherry.Clear(); Playing=true;
 }
 IEnumerator EndRound(bool completed)
 {
  if(ending) yield break; ending=true; Playing=false; exitButton.interactable=false;
  player.animator.speed=0; SfxManager.I?.StopMoveLoop(); blockingPanel.SetActive(true);
  message.text=completed ? "GAME COMPLETE!\n<size=48>CONGRATULATIONS!</size>" : "GAME OVER";
  message.fontSize=completed ? 76 : 100;
  message.color=completed ? new Color(.4f,1f,.75f) : Color.white;
  SaveRecord(levelIndex,score,runTime);
  yield return new WaitForSeconds(3); SceneManager.LoadScene("StartScene");
 }
 public static void SaveRecord(int level,int value,float time)
 {
  string key="PacStudent.Level"+level;
  int best=PlayerPrefs.GetInt(key+".Score",0); float fastest=PlayerPrefs.GetFloat(key+".Time",float.MaxValue);
  if(value>best || value==best && time<fastest)
  { PlayerPrefs.SetInt(key+".Score",value); PlayerPrefs.SetFloat(key+".Time",time); PlayerPrefs.Save(); }
 }
 public void TryPulse()
 {
  if(!Playing||levelIndex!=2||Energy<50) return;
  Energy-=50; foreach(var ghost in ghosts) if(Vector3.Distance(ghost.transform.position,player.transform.position)<=6) ghost.Scare(5);
  if(pulse) { pulse.transform.position=player.transform.position; pulse.Play(); }
  SfxManager.I?.PlayCherry(); UpdateAbility();
 }
 void UpdateAbility()
 {
  if(!abilityText) return; abilityText.gameObject.SetActive(levelIndex==2);
  abilityText.text="ENERGY "+Energy+"/100   |   SPACE: PULSE (50)   |   EAT TO RECHARGE";
 }
}
