# Packs actual Unity JPEG frames into standard Motion JPEG AVI, without re-rendering.
import struct,pathlib,sys
p=lambda fmt,*a:struct.pack('<'+fmt,*a)
def chunk(tag,data):return tag+p('I',len(data))+data+(b'\0' if len(data)%2 else b'')
def listing(tag,data):return chunk(b'LIST',tag+data)
def encode(folder,out,fps=15):
 frames=[f.read_bytes() for f in sorted(pathlib.Path(folder).glob('*.jpg'))]
 if not frames:raise ValueError('No frames')
 w,h=1280,720;n=len(frames);maxsize=max(map(len,frames))
 avih=p('14I',1000000//fps,maxsize*fps,0,0x10,n,0,1,maxsize,w,h,0,0,0,0)
 strh=p('4s4sIHHIIIIIIIIhhhh',b'vids',b'MJPG',0,0,0,0,1,fps,0,n,maxsize,0xffffffff,0,0,0,w,h)
 strf=p('IiiHH4sIiiII',40,w,h,1,24,b'MJPG',w*h*3,0,0,0,0)
 head=listing(b'hdrl',chunk(b'avih',avih)+listing(b'strl',chunk(b'strh',strh)+chunk(b'strf',strf)))
 video=bytearray();index=bytearray()
 for frame in frames:
  index+=p('4sIII',b'00dc',0x10,len(video)+4,len(frame));video+=chunk(b'00dc',frame)
 data=b'AVI '+head+listing(b'movi',video)+chunk(b'idx1',index)
 pathlib.Path(out).write_bytes(b'RIFF'+p('I',len(data))+data)
 print(out,n,'frames',len(data),'bytes')
if __name__=='__main__':encode(sys.argv[1],sys.argv[2])
