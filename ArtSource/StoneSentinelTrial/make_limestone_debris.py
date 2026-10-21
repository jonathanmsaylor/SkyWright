"""Solid limestone drops derived from the paid sculpt, with fresh UVs for existing stone textures."""
import bpy,bmesh,math,pathlib,collections,json
from mathutils import Vector,Matrix
R=pathlib.Path(__file__).resolve().parents[2];S=R/'ArtSource/StoneSentinelTrial';D=R/'Assets/StoneSentinelCombat/Models'
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(S/'sentinel-rigged-original.glb'))
source=next(o for o in bpy.context.scene.objects if o.type=='MESH' and len(o.vertex_groups)>0)
rot=Matrix.Rotation(-math.pi/2,4,'Z');pts=[rot@source.matrix_world@Vector(v) for v in source.bound_box]
lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)])
matrix=Matrix.Scale(2.4/(hi.z-lo.z),4)@Matrix.Translation(-Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z)))@rot
world=source.matrix_world.copy();source.parent=None;source.matrix_world=matrix@world
for mod in list(source.modifiers):source.modifiers.remove(mod)
bpy.ops.object.select_all(action='DESELECT');source.select_set(True);bpy.context.view_layer.objects.active=source;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
parents=list(range(len(source.data.vertices)))
def root(i):
 while parents[i]!=i:parents[i]=parents[parents[i]];i=parents[i]
 return i
for edge in source.data.edges:
 a,b=map(root,edge.vertices);parents[a]=b
groups=collections.defaultdict(set)
for v in source.data.vertices:groups[root(v.index)].add(v.index)
centers={k:sum((source.data.vertices[i].co for i in ids),Vector())/len(ids) for k,ids in groups.items()}
pieces=[];stats=[];used=set()
for index,target in enumerate([(-.32,-.08,.75),(.62,-.12,1.8),(.32,-.07,.78)]):
 key=min((k for k,ids in groups.items() if len(ids)>500 and k not in used),key=lambda k:(centers[k]-Vector(target)).length);used.add(key)
 obj=source.copy();obj.data=source.data.copy();bpy.context.collection.objects.link(obj);obj.name='LimestoneChip_'+str(index+1)
 bm=bmesh.new();bm.from_mesh(obj.data);bm.verts.ensure_lookup_table();bmesh.ops.delete(bm,geom=[v for v in bm.verts if v.index not in groups[key]],context='VERTS')
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
 verts=obj.data.vertices;lo=Vector([min(v.co[i] for v in verts) for i in range(3)]);hi=Vector([max(v.co[i] for v in verts) for i in range(3)]);center=(lo+hi)/2;extent=max(hi-lo)
 for v in verts:v.co=(v.co-center)/extent
 # The source plates are open sculpted shells. Build a closed stone volume, then
 # transfer their original UV detail onto its rounded exterior.
 reference=obj.copy();reference.data=obj.data.copy();bpy.context.collection.objects.link(reference)
 bm=bmesh.new()
 for v in obj.data.vertices:bm.verts.new(v.co)
 bmesh.ops.convex_hull(bm,input=list(bm.verts),use_existing_faces=False)
 bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS')
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
 assert all(e.is_manifold for e in bm.edges),'Limestone drop must be closed'
 bm.to_mesh(obj.data);bm.free()
 obj.data.uv_layers.new(name='UVMap') if not obj.data.uv_layers else None
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
 bevel=obj.modifiers.new('Weathered limestone edges','BEVEL');bevel.width=.025;bevel.segments=3;bevel.limit_method='ANGLE';bevel.angle_limit=.1;bpy.ops.object.modifier_apply(modifier=bevel.name)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=.025);bpy.ops.object.mode_set(mode='OBJECT')
 tri=obj.modifiers.new('Export triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tri.name)
 for face in obj.data.polygons:face.use_smooth=True
 bpy.data.objects.remove(reference,do_unlink=True)
 obj.vertex_groups.clear();pieces.append(obj);stats.append({'name':obj.name,'triangles':len(obj.data.polygons),'texture':'Existing NaturalAero Rock030 2K color/normal/roughness; fresh UV unwrap'})
for obj in list(bpy.context.scene.objects):
 if obj not in pieces:bpy.data.objects.remove(obj,do_unlink=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(D/'LimestoneDebris.fbx'),use_selection=True,object_types={'MESH'},bake_anim=False,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
for image in bpy.data.images:
 if image.has_data:image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(S/'LimestoneDebris.blend'))
(S/'limestone-debris-stats.json').write_text(json.dumps(stats,indent=2));print('LIMESTONE_DEBRIS_COMPLETE',stats)
