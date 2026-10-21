import bpy,json,pathlib
R=pathlib.Path(__file__).resolve().parent
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(R/'sentinel-rigged-original.glb'))
for o in bpy.context.scene.objects:
 print('OBJECT',o.name,o.type,list(o.location),list(o.rotation_euler),list(o.dimensions))
 if o.type=='ARMATURE':
  for b in o.data.bones:print('BONE',b.name,'HEAD',list(b.head_local),'TAIL',list(b.tail_local))
 if o.type=='MESH':print('FACES',len(o.data.polygons),'GROUPS',len(o.vertex_groups))
