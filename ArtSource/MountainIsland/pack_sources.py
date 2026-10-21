import bpy
from pathlib import Path
root=Path(r'C:\Users\jsayl\Desktop\UnityProjects\Skywright')
for label in ('Broadleaf','Limestone'):
 p=root/'ArtSource/MountainIsland'/(label+'_prepared.blend')
 bpy.ops.wm.open_mainfile(filepath=str(p))
 packed=[]
 for image in bpy.data.images:
  if image.users and image.source=='FILE':
   f=Path(bpy.path.abspath(image.filepath))
   if f.exists():
    try:image.pack();packed.append(image.name)
    except RuntimeError:pass
 bpy.ops.wm.save_as_mainfile(filepath=str(p))
 print('PACKED',label,packed,flush=True)
