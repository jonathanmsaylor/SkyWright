# M1.10.1 — Playtest follow-up

2026-10-19. Unity 6000.4.7f1 / URP 17.4.0, Windows standalone. Implemented against local checkpoint `aa92ec1` on `codex/m1-10-tutorial-wind`. The integrated preview is ready for Jonathan's playtest; technical verification is not gameplay, visual or audio approval.

## Delivery and preservation

- Current executable: `Builds/Visual_Preview/Skywright.exe`, existing Visual Preview shortcut or `-visualpreview`.
- Previous playable M1.10 retained in `Builds/Visual_M1_10_Backup`. The old-behavior profiling build is separately retained in `Builds/Visual_M1_10_Profile`; M1.9 backup remains.
- Tests used `-visualqa`, real keyboard/mouse input through the previously authorized native helper, and read-only QA observations. No resource or gameplay commands were injected into QA files. Previously earned isolated saves were reused without editing their ledgers.
- Original save files and a SHA256 manifest were retained before testing in ignored `Evidence/PlaytestFollowup/saves-before/` and `preservation-before.json`. Newly earned test saves were retained separately. Personal slots were not used for testing.
- At handoff, the four changed isolated test/diagnostic files were restored from verified backups. All **20 original files** match their pre-test SHA256 hashes (`preservation-after.json`); no test preview remains running.
- Licensed assets, logs, screenshots and save evidence remain local/private. No sibling project, driver or system setting was changed; no monitoring software was installed.

## Implemented behavior

### Air input and intentional flight

Air LMB suctions; Shift+LMB pushes; plain RMB grabs/releases. Shift changes the active stream while LMB remains down, interrupting incoming collectibles before push starts. The HUD, opening screen and Controls text match. Pause/tool-change rearming remains intact.

Double jump now supplies the personal leap/glide inside and outside the field. Entering a field does not start or recapture hover. R is the explicit flight toggle. Saved flight is restored once only if its anchor and location are valid; saved glide has no flight-restoration flag. Full-attunement Shift flight remains 10.5 m/s; sprint/glide remains 7 m/s. The existing current still owns movement at up to 14 m/s.

### Earthstone

Holding LMB repeats only at the end of the existing bound strike/rebound cycle. It does not damage every frame. Release stops new strikes after the committed cycle. RMB recall, pause/stow and target loss disarm repetition. A held button cannot launch the next prepared stone after destruction.

Attachment uses explicit component classification: live harvesting nodes, cargo props, furniture and built pieces. Ordinary ground and landscape collision ricochet. Buildings/furniture ding without taking damage. Trees remain five hits, limestone eight, tutorial cargo crates three with one six-wood award.

| Visual tuning | Value |
| --- | ---: |
| Outbound speed | 26 m/s |
| Separation budget | 13 m |
| Cumulative outbound path | 26 m |
| Active flight budget | 3 s |
| Recall acceleration | 130 m/s² |

The distance check anticipates braking distance instead of waiting until the outer limit. Actual stationary-player samples reached **12.80–12.93 m** including the turning return arc; the last outbound samples were about **10.32–10.60 m**. This is a substantial reduction from the earlier observed 20.9 m arc. Swept collision and bounded return fallback remain. These are measured casts, not a mathematical maximum under arbitrary player movement or obstructed returns.

### Shoreline-aware bubble and rock contact

The field has a baked 256-direction profile sampled against the actual home-landform collider. As radius exceeds each direction's shoreline, its lower shell reveals over a 6 m expansion distance. Terrain still depth-occludes the shell. A bottom-pole blend prevents a meridian seam; no tier-wide hemisphere switch is used. The support-volume test uses the same profile and shell latitude.

The farthest home vertex lies 86.27 m horizontally from the field center; the rocky underside reaches y=-47.93 m. Derived full radii are **(94,73,94)** with horizontal stages **23.5, 47, 70.5, 94 m**. Every home mesh vertex passes the full ellipsoid/reveal containment check (largest normalized squared distance 0.8876); the neighboring island is outside. Offering/expansion timings and payments are unchanged. Supported flight below y=-25 no longer invokes the old recovery floor; unsupported falling still recovers.

