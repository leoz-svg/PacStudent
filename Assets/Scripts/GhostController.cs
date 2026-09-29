using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public enum GhostState { Normal, Scared, Recovering, Dead }
    public int number=1;
    public LevelGenerator level;
    public PacStudentController player;
    public Animator animator;
    public GhostState State {get; private set;}
    public Vector3 spawn;
    Vector3 start,target;
    Vector3Int direction=Vector3Int.up;
    float progress;
    bool moving,leaving=true;
    int corner;
    static readonly Vector3Int[] Corners = { new Vector3Int(-13,12,0),new Vector3Int(12,12,0),new Vector3Int(12,-13,0),new Vector3Int(-13,-13,0) };
    public float localScared;
    SpriteRenderer body;
    TMPro.TMP_Text stateLabel;
    string originalLabel;
    void Awake()
    {
        body=GetComponent<SpriteRenderer>();
        stateLabel=GetComponentInChildren<TMPro.TMP_Text>();
        originalLabel=number.ToString();
    }
    void Start() { ResetGhost(); }
    public void ResetGhost()
    {
        transform.position=target=spawn; start=spawn; progress=0; moving=false; leaving=true; localScared=0; corner=0;
        direction=(number==1||number==3)?Vector3Int.up:Vector3Int.down;
        State=GhostState.Normal; ApplyState();
    }
    public void Scare(float duration) { if(State!=GhostState.Dead) localScared=Mathf.Max(localScared,duration); }
    public void Kill() { State=GhostState.Dead; moving=false; ApplyState(); }
    void ApplyState()
    {
        animator.speed=1;
        animator.Play(State==GhostState.Normal ? "MoveBT" : State.ToString());
    }
    void LateUpdate()
    {
        bool scared=State==GhostState.Scared||State==GhostState.Recovering;
        // Keep each ghost's own silhouette; cyan tint and an explicit marker
        // communicate vulnerability, with rapid flashing during recovery.
        float alpha=State==GhostState.Recovering ? (Mathf.Sin(Time.time*18)>0?.35f:1f) : 1f;
        if(body) body.color=scared ? new Color(.55f,1f,1f,alpha) : Color.white;
        if(stateLabel)
        {
            stateLabel.text=originalLabel+(State==GhostState.Scared?" !":State==GhostState.Recovering?" !!":State==GhostState.Dead?" x":"");
            stateLabel.color=scared?Color.cyan:Color.white;
        }
    }
    void Update()
    {
        if(!GameSystem.I || !GameSystem.I.Playing) { animator.speed=0; return; }
        animator.speed=1;
        localScared=Mathf.Max(0,localScared-Time.deltaTime);
        if(State==GhostState.Dead)
        {
            transform.position=Vector3.MoveTowards(transform.position,spawn,player.cellsPerSecond*.45f*Time.deltaTime);
            if(Vector3.Distance(transform.position,spawn)<.001f) { moving=false; leaving=true; State=DesiredState(); ApplyState(); }
            return;
        }
        var desired=DesiredState(); if(State!=desired) { State=desired; ApplyState(); }
        float budget=Time.deltaTime*player.cellsPerSecond*(State==GhostState.Normal?.9f:.45f);
        for(int guard=0;guard<32;guard++)
        {
            if(!moving)
            {
                var c=level.Cell(transform.position);
                if(leaving && ((number==1||number==3)?c.y>=2:c.y<=-3)) leaving=false;
                direction=Choose(c);
                if(direction==Vector3Int.zero) return;
                start=transform.position; target=level.Center(c+direction); progress=0; moving=true;
                animator.SetFloat("MoveX",direction.x); animator.SetFloat("MoveY",direction.y);
            }
            float used=Mathf.Min(1-progress,budget); progress+=used; budget-=used;
            transform.position=Vector3.Lerp(start,target,progress);
            if(progress<1) return;
            moving=false; if(budget<=0) return;
        }
    }
    GhostState DesiredState()
    {
        float t=Mathf.Max(localScared,GameSystem.I.ScareTimer);
        return t<=0?GhostState.Normal:t<=3?GhostState.Recovering:GhostState.Scared;
    }
    public Vector3Int Choose(Vector3Int c)
    {
        if(leaving)
        {
            if(c.x < -1) return Vector3Int.right;
            if(c.x > 0) return Vector3Int.left;
            return number==1||number==3?Vector3Int.up:Vector3Int.down;
        }
        var options=new List<Vector3Int>();
        foreach(var d in LevelGenerator.Directions) if(level.CanStep(c,d,true,false)) options.Add(d);
        if(options.Count>1) options.Remove(-direction);
        if(options.Count==0) return Vector3Int.zero;
        if(State==GhostState.Normal && number==4)
        {
            // Visit the four outside corners clockwise. A small grid search handles
            // the breaks in the outer corridors; no Unity navigation is used.
            if(c==Corners[corner]) corner=(corner+1)%4;
            var distance=Distances(Corners[corner]);
            int best=int.MaxValue; var choice=options[0];
            foreach(var d in options) if(distance.TryGetValue(c+d,out int value)&&value<best) { best=value;choice=d; }
            return choice;
        }
        if(number!=3 || State!=GhostState.Normal)
        {
            bool flee=State!=GhostState.Normal || number==1;
            float before=(level.Center(c)-player.transform.position).sqrMagnitude;
            var preferred=options.FindAll(d=>flee?(level.Center(c+d)-player.transform.position).sqrMagnitude>=before:(level.Center(c+d)-player.transform.position).sqrMagnitude<=before);
            if(preferred.Count>0) options=preferred;
        }
        return options[Random.Range(0,options.Count)];
    }
    Dictionary<Vector3Int,int> Distances(Vector3Int goal)
    {
        var result=new Dictionary<Vector3Int,int>{{goal,0}};var queue=new Queue<Vector3Int>();queue.Enqueue(goal);
        while(queue.Count>0)
        {
            var c=queue.Dequeue();
            foreach(var d in LevelGenerator.Directions)
            {
                var n=c+d;
                if(result.ContainsKey(n)||!level.CanStep(c,d,true,false)) continue;
                result[n]=result[c]+1;queue.Enqueue(n);
            }
        }
        return result;
    }
}
