#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class SmokeValidation : MonoBehaviour
{
 string directory;
 readonly List<string> results=new List<string>();
 int errors;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Launch()
 {
  if(!SessionState.GetBool("PacStudent.Smoke",false)) return;
  SessionState.SetBool("PacStudent.Smoke",false);
  var go=new GameObject("DevelopmentValidation");DontDestroyOnLoad(go);go.AddComponent<SmokeValidation>();
 }
 void Check(bool ok,string name) { results.Add((ok?"PASS ":"FAIL ")+name); if(!ok)errors++;Flush(); }
 void Flush() { File.WriteAllLines(Path.Combine(directory,"play-tests.txt"),results); }
 void Log(string message,string stack,LogType type) { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert) { errors++;results.Add("RUNTIME ERROR "+message);Flush(); } }
 IEnumerator Start()
 {
  Application.runInBackground=true;
  directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Validation",Screen.width+"x"+Screen.height));Directory.CreateDirectory(directory);Application.logMessageReceived+=Log;
  yield return new WaitForSeconds(1);
  Check(SceneManager.GetActiveScene().name=="StartScene","Start menu loaded");
  Check(FindFirstObjectByType<MenuController>()!=null,"Menu records and animated border wired");
  ScreenCapture.CaptureScreenshot(Path.Combine(directory,"menu.png"));yield return new WaitForSeconds(.3f);
  SceneManager.LoadScene("Level1");yield return null;yield return null;
  var g=GameSystem.I;Check(g!=null,"Level 1 game system exists");
  Check(!g.Playing&&g.runTime==0&&g.lives==3,"Countdown freezes gameplay and timer; three lives");
  yield return new WaitForSeconds(4.3f);
  Check(g.Playing&&g.runTime>0,"Countdown releases gameplay");
  foreach(var ghost in g.ghosts) ghost.enabled=false;
  var p=g.player;var initial=p.transform.position;p.SetInput(Vector3Int.right);
  yield return new WaitForSeconds(.4f);
  Check(p.transform.position.x>initial.x+1,"One key press continues movement without hold");
  Check(g.score>=10,"Eating pellets awards points");
  Check(p.lastInput==Vector3Int.right,"lastInput remains buffered");
  p.SetInput(Vector3Int.up);yield return new WaitForSeconds(.25f);
  Check(p.currentInput==Vector3Int.right,"Blocked requested turn preserves current movement");
  p.ResetPlayer();p.SetInput(Vector3Int.up);var before=p.transform.position;yield return new WaitForSeconds(.3f);
  Check(Vector3.Distance(before,p.transform.position)<.001f,"Wall prevents movement");
  Check(HUDController.FormatTime(92.25f)=="01:32:25","Timer displays minutes seconds centiseconds");
  var spawn=p.spawn;p.spawn=g.level.Center(new Vector3Int(g.level.bounds.xMin,0,0));p.ResetPlayer();p.SetInput(Vector3Int.left);yield return new WaitForSeconds(.2f);
  Check(p.transform.position.x>10,"Left tunnel wraps to right");p.spawn=spawn;p.ResetPlayer();
  g.StartScared();yield return null;Check(g.ScareTimer>9.8f&&g.ScareTimer<=10,"Power pill lasts ten seconds");
  var gh=g.ghosts[0];gh.enabled=true;gh.transform.position=p.transform.position;yield return null;yield return null;
  Check(gh.State==GhostController.GhostState.Dead&&g.score>=300,"Scared collision awards 300 and enters Dead state");
  gh.transform.position=gh.spawn;yield return null;yield return null;Check(gh.State!=GhostController.GhostState.Dead,"Dead ghost revives only at spawn");
  foreach(var ghost in g.ghosts) { ghost.enabled=true;ghost.ResetGhost(); }
  yield return new WaitForSeconds(2);
  ScreenCapture.CaptureScreenshot(Path.Combine(directory,"level1.png"));yield return new WaitForSeconds(.3f);
  SceneManager.LoadScene("InnovationScene");yield return null;yield return new WaitForSeconds(4.3f);g=GameSystem.I;p=g.player;
  foreach(var ghost in g.ghosts)ghost.enabled=false;
  Check(g.levelIndex==2,"Innovation scene wired to level 2");
  int consumed=0;foreach(var cell in g.level.bounds.allPositionsWithin) if(g.level.pellets.HasTile(cell)&&!g.level.powerPellets.HasTile(cell)) { g.Consume(cell);if(++consumed==10)break; }
  Check(g.Energy==50,"Ten pellets charge one pulse");
  var near=g.ghosts[0];near.transform.position=p.transform.position+Vector3.right*2;g.TryPulse();
  Check(g.Energy==0&&near.localScared==5,"Pulse consumes energy and scares nearby ghost");
  var distant=g.ghosts[1];Check(distant.localScared==0,"Pulse respects six-cell range");
  ScreenCapture.CaptureScreenshot(Path.Combine(directory,"innovation.png"));yield return new WaitForSeconds(.3f);
  SceneManager.LoadScene("Level1");yield return null;yield return new WaitForSeconds(4.3f);g=GameSystem.I;
  foreach(var ghost in g.ghosts)ghost.enabled=false;
  g.LoseLife();Check(!g.Playing&&g.lives==2,"Death removes one life and freezes round");yield return new WaitForSeconds(1.8f);
  Check(g.Playing&&g.player.lastInput==Vector3Int.zero&&Vector3.Distance(g.player.spawn,g.player.transform.position)<.01f,"Death respawns player awaiting new input");
  g.LoseLife();yield return new WaitForSeconds(1.8f);g.LoseLife();yield return new WaitForSeconds(1.8f);
  Check(!g.Playing&&g.blockingPanel.activeSelf&&!g.exitButton.interactable,"No lives displays Game Over and blocks buttons");
  yield return new WaitForSeconds(3.2f);Check(SceneManager.GetActiveScene().name=="StartScene","Game Over returns to menu");
  GameSystem.SaveRecord(99,100,30);GameSystem.SaveRecord(99,100,20);GameSystem.SaveRecord(99,50,10);
  Check(PlayerPrefs.GetInt("PacStudent.Level99.Score")==100&&PlayerPrefs.GetFloat("PacStudent.Level99.Time")==20,"Record saves score then faster time tie-break");
  PlayerPrefs.DeleteKey("PacStudent.Level99.Score");PlayerPrefs.DeleteKey("PacStudent.Level99.Time");PlayerPrefs.Save();
  GameObject.Find("PanelL2").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;yield return new WaitForSeconds(4.3f);
  Check(SceneManager.GetActiveScene().name=="InnovationScene","Actual Level 2 button opens innovation scene");
  g=GameSystem.I;foreach(var cell in g.level.bounds.allPositionsWithin)g.Consume(cell);
  Check(!g.Playing&&g.blockingPanel.activeSelf,"Eating every pellet triggers Game Over");
  yield return new WaitForSeconds(3.2f);
  GameObject.Find("PanelL1").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;yield return new WaitForSeconds(4.3f);
  Check(SceneManager.GetActiveScene().name=="Level1","Actual Level 1 button opens first level");
  GameSystem.I.exitButton.onClick.Invoke();yield return null;yield return null;
  Check(SceneManager.GetActiveScene().name=="StartScene","Actual Exit button returns to menu");
  // These new record keys belong to this project's validation runs, not imported saves.
  foreach(int number in new[]{1,2}) { PlayerPrefs.DeleteKey("PacStudent.Level"+number+".Score");PlayerPrefs.DeleteKey("PacStudent.Level"+number+".Time"); }
  PlayerPrefs.Save();
  results.Add("TOTAL_ERRORS="+errors);Flush();Application.logMessageReceived-=Log;EditorApplication.isPlaying=false;
 }
}
#endif
