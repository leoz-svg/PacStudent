using UnityEngine;
using TMPro;
public class MenuController : MonoBehaviour
{
    public TextMeshProUGUI[] scores,times;
    public RectTransform border;
    public RectTransform[] dots;
    void Start()
    {
        for(int i=0;i<2;i++)
        {
            string key="PacStudent.Level"+(i+1);
            scores[i].text=PlayerPrefs.GetInt(key+".Score",0).ToString("D6");
            times[i].text=HUDController.FormatTime(PlayerPrefs.GetFloat(key+".Time",0));
        }
    }
    void Update()
    {
        float w=border.rect.width-20,h=border.rect.height-20,length=2*(w+h);
        for(int i=0;i<dots.Length;i++)
        {
            float p=Mathf.Repeat(Time.unscaledTime*90+i*length/dots.Length,length);
            Vector2 v=p<w?new Vector2(-w/2+p,h/2):p<w+h?new Vector2(w/2,h/2-(p-w)):p<2*w+h?new Vector2(w/2-(p-w-h),-h/2):new Vector2(-w/2,-h/2+(p-2*w-h));
            dots[i].anchoredPosition=v;
        }
    }
}
