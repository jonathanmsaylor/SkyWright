"""Offline animation authoring for the approved Sentinel; no paid/network operations."""
import bpy,math,pathlib,json
from mathutils import Vector,Matrix,Quaternion
R=pathlib.Path(__file__).resolve().parents[2];S=R/'ArtSource/StoneSentinelTrial';D=R/'Assets/StoneSentinelCombat/Models';D.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(S/'sentinel-rigged-original.glb'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH' and len(o.vertex_groups)>0)
for o in list(bpy.context.scene.objects):
 if o not in (arm,mesh):bpy.data.objects.remove(o,do_unlink=True)
# Bake the same orientation, scale, and ground pivot as the approved static model.
rot=Matrix.Rotation(-math.pi/2,4,'Z');pts=[rot@mesh.matrix_world@Vector(v) for v in mesh.bound_box]
lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)])
sc=2.4/(hi.z-lo.z);off=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
matrix=Matrix.Scale(sc,4)@Matrix.Translation(-off)@rot
worlds={o:o.matrix_world.copy() for o in (arm,mesh)}
for o in (mesh,arm):o.parent=None;o.matrix_world=matrix@worlds[o]
bpy.ops.object.select_all(action='DESELECT')
for o in (arm,mesh):o.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
bpy.context.view_layer.update()
bpy.context.view_layer.objects.active=mesh
dec=mesh.modifiers.new('Combat silhouette reduction','DECIMATE');dec.ratio=70000/99710;dec.use_collapse_triangulate=True
bpy.ops.object.modifier_apply(modifier=dec.name)
arm.name='Sentinel_Rig';mesh.name='Sentinel_Body'
scene=bpy.context.scene;scene.render.fps=30
def bone(short):return arm.pose.bones.get('mixamorig:'+short)
def rotate(short,angle,axis=(1,0,0)):
 b=bone(short)
 if b:
  # Model-space axis expressed in each bone's rest coordinates, avoiding guessed local axes.
  local=b.bone.matrix_local.to_quaternion().inverted()@Vector(axis)
  b.rotation_quaternion=Quaternion(local,math.radians(angle))
def pose(kind,t):
 for b in arm.pose.bones:b.rotation_mode='QUATERNION';b.rotation_quaternion=Quaternion();b.location=(0,0,0)
 cycle=math.sin(t*math.tau/1.5)
 if kind=='Idle':
  rotate('Spine2',math.sin(t*math.tau/3)*1.1);rotate('Head',math.sin(t*math.tau/3)*-1)
 elif kind=='Walk':
  for side,sign in [('Left',1),('Right',-1)]:
   rotate(side+'UpLeg',cycle*sign*18);rotate(side+'Leg',max(0,-cycle*sign)*23)
   rotate(side+'Arm',-cycle*sign*8);rotate(side+'Foot',-cycle*sign*6)
  rotate('Spine2',cycle*2,(0,0,1))
  bone('Hips').location=bone('Hips').bone.matrix_local.to_quaternion().inverted()@Vector((0,0,abs(cycle)*.035))
 elif kind in ('Windup','Slam','Recover'):
  if kind=='Windup':a=min(1,t/1.15);bend=-10*a;arms=-105*a;drop=0;crouch=0
  elif kind=='Slam':a=min(1,t/.22);bend=-10+54*a;arms=-105+70*a;drop=.44*a;crouch=a
  else:a=min(1,t/1.6);bend=44*(1-a);arms=-35*(1-a);drop=.44*(1-a);crouch=1-a
  rotate('Spine',bend*.45);rotate('Spine2',bend*.55)
  for side in ('Left','Right'):
   rotate(side+'Arm',arms);rotate(side+'ForeArm',-12*(1-crouch));rotate(side+'UpLeg',-32*crouch);rotate(side+'Leg',65*crouch)
  bone('Hips').location=bone('Hips').bone.matrix_local.to_quaternion().inverted()@Vector((0,0,-drop))
 elif kind=='Stagger':
  a=min(1,t/.15);rotate('Spine',-9*a);rotate('Spine2',-9*a);rotate('Head',12*a)
  rotate('LeftArm',-24*a);rotate('RightArm',-20*a);rotate('Spine1',math.sin(t*15)*2*(1-t/2),(0,0,1))
 elif kind=='Swat':
  # Brief brace, a broad right forearm sweep, then a weighted recovery.
  if t<.32:a=t/.32;turn=-12*a;sweep=-35*a
  elif t<.52:a=(t-.32)/.2;turn=-12+32*a;sweep=-35+115*a
  else:a=min(1,(t-.52)/.38);turn=20*(1-a);sweep=80*(1-a)
  rotate('Spine2',turn,(0,0,1));rotate('RightArm',-42);rotate('RightForeArm',sweep,(0,0,1));rotate('LeftArm',-12)
 elif kind=='Defeat':
  a=min(1,t/1.2);a=a*a*(3-2*a)
  rotate('Spine',26*a);rotate('Spine2',18*a);rotate('Head',23*a)
  for side in ('Left','Right'):rotate(side+'UpLeg',-32*a);rotate(side+'Leg',65*a);rotate(side+'Arm',-35*a)
  bone('Hips').location=bone('Hips').bone.matrix_local.to_quaternion().inverted()@Vector((0,0,-.44*a))
