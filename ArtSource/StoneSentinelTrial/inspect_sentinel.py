import bpy, json, pathlib
from mathutils import Vector
root=pathlib.Path(__file__).resolve().parents[2]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'ArtSource/StoneSentinelTrial/sentinel-original.glb'))
for obj in bpy.context.scene.objects:
    print('OBJECT',obj.name,obj.type,tuple(obj.dimensions),tuple(obj.rotation_euler))
    if obj.type=='MESH': print('MESH',len(obj.data.vertices),len(obj.data.polygons),[m.name for m in obj.data.materials])
for mat in bpy.data.materials:
    print('MATERIAL',mat.name)
    for node in mat.node_tree.nodes:
        print('NODE',node.type,node.name, getattr(getattr(node,'image',None),'name',None))
    for link in mat.node_tree.links: print('LINK',link.from_node.name,link.from_socket.name,link.to_node.name,link.to_socket.name)
for im in bpy.data.images: print('IMAGE',im.name,tuple(im.size),im.colorspace_settings.name)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=12
scene.render.resolution_x=800;scene.render.resolution_y=800;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Inspection world');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.5,.6,.7,1)
scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.65
obj=next(o for o in scene.objects if o.type=='MESH')
bounds=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(bounds,Vector())/8
for loc,energy,size in [((2,-3,4),220,3),((-3,1,3),180,3)]:
    bpy.ops.object.light_add(type='AREA',location=loc);light=bpy.context.object;light.data.energy=energy;light.data.shape='DISK';light.data.size=size;light.rotation_euler=(center-light.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=1.3
folder=root/'Evidence/PlaytestFollowup/StoneSentinel';folder.mkdir(parents=True,exist_ok=True)
for name,offset in [('positive-x',(2,-.6,.4)),('negative-x',(-2,-.6,.4))]:
    cam.location=center+Vector(offset);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(folder/(name+'.png'));bpy.ops.render.render(write_still=True)
