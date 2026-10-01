from pathlib import Path
import numpy as np, wave
out=Path('outputs/Pacstudent_Assess4/Assets/AudioClips/Music/NeonMaze');out.mkdir(exist_ok=True)
sr=44100; seconds=16; rng=np.random.default_rng(420)
def tone(a,at,note,dur,gain,kind=0):
 n=int(dur*sr);t=np.arange(n)/sr;freq=440*2**((note-69)/12)
 w=np.sin(2*np.pi*freq*t)
 if kind==0: w+=.28*np.sin(6*np.pi*freq*t)+.12*np.sin(10*np.pi*freq*t)
 env=np.minimum(t/.012,1)*np.minimum((dur-t)/.055,1)
 x=w*env*gain;start=int(at*sr);end=min(len(a),start+n);a[start:end]+=x[:end-start]
roots=[45,41,48,43]; melody=[0,7,12,7,3,7,10,7]
for mood in ['Normal','Scared','Returning','Intro']:
 a=np.zeros(sr*seconds)
 for bar in range(8):
  root=roots[bar%4]
  for beat in range(4):
   at=bar*2+beat*.5
   tone(a,at,root,.34,.15,1)
   if mood!='Returning':
    n=int(.12*sr);t=np.arange(n)/sr;kick=np.sin(2*np.pi*(65*t-25*t*t))*np.exp(-35*t)*.11
    a[int(at*sr):int(at*sr)+n]+=kick
   for half in range(2):
    note=root+24+melody[beat*2+half]
    tone(a,at+half*.25,note,.19,.055 if mood=='Returning' else .085)
    if mood=='Scared': tone(a,at+half*.25+.125,note+12,.1,.038)
  if mood=='Scared':
   for i in range(16):
    n=int(.045*sr);t=np.arange(n)/sr;noise=rng.uniform(-1,1,n)*np.exp(-80*t)*.023
    st=int((bar*2+i*.125)*sr);a[st:st+n]+=noise
 if mood=='Intro':
  a=a[:int(4.8*sr)]
  a*=np.minimum(np.arange(len(a))/sr/1.5,.85)
 a[:220]*=np.linspace(0,1,220);a[-220:]*=np.linspace(1,0,220)
 pcm=(np.clip(a,-.95,.95)*32767).astype('<i2')
 with wave.open(str(out/(mood+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(sr);f.writeframes(pcm.tobytes())
 print(mood,len(a)/sr,'peak',round(float(np.max(np.abs(a))),3))
