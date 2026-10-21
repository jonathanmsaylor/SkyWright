"""Bounded continuation of the same $3 Sentinel trial; credentials stay in memory."""
import json,pathlib,sys,urllib.request,urllib.error,winreg
ROOT=pathlib.Path(__file__).resolve().parents[2];OUT=ROOT/'ArtSource/StoneSentinelTrial'
PRIVATE=ROOT/'Evidence/PlaytestFollowup/StoneSentinel/Combat';PRIVATE.mkdir(parents=True,exist_ok=True)
BASE='https://openapi.tripo3d.ai/v3'
def api(path,data=None):
    with winreg.OpenKey(winreg.HKEY_CURRENT_USER,'Environment') as reg:key=winreg.QueryValueEx(reg,'TRIPO_API_KEY')[0]
    req=urllib.request.Request(BASE+path,data=json.dumps(data).encode() if data else None,headers={'Authorization':'Bearer '+key,'Content-Type':'application/json'})
    try:
        with urllib.request.urlopen(req,timeout=90) as r:result=json.load(r)
    except urllib.error.HTTPError as e:print(json.dumps({'http_error':e.code}));sys.exit(2)
    except Exception as e:print(json.dumps({'transport_error':type(e).__name__,'no_auto_retry':True}));sys.exit(3)
    if result.get('code')!=0:print(json.dumps({'api_code':result.get('code'),'message':result.get('message')}));sys.exit(4)
    return result['data']
action=sys.argv[1];kind=sys.argv[2] if len(sys.argv)>2 else 'check'
state=PRIVATE/(kind+'-task.json');lock=PRIVATE/(kind+'-submission.json')
if action=='submit':
    if kind not in ('check','rig'):raise SystemExit('Unknown operation')
    if state.exists() or lock.exists():raise SystemExit('Already submitted; poll existing task')
    balance=api('/account/balance');print(json.dumps({'balance':balance}))
    if float(balance['frozen'])!=0:raise SystemExit('Wait for pending charges')
    if kind=='rig':
        check=json.loads((PRIVATE/'check-result.json').read_text())
        if not check.get('output',{}).get('riggable'):raise SystemExit('Rig check has not passed')
        if float(balance['balance'])<25:raise SystemExit('Insufficient balance')
    task=json.loads((OUT/'generation-settings.json').read_text())['task_id']
    request={'input':task}
    if kind=='rig':request.update(model='v3.0-20260909',rig_type='biped',spec='mixamo',out_format='glb')
    lock.write_text(json.dumps({'request':request,'estimate_credits':25 if kind=='rig' else 0,'prior_trial_spend':60,'cap_credits':300,'balance_before':balance},indent=2))
    result=api('/animations/'+('rig-check' if kind=='check' else 'rig'),request)
    state.write_text(json.dumps(result,indent=2));print(json.dumps(result))
elif action=='poll':
    result=api('/tasks/'+json.loads(state.read_text())['task_id']);(PRIVATE/(kind+'-result.json')).write_text(json.dumps(result,indent=2))
    public={k:result.get(k) for k in ('task_id','status','progress','credits_consumed','error_code','error_message')}
    if kind=='check':public['output']=result.get('output')
    print(json.dumps(public));print(json.dumps({'balance':api('/account/balance')}))
elif action=='download':
    result=json.loads((PRIVATE/(kind+'-result.json')).read_text())
    if result['status']!='success':raise SystemExit('Not successful')
    target=OUT/'sentinel-rigged-original.glb'
    with urllib.request.urlopen(result['output']['model_url'],timeout=180) as r,target.open('wb') as f:
        while chunk:=r.read(1048576):f.write(chunk)
    ledger={'generation_credits':60,'rig_credits':result.get('credits_consumed'),'total_credits':60+float(result.get('credits_consumed',0)),'balance_after':api('/account/balance'),'rig_task_id':result['task_id']}
    (OUT/'combat-cost-ledger.json').write_text(json.dumps(ledger,indent=2));print(json.dumps(ledger));print('Downloaded rigged source')
