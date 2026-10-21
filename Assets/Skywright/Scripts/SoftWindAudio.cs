using UnityEngine;
namespace Skywright {
// One continuously playing voice. Gain is smoothed per audio sample, including menu/focus fades.
[RequireComponent(typeof(AudioSource))]
public class SoftWindAudio : MonoBehaviour {
 volatile float target;float gain;float smoothing;AudioSource source;
 public float Gain=>gain;public int SamplePosition=>source?source.timeSamples:0;
 void Awake(){source=GetComponent<AudioSource>();smoothing=1-Mathf.Exp(-1f/(AudioSettings.outputSampleRate*.065f));}
 public void SetLevel(float level){target=level;}
 void OnAudioFilterRead(float[] data,int channels){
  float desired=target;
  for(int i=0;i<data.Length;i+=channels){gain+=(desired-gain)*smoothing;for(int c=0;c<channels;c++)data[i+c]*=gain;}
 }
 public static AudioClip CreateLoop(){
  const int rate=22050,n=rate*32,blend=rate*2;
  var raw=new float[n+blend];var rng=new System.Random(172);float low=0,soft=0,dc=0;
  // Warm filters before recording. Two low-pass stages soften hiss; the slow average removes rumble/DC.
  for(int i=-rate;i<raw.Length;i++){float white=(float)rng.NextDouble()*2-1;low+=(white-low)*.11f;soft+=(low-soft)*.08f;dc+=(soft-dc)*.012f;if(i>=0)raw[i]=soft-dc;}
  var samples=new float[n];double energy=0;
  for(int i=0;i<n;i++){
   float value=i<blend?Mathf.Lerp(raw[n+i],raw[i],Mathf.SmoothStep(0,1,i/(float)blend)):raw[i];
   float t=i/(float)n;value*=.94f+.035f*Mathf.Sin(2*Mathf.PI*t*3)+.025f*Mathf.Sin(2*Mathf.PI*t*5+.8f);
   samples[i]=value;energy+=value*value;
  }
  float scale=.075f/(float)System.Math.Sqrt(energy/n);
  for(int i=0;i<n;i++)samples[i]*=scale;
  var clip=AudioClip.Create("Original continuous soft airflow - 32 seconds",n,1,rate,false);clip.SetData(samples,0);return clip;
 }
}
}
