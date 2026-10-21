import bpy,math,pathlib,json
from mathutils import Vector, noise as field
base=pathlib.Path(r'C:\Users\jsayl\Desktop\UnityProjects\Skywright');out=base/'Assets/FinishedMeadow/Models'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def noise(x,z,s):return math.sin(x*.027+s)*math.cos(z*.031-s)*.55+math.sin(x*.061+z*.039+s)*.25+math.cos(x*.113-z*.084)*.2
def island(label,seed):
 N=64;v=[];faces=[]
 for side in range(2):
  for j in range(N+1):
   for i in range(N+1):
    u=i/N*2-1;w=j/N*2-1;x0=u*math.sqrt(1-w*w/2);z0=w*math.sqrt(1-u*u/2);a=math.atan2(z0,x0);r=math.sqrt(x0*x0+z0*z0)
    shape=1+.095*math.sin(3*a+seed)+.055*math.cos(7*a-seed)+.025*math.sin(11*a+seed);x=x0*220*shape;z=z0*155*shape
    if side==0:h=5+7*noise(x,z,seed)+32*math.exp(-((x+65)**2/7500+(z-25)**2/4200))+58*math.exp(-((x-52)**2/3400+(z+8)**2/6800))+math.sin(x*.052+z*.018)*3*(1-r)
    else:h=-42-20*(1-r)-77*math.exp(-((x+90)**2/2700+(z+20)**2/4800))-105*math.exp(-((x-35)**2/4300+(z-42)**2/1900))-68*math.exp(-((x-100)**2/1700+(z+70)**2/3000))+noise(x,z,seed)*13+math.sin(x*.082+z*.046)*3
    if side==1:
     h+=field.noise_vector(Vector((x*.025,z*.025,seed*.73)))[2]*18+field.noise_vector(Vector((x*.071,z*.071,seed)))[1]*6
     h=math.floor(h/11)*3.0+h*(8/11)
    v.append((x,z,h))
  off=side*(N+1)**2
  for j in range(N):
   for i in range(N):
    a=off+j*(N+1)+i;b=a+1;c=a+N+1;d=c+1;faces.append((a,b,d,c) if side==0 else (c,d,b,a))
 boundary=list(range(N+1))+[j*(N+1)+N for j in range(1,N+1)]+[N*(N+1)+i for i in range(N-1,-1,-1)]+[j*(N+1) for j in range(N-1,0,-1)]
 prev=boundary;tot=(N+1)**2
 for ring in range(1,9):
  t=ring/9;cur=[]
  for idx in boundary:
   p=Vector(v[idx]).lerp(Vector(v[tot+idx]),t);a=math.atan2(p.y,p.x);offset=(field.noise_vector(Vector((p.x*.035,p.y*.035,p.z*.047+seed)))[0]*10+math.sin(a*9+t*19)*2)*math.sin(t*math.pi);p.x+=math.cos(a)*offset;p.y+=math.sin(a)*offset;cur.append(len(v));v.append(tuple(p))
  for k in range(len(cur)):l=(k+1)%len(cur);faces.append((prev[l],prev[k],cur[k],cur[l]))
  prev=cur
 bottom=[tot+i for i in boundary]
 for k in range(len(prev)):l=(k+1)%len(prev);faces.append((prev[l],prev[k],bottom[k],bottom[l]))
 return make(label,v,faces)
def make(label,v,faces):
 mesh=bpy.data.meshes.new(label);mesh.from_pydata(v,[],faces);mesh.update();obj=bpy.data.objects.new(label,mesh);bpy.context.collection.objects.link(obj)
 for f in mesh.polygons:f.use_smooth=True
 return obj
def buttress(label,seed):
 v=[];faces=[];n=18;levels=[(0,1),(1.2,1.15),(2,.95),(3.5,1.08),(4,.86),(5.3,.97),(6.2,.72),(7,.55)]
 for j,(h,f) in enumerate(levels):
  for i in range(n):
   a=i*2*math.pi/n;rad=f*(1+.13*math.sin(a*3+seed)+.09*math.cos(a*5+h));v.append((math.cos(a)*rad*2,math.sin(a)*rad*1.4,h+(.16*math.sin(a*4+seed) if j else 0)))
 for j in range(len(levels)-1):
  for i in range(n):a=j*n+i;b=j*n+(i+1)%n;faces.append((a,b,b+n,a+n))
 faces.append(tuple(reversed(range(n))));faces.append(tuple((len(levels)-1)*n+i for i in range(n)));obj=make(label,v,faces)
 bevel=obj.modifiers.new('Weathered ledge edges','BEVEL');bevel.width=.13;bevel.segments=2;bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=bevel.name);return obj
objects=[island('ScenicIsland_'+str(i),i+8) for i in range(3)]+[buttress('LimestoneButtress_'+str(i),i+3) for i in range(3)]
for obj in objects:
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj;bpy.ops.export_scene.fbx(filepath=str(out/(obj.name+'.fbx')),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=str(base/'ArtSource/FinishedMeadow/Landforms.blend'))
(base/'ArtSource/FinishedMeadow/landform-budgets.json').write_text(json.dumps({o.name:{'vertices':len(o.data.vertices),'polygons':len(o.data.polygons)} for o in objects},indent=2))
