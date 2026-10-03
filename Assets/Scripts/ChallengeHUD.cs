using UnityEngine;
using TMPro;
public class ChallengeHUD:MonoBehaviour
{
 public GameSystem game;
 public TextMeshProUGUI title,rows,ratingGuide;
 string last;
 void LateUpdate()
 {
  if(game==null||game.Challenges==null)return;
  var rules=game.Challenges;
  string text="";
  foreach(var task in rules.Tasks)text+=(task.Complete?"<color=#66FFBF>DONE  ":"<color=#EEEEFF>")+task.Label+"\n"+task.Progress+" / "+task.Target+(task.Complete?"   +200":"")+"</color>\n";
  if(text==last)return;last=text;rows.text=text;
  title.text="TASKS "+rules.CompletedCount+" / 3";
 }
}
