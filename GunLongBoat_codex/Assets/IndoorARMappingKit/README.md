# AR 지도 맵핑 unitypackage

## 준비

Unity **6000.3.25f1**에서 Android용 프로젝트를 준비하고 Package Manager로 다음 패키지를 설치합니다.

| 패키지 | 버전 |
|---|---|
| AR Foundation (`com.unity.xr.arfoundation`) | 6.3.5 |
| ARCore XR Plugin (`com.unity.xr.arcore`) | 6.3.5 |
| XR Plug-in Management (`com.unity.xr.management`) | 4.6.1 |
| XR Core Utilities (`com.unity.xr.core-utils`) | 2.6.0 |
| Input System (`com.unity.inputsystem`) | 1.17.0 |
| Universal RP (`com.unity.render-pipelines.universal`) | 17.0.1 |
| uGUI / TextMeshPro (`com.unity.ugui`) | 2.0.0 |

## Import 및 Android 설정

1. `IndoorARMappingKit.unitypackage`를 다운로드합니다.
2. Unity의 **Assets → Import Package → Custom Package**에서 파일을 선택하고 전체 항목을 Import합니다.
3. `Assets/IndoorARMappingKit/Scenes/FreeMappingClean.unity`를 엽니다.
4. Android Build Profile을 활성화하고 이 씬을 빌드 씬 목록에 추가합니다.
5. **XR Plug-in Management → Android**에서 **ARCore**와 **Initialize XR on Startup**을 켭니다. 원본과 같은 설정은 ARCore 및 Depth **Required**이며, 이 설정을 사용할 때는 ARCore Depth 지원 기기를 준비합니다.
6. **Player Settings**에서 다음을 설정합니다.
   - Scripting Backend: **IL2CPP**
   - Target Architectures: **ARM64**
   - Minimum API Level: **25**
   - Target API Level: **Automatic / 설치된 SDK**
   - Active Input Handling: **Input System Package (New)**
   - Auto Graphics API: 해제, Graphics APIs: **OpenGLES3**
   - Application Identifier: 수신 프로젝트의 고유한 앱 ID
7. **Graphics**와 Android용 **Quality**의 Render Pipeline Asset에 `Assets/IndoorARMappingKit/Settings/Mobile_RPAsset.asset`를 지정합니다. 기존 URP Renderer를 사용한다면 **ARBackgroundRendererFeature**와 **ARCommandBufferSupportRendererFeature**를 추가합니다.
8. **Graphics → Always Included Shaders**에 다음 셰이더를 추가합니다.
   - `Sprites/Default`
   - `Universal Render Pipeline/Unlit`
   - `Universal Render Pipeline/Lit`
9. ARCore Project Validation의 오류를 해결하고 Android 기기에 빌드·설치합니다. 앱 실행 시 카메라 권한을 허용합니다. 사용자 정의 Android Manifest를 사용한다면 `android.permission.CAMERA`를 포함합니다.

패키지에는 TMP 기본 자산이 포함돼 있습니다. 동일 클래스나 동일 GUID의 자산이 이미 있는 프로젝트에는 중복 Import하지 마세요. TMP Essentials가 이미 설치돼 있다면 새 프로젝트에서 먼저 Import하거나 기존 자산과의 충돌을 확인합니다.

## 지도 맵핑

1. 카메라로 주변을 비추고 AR 추적이 준비되면 **Mapping Begin**을 누릅니다.
2. 출발점에서 휴대전화 방향을 맞춘 후 **START**를 누릅니다. 현재 위치와 수평 방향이 이번 측정의 원점이 됩니다.
3. 경로를 따라 걸으며 기록할 지점에서 **WP**를 누릅니다. 첫 WP는 START에서 수평 거리 **0.5m 이상** 떨어진 곳에 기록합니다. AR 추적 중에만 Pose Sample과 WP가 기록됩니다.
4. 도착점에서 **STOP**을 누릅니다. 측정 JSON이 저장되면 완료 횟수가 증가합니다.
5. **FR(정방향) → RR(역방향)**으로 자동 교대합니다. 화면의 방향에 맞춰 각각 **5회**, 총 **10회** 측정합니다. 같은 방향의 측정에서는 같은 경로와 대응하는 WP를 유지합니다.
6. FR/RR 모두 5회 완료하면 **Finish**를 누릅니다. 각 방향의 5개 기록을 합성해 `FinalRoute.json`을 생성합니다. 두 방향 모두 생성되면 **DONE**으로 표시됩니다.
7. **Mapping Begin**을 다시 누르면 새 세션을 시작합니다.

START 직후 바로 STOP하지 말고 최소 2개 Pose Sample과 유효한 WP를 기록하세요. Finish가 **FAILED**로 표시되면 방향별 Run 파일 수와 첫 WP의 거리, 기록 데이터의 유효성을 확인합니다.

## 저장된 파일 확인

측정 결과는 `Application.persistentDataPath` 아래에 저장됩니다.

```text
FreeMappings/
└── Session_yyyyMMdd_HHmmss_fff/
    ├── Forward/
    │   ├── Run_01.json ~ Run_05.json
    │   └── FinalRoute.json
    └── Reverse/
        ├── Run_01.json ~ Run_05.json
        └── FinalRoute.json
```

실제 저장 위치는 플랫폼과 앱 ID에 따라 달라집니다. Android에서 파일을 확인할 때는 기기의 앱 데이터 폴더 또는 Android Studio의 Device Explorer를 사용합니다. 새 세션을 시작해도 기존 세션 파일은 유지됩니다.
