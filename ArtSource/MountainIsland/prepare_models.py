"""Explicit offline Blender preparation. CC0 source downloads remain in Evidence/MountainAero/sources."""
import bpy,json
from pathlib import Path
P=Path(r'C:\Users\jsayl\Desktop\UnityProjects\Skywright')
S=P/'Evidence/MountainAero/sources'; OUT=P/'Assets/MountainAero/Models'; SOURCE=P/'ArtSource/MountainIsland'
report=[]
for asset,source_name,label,limits in [('tree_small_02','tree_small_02_LOD1','Broadleaf',[380000,160000,65000]),('coast_rocks_05','coast_rocks_05_LOD3','Limestone',[14000,4000,900])]:
 bpy.ops.wm.open_mainfile(filepath=str(S/asset/(asset+'.blend')))
 original=bpy.data.objects[source_name]; original.data.calc_loop_triangles(); base_tris=len(original.data.loop_triangles)
 keep=[]
 for level,budget in enumerate(limits):
  obj=original.copy();obj.data=original.data.copy();bpy.context.scene.collection.objects.link(obj);obj.name=label+'_LOD'+str(level)
  bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
  mod=obj.modifiers.new('Realtime triangle budget','DECIMATE');mod.ratio=min(1,budget/base_tris);mod.use_collapse_triangulate=True
  bpy.ops.object.modifier_apply(modifier=mod.name)
  import bmesh
  bm=bmesh.new();bm.from_mesh(obj.data)
  bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS')
  bm.to_mesh(obj.data);bm.free();obj.data.update()
  obj.data.calc_loop_triangles();tris=len(obj.data.loop_triangles)
  for poly in obj.data.polygons:poly.use_smooth=True
  bpy.ops.export_scene.fbx(filepath=str(OUT/(obj.name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,use_mesh_modifiers=True,path_mode='STRIP')
  report.append({'asset':asset,'name':obj.name,'triangles':tris,'vertices':len(obj.data.vertices),'dimensions':list(obj.dimensions),'materials':[m.name for m in obj.data.materials]});keep.append(obj)
 for obj in list(bpy.data.objects):
  if obj not in keep:bpy.data.objects.remove(obj,do_unlink=True)
 for image in bpy.data.images:
  n=image.name.lower();key=None
  if 'leaves' in n:key='leaves_alpha' if 'alpha' in n else 'leaves_nor_gl' if 'nor' in n else 'leaves_diff' if 'diff' in n else None
  elif 'branch' in n:key='branch_nor_gl' if 'nor' in n else 'branch_rough' if 'rough' in n else 'branch_diff' if 'diff' in n else None
  else:key='nor_gl' if 'nor' in n else 'Rough' if 'rough' in n else 'Diffuse' if 'diff' in n else None
  if key and (S/asset/(key+'.jpg')).exists():image.filepath=str(S/asset/(key+'.jpg'))
 bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(label+'_prepared.blend')))
(SOURCE/'model-budgets.json').write_text(json.dumps(report,indent=2))
print('PREPARED_MODEL_BUDGETS',json.dumps(report),flush=True)
