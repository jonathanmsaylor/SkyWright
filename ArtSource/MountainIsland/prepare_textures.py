from pathlib import Path
import sys,shutil
P=Path(r'C:\Users\jsayl\Desktop\UnityProjects\Skywright');sys.path.insert(0,str(P/'Evidence/NaturalAero/python-deps'))
from PIL import Image
import numpy as np
S=P/'Evidence/MountainAero/sources';D=P/'Assets/MountainAero/Textures'
for src,pre,keys in [('tree_small_02','Tree',['Diffuse','nor_gl','Rough','branch_diff','branch_nor_gl','branch_rough','leaves_nor_gl']),('coast_rocks_05','Coast',['Diffuse','nor_gl','Rough'])]:
 for key in keys:shutil.copy2(S/src/(key+'.jpg'),D/(pre+'_'+key+'.jpg'))
leaf=Image.open(S/'tree_small_02/leaves_diff.jpg').convert('RGBA');alpha=Image.open(S/'tree_small_02/leaves_alpha.jpg').convert('L');leaf.putalpha(alpha.resize(leaf.size));leaf.save(D/'Tree_Leaves_RGBA.png')
# Keep scanned limestone detail while bringing its albedo into the pale Skywright palette.
im=np.asarray(Image.open(D/'Coast_Diffuse.jpg').convert('RGB'),dtype=np.float32)/255
lum=im.mean(axis=2,keepdims=True);im=(im*.65+lum*.35)*.72+.22
Image.fromarray((np.clip(im,0,1)*255).astype('uint8')).save(D/'Coast_Diffuse.jpg',quality=95)
print('Prepared scanned texture set',flush=True)