Seating covers 68 scenery outcrops and the two original trial outcrops. The scanned limestone is one connected body including a low ground skirt: simple minimum bounds had left its larger body apparently suspended. The authoring pass welds scan seams, evaluates substantial components and low visual vertices, and embeds the lower shoulder. LODs and collision move together under unchanged resource roots. Original outcrops use baked visual/collision prefabs. Construction roots/transforms and all explicit resource identities remain unchanged.

### Flash investigation

Ground sprint start/stop and Shift flight were exercised. A concrete artifact reproduced near the island current: camera-facing world wind ribbons could occupy a large part of the viewport as broad pale bands, resembling a lighting flash. `SoftWindRibbon` adds near-camera fade, soft cross-section edges and a projected-width opacity limit. The same neighbor-edge view before/after shows those broad bands removed while retaining visible wind.

Evidence: `full-bubble-neighbor-edge.png` before the ribbon correction, `flash-correction.png` after, and `sprint-flash-0.png` through `sprint-flash-7.png` for the ground sequence. No separate sun-light pulse was isolated in the ground sequence. The sky/sprint code did not show a sprint-driven sun or exposure change. Jonathan reported the symptom probably occurs both on the ground and in flight; whether the ribbon artifact explains every instance remains unconfirmed and needs his playtest.

## Performance

Machine: Intel i7-14700F (28 logical processors), NVIDIA RTX 4070 SUPER, D3D12, 2560×1440. Both compared players loaded the same previously earned full-attunement save at (14.2825,4,-15.8735), same view and 29 construction pieces. Frame timing uses Unity FrameTimingManager; Windows process CPU time and per-process GPU-engine counters were sampled for six seconds after warmup. Five steady two-second CSV windows provide the timing averages below.

| Measurement | Old M1.10, 120 FPS + QA | M1.10.1, 60 FPS + QA | M1.10.1, 60 FPS, QA output off |
| --- | ---: | ---: | ---: |
| Frame interval | 8.338 ms | 16.669 ms | 16.667 ms |
| CPU main-thread timing | 1.526 ms | 1.729 ms | 1.628 ms |
| CPU render-thread timing | 2.173 ms | 2.524 ms | 2.499 ms |
| GPU frame timing | 4.815 ms | 5.892 ms | 5.894 ms |
| Process GPU 3D utilization | 41.74% | 25.32% | 25.51% |
| Process CPU, one-core equivalent | 111.70% | 64.48% | 62.94% |
| Working set | 680.1 MiB | 681.6 MiB | 679.8 MiB |
| Reported triangles | 6,714,422 | 6,722,870 | 6,722,870 |

One-core equivalent CPU means 100% is one logical CPU, not the entire machine. The matched QA comparison reduces observed GPU utilization by about **39%** and CPU work per second by about **42%**. Per-frame GPU time did not improve; frame pacing reduced sustained work. Clock/power behavior was not measured, so the longer per-frame timings at the lower cap are not assigned a cause. Small QA-on/off differences are not enough to establish a significant serialization bottleneck.

Canonical evidence: `sole-baseline120-performance.csv`, `sole-final60-performance.csv`, `sole-final60-noqa-performance.csv` and corresponding `sole-*-os-performance.json`. Timing windows are 14–23 s, 28–37 s and 14–23 s respectively. A later comparison attempt accidentally retained the old profiling player alongside the new one; those samples were discarded and both processes closed before these single-instance runs. Do not use the earlier `final60-qa*` or `invalid-two-instance-performance.csv` as matched evidence.

The old prototype rendered at 120 FPS even in menus. The new policy is **60 gameplay / 30 menus / 15 unfocused**, with VSync still off. Actual focused/menu/minimized CSV samples confirm approximately 16.67/33.34/67 ms frame intervals. The expensive scene remains roughly 6.7 million submitted triangles with foliage and four shadow cascades; rendering is the larger measured active-frame component, but this test does not isolate a per-pass foliage/shadow bottleneck. No broad quality reduction was applied.

