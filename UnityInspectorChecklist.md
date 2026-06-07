# Unity Inspector 연결 체크리스트

> 스크립트를 직접 읽어 SerializeField 기준으로 작성.  
> ✅ = 기존 필드 (값 확인만) · 🆕 = 이번에 추가된 필드/오브젝트 · 🗑️ = 제거 필요

---

## 사전 작업 — AudioMixer 에셋 생성 (양쪽 씬 공통)

> `VolumeSettingsManager`가 AudioMixer를 참조하므로 먼저 만들어야 함.

- [ ] 🆕 `Assets/_Project/Audio/` 폴더에 **AudioMixer** 에셋 생성 (`Create → Audio → Audio Mixer`)
- [ ] 🆕 Mixer 안에 **BGM**, **SFX** 두 개의 Group 추가
- [ ] 🆕 BGM Group의 **Exposed Parameter** 이름 → `BGMVolume`
- [ ] 🆕 SFX Group의 **Exposed Parameter** 이름 → `SFXVolume`

---

## TitleScene

### 새로 만들어야 할 오브젝트

- [ ] 🆕 빈 GameObject `AudioManager` 생성
  - 컴포넌트: `AudioManager`
  - 자식으로 AudioSource × 2 추가 (`BGMSource`, `SFXSource`)
  - BGMSource: Loop = true, Output → BGM Mixer Group
  - SFXSource: Loop = false, Output → SFX Mixer Group

- [ ] 🆕 빈 GameObject `VolumeSettingsManager` 생성
  - 컴포넌트: `VolumeSettingsManager`

- [ ] 🆕 Canvas 안에 `VolumeSettingsPanel` UI 패널 생성
  - 컴포넌트: `VolumeSettingsUI`
  - 자식: BGM Slider (`_bgmSlider`), SFX Slider (`_sfxSlider`), 닫기 Button (`_closeVolumeButton`)
  - 초기 상태: **SetActive(false)** (코드에서 제어)

- [ ] 🆕 타이틀 버튼 2개 추가
  - `SettingsButton` → TitleController의 `_settingsButton`
  - `QuitButton` → TitleController의 `_quitButton`

---

### TitleController (TitleManager 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| ✅ `_titlePanel` | 타이틀 이미지·버튼 그룹 패널 |
| ✅ `_videoPanel` | 인트로 영상 패널 |
| ✅ `_videoPlayer` | VideoPlayer 컴포넌트 |
| ✅ `_startButton` | 시작 버튼 |
| ✅ `_skipButton` | 스킵 버튼 |
| ✅ `_languageButton` | 언어 버튼 |
| ✅ `_languagePanel` | 언어 선택 패널 |
| ✅ `_koreanButton` | 한국어 버튼 |
| ✅ `_englishButton` | 영어 버튼 |
| ✅ `_backButton` | 뒤로가기 버튼 |
| ✅ `_titleImage` | 타이틀 이미지 |
| ✅ `_languagePanelImage` | 언어 패널 이미지 |
| ✅ `_titleSprites` | [0]=En, [1]=Ko 스프라이트 |
| ✅ `_languagePanelSprites` | [0]=En, [1]=Ko 스프라이트 |
| ✅ `_buttonClickSound` | 버튼 클릭 AudioClip |
| ✅ `_titleBGM` | 타이틀 BGM AudioClip |
| 🆕 `_volumeSettingsPanel` | 위에서 만든 VolumeSettingsPanel |
| 🆕 `_settingsButton` | 위에서 만든 SettingsButton |
| 🆕 `_closeVolumeButton` | VolumeSettingsPanel 안의 닫기 버튼 |
| 🆕 `_quitButton` | 위에서 만든 QuitButton |

---

### VolumeSettingsManager (TitleScene 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| 🆕 `_mixer` | 사전 작업에서 만든 AudioMixer 에셋 |

### AudioManager (TitleScene 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| 🆕 `_bgmSource` | 자식 BGMSource AudioSource |
| 🆕 `_sfxSource` | 자식 SFXSource AudioSource |

### VolumeSettingsUI (VolumeSettingsPanel 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| 🆕 `_bgmSlider` | 패널 안 BGM Slider |
| 🆕 `_sfxSlider` | 패널 안 SFX Slider |

---

## AntDemoScene

### 새로 만들어야 할 오브젝트/UI

- [ ] 🆕 PausePanel 안에 **SettingsButton** 추가 → UIManager `_settingsButton`

