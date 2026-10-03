using UnityEngine;
public class StarRatingView:MonoBehaviour
{
 public UnityEngine.UI.Image[] stars;
 public int savedLevel;
 void Start(){if(savedLevel>0)Show(PlayerPrefs.GetInt(RoundRating.Key(savedLevel),0));}
 public void Show(int count){for(int i=0;i<stars.Length;i++)stars[i].color=i<count?new Color(1,.8f,.25f):new Color(.3f,.27f,.36f);}
}
