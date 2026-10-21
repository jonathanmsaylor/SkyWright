"""Lossless TGA-to-PNG conversion of actual Unity framebuffer captures (stdlib only)."""
import pathlib, struct, zlib, sys
ROOT=pathlib.Path(__file__).resolve().parents[2]
source=pathlib.Path(sys.argv[1]) if len(sys.argv)>1 else ROOT/'Evidence/PlaytestFollowup/StoneSentinel/Captures'
dest=pathlib.Path(sys.argv[2]) if len(sys.argv)>2 else ROOT/'ArtSource/StoneSentinelTrial/Review';dest.mkdir(parents=True,exist_ok=True)
def chunk(tag,data):return struct.pack('>I',len(data))+tag+data+struct.pack('>I',zlib.crc32(tag+data)&0xffffffff)
for path in source.glob('*.tga'):
    raw=path.read_bytes();w,h=struct.unpack_from('<HH',raw,12)
    assert raw[2]==2 and raw[16]==24 and raw[17]==0
    bgr=raw[18:];rgb=bytearray(len(bgr));rgb[0::3]=bgr[2::3];rgb[1::3]=bgr[1::3];rgb[2::3]=bgr[0::3]
    scan=b''.join(b'\0'+rgb[y*w*3:(y+1)*w*3] for y in range(h-1,-1,-1))
    png=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(scan,6))+chunk(b'IEND',b'')
    target=dest/(path.stem+'.png');target.write_bytes(png);print(target)
