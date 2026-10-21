"""One authorized image-to-model trial. Never prints/stores credentials; POST is never retried."""
import json, pathlib, sys, urllib.request, urllib.error, uuid, winreg
ROOT = pathlib.Path(__file__).resolve().parents[2]
OUT = ROOT / 'ArtSource/StoneSentinelTrial'
PRIVATE = ROOT / 'Evidence/PlaytestFollowup/StoneSentinel'
PRIVATE.mkdir(parents=True, exist_ok=True)
STATE = PRIVATE / 'tripo-task.json'
BASE = 'https://openapi.tripo3d.ai/v3'

def api(path, data=None, content='application/json'):
    with winreg.OpenKey(winreg.HKEY_CURRENT_USER, 'Environment') as reg:
        key = winreg.QueryValueEx(reg, 'TRIPO_API_KEY')[0]
    req = urllib.request.Request(BASE+path, data=data, headers={'Authorization':'Bearer '+key, 'Content-Type':content})
    try:
        with urllib.request.urlopen(req, timeout=90) as r: result=json.load(r)
    except urllib.error.HTTPError as e:
        print(json.dumps({'http_error':e.code})); sys.exit(2)
    except Exception as e:
        print(json.dumps({'transport_error':type(e).__name__, 'warning':'Do not retry a submission until task/account state is checked.'})); sys.exit(3)
    if result.get('code') != 0:
        print(json.dumps({'api_code':result.get('code'), 'message':result.get('message')})); sys.exit(4)
    return result['data']

action=sys.argv[1]
if action=='submit':
    if STATE.exists() or (PRIVATE/'submission-started.json').exists():
        raise SystemExit('Submission already recorded. Poll it; do not duplicate the charge.')
    balance=api('/account/balance')
    if float(balance['balance']) < 60 or float(balance['frozen']) != 0:
        raise SystemExit('Insufficient available credits or other tasks pending.')
    source=ROOT/'ArtSource/CombatReferences/stone-sentinel-input-v1.png'
    boundary='Skywright'+uuid.uuid4().hex
    body=(f'--{boundary}\r\nContent-Disposition: form-data; name="file"; filename="stone-sentinel-input-v1.png"\r\nContent-Type: image/png\r\n\r\n'.encode()+source.read_bytes()+f'\r\n--{boundary}--\r\n'.encode())
    upload=api('/files',body,'multipart/form-data; boundary='+boundary)
    request={'input':upload['file_token'],'model':'v3.1-20260211','texture':True,'pbr':True,'geometry_quality':'detailed','texture_quality':'detailed','texture_version':'v3.5-20260815','delight':True,'face_limit':100000,'texture_alignment':'original_image','auto_size':False}
    (PRIVATE/'submission-started.json').write_text(json.dumps({'starting_balance':balance,'estimated_credits':60,'authorized_max_credits':300,'request':request},indent=2))
    result=api('/generation/image-to-model',json.dumps(request).encode())
    STATE.write_text(json.dumps(result,indent=2))
    public={k:v for k,v in request.items() if k!='input'}
    public.update({'source':'../CombatReferences/stone-sentinel-input-v1.png','task_id':result['task_id'],'estimated_credits':60,'authorized_max_credits':300})
    (OUT/'generation-settings.json').write_text(json.dumps(public,indent=2))
    print(json.dumps({'submitted':True,'task_id':result['task_id'],'estimated_credits':60}))
elif action=='poll':
    state=json.loads(STATE.read_text())
    result=api('/tasks/'+state['task_id'])
    (PRIVATE/'tripo-result.json').write_text(json.dumps(result,indent=2))
    print(json.dumps({k:result.get(k) for k in ('task_id','status','progress','credits_consumed','error_code','error_message')}))
    print(json.dumps({'balance':api('/account/balance')}))
elif action=='download':
    result=json.loads((PRIVATE/'tripo-result.json').read_text())
    if result['status']!='success': raise SystemExit('Task has not succeeded')
    print(json.dumps({'output_fields':list(result.get('output',{}))}))
    url=result['output'].get('model_url')
    if not url: raise SystemExit('Inspect output schema privately before download')
    target=OUT/'sentinel-original.glb'
    with urllib.request.urlopen(url,timeout=180) as r, target.open('wb') as f:
        while chunk:=r.read(1024*1024): f.write(chunk)
    print(json.dumps({'downloaded':str(target),'bytes':target.stat().st_size}))
    ledger={'task_id':result['task_id'],'credits_consumed':result.get('credits_consumed'),'balance_after':api('/account/balance'),'source_image':'stone-sentinel-input-v1.png'}
    (OUT/'cost-ledger.json').write_text(json.dumps(ledger,indent=2))
else: raise SystemExit('Expected submit, poll or download')
