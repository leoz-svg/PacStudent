using System;
using System.Collections.Generic;

// Pure per-round rules: no saved progression or Unity scene dependencies.
public sealed class RoundChallenges
{
 public enum Kind { Chain, PulseHits, SuperPulses, Ghosts, PowerPellets, HuntPellets }
 public sealed class Task
 {
  public Kind Type { get; private set; }
  public int Target { get; private set; }
  public int Progress { get; private set; }
  public bool Complete => Progress >= Target;
  public string Label => Labels[(int)Type];
  internal bool Paid;
  internal Task(Kind kind) { Type=kind; Target=Targets[(int)kind]; }
  internal void Observe(int value) { Progress=Math.Min(Target,Math.Max(Progress,value)); }
 }
 static readonly int[] Targets={25,3,2,3,4,20};
 static readonly string[] Labels={"BEST CHAIN","ONE PULSE HITS","SUPER PULSES","GHOSTS CAUGHT","POWER PELLETS","HUNT PELLETS"};
 readonly Task[] tasks;
 public IReadOnlyList<Task> Tasks => tasks;
 public int CompletedCount { get { int count=0;foreach(var task in tasks)if(task.Complete)count++;return count; } }
 public int Bonus { get; private set; }
 public const int Reward=200;
 public RoundChallenges(int seed)
 {
  var kinds=new[]{Kind.Chain,Kind.PulseHits,Kind.SuperPulses,Kind.Ghosts,Kind.PowerPellets,Kind.HuntPellets};
  var random=new Random(seed);
  for(int i=kinds.Length-1;i>0;i--){int j=random.Next(i+1);var temp=kinds[i];kinds[i]=kinds[j];kinds[j]=temp;}
  tasks=new[]{new Task(kinds[0]),new Task(kinds[1]),new Task(kinds[2])};
 }
 public int Observe(int chain,int pulseHits,int superPulses,int ghosts,int powerPellets,int huntPellets)
 {
  int reward=0;
  foreach(var task in tasks)
  {
   int value=task.Type==Kind.Chain?chain:task.Type==Kind.PulseHits?pulseHits:task.Type==Kind.SuperPulses?superPulses:task.Type==Kind.Ghosts?ghosts:task.Type==Kind.PowerPellets?powerPellets:huntPellets;
   task.Observe(value);
   if(task.Complete&&!task.Paid){task.Paid=true;reward+=Reward;}
  }
  Bonus+=reward;return reward;
 }
}

public static class RoundRating
{
 public static float TimeLimit(int level)=>level==2?240f:180f;
 public static bool Advanced(int level,float seconds,int tasks)=>seconds<=TimeLimit(level)&&(level!=2||tasks==3);
 public static int Evaluate(bool cleared,int level,int lives,float seconds,int tasks)
 { return !cleared?0:1+(lives>=2?1:0)+(Advanced(level,seconds,tasks)?1:0); }
 public static string Key(int level)=>"PacStudent.Level"+level+".Stars";
}
