import bpy,pathlib,collections,json
S=pathlib.Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(S/'StoneSentinelCombat.blend'))
m=bpy.data.objects['Sentinel_Body'].data
parents=list(range(len(m.vertices)))
def root(i):
 while parents[i]!=i:parents[i]=parents[parents[i]];i=parents[i]
 return i
for e in m.edges:
 a,b=map(root,e.vertices);parents[a]=b
groups=collections.defaultdict(list)
for v in m.vertices:groups[root(v.index)].append(v)
rows=[]
for vs in groups.values():
 if len(vs)<30:continue
 lo=[min(v.co[i] for v in vs) for i in range(3)];hi=[max(v.co[i] for v in vs) for i in range(3)]
 rows.append({'verts':len(vs),'lo':lo,'hi':hi})
print(json.dumps(sorted(rows,key=lambda x:-x['verts'])[:35],indent=2));print('COMPONENTS',len(groups))
