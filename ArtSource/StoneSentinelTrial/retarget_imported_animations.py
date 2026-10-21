"""Retarget the user's four FBX motions onto the approved, segmented Sentinel.
No service calls. The character's geometry, skin weights and materials are retained.
Source displacement is removed; Unity owns collision-aware encounter movement.
"""
import bpy, pathlib, math, json
from mathutils import Vector, Matrix, Quaternion
R=pathlib.Path(__file__).resolve().parents[2]
S=R/'ArtSource/StoneSentinelTrial'
bpy.ops.wm.open_mainfile(filepath=str(S/'StoneSentinelCombat.blend'))
scene=bpy.context.scene
target=next(o for o in scene.objects if o.type=='ARMATURE')
parts=[o for o in scene.objects if o.type=='MESH']
original=set(scene.objects)
target.animation_data.action=None
for b in target.pose.bones: b.matrix_basis=Matrix.Identity(4)
# Preserve the original weighted surfaces; cap extreme shoulder/elbow rotations below
# to adapt the supplied humanoid gestures to the Sentinel's broad stone proportions.
# Keep the locally authored reactions, swat and defeat. Replace the four locomotion/attack clips.
new_names={'Walk','Windup','Slam','Recover','Awaken','Dormant','LeapWindup','Leap','LeapRecover'}
for a in list(bpy.data.actions):
 if a.name in new_names: bpy.data.actions.remove(a)
scene.render.fps=30
report={}
specs={
 'Mutant Walking.fbx':[('Walk',1,86,1.42)],
 'Dual Weapon Combo.fbx':[('Windup',1,31,.9),('Slam',31,190,2.65),('Recover',190,219,.8)],
 'Zombie Stand Up.fbx':[('Dormant',1,1,1),('Awaken',1,186,3.1)],
 'Mutant Jump Attack.fbx':[('LeapWindup',1,32,.65),('Leap',32,101,1.0),('LeapRecover',101,175,1.15)]
}
for filename,clips in specs.items():
 before=set(scene.objects)
 bpy.ops.import_scene.fbx(filepath=str(S/'ImportedAnimations'/filename))
 imported=set(scene.objects)-before
 source=next(o for o in imported if o.type=='ARMATURE')
 source_action=source.animation_data.action
 for o in imported:
  if o.type=='MESH':o.hide_render=True;o.hide_set(True)
 mapping={b.name:source.pose.bones.get(b.name) for b in target.pose.bones}
 print('UNMAPPED', [k for k,v in mapping.items() if v is None],flush=True)
 assert all(mapping['mixamorig:'+k] for k in ('Hips','Head','LeftArm','RightArm','LeftUpLeg','RightUpLeg'))
 source_rest={b.name:(source.matrix_world@b.bone.matrix_local).to_quaternion() for b in source.pose.bones}
 target_rest={b.name:b.bone.matrix_local.to_quaternion() for b in target.pose.bones}
 # Cache source poses before keying target actions, so differing FBX/source frame rates stay explicit.
 for name,start,end,duration in clips:
  frames=round(duration*30)
  samples=[]
  for i in range(frames+1):
   f=start+(end-start)*i/frames
   scene.frame_set(int(f),subframe=f-int(f))
   samples.append({b.name:(source.matrix_world@b.matrix).to_quaternion() for b in source.pose.bones})
  action=bpy.data.actions.new(name);target.animation_data_create();target.animation_data.action=action
  action.use_fake_user=True
  ground_offsets=[]
  for i,sample in enumerate(samples):
   frame=i+1;scene.frame_set(frame)
   desired={}
   for b in target.pose.bones:
    b.rotation_mode='QUATERNION';b.location=(0,0,0);b.scale=(1,1,1)
    q=sample[b.name]@source_rest[b.name].inverted()@target_rest[b.name] if b.name in sample else (desired[b.parent.name]@target_rest[b.parent.name].inverted()@target_rest[b.name] if b.parent else target_rest[b.name])
    desired[b.name]=q
    # Convert model-space rotations into basis rotations without changing bone lengths.
    if b.parent:
     rest_relative=target_rest[b.parent.name].inverted()@target_rest[b.name]
     local=rest_relative.inverted()@desired[b.parent.name].inverted()@q
    else:local=target_rest[b.name].inverted()@q
    local.normalize()
    if local.w<0:local.negate()
    limit=95 if b.name.endswith(('LeftArm','RightArm')) else 115 if b.name.endswith(('LeftForeArm','RightForeArm')) else 180
    if local.angle>math.radians(limit):local=Quaternion(local.axis,math.radians(limit))
    desired[b.name]=desired[b.parent.name]@rest_relative@local if b.parent else target_rest[b.name]@local
    b.rotation_quaternion=local
   bpy.context.view_layer.update()
   # Solid surface grounding also handles the lying-to-standing motion and different leg proportions.
   deps=bpy.context.evaluated_depsgraph_get()
   lowest=float('inf')
   for obj in parts:
    if obj.name.startswith('Core_'):continue
    evaluated=obj.evaluated_get(deps);mesh=evaluated.to_mesh()
    lowest=min(lowest,min((evaluated.matrix_world@v.co).z for v in mesh.vertices))
    evaluated.to_mesh_clear()
   hip=target.pose.bones['mixamorig:Hips']
   hip.location=target_rest[hip.name].inverted()@Vector((0,0,.012-lowest))
   ground_offsets.append(.012-lowest)
   for b in target.pose.bones:
    b.keyframe_insert('rotation_quaternion',frame=frame,group=b.name)
    if b==hip:b.keyframe_insert('location',frame=frame,group=b.name)
  target.animation_data.action=None
  report[name]={'source':filename,'source_frames':[start,end],'seconds':duration,'ground_offset_range':[min(ground_offsets),max(ground_offsets)]}
  print('RETARGETED',name,flush=True)
 # Only the Sentinel and its source-derived actions are exported.
 for o in imported:bpy.data.objects.remove(o,do_unlink=True)
 if source_action.users==0:bpy.data.actions.remove(source_action)
scene.render.fps=30
target.animation_data.action=None
for b in target.pose.bones:b.matrix_basis=Matrix.Identity(4)
bpy.ops.object.select_all(action='DESELECT')
for o in original:o.select_set(True)
bpy.context.view_layer.objects.active=target
bpy.ops.export_scene.fbx(filepath=str(R/'Assets/StoneSentinelCombat/Models/SentinelCombat.fbx'),
 use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=True,
 bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,
 axis_forward='-Z',axis_up='Y',path_mode='STRIP')
bpy.ops.wm.save_as_mainfile(filepath=str(S/'StoneSentinelAnimated.blend'))
(S/'imported-animation-report.json').write_text(json.dumps(report,indent=2))

print('SENTINEL_RETARGET_COMPLETE')
