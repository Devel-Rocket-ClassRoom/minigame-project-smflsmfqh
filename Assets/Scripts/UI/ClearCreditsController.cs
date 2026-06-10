using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 히든 엔딩 클리어 패널에서 10초 무조작 시 엔딩 크레딧을 자동 스크롤하는 컨트롤러.
/// GameClearPanel 오브젝트에 부착하고, UIManager에서 Activate() / CancelScroll()만 호출.
/// </summary>
public class ClearCreditsController : MonoBehaviour
{
    [Header("UI 참조")]
    [Tooltip("점수+태그라인이 들어있는 공통 부모 RectTransform")]
    [SerializeField] private RectTransform _scoreSection;

    [Tooltip("스크롤할 크레딧 텍스트 컨테이너 (Mask 자식)")]
    [SerializeField] private RectTransform _creditsContent;

    [Tooltip("크레딧 본문 TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI _creditsText;

    [Tooltip("클리어 버튼 3개(재시작·종료·타이틀)의 공통 부모 CanvasGroup")]
    [SerializeField] private CanvasGroup _buttonsGroup;

    [Header("타이밍")]
    [SerializeField] private float _autoScrollDelay = 10f;
    [SerializeField] private float _creditsDuration = 8f;

    private bool _buttonPressed;
    private Coroutine _routine;

    private void Awake()
    {
        StringTableManager.Instance.OnLanguageChanged += RefreshCreditsText;
    }

    private void OnDestroy()
    {
        StringTableManager.Instance.OnLanguageChanged -= RefreshCreditsText;
    }

    private void RefreshCreditsText()
    {
        if (_creditsText == null) return;
        _creditsText.text = StringTableManager.Instance.GetMessage("CREDITS_BODY").message
            .Replace("\\n", "\n");
    }

    /// <summary>
    /// 클리어 패널이 열릴 때 호출. 자동 스크롤 카운트다운 시작.
    /// </summary>
    public void Activate()
    {
        _buttonPressed = false;
        RefreshCreditsText();

        if (_creditsContent != null)
            _creditsContent.anchoredPosition = CalcStartPos();

        if (_buttonsGroup != null)
        {
            _buttonsGroup.alpha = 1f;
            _buttonsGroup.interactable = true;
            _buttonsGroup.blocksRaycasts = true;
        }

        if (_creditsContent != null)
            _routine = StartCoroutine(CoWaitThenScroll());
    }

    /// <summary>
    /// 클리어 버튼 클릭 시 호출. 진행 중인 크레딧 스크롤을 즉시 취소.
    /// </summary>
    public void CancelScroll()
    {
        _buttonPressed = true;
        if (_routine == null) return;
        StopCoroutine(_routine);
        _routine = null;
    }

    // ── 메인 시퀀스 ─────────────────────────────────────────────────────

    private IEnumerator CoWaitThenScroll()
    {
        yield return new WaitForSecondsRealtime(_autoScrollDelay);
        if (_buttonPressed) yield break;

        // 1. 버튼 페이드아웃
        if (_buttonsGroup != null)
        {
            _buttonsGroup.interactable = false;
            _buttonsGroup.blocksRaycasts = false;
            yield return CoFade(_buttonsGroup, 1f, 0f, 0.5f);
        }

        // 2. 점수 섹션 위로 슬라이드아웃
        if (_scoreSection != null)
            yield return CoSlideOut(_scoreSection, 0.6f);

        // 3. 크레딧 스크롤
        yield return CoScrollCredits();

        // 4. 버튼 페이드인 + 재활성화
        if (_buttonsGroup != null)
        {
            yield return CoFade(_buttonsGroup, 0f, 1f, 0.5f);
            _buttonsGroup.interactable = true;
            _buttonsGroup.blocksRaycasts = true;
        }

        _routine = null;
    }

    // ── 애니메이션 헬퍼 ─────────────────────────────────────────────────

    private IEnumerator CoFade(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator CoSlideOut(RectTransform rt, float duration)
    {
        Vector2 start = rt.anchoredPosition;
        Vector2 end   = start + new Vector2(0f, rt.rect.height + 60f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rt.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        rt.anchoredPosition = end;
    }

    private IEnumerator CoScrollCredits()
    {
        // 텍스트 변경 후 레이아웃이 확정될 때까지 한 프레임 대기
        yield return null;
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_creditsContent);

        Vector2 startPos = CalcStartPos();
        Vector2 endPos   = CalcEndPos();
        _creditsContent.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < _creditsDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _creditsContent.anchoredPosition = Vector2.Lerp(
                startPos, endPos,
                Mathf.Clamp01(elapsed / _creditsDuration)
            );
            yield return null;
        }
        _creditsContent.anchoredPosition = endPos;
    }

    // ── 위치 계산 ────────────────────────────────────────────────────────

    private float ViewportHeight =>
        _creditsContent.parent is RectTransform vp ? vp.rect.height : 800f;

    // 크레딧 시작 위치: 뷰포트 아래쪽 바깥
    private Vector2 CalcStartPos() =>
        new Vector2(0f, -(ViewportHeight * 0.5f + _creditsContent.rect.height * 0.5f));

    // 크레딧 끝 위치: 뷰포트 위쪽 바깥
    private Vector2 CalcEndPos() =>
        new Vector2(0f, ViewportHeight * 0.5f + _creditsContent.rect.height * 0.5f);
}
