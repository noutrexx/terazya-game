using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DengeGame.Application;
using DengeGame.Application.Input;
using DengeGame.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DengeGame.Presentation.UI
{
    /// <summary>
    /// Sürüklenebilir olay kartı. Sola/sağa sürüklenir, miktara göre döner, eşik geçilince
    /// görsel geri bildirim verir ve bırakılınca kararı taahhüt eder. Animasyon sırasında yeni
    /// input alınmaz; bir karar iki kez taahhüt edilemez. Dokunma/mouse/test ortamını destekler.
    /// </summary>
    public sealed class CardView : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float ThresholdCanvasX = 240f;
        private const float MaxRotation = 12f;

        private RectTransform _rect;
        private Canvas _canvas;
        private Image _background;
        private TextMeshProUGUI _leftText, _rightText, _leftHints, _rightHints;
        private Image _avatar;
        private TextMeshProUGUI _avatarInitials, _nameLabel;

        private Vector2 _home;
        private float _pointerStartX;
        private bool _dragging;
        private bool _animating;
        private bool _committed;

        /// <summary>Bir karar taahhüt edildiğinde (kaydırma veya buton) bir kez yayınlanır.</summary>
        public event Action<DecisionSide> Committed;

        public bool Interactable => !_animating && !_committed;

        public static CardView Create(Transform parent)
        {
            var go = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(CardView));
            go.transform.SetParent(parent, false);
            var view = go.GetComponent<CardView>();
            view.Build();
            return view;
        }

        private void Build()
        {
            _rect = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
            _background = GetComponent<Image>();
            _background.color = UiTheme.Card;
            _background.raycastTarget = true;

            // Karakter: tema renginde avatar (baş harf) + isim
            _avatar = CharacterAvatar.Build(transform, out _avatarInitials);
            UiBuilder.Anchor(UiBuilder.Rect(_avatar), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(40, -140), new Vector2(140, -40));
            _nameLabel = UiBuilder.CreateText(transform, "CharacterName", "", UiTheme.FontBody, UiTheme.Accent,
                TextAlignmentOptions.Left);
            UiBuilder.Anchor(UiBuilder.Rect(_nameLabel), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(160, -130), new Vector2(-40, -50));

            var title = UiBuilder.CreateText(transform, "Title", "", UiTheme.FontTitle, UiTheme.CardText);
            UiBuilder.Anchor(UiBuilder.Rect(title), new Vector2(0, 0.62f), new Vector2(1, 0.82f),
                new Vector2(40, 0), new Vector2(-40, 0));

            var desc = UiBuilder.CreateText(transform, "Description", "", UiTheme.FontBody, UiTheme.CardText);
            UiBuilder.Anchor(UiBuilder.Rect(desc), new Vector2(0, 0.30f), new Vector2(1, 0.60f),
                new Vector2(50, 0), new Vector2(-50, 0));
            desc.enableAutoSizing = true;
            desc.fontSizeMin = 22; desc.fontSizeMax = UiTheme.FontBody;

            _leftText = UiBuilder.CreateText(transform, "LeftChoice", "", UiTheme.FontBody, UiTheme.Negative,
                TextAlignmentOptions.TopLeft);
            UiBuilder.Anchor(UiBuilder.Rect(_leftText), new Vector2(0, 0.84f), new Vector2(0.5f, 0.98f),
                new Vector2(40, 0), new Vector2(0, 0));

            _rightText = UiBuilder.CreateText(transform, "RightChoice", "", UiTheme.FontBody, UiTheme.Positive,
                TextAlignmentOptions.TopRight);
            UiBuilder.Anchor(UiBuilder.Rect(_rightText), new Vector2(0.5f, 0.84f), new Vector2(1, 0.98f),
                new Vector2(0, 0), new Vector2(-40, 0));

            _leftHints = UiBuilder.CreateText(transform, "LeftHints", "", UiTheme.FontSmall, UiTheme.CardText,
                TextAlignmentOptions.BottomLeft);
            UiBuilder.Anchor(UiBuilder.Rect(_leftHints), new Vector2(0, 0.02f), new Vector2(0.5f, 0.16f),
                new Vector2(40, 0), new Vector2(0, 0));

            _rightHints = UiBuilder.CreateText(transform, "RightHints", "", UiTheme.FontSmall, UiTheme.CardText,
                TextAlignmentOptions.BottomRight);
            UiBuilder.Anchor(UiBuilder.Rect(_rightHints), new Vector2(0.5f, 0.02f), new Vector2(1, 0.16f),
                new Vector2(0, 0), new Vector2(-40, 0));

            SetOverlay(0f);
        }

        /// <summary>Kartın karakter avatarını ve adını ayarlar (id bir karaktere çözülemezse id gösterilir).</summary>
        public void SetCharacter(string displayName, ColorRGB color)
        {
            _nameLabel.text = displayName ?? "";
            _avatarInitials.text = CharacterAvatar.Initials(displayName);
            _avatar.color = CharacterAvatar.ToColor(color);
        }

        public void Setup(EventCard card)
        {
            transform.Find("Title").GetComponent<TextMeshProUGUI>().text = card.Title ?? "";
            transform.Find("Description").GetComponent<TextMeshProUGUI>().text = card.Description ?? "";
            _leftText.text = card.LeftText ?? "";
            _rightText.text = card.RightText ?? "";
            _leftHints.text = BuildHints(DecisionPreview.Preview(card, DecisionSide.Left));
            _rightHints.text = BuildHints(DecisionPreview.Preview(card, DecisionSide.Right));
            SetCharacter(card.CharacterId, new ColorRGB(0.30f, 0.40f, 0.55f)); // çözülemezse varsayılan

            _home = _rect.anchoredPosition;
            _committed = false;
            _animating = false;
            SetOverlay(0f);
        }

        private static string BuildHints(IReadOnlyDictionary<CountryValue, HintDirection> hints)
        {
            var sb = new StringBuilder();
            foreach (var v in CountryValueInfo.All)
                if (hints.TryGetValue(v, out var dir))
                    sb.Append(CountryValueLabels.Short(v)).Append(dir == HintDirection.Up ? " ▲  " : " ▼  ");
            return sb.ToString().TrimEnd();
        }

        // --- Sürükleme ---

        public void OnBeginDrag(PointerEventData e)
        {
            if (!Interactable) return;
            _dragging = true;
            _pointerStartX = e.position.x;
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_dragging || !Interactable) return;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            float deltaX = (e.position.x - _pointerStartX) / Mathf.Max(0.0001f, scale);
            ApplyDrag(deltaX);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragging) return;
            _dragging = false;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            float deltaX = (e.position.x - _pointerStartX) / Mathf.Max(0.0001f, scale);
            var state = SwipeEvaluator.Evaluate(deltaX, ThresholdCanvasX, MaxRotation);

            if (state.IsCommitted) Commit(state.ToDecisionSide());
            else ReturnHome();
        }

        private void ApplyDrag(float deltaX)
        {
            var state = SwipeEvaluator.Evaluate(deltaX, ThresholdCanvasX, MaxRotation);
            _rect.anchoredPosition = _home + new Vector2(deltaX, 0f);
            _rect.localRotation = Quaternion.Euler(0, 0, -state.Rotation);
            SetOverlay(state.Progress);
        }

        /// <summary>Erişilebilirlik butonu veya testlerden kararı zorlar.</summary>
        public void ForceCommit(DecisionSide side)
        {
            if (!Interactable) return;
            Commit(side);
        }

        private void Commit(DecisionSide side)
        {
            if (_committed) return;
            _committed = true;
            _dragging = false;
            if (!UiPreferences.ReduceMotion && isActiveAndEnabled)
                StartCoroutine(SwipeOut(side));
            else
            {
                Committed?.Invoke(side);
            }
        }

        private void ReturnHome()
        {
            if (!UiPreferences.ReduceMotion && isActiveAndEnabled)
                StartCoroutine(AnimateReturn());
            else
            {
                _rect.anchoredPosition = _home;
                _rect.localRotation = Quaternion.identity;
                SetOverlay(0f);
            }
        }

        private IEnumerator SwipeOut(DecisionSide side)
        {
            _animating = true;
            float dir = side == DecisionSide.Right ? 1f : -1f;
            Vector2 start = _rect.anchoredPosition;
            Vector2 target = _home + new Vector2(dir * 1400f, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                _rect.anchoredPosition = Vector2.Lerp(start, target, t);
                _rect.localRotation = Quaternion.Euler(0, 0, -dir * MaxRotation);
                yield return null;
            }
            _animating = false;
            Committed?.Invoke(side);
        }

        private IEnumerator AnimateReturn()
        {
            _animating = true;
            Vector2 start = _rect.anchoredPosition;
            Quaternion startRot = _rect.localRotation;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 8f;
                _rect.anchoredPosition = Vector2.Lerp(start, _home, t);
                _rect.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
                SetOverlay(Mathf.Lerp(SignedProgress(start), 0f, t));
                yield return null;
            }
            _rect.anchoredPosition = _home;
            _rect.localRotation = Quaternion.identity;
            SetOverlay(0f);
            _animating = false;
        }

        private float SignedProgress(Vector2 pos) =>
            Mathf.Clamp((pos.x - _home.x) / ThresholdCanvasX, -1f, 1f);

        /// <summary>progress: -1 (sol) .. +1 (sağ). İlgili karar metnini ve eşik geri bildirimini gösterir.</summary>
        private void SetOverlay(float progress)
        {
            float left = Mathf.Clamp01(-progress);
            float right = Mathf.Clamp01(progress);
            SetAlpha(_leftText, left);
            SetAlpha(_rightText, right);

            // Eşik geçildiğinde kart kenarını ilgili renge boya (görsel geri bildirim).
            if (right >= 1f) _background.color = Color.Lerp(UiTheme.Card, UiTheme.Positive, 0.25f);
            else if (left >= 1f) _background.color = Color.Lerp(UiTheme.Card, UiTheme.Negative, 0.25f);
            else _background.color = UiTheme.Card;
        }

        private static void SetAlpha(TextMeshProUGUI t, float a)
        {
            var c = t.color; c.a = a; t.color = c;
        }
    }
}
