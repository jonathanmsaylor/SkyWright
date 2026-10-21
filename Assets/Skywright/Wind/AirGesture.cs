namespace Skywright {
// One gesture owns both mouse buttons until both have been released.
// The modifier is sampled on LMB-down, never during an existing gesture.
public sealed class AirGesture {
 public enum Mode { None, Pull, Push, Grab, Blocked }
 public Mode Current {get;private set;}=Mode.Blocked;
 public bool Toggle {get;private set;}
 public void Cancel(){Current=Mode.Blocked;Toggle=false;}
 public void Tick(bool left,bool right,bool leftDown,bool rightDown,bool shift,bool equipped,bool holding){
  Toggle=false;
  if(!equipped){Cancel();return;}
  if(!left&&!right){Current=Mode.None;return;}
  if(Current==Mode.Blocked||Current==Mode.Grab)return;
  if(Current==Mode.Pull){if(!left||holding)Current=Mode.Blocked;return;}
  if(Current==Mode.Push){if(!right||holding)Current=Mode.Blocked;return;}
  // Simultaneous presses resolve to the grab chord, then RMB, then LMB.
  if(leftDown&&shift){Current=Mode.Grab;Toggle=true;}
  else if(holding)Current=Mode.Blocked;
  else if(rightDown)Current=Mode.Push;
  else if(leftDown)Current=Mode.Pull;
 }
}
}