- [ ] 🆕 Canvas 안에 **VolumeSettingsPanel** 패널 생성
  - 컴포넌트: `VolumeSettingsUI`
  - 자식: BGM Slider (`_bgmSlider`), SFX Slider (`_sfxSlider`), 닫기 Button
  - 초기 상태: **SetActive(false)**

- [ ] 🆕 Canvas 안에 **BlackFadePanel** 전체화면 패널 생성
  - 검정색 Image 컴포넌트, 전체화면 (Stretch All)
  - 컴포넌트: **CanvasGroup** 추가 (`_blackFadeCanvasGroup`)
  - 초기 상태: **SetActive(false)**

- [ ] 🆕 BlackFadePanel 자식(또는 별도)으로 **HiddenQuestionTextPanel** 생성
  - "???" TextMeshPro 텍스트 포함
  - 초기 상태: **SetActive(false)**

- [ ] 🆕 TutorialUI 그룹 안에 **KeyInfoTextZone** 패널 생성
  - 키 조작 안내 텍스트 (WASD/Sprint/Jump/Roll/Hide/E 등)
  - TutorialManager `_keyInfoTextZone`에 연결
  - 초기 상태: SetActive(true) — Awake에서 활성화됨

- [ ] 🆕 MissionMessageUI 패널 자식에 **FlowerMonologueText** (TextMeshProUGUI) 추가
  - 이탤릭 + 작은 글자 + 다른 색상으로 스타일링
  - 초기 상태: **SetActive(false)** — 코드에서 on/off

---

### UIManager (UIManager 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| ✅ `_hudPanel` | HUD 전체 패널 |
| ✅ `_missionCheckListUI` | 미션 체크리스트 패널 |
| ✅ `_gameOverPanel` | 게임오버 패널 |
| ✅ `_gameOverDescText` | 게임오버 설명 TMP |
| ✅ `_restartButton1` | 게임오버 재시작 버튼 |
| ✅ `_exitButton1` | 게임오버 종료 버튼 |
| ✅ `_backButton1` | 게임오버 타이틀로 버튼 |
| ✅ `_gameClearPanel` | 게임클리어 패널 |
| ✅ `__scoreText` | 클리어 점수 TMP (언더바 2개 주의) |
| ✅ `_clearTaglineText` | 클리어 태그라인 TMP |
| ✅ `_restartButton2` | 클리어 재시작 버튼 |
| ✅ `_exitButton2` | 클리어 종료 버튼 |
| ✅ `_backButton2` | 클리어 타이틀로 버튼 |
| ✅ `_pausePanel` | 일시정지 패널 |
| ✅ `_resumeButton` | 재개 버튼 |
| 🆕 `_settingsButton` | PausePanel 안의 설정 버튼 |
| ✅ `_exitButton3` | 일시정지 종료 버튼 |
| ✅ `_backButton3` | 일시정지 타이틀로 버튼 |
| ✅ `_pauseExitText` | 일시정지 종료 텍스트 TMP |
| 🆕 `_volumeSettingsPanel` | 새로 만든 VolumeSettingsPanel |
| 🆕 `_closeVolumeButton` | VolumeSettingsPanel 안의 닫기 버튼 |
| ✅ `_proximityPanelFeedbackUI` | ProximityPanelFeedbackUI 컴포넌트 |
| ✅ `_hiddenVideoPanel` | 히든 엔딩 영상 패널 |
| ✅ `_hiddenVideoPlayer` | VideoPlayer 컴포넌트 |
| ✅ `_hiddenSkipButton` | 히든 영상 스킵 버튼 |
| 🆕 `_blackFadePanel` | 새로 만든 BlackFadePanel |
| 🆕 `_blackFadeCanvasGroup` | BlackFadePanel의 CanvasGroup 컴포넌트 |
| 🆕 `_hiddenQuestionTextPanel` | 새로 만든 HiddenQuestionTextPanel |
| ✅ `_buttonClickSound` | 버튼 클릭 AudioClip |
| ✅ `_gameplayBGM` | 게임플레이 BGM AudioClip |
| ✅ `_gameOverBGM` | 게임오버 BGM AudioClip |
| ✅ `_clearBGM` | 클리어 BGM AudioClip |

---

### GameManager (GameManager 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| ✅ `_playerHealth` | Player 오브젝트의 PlayerHealth |
| ✅ `_angerSystem` | AngerSystem 컴포넌트 |
| 🆕 `_missionMessageUI` | MissionMessageUI 컴포넌트 (씬 내) |

---

