using UnityEngine;
namespace Skywright {
// Original short water-bubble resonators; all endpoints taper to zero.
public static class BubbleAudio {
 public const float PickupVolume=.50f,PlacementVolume=.42f,PitchVariation=.018f;
 public static AudioClip Pickup(){
  const int rate=44100;var data=new float[rate/5];float phase=0;var random=new System.Random(19010);
  for(int i=0;i<data.Length;i++){float t=i/(float)rate;phase+=2*Mathf.PI*(430+620*(1-Mathf.Exp(-t*65)))/rate;float env=(1-Mathf.Exp(-t*850))*Mathf.Exp(-t*48)*Mathf.Pow(1-t/.2f,2);data[i]=(Mathf.Sin(phase)*.54f+((float)random.NextDouble()*2-1)*.08f*Mathf.Exp(-t*170))*env;}
  var clip=AudioClip.Create("Soft water bubble pop",data.Length,1,rate,false);clip.SetData(data,0);return clip;
 }
 public static AudioClip Ding(){
  const int rate=44100;var data=new float[rate/3];for(int i=0;i<data.Length;i++){float t=i/(float)rate;float e=(1-Mathf.Exp(-t*500))*Mathf.Exp(-t*16)*Mathf.Pow(1-t*3,2);data[i]=e*(Mathf.Sin(t*2*Mathf.PI*1050)*.35f+Mathf.Sin(t*2*Mathf.PI*1575)*.07f);}
  var clip=AudioClip.Create("Earthstone solid contact ding",data.Length,1,rate,false);clip.SetData(data,0);return clip;
 }
 public static AudioClip Placement(){
  const int rate=44100;const float duration=.19f;var samples=new float[Mathf.CeilToInt(rate*duration)];var rng=new System.Random(174);float phase=0,water=0;
  for(int i=0;i<samples.Length;i++){
   float t=i/(float)rate,u=t/duration;float attack=1-Mathf.Exp(-t*420);float tail=Mathf.Pow(1-u,2);
   // A rounded, damped water cavity with a tiny softer satellite bubble. No hard click or metal partials.
   phase+=2*Mathf.PI*(760-55*(1-Mathf.Exp(-t*48)))/rate;
   float body=.66f*Mathf.Sin(phase)*attack*Mathf.Exp(-t*42);
   float delay=t-.023f;float satellite=delay>0?.19f*Mathf.Sin(2*Mathf.PI*540*delay)*(1-Mathf.Exp(-delay*280))*Mathf.Exp(-delay*55):0;
   water=Mathf.Lerp(water,(float)rng.NextDouble()*2-1,.09f);
   samples[i]=(body+satellite+water*.055f*attack*Mathf.Exp(-t*65))*tail*.38f;
  }
  var clip=AudioClip.Create("Pearl placement bubble",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
 }
 public static AudioClip Create(string name,float duration,float baseHz){
  const int rate=44100;var data=new float[Mathf.CeilToInt(duration*rate)];var random=new System.Random(172);float phase=0,second=0,water=0;
  for(int i=0;i<data.Length;i++){
   float t=i/(float)rate,u=t/duration;
   phase+=2*Mathf.PI*baseHz*(.96f+.04f*Mathf.Exp(-t*35))/rate;
   second+=2*Mathf.PI*baseHz*.72f/rate;
   float attack=1-Mathf.Exp(-t*150),tail=Mathf.Pow(1-u,3);
   float body=attack*Mathf.Exp(-t*24)*(Mathf.Sin(phase)+.10f*Mathf.Sin(phase*1.43f));
   float delay=Mathf.Max(0,t-.028f);float small=t>.028f?.16f*(1-Mathf.Exp(-delay*130))*Mathf.Exp(-delay*45)*Mathf.Sin(second):0;
   water=Mathf.Lerp(water,(float)random.NextDouble()*2-1,.05f);
   data[i]=(body+small+water*.12f*attack*Mathf.Exp(-t*55))*tail*.42f;
  }
  var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
 }
}
}