clips={'Idle':3,'Walk':1.5,'Windup':1.25,'Slam':.35,'Recover':1.6,'Stagger':2,'Defeat':1.6,'Swat':.9}
for kind,duration in clips.items():
 arm.animation_data_create();arm.animation_data.action=bpy.data.actions.new(kind)
 for frame in range(1,round(duration*30)+2):
  scene.frame_set(frame);pose(kind,(frame-1)/30)
  for b in arm.pose.bones:
   b.keyframe_insert('rotation_quaternion',frame=frame,group=b.name)
   if b.name.endswith(':Hips'):b.keyframe_insert('location',frame=frame,group=b.name)
 arm.animation_data.action.use_fake_user=True
arm.animation_data.action=None;pose('Idle',0);scene.frame_set(1)
# Split existing disconnected stones without cutting across their sculpted surfaces.
# Each original triangle belongs to exactly one exported section; UVs and weights survive.
import bmesh,collections
parents=list(range(len(mesh.data.vertices)))
def find(i):
 while parents[i]!=i:parents[i]=parents[parents[i]];i=parents[i]
 return i
for edge in mesh.data.edges:
 a,b=map(find,edge.vertices);parents[a]=b
components=collections.defaultdict(list)
for v in mesh.data.vertices:components[find(v.index)].append(v.index)
centers={k:sum((mesh.data.vertices[i].co for i in ids),Vector())/len(ids) for k,ids in components.items()}
large=[k for k,ids in components.items() if len(ids)>=220]
targets=[(-.65,-.16,1.72),(.65,-.16,1.72),(-.60,-.16,1.49),(.65,-.20,1.49),(-.31,-.08,.64),(.30,-.08,.63)]
armor=[]
for target in targets:
 candidates=[k for k in large if k not in armor and abs(centers[k].x)>(.45 if len(armor) in (2,3) else .18)]
 armor.append(min(candidates,key=lambda k:(centers[k]-Vector(target)).length))
seeds=armor+[k for k in sorted(large,key=lambda k:-len(components[k])) if k not in armor][:18]
groups={k:set(components[k]) for k in seeds}
for k,ids in components.items():
 if k in groups:continue
 # Only small neighboring detail follows a shed plate; other stones stay in body clusters.
 candidates=seeds if len(ids)<100 else seeds[6:]
 nearest=min(candidates,key=lambda s:(centers[k]-centers[s]).length)
 groups[nearest].update(ids)
pieces=[];report=[]
for index,k in enumerate(seeds):
 obj=mesh.copy();obj.data=mesh.data.copy();bpy.context.collection.objects.link(obj)
 obj.name=('Armor_'+str(index//2+1)+'_' if index<6 else 'Body_')+str(index).zfill(2)
 bm=bmesh.new();bm.from_mesh(obj.data);bm.verts.ensure_lookup_table();bmesh.ops.delete(bm,geom=[v for v in bm.verts if v.index not in groups[k]],context='VERTS');bm.to_mesh(obj.data);bm.free();obj.data.update()
 pieces.append(obj);report.append({'name':obj.name,'triangles':len(obj.data.polygons),'center':list(centers[k])})
 if index<6:
  # A compact inner stone fills the removed shell, with a restrained aqua material in Unity.
  inner=obj.copy();inner.data=obj.data.copy();bpy.context.collection.objects.link(inner);inner.name='Core_'+str(index//2+1)+'_'+str(index).zfill(2)
  # Keep the plate's solid component only; foliage must not become inner armor.
  bm=bmesh.new();bm.from_mesh(inner.data)
  connected=[];unseen=set(bm.verts)
  while unseen:
   stack=[unseen.pop()];group=set(stack)
   while stack:
    v=stack.pop()
    for edge in v.link_edges:
     other=edge.other_vert(v)
     if other in unseen:unseen.remove(other);group.add(other);stack.append(other)
   connected.append(group)
  keep=max(connected,key=len);bmesh.ops.delete(bm,geom=[v for v in bm.verts if v not in keep],context='VERTS');bm.to_mesh(inner.data);bm.free()
  center=sum((v.co for v in inner.data.vertices),Vector())/len(inner.data.vertices)
  for v in inner.data.vertices:v.co=center+(v.co-center)*.78
  bpy.ops.object.select_all(action='DESELECT');inner.select_set(True);bpy.context.view_layer.objects.active=inner
  d=inner.modifiers.new('Inner stone simplification','DECIMATE');d.ratio=min(1,1200/len(inner.data.polygons));bpy.ops.object.modifier_apply(modifier=d.name)
  pieces.append(inner)
original_triangles=len(mesh.data.polygons);bpy.data.objects.remove(mesh,do_unlink=True)
assert sum(r['triangles'] for r in report)==original_triangles
(S/'armor-sections.json').write_text(json.dumps(report,indent=2))
bpy.ops.object.select_all(action='DESELECT')
for obj in pieces:obj.select_set(True)
arm.select_set(True);bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=str(D/'SentinelCombat.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
for im in bpy.data.images:
 if im.has_data:im.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(S/'StoneSentinelCombat.blend'))
(S/'combat-animation-stats.json').write_text(json.dumps({'outer_triangles':original_triangles,'sections':len(pieces),'bones':len(arm.data.bones),'height':2.4,'clips_seconds':clips,'animation':'Locally authored skeletal clips; original Tripo Mixamo-compatible biped rig'},indent=2))
print('COMBAT_ANIMATION_EXPORT_COMPLETE',original_triangles,'sections',len(pieces))