### TutorialManager (TutorialManager 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| ✅ `_messageUI` | MissionMessageUI 컴포넌트 |
| ✅ `_angerSystem` | AngerSystem 컴포넌트 |
| ✅ `_catPrefab` | CatMovement 프리팹 |
| ✅ `_player` | Player 오브젝트의 PlayerHealth |
| ✅ `_movementZone` | 플레이어 시작 위치 SphereCollider |
| ✅ `_skipTextZone` | Q키 스킵 안내 UI 패널 |
| 🆕 `_keyInfoTextZone` | 새로 만든 KeyInfoTextZone 패널 |
| ✅ `_pressCZone` | C키 구르기 안내 UI 패널 |

---

### MissionMessageUI (MissionMessageUI 오브젝트)

| 필드 | 연결 대상 |
|------|----------|
| ✅ `_icon` | 발신자 아이콘 Image |
| ✅ `_senderName` | 발신자 이름 TMP |
| ✅ `_message` | 메시지 본문 TMP |
| ✅ `_panel` | 슬라이드 패널 RectTransform |
| ✅ `_angerSystem` | AngerSystem 컴포넌트 |
| ✅ `_slideInSound` | 슬라이드인 AudioClip |
| 🆕 `_flowerMonologueText` | 새로 만든 FlowerMonologueText TMP |

---

### VignetteHealthController (Player 오브젝트에 부착)

> `_volume`을 비워두면 씬에서 자동 탐색하므로, URP Global Volume이 씬에 있으면 생략 가능.

| 필드 | 연결 대상 |
|------|----------|
| 🆕 `_playerHealth` | 같은 오브젝트의 PlayerHealth |
| 🆕 `_volume` | 씬의 Global Volume (없으면 자동 탐색) |

> ⚠️ Global Volume의 **VolumeProfile**에 **Vignette** 오버라이드가 추가되어 있어야 작동함.

---

### VolumeSettingsManager (AntDemoScene 오브젝트)

> TitleScene에서 DontDestroyOnLoad로 넘어오지 않을 경우를 대비해 씬에도 배치.  
> 이미 Instance가 있으면 자동으로 Destroy됨.

| 필드 | 연결 대상 |
|------|----------|
| 🆕 `_mixer` | AudioMixer 에셋 |

---

### CarMovement 프리팹 / 씬 내 Car 오브젝트

- [ ] 🆕 자식 AudioSource의 **Output** → **SFX Mixer Group** 연결
  - `_audioSource`는 Start()에서 `GetComponentInChildren`으로 자동 할당되므로 Inspector 직접 연결 불필요
  - **Output 설정만** 프리팹에서 해주면 됨

### CatMovement 프리팹 / 씬 내 Cat 오브젝트

- [ ] 🆕 `_audioSource` (야옹 소리용) AudioSource의 **Output** → **SFX Mixer Group** 연결

---

### PlayerController (Player 오브젝트)

- [ ] 🗑️ AudioSource 컴포넌트가 남아있다면 **Remove Component**
  - 스크립트에서 AudioSource 참조가 제거됨
  - Inspector에 Missing 참조가 뜰 수 있으니 확인 후 정리

---

## 요약 — 새로 만들 것 목록

| # | 씬 | 만들 것 |
|---|----|----|
| 1 | 공통 | AudioMixer 에셋 (BGM/SFX Group + Exposed Parameters) |
| 2 | TitleScene | AudioManager 오브젝트 (BGMSource + SFXSource 자식) |
| 3 | TitleScene | VolumeSettingsManager 오브젝트 |
| 4 | TitleScene | VolumeSettingsPanel UI (Slider × 2 + 닫기 버튼) |
| 5 | TitleScene | SettingsButton, QuitButton |
| 6 | AntDemoScene | VolumeSettingsPanel UI (Slider × 2 + 닫기 버튼) |
| 7 | AntDemoScene | PausePanel 안 SettingsButton |
| 8 | AntDemoScene | BlackFadePanel (전체화면 검정 + CanvasGroup) |
| 9 | AntDemoScene | HiddenQuestionTextPanel ("???" 텍스트) |
| 10 | AntDemoScene | KeyInfoTextZone (키 안내 UI 패널) |
| 11 | AntDemoScene | FlowerMonologueText TMP (MissionMessageUI 자식, Inactive) |
| 12 | AntDemoScene | VignetteHealthController를 Player 오브젝트에 추가 |
| 13 | AntDemoScene | Global Volume에 Vignette 오버라이드 추가 |
