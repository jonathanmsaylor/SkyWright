using UnityEngine;
namespace Skywright {
[CreateAssetMenu(menuName="Skywright/Air tuning")]
public class AirConfig : ScriptableObject {
 public float grabRange=6,carrySpeed=5,anchorRadius=10,anchorHeight=8,hoverSpeed=3,liftHeight=2,descentSpeed=3,gustRange=7,gustSpeed=3,gustAcceleration=10;
 public float windVolume=.35f;
 public Material bubble;
 public GameObject crate,focus;public Material ribbon,leaf;public AudioClip gather,release,settle,hover;
}
}
