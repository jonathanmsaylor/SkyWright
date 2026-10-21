import bpy,json,pathlib
R=pathlib.Path(__file__).resolve().parents[2];S=R/'ArtSource/StoneSentinelTrial'
report={}
for p in (S/'ImportedAnimations').glob('*.fbx'):
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(p))
 arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
 action=arm.animation_data.action
 entry={'armature':arm.name,'matrix':[list(r) for r in arm.matrix_world],'fps':bpy.context.scene.render.fps,'frames':list(action.frame_range),'bones':{},'samples':[]}
 for b in arm.data.bones:
  if any(b.name.endswith(x) for x in ('Hips','Spine','Head','LeftArm','LeftForeArm','LeftUpLeg','LeftLeg','LeftFoot','RightHand')):entry['bones'][b.name]={'head':list(arm.matrix_world@b.head_local),'tail':list(arm.matrix_world@b.tail_local)}
 for f in range(int(action.frame_range[0]),int(action.frame_range[1])+1,5):
  bpy.context.scene.frame_set(f)
  entry['samples'].append({'f':f,**{b.name:list(arm.matrix_world@b.head) for b in arm.pose.bones if any(b.name.endswith(x) for x in ('Hips','LeftFoot','RightFoot','LeftHand','RightHand','Head'))}})
 report[p.name]=entry
bpy.ops.wm.open_mainfile(filepath=str(S/'StoneSentinelCombat.blend'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
report['target']={'matrix':[list(r) for r in arm.matrix_world],'bones':{b.name:{'head':list(b.head_local),'tail':list(b.tail_local)} for b in arm.data.bones},'actions':[(a.name,list(a.frame_range)) for a in bpy.data.actions]}
(S/'animation-source-inspection.json').write_text(json.dumps(report,indent=2))
print('ANIMATION_INSPECTION_COMPLETE')
