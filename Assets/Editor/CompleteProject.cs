using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public static class CompleteProject
{
 const string Root="Assets/";
 static T Load<T>(string p) where T:UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(Root+p);
 static Sprite Sprite(string p)=>Load<Sprite>("Art/"+p);
 static T Find<T>(string name) where T:Component => GameObject.Find(name).GetComponent<T>();
 static void Remove<T>() where T:Component { foreach(var c in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(c); }
 static void Anchor(RectTransform r,Vector2 min,Vector2 max,Vector2 pos,Vector2 size) { r.anchorMin=min;r.anchorMax=max;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one; }
 static TextMeshProUGUI Text(string name,Transform parent,string value,int size)
 {
  var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
  var t=go.GetComponent<TextMeshProUGUI>();t.font=Load<TMP_FontAsset>("TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");t.text=value;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;t.raycastTarget=false;return t;
 }
 static AnimationClip SpriteClip(string name,Sprite[] sprites,bool loop)
 {
  string path=Root+"Animations/Completed/"+name+".anim";
  var clip=new AnimationClip();clip.frameRate=8;
  var frames=new ObjectReferenceKeyframe[sprites.Length+1];
  for(int i=0;i<frames.Length;i++) frames[i]=new ObjectReferenceKeyframe{time=i*.16f,value=sprites[Mathf.Min(i,sprites.Length-1)]};
  AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),frames);
  var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
  AssetDatabase.CreateAsset(clip,path);return clip;
 }
 static RuntimeAnimatorController Controller(int ghost)
 {
  string label=ghost==0?"PacStudent":"Ghost"+ghost;
  var controller=AnimatorController.CreateAnimatorControllerAtPath(Root+"Animations/Completed/"+label+".controller");
  controller.AddParameter("MoveX",AnimatorControllerParameterType.Float);controller.AddParameter("MoveY",AnimatorControllerParameterType.Float);
  var state=controller.layers[0].stateMachine.AddState("MoveBT");controller.layers[0].stateMachine.defaultState=state;
  var tree=new BlendTree{name="Directions",blendType=BlendTreeType.SimpleDirectional2D,blendParameter="MoveX",blendParameterY="MoveY"};AssetDatabase.AddObjectToAsset(tree,controller);
  string prefix=ghost==0?"Player/Player":"Ghost"+ghost+"/Ghost"+ghost;
  tree.AddChild(Load<AnimationClip>("Animations/"+prefix+"_Up.anim"),Vector2.up);tree.AddChild(Load<AnimationClip>("Animations/"+prefix+"_Right.anim"),Vector2.right);tree.AddChild(Load<AnimationClip>("Animations/"+prefix+"_Down.anim"),Vector2.down);tree.AddChild(Load<AnimationClip>("Animations/"+prefix+"_Left.anim"),Vector2.left);state.motion=tree;
  if(ghost==0) controller.layers[0].stateMachine.AddState("Death").motion=SpriteClip("PacDeath",new[]{Sprite("player/PacDeath/PacDeath-1.png.png"),Sprite("player/PacDeath/PacDeath-2.png.png")},false);
  else
  {
   var scared=Sprite("Ghosts/GhostScared/GhostScared.png.png");var dead=Sprite("Ghosts/GhostDeath/GhostDeath.png.png");
   var normal=Sprite("Ghosts/Ghost"+ghost+"_NormalState/Right/Ghost"+ghost+"-NormalRight.png.png");
   controller.layers[0].stateMachine.AddState("Scared").motion=SpriteClip(label+"Scared",new[]{scared,scared},true);
   controller.layers[0].stateMachine.AddState("Recovering").motion=SpriteClip(label+"Recovering",new[]{scared,normal},true);
   controller.layers[0].stateMachine.AddState("Dead").motion=SpriteClip(label+"Dead",new[]{dead,dead},true);
  }
  return controller;
 }
 static ParticleSystem Particles(string name,Transform parent,Color color,bool loop,float speed,int count)
 {
  var go=new GameObject(name);go.transform.SetParent(parent,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=ps.main;main.loop=loop;main.playOnAwake=false;main.startLifetime=.45f;main.startSpeed=speed;main.startSize=.12f;main.startColor=color;main.simulationSpace=ParticleSystemSimulationSpace.World;
  var emission=ps.emission;emission.rateOverTime=loop?18:0;if(!loop)emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
  var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=.25f;
  var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Load<Material>("Animations/Completed/Particles.mat");renderer.sortingOrder=60;
  return ps;
 }
 public static void Run()
 {
  if(!AssetDatabase.IsValidFolder(Root+"Animations/Completed")) AssetDatabase.CreateFolder(Root+"Animations","Completed");
  var mat=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(mat,Root+"Animations/Completed/Particles.mat");
  var controllers=Enumerable.Range(0,5).Select(Controller).ToArray();
  Rename("Start","StartScene");Rename("Level2","InnovationScene");
  SetupLevel("Level1",1,controllers);
  // The original second scene has no completed innovation. Reuse the repaired authored map.
  var level=EditorSceneManager.OpenScene(Root+"Scenes/Level1.unity");
  EditorSceneManager.SaveScene(level,Root+"Scenes/InnovationScene.unity");
  var game=UnityEngine.Object.FindFirstObjectByType<GameSystem>();game.levelIndex=2;game.hud.SetLevel(2);game.abilityText.gameObject.SetActive(true);game.abilityText.text="ENERGY 0/100  |  SPACE: PULSE (50)  |  EAT TO RECHARGE";
  EditorSceneManager.SaveScene(level);
  SetupMenu();
  EditorBuildSettings.scenes=new[]{"StartScene","Level1","InnovationScene"}.Select(n=>new EditorBuildSettingsScene(Root+"Scenes/"+n+".unity",true)).ToArray();
  PlayerSettings.productName="PacStudent - Midnight Cram";PlayerSettings.companyName="PacStudent";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
  QualitySettings.antiAliasing=0;AssetDatabase.SaveAssets();Debug.Log("COMPLETION_SETUP_SUCCESS");
 }
 static void Rename(string oldName,string newName) { if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Root+"Scenes/"+oldName+".unity")) { string error=AssetDatabase.RenameAsset(Root+"Scenes/"+oldName+".unity",newName);if(!string.IsNullOrEmpty(error))throw new Exception(error); } }
 static void SetupLevel(string name,int index,RuntimeAnimatorController[] controllers)
 {
  var scene=EditorSceneManager.OpenScene(Root+"Scenes/"+name+".unity");
  foreach(var go in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go.gameObject);
  Remove<PlayerMoveGrid_HoldToMove>();Remove<PlayerController>();Remove<SnapToGridCenter>();Remove<GhostControllerGrid>();Remove<PelletTrigger>();Remove<PowerPelletTrigger>();
  var map=GameObject.Find("Grid").AddComponent<LevelGenerator>();map.grid=Find<Grid>("Grid");map.walls=Find<Tilemap>("Walls");map.gates=Find<Tilemap>("Gates");map.pellets=Find<Tilemap>("Pellets");map.powerPellets=Find<Tilemap>("PowerPellets");map.RefreshMap();
  // Power pills remain manually authored tiles; blink uses the renderer, never AnimatedTile.
  if(map.powerPellets.GetUsedTilesCount()==0)
  {
   var tile=ScriptableObject.CreateInstance<Tile>();tile.sprite=Sprite("piskel tiles/piskel tiles/large bean.png.png");AssetDatabase.CreateAsset(tile,Root+"Animations/Completed/PowerPill.asset");
   foreach(var c in new[]{new Vector3Int(-13,8,0),new Vector3Int(12,8,0),new Vector3Int(-13,-9,0),new Vector3Int(12,-9,0)}) { map.powerPellets.SetTile(c,tile);map.pellets.SetTile(c,null); }
  }
  var p=GameObject.Find("Player").AddComponent<PacStudentController>();p.level=map;p.animator=p.GetComponent<Animator>();p.animator.runtimeAnimatorController=controllers[0];p.spawn=map.Center(new Vector3Int(-13,12,0));p.transform.position=p.spawn;
  p.dust=Particles("RunningDust",p.transform,new Color(.7f,.5f,.35f),true,.8f,0);p.impact=Particles("WallImpact",null,Color.yellow,false,2,12);p.death=Particles("DeathBurst",p.transform,new Color(1,.2f,.3f),false,4,50);
  var game=UnityEngine.Object.FindFirstObjectByType<GameSystem>();game.level=map;game.player=p;game.levelIndex=index;game.scaredDuration=10;game.startLives=3;
  game.ghosts=new GhostController[4];
  for(int i=0;i<4;i++)
  {
   var go=GameObject.Find("Ghost"+(i+1));var g=go.AddComponent<GhostController>();game.ghosts[i]=g;g.number=i+1;g.level=map;g.player=p;g.animator=go.GetComponent<Animator>();g.animator.runtimeAnimatorController=controllers[i+1];g.spawn=map.Center(new Vector3Int(-2+i,i%2==0?0:-1,0));go.transform.position=g.spawn;
   var label=go.GetComponentInChildren<TextMeshProUGUI>(true);
   var canvasGo=new GameObject("Ghost"+(i+1)+"Canvas",typeof(RectTransform),typeof(Canvas));canvasGo.transform.SetParent(go.transform,false);var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.sortingOrder=100;
   var cr=canvasGo.GetComponent<RectTransform>();cr.sizeDelta=new Vector2(100,50);cr.localScale=Vector3.one*.02f;cr.localPosition=new Vector3(0,.8f,0);
   if(!label) { var oldLabel=go.GetComponentInChildren<TMP_Text>(true);if(oldLabel)oldLabel.gameObject.SetActive(false);label=Text("Number",cr,(i+1).ToString(),36); }
   label.transform.SetParent(cr,false);Anchor(label.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);label.text=(i+1).ToString();label.fontSize=36;label.color=Color.white;label.alignment=TextAlignmentOptions.Center;
  }
  foreach(var body in UnityEngine.Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None)) { body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0; }
  game.cherry=game.gameObject.AddComponent<CherryController>();game.cherry.level=map;game.cherry.sprite=Sprite("piskel tiles/BonusScoreCherry.png.png");
  var tex=new Texture2D(1,1);tex.SetPixel(0,0,Color.white);tex.Apply();File.WriteAllBytes(Root+"Animations/Completed/Mask.png",tex.EncodeToPNG());AssetDatabase.ImportAsset(Root+"Animations/Completed/Mask.png");
  var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"Animations/Completed/Mask.png");importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=1;importer.SaveAndReimport();
  var mask=new GameObject("LevelCherryMask").AddComponent<SpriteMask>();mask.sprite=Load<Sprite>("Animations/Completed/Mask.png");mask.transform.position=map.bounds.center;mask.transform.localScale=new Vector3(map.bounds.size.x,map.bounds.size.y,1);mask.isCustomRangeActive=true;mask.frontSortingOrder=51;mask.backSortingOrder=49;
  game.music=UnityEngine.Object.FindFirstObjectByType<MusicManager>();game.music.bgmIntro=Load<AudioClip>("AudioClips/BGM-GameIntro.wav");game.music.bgmGhostNormal=Load<AudioClip>("AudioClips/BGM-GhostNormal.wav");game.music.bgmGhostScared=Load<AudioClip>("AudioClips/BGM-GhostScared.wav");game.music.bgmGhostDead=Load<AudioClip>("AudioClips/BGM-GhostDead.wav");
  var hud=game.hud;var canvasHud=hud.GetComponent<Canvas>();if(!canvasHud) canvasHud=hud.gameObject.AddComponent<Canvas>();canvasHud.renderMode=RenderMode.ScreenSpaceOverlay;
  var scaler=hud.GetComponent<UnityEngine.UI.CanvasScaler>();if(!scaler)scaler=hud.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
  if(!hud.GetComponent<UnityEngine.UI.GraphicRaycaster>())hud.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
  string[] labels={"LevelLabel","LivesLabel","ScoreLabel","TimeLabel","ScaredLabel"};float[] xs={.1f,.28f,.48f,.67f,.85f};
  for(int i=0;i<labels.Length;i++) { var t=Find<TextMeshProUGUI>(labels[i]);t.transform.SetParent(hud.transform,false);Anchor(t.rectTransform,new Vector2(xs[i],1),new Vector2(xs[i],1),new Vector2(0,-24),new Vector2(180,32));t.fontSize=22; }
  Anchor(hud.scoreValue.rectTransform,new Vector2(.48f,1),new Vector2(.48f,1),new Vector2(0,-55),new Vector2(170,32));
  Anchor(hud.timeValue.rectTransform,new Vector2(.67f,1),new Vector2(.67f,1),new Vector2(0,-55),new Vector2(170,32));
  Anchor(hud.scaredValue.rectTransform,new Vector2(.85f,1),new Vector2(.85f,1),new Vector2(0,-55),new Vector2(130,32));
  hud.livesContainer.SetParent(hud.transform,false);Anchor((RectTransform)hud.livesContainer,new Vector2(.28f,1),new Vector2(.28f,1),new Vector2(0,-55),new Vector2(130,32));
  var icons=hud.livesContainer.GetComponentsInChildren<UnityEngine.UI.Image>();for(int i=0;i<icons.Length;i++) Anchor(icons[i].rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2((i-1)*34,0),new Vector2(28,28));
  game.exitButton=Find<UnityEngine.UI.Button>("ExitButton");game.exitButton.transform.SetParent(hud.transform,false);Anchor(game.exitButton.GetComponent<RectTransform>(),new Vector2(1,0),new Vector2(1,0),new Vector2(-85,32),new Vector2(140,45));
  game.abilityText=Text("PulseInstructions",hud.transform,"",20);Anchor(game.abilityText.rectTransform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-50,30),new Vector2(900,40));game.abilityText.gameObject.SetActive(index==2);
  game.pulse=Particles("EnergyPulse",null,Color.cyan,false,12,150);
  var panel=new GameObject("RoundBlocker",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(hud.transform,false);Anchor(panel.GetComponent<RectTransform>(),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);panel.GetComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.72f);game.blockingPanel=panel;
  game.message=Text("RoundMessage",panel.transform,"3",100);Anchor(game.message.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);panel.SetActive(false);
  var cam=Camera.main;cam.transform.position=new Vector3(0,0,-10);cam.orthographicSize=17;cam.allowMSAA=false;cam.allowHDR=false;cam.gameObject.AddComponent<CameraFit>();
  EditorSceneManager.SaveScene(scene);Debug.Log("LEVEL_SETUP food="+map.CountFood()+" ghosts="+game.ghosts.Length);
 }
 public static void Finish()
 {
  SetupMenu();
  EditorBuildSettings.scenes=new[]{"StartScene","Level1","InnovationScene"}.Select(n=>new EditorBuildSettingsScene(Root+"Scenes/"+n+".unity",true)).ToArray();
  PlayerSettings.productName="PacStudent - Midnight Cram";PlayerSettings.companyName="PacStudent";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;QualitySettings.antiAliasing=0;
  AssetDatabase.SaveAssets();Debug.Log("COMPLETION_SETUP_SUCCESS");
 }
 public static void FixLayout()
 {
  foreach(var sceneName in new[]{"StartScene","Level1","InnovationScene"})
  {
   var scene=EditorSceneManager.OpenScene(Root+"Scenes/"+sceneName+".unity");
   foreach(var t in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include,FindObjectsSortMode.None)) { t.margin=Vector4.zero;t.alignment=TextAlignmentOptions.Center;t.enableAutoSizing=false;t.raycastTarget=false; }
   if(sceneName!="StartScene")
   {
    var hud=UnityEngine.Object.FindFirstObjectByType<HUDController>();hud.transform.SetParent(null,false);
    var canvas=hud.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;
    var r=hud.GetComponent<RectTransform>();r.localScale=Vector3.one;
    hud.scoreValue.fontSize=28;hud.timeValue.fontSize=28;hud.scaredValue.fontSize=28;
    foreach(var t in hud.GetComponentsInChildren<TextMeshProUGUI>(true)) if(t.name=="Text (TMP)")t.fontSize=24;
    Camera.main.backgroundColor=new Color(.035f,.025f,.07f);Camera.main.GetComponent<CameraFit>().height=36;
   }
   EditorSceneManager.SaveScene(scene);
  }
  PlayerSettings.runInBackground=true;AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"Scenes/StartScene.unity");
 }
 static void SetupMenu()
 {
  var scene=EditorSceneManager.OpenScene(Root+"Scenes/StartScene.unity");Remove<GameSystem>();
  var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
  foreach(var layout in canvas.GetComponentsInChildren<UnityEngine.UI.LayoutGroup>(true)) layout.enabled=false;
  foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>(true)) { text.raycastTarget=false;text.alignment=TextAlignmentOptions.Center; }
  var background=GameObject.Find("BG").GetComponent<UnityEngine.UI.Image>();background.transform.SetParent(canvas.transform,false);background.transform.SetAsFirstSibling();Anchor(background.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);background.color=new Color(.055f,.035f,.07f,1);background.sprite=null;background.raycastTarget=false;
  var title=canvas.GetComponentsInChildren<TextMeshProUGUI>(true).First(t=>t.text.Contains("PAC STUDENT"));title.transform.SetParent(canvas.transform,false);Anchor(title.rectTransform,new Vector2(.5f,.84f),new Vector2(.5f,.84f),Vector2.zero,new Vector2(800,85));title.text="PacStudent";title.fontSize=70;title.color=new Color(1,.28f,.4f);
  var slogan=Find<TextMeshProUGUI>("Slogan");slogan.transform.SetParent(canvas.transform,false);Anchor(slogan.rectTransform,new Vector2(.5f,.74f),new Vector2(.5f,.74f),Vector2.zero,new Vector2(800,40));slogan.fontSize=24;slogan.color=Color.white;
  var menu=canvas.gameObject.AddComponent<MenuController>();menu.scores=new TextMeshProUGUI[2];menu.times=new TextMeshProUGUI[2];
  for(int i=0;i<2;i++)
  {
   var panel=GameObject.Find("PanelL"+(i+1));menu.scores[i]=panel.GetComponentsInChildren<TextMeshProUGUI>().First(t=>t.name=="HighScoreValue");menu.times[i]=panel.GetComponentsInChildren<TextMeshProUGUI>().First(t=>t.name=="TimeValue");
   panel.transform.SetParent(canvas.transform,false);Anchor(panel.GetComponent<RectTransform>(),new Vector2(i==0?.29f:.71f,.35f),new Vector2(i==0?.29f:.71f,.35f),Vector2.zero,new Vector2(320,270));
   var bg=panel.GetComponent<UnityEngine.UI.Image>();bg.color=new Color(.2f,.075f,.12f,1);bg.raycastTarget=true;
   foreach(var t in panel.GetComponentsInChildren<TextMeshProUGUI>()) { float y=t.name=="Title"?.83f:t.name=="HighScoreLabel"?.62f:t.name=="HighScoreValue"?.48f:t.name=="TimeLabel"?.32f:.18f;Anchor(t.rectTransform,new Vector2(.5f,y),new Vector2(.5f,y),Vector2.zero,new Vector2(300,44));t.fontSize=t.name=="Title"?32:24;t.color=Color.white; }
  }
  var border=new GameObject("AnimatedBorder",typeof(RectTransform));border.transform.SetParent(canvas.transform,false);menu.border=border.GetComponent<RectTransform>();Anchor(menu.border,Vector2.zero,Vector2.one,Vector2.zero,new Vector2(-24,-24));
  menu.dots=new RectTransform[40];for(int i=0;i<40;i++) { var dot=new GameObject("BorderDot",typeof(RectTransform),typeof(UnityEngine.UI.Image));dot.transform.SetParent(border.transform,false);var img=dot.GetComponent<UnityEngine.UI.Image>();img.color=new Color(.9f,.25f,.4f);img.raycastTarget=false;menu.dots[i]=dot.GetComponent<RectTransform>();menu.dots[i].sizeDelta=new Vector2(7,7); }
  foreach(var c in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.CanvasScaler>(FindObjectsSortMode.None)) { c.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;c.referenceResolution=new Vector2(1280,720);c.matchWidthOrHeight=.5f; }
  var source=menu.gameObject.AddComponent<AudioSource>();source.clip=Load<AudioClip>("AudioClips/BGM-StartScene.wav");source.loop=true;source.playOnAwake=true;source.volume=.3f;
  for(int i=0;i<5;i++)
  {
   var original=GameObject.Find(i==0?"Player":"Ghost"+i);var icon=new GameObject("CharacterPreview"+i,typeof(RectTransform),typeof(UnityEngine.UI.Image));icon.transform.SetParent(canvas.transform,false);var image=icon.GetComponent<UnityEngine.UI.Image>();image.sprite=original.GetComponent<SpriteRenderer>().sprite;image.preserveAspect=true;image.raycastTarget=false;Anchor(image.rectTransform,new Vector2(.34f+i*.08f,.63f),new Vector2(.34f+i*.08f,.63f),Vector2.zero,new Vector2(45,45));
  }
  var help=Text("Controls",canvas.transform,"WASD / ARROWS TO MOVE   |   LEVEL 2: SPACE TO PULSE",20);Anchor(help.rectTransform,new Vector2(.5f,.085f),new Vector2(.5f,.085f),Vector2.zero,new Vector2(950,40));
  foreach(var b in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None)) { var colors=b.colors;colors.highlightedColor=new Color(1,.6f,.65f);colors.pressedColor=new Color(.6f,.25f,.4f);b.colors=colors; }
  EditorSceneManager.SaveScene(scene);
 }
 public static void Build()
 {
  var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"../PacStudent-Windows/PacStudent.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
  Debug.Log("WINDOWS_BUILD_SUCCESS");
 }
}
