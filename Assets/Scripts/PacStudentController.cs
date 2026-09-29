using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public LevelGenerator level;
    public float cellsPerSecond = 6;
    public Vector3Int lastInput, currentInput;
    public Animator animator;
    public ParticleSystem dust, impact, death;
    public Vector3 spawn;
    Vector3 start, target;
    float progress;
    bool moving, bumped;
    public bool IsMoving => moving;
    void Start() { spawn = level.Center(new Vector3Int(-13,12,0)); ResetPlayer(); }
    public void ResetPlayer()
    {
        transform.position = target = spawn; start = spawn; progress = 0;
        lastInput = currentInput = Vector3Int.zero; moving = bumped = false;
        animator.speed = 1; animator.Play("MoveBT"); Face(Vector3Int.right); animator.Update(0); animator.speed = 0;
        if (dust) dust.Stop(); SfxManager.I?.StopMoveLoop();
    }
    void Face(Vector3Int d) { animator.SetFloat("MoveX",d.x); animator.SetFloat("MoveY",d.y); }
    void Update()
    {
        if (!GameSystem.I || !GameSystem.I.Playing) { if(dust) dust.Stop(); SfxManager.I?.StopMoveLoop(); return; }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) SetInput(Vector3Int.up);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) SetInput(Vector3Int.right);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) SetInput(Vector3Int.down);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) SetInput(Vector3Int.left);
        if (Input.GetKeyDown(KeyCode.Space)) GameSystem.I.TryPulse();
        Advance(Time.deltaTime);
    }
    public void SetInput(Vector3Int d) { lastInput=d; bumped=false; }
    public void Advance(float delta)
    {
        // Carry unused time into subsequent tiles, preserving speed at low frame rates.
        float budget = delta * cellsPerSecond;
        for(int guard=0; guard<32; guard++)
        {
            if (!moving)
            {
                var c=level.Cell(transform.position);
                var d=level.CanStep(c,lastInput) ? lastInput : level.CanStep(c,currentInput) ? currentInput : Vector3Int.zero;
                if (d==Vector3Int.zero)
                {
                    if(lastInput!=Vector3Int.zero && !bumped) { bumped=true; SfxManager.I?.PlayHitWall(); if(impact) { impact.transform.position=transform.position+(Vector3)lastInput*.45f; impact.Play(); } }
                    animator.speed=0; if(dust) dust.Stop(); SfxManager.I?.StopMoveLoop(); return;
                }
                currentInput=d; bumped=false; start=transform.position; target=level.Center(c+d); progress=0; moving=true;
                Face(d); animator.speed=1; if(dust && !dust.isPlaying) dust.Play();
                SfxManager.I?.StartMoveLoop(level.pellets.HasTile(c+d)||level.powerPellets.HasTile(c+d));
            }
            float used=Mathf.Min(1-progress,budget); progress+=used; budget-=used;
            transform.position=Vector3.Lerp(start,target,progress);
            if(progress<1) return;
            moving=false;
            var arrived=level.Cell(target);
            if(arrived.x<level.bounds.xMin) arrived.x=level.bounds.xMax-1;
            else if(arrived.x>=level.bounds.xMax) arrived.x=level.bounds.xMin;
            transform.position=target=level.Center(arrived);
            GameSystem.I.Consume(arrived);
            if(!GameSystem.I.Playing || budget<=0) return;
        }
    }
    public void Die()
    {
        moving=false; animator.speed=1; animator.Play("Death"); if(dust) dust.Stop(); if(death) death.Play(); SfxManager.I?.StopMoveLoop();
    }
}
