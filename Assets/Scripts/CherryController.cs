using UnityEngine;

public class CherryController : MonoBehaviour
{
    public LevelGenerator level;
    public Sprite sprite;
    SpriteRenderer cherry;
    Vector3 from,to;
    float wait=5,elapsed,duration;
    void Update()
    {
        if(!GameSystem.I) return;
        if(!GameSystem.I.Playing) { if(GameSystem.I.runTime==0)wait=Mathf.Max(0,wait-Time.deltaTime);return; }
        if(!cherry)
        {
            wait-=Time.deltaTime; if(wait>0) return;
            Vector3 center=level.bounds.center;
            float angle=Random.Range(0f,Mathf.PI*2);
            Vector3 direction=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0);
            float edge=Mathf.Min(level.bounds.size.x*.5f/Mathf.Max(.001f,Mathf.Abs(direction.x)),level.bounds.size.y*.5f/Mathf.Max(.001f,Mathf.Abs(direction.y)));
            from=center+direction*(edge+1);
            var camera=Camera.main;
            float outside=Mathf.Min(camera.orthographicSize*camera.aspect/Mathf.Max(.001f,Mathf.Abs(direction.x)),camera.orthographicSize/Mathf.Max(.001f,Mathf.Abs(direction.y)))+2;
            to=center-direction*Mathf.Max(outside,edge+1);duration=Vector3.Distance(from,to)/5; elapsed=0;
            cherry=new GameObject("BonusCherry").AddComponent<SpriteRenderer>(); cherry.sprite=sprite; cherry.sortingOrder=50; cherry.transform.position=from;
            cherry.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
        }
        elapsed+=Time.deltaTime; cherry.transform.position=Vector3.Lerp(from,to,elapsed/duration);
        if(Vector3.Distance(cherry.transform.position,GameSystem.I.player.transform.position)<.65f)
        { GameSystem.I.AddScore(100); SfxManager.I?.PlayCherry(); Clear(); }
        else if(elapsed>=duration) Clear();
    }
    public void Clear() { if(cherry) Destroy(cherry.gameObject); cherry=null; wait=5; }
    void OnDestroy() { if(cherry) Destroy(cherry.gameObject); }
}
