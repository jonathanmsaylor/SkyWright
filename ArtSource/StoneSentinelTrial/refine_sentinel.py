"""Reproducible Blender refinement/export of the paid Tripo source; no network calls."""
import bpy, math, pathlib, json, numpy as np
from mathutils import Vector
ROOT=pathlib.Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/StoneSentinelTrial'; ASSET=ROOT/'Assets/StoneSentinelTrial'
TEX=ASSET/'Textures';MODELS=ASSET/'Models'
TEX.mkdir(parents=True,exist_ok=True);MODELS.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(SOURCE/'sentinel-original.glb'))
body=next(o for o in bpy.context.scene.objects if o.type=='MESH')
body.name='Sentinel_Master';mat=body.data.materials[0];mat.name='Sentinel limestone moss and aether'
body.rotation_mode='XYZ';body.rotation_euler.z=-math.pi/2
bpy.context.view_layer.objects.active=body;body.select_set(True)
bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
bpy.context.view_layer.update()
points=[body.matrix_world@Vector(v) for v in body.bound_box]
lo=Vector(tuple(min(p[i] for p in points) for i in range(3)));hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
scale=2.4/(hi.z-lo.z);offset=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
for v in body.data.vertices:v.co=(v.co-offset)*scale
body.data.update()
# Retain UV seams and original split normals. Remove only actual zero-area faces.
original_faces=len(body.data.polygons)
color=next(im for im in bpy.data.images if im.name.startswith('Color_'))
orm=next(im for im in bpy.data.images if im.name.startswith('ORM_'))
normal=next(im for im in bpy.data.images if im.name.startswith('NormalGL_'))
def pixels(im):
    a=np.empty(len(im.pixels),dtype=np.float32);im.pixels.foreach_get(a);return a.reshape(-1,4)
def save_existing(im,name):
    im.filepath_raw=str(TEX/(name+'.png'));im.file_format='PNG';im.save()
save_existing(color,'base-color');save_existing(normal,'normal')
c=pixels(color);o=pixels(orm);r,g,b=c[:,0],c[:,1],c[:,2]
# Saturated cyan only: excludes cream limestone, green moss and white highlights.
cyan=np.clip((np.minimum(g,b)-r-.045)*9,0,1)*np.clip((g-.18)*4,0,1)
em=np.zeros_like(c);em[:,:3]=c[:,:3]*cyan[:,None];em[:,3]=1
packed=np.zeros_like(c);packed[:,0]=np.minimum(o[:,2],.10)*cyan
rough=np.maximum(o[:,1],.67)*(1-cyan)+np.clip(o[:,1],.22,.45)*cyan
packed[:,3]=1-rough
def save_pixels(name,pix,srgb=False):
    im=bpy.data.images.new(name,width=color.size[0],height=color.size[1],alpha=True)
    im.colorspace_settings.name='sRGB' if srgb else 'Non-Color'
    im.pixels.foreach_set(pix.ravel());im.filepath_raw=str(TEX/(name+'.png'));im.file_format='PNG';im.save();return im
emission=save_pixels('emission',em,True);save_pixels('metallic-smoothness',packed)
bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
em_node=mat.node_tree.nodes.new('ShaderNodeTexImage');em_node.image=emission
mat.node_tree.links.new(em_node.outputs['Color'],bsdf.inputs['Emission Color']);bsdf.inputs['Emission Strength'].default_value=.8
# Inspect the central cyan area to make later sculpting decisions reviewable.
uv=body.data.uv_layers.active.data;centers=[]
for poly in body.data.polygons:
    center=poly.center
    if abs(center.x)>.30 or not 1.25<center.z<1.78 or center.y>-.10:continue
    for li in poly.loop_indices[:1]:
        u,v=uv[li].uv;idx=int(max(0,min(color.size[1]-1,v*color.size[1])))*color.size[0]+int(max(0,min(color.size[0]-1,u*color.size[0])))
        if cyan[idx]>.4:centers.append(list(center))
print('CENTRAL_AETHER_BOUNDS',np.min(centers,axis=0).tolist() if centers else None,np.max(centers,axis=0).tolist() if centers else None)
body.hide_render=True;body.hide_set(True)
lod_stats=[]
for i,budget in enumerate((70000,28000,10000)):
    obj=body.copy();obj.data=body.data.copy();obj.name='Sentinel_LOD'+str(i);bpy.context.collection.objects.link(obj)
    obj.hide_render=False;obj.hide_set(False)
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
    dec=obj.modifiers.new('Silhouette-preserving reduction','DECIMATE');dec.ratio=min(1,budget/original_faces);dec.use_collapse_triangulate=True
    bpy.ops.object.modifier_apply(modifier=dec.name)
    bpy.ops.export_scene.fbx(filepath=str(MODELS/(obj.name+'.fbx')),use_selection=True,object_types={'MESH'},use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',path_mode='STRIP',use_custom_props=False)
    lod_stats.append({'name':obj.name,'triangles':sum(len(p.vertices)-2 for p in obj.data.polygons),'vertices':len(obj.data.vertices)})
    if i>0:obj.hide_render=True;obj.hide_set(True)
for im in bpy.data.images:
    if im.has_data:im.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'StoneSentinel.blend'))
report={'height_metres':2.4,'master_triangles':original_faces,'lods':lod_stats,'texture_size':list(color.size),'pivot':'ground centre','front':'Blender -Y','changes':['uniform scale and ground pivot','front orientation','three static LOD meshes','matte limestone with cyan-only emission and roughness mask','explicit FBX and URP texture exports'],'not_included':['rig','animations','combat','hand-retopology'],'central_aether_sample_bounds':{'min':np.min(centers,axis=0).tolist(),'max':np.max(centers,axis=0).tolist()} if centers else None}
(SOURCE/'asset-stats.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