Targeted changes also reuse the anchor's MaterialPropertyBlock and draw the screen gust only during GUI repaint. The denser baked shell adds a small triangle cost. Static review found existing QA object/JSON construction every 0.4 s, renderer enumeration for suction shimmer and component lookups for furniture. Those are candidates for a later measured allocation pass, not established causes of the fan noise. Startup/restore scene searches are distinct from the steady frame loop.

Traversal, ground sprint, harvesting, full-field flight and the island journey have timing/log evidence (`fresh-actions-performance.csv`, `final-journey-performance.csv`, sprint images and gameplay logs). Only the stationary identical-view case received a controlled before/after OS-counter comparison. A complete matched scenario-by-scenario benchmark and a rendering pass breakdown remain unperformed.

The release player's allocation and batch recorders were unavailable. Earlier zero allocation readings are invalid; the final probe explicitly records unavailable values as -1. No claim of zero allocations or eliminated GC spikes is made. Temperature, fan RPM and power were not measured; quieter fans or lower temperatures are not claimed. The measured results support reducing unnecessary sustained rendering, not diagnosing overheating.

## Standalone verification

Actual input and read-only observations verified:

- LMB pull, Shift+LMB push and switching Shift while held; plain RMB crate grab/release.
- Held five-hit tree and eight-hit limestone destruction; release; held-button return to Prepared without relaunch; three-hit crate destruction and exactly one six-wood award.
- RMB during held repeated crate strikes returned the stone and stopped further damage. Building balusters accepted the bound strikes, remained undamaged, and stayed at 29 construction pieces. Pause/resume while held left the stone Waiting without resuming repeated strikes.
- Ordinary ground produced upward ricochets; eligible limestone bound. Clear casts returned smoothly with the shorter measured arc and one owned stone.
- Shift double jump inside the full field stayed a leap/glide. Fresh small-field sprint jumps crossed out and back with only Grounded/Short lift/Descending states; no Anchor hover. An actual glide save reloaded without entering flight.
- Explicit R flight, deliberate cancel, full-attunement Shift flight, and saved explicit-flight restore. Unsupported R outside the home field supplied only personal lift.
- Full field viewed from the home edge and neighbor, above and below the home shoreline. Explicit flight beside the island reached y=-35.72. Tier-one partial shoreline view saved in `partial-shoreline.png`; the lower curve continues into open air.
- Both outbound and returning current arrivals, plus neighbor save/restart with earned ledger and construction retained. A navigation helper continued walking after one return arrival and caused a later recovery; the log distinguishes that helper overshoot from the successful current arrival.
- Revised outcrop visual/collision was harvestable for eight hits. Nearby and traversal views were inspected; all seventy placements were processed, but every cluster at every LOD/viewpoint was not individually approved.

Input evidence includes `held-destruction.json`, `final-outcrop-held.json`, `final-crate-held.json`, `final-crate-manual-recall.json`, `final-air-switch.json`, `bound-building.json`, `bound-pause-resume.json`, `crossing-in.json`, `crossing-out.json`, `earned-glide.json`, `glide-restart.json`, `below-edge.png`, `final-current-arrival.json` and `final-restart.json` in the private evidence folder. Some early targeting attempts were out of range or hit an intervening crate; only the subsequent logged impacts count as successful target checks.

`PlaytestFollowupBuild.VerifyAndBuild` passed the existing Tutorial Wind contracts and the new all-home-vertices/neighbor-exclusion field check. `final-build.log` records successful Windows build completion. Compilation is not treated as visual approval.

## Remaining review and known issue

- Jonathan should judge rock contact, partial/full bubble readability, rock speed/weight, glide momentum and whether his reported flash is gone.
- Audio listening approval, exhaustive LOD inspection, a continuous filmed expansion at every intermediate radius, complete per-scenario matched performance tests and development-profiler allocation attribution remain outstanding.
- The known intermittent native shutdown crash reproduced again after `CodeReloadManager destroyed` during the glide-restart test. The same shutdown failure was already reproduced in untouched M1.9 and M1.10 builds. It is unresolved; do not describe it as fixed or newly introduced by this follow-up. No corresponding managed gameplay exception was observed.

Stop here for Jonathan's playtest; do not advance to a new milestone or publish the project.
