using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PoolScene.ItemData.WeaponData;

namespace PoolScene
{
    /// <summary>
    /// アイテム取得時に画面右上へ表示するポップアップUIを動的に作成して表示するクラス。
    /// </summary>
    public class ItemPickupPopup : MonoBehaviour
    {
        // ポップアップを表示し続ける秒数。
        private const float VisibleSeconds = 2.2f;

        // フェードイン・フェードアウトに使う秒数。
        private const float FadeSeconds = 0.25f;

        // ポップアップ表示を管理する共有インスタンス。
        private static ItemPickupPopup _instance;

        // 順番待ちのポップアップメッセージ。
        private readonly Queue<PopupMessage> _messages =
            new Queue<PopupMessage>(8);

        // フェード制御に使うCanvasGroup。
        private CanvasGroup _canvasGroup;

        // アイテム名を表示するテキスト。
        private TMP_Text _titleText;

        // レアリティを表示するテキスト。
        private TMP_Text _rarityText;

        // アイコンを表示するImage。
        private Image _iconImage;

        // レアリティ色のアクセント帯。
        private Image _accentImage;

        // 現在動作中の表示コルーチン。
        private Coroutine _displayCoroutine;

        // 1件分のポップアップ表示内容。
        private struct PopupMessage
        {
            // 表示するアイテム名。
            public string title;

            // 表示するレアリティ文字列。
            public string rarity;

            // 表示するアイコン。
            public Sprite icon;

            // レアリティに応じたアクセント色。
            public Color accentColor;
        }

        /// <summary>
        /// 通常アイテムの取得通知を表示キューへ追加する。
        /// </summary>
        /// <param name="item">表示するアイテムデータ。</param>
        public static void ShowItem(global::ItemData item)
        {
            if (item == null)
            {
                return;
            }

            Instance.Enqueue(
                item.itemName,
                item.rarity.ToString(),
                item.icon,
                GetRarityColor(item.rarity));
        }

        /// <summary>
        /// 武器インスタンスの取得通知を表示キューへ追加する。
        /// </summary>
        /// <param name="weapon">表示する武器インスタンス。</param>
        public static void ShowWeapon(WeaponInstance weapon)
        {
            if (weapon == null)
            {
                return;
            }

            Instance.Enqueue(
                weapon.displayName,
                weapon.data.rarity.ToString(),
                weapon.data.icon,
                GetRarityColor(weapon.data.rarity));
        }

        // ポップアップ用オブジェクトを遅延生成して返す。
        private static ItemPickupPopup Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var popupObject =
                    new GameObject("Item Pickup Popup");
                _instance =
                    popupObject.AddComponent<ItemPickupPopup>();
                _instance.BuildUi();
                DontDestroyOnLoad(popupObject);

                return _instance;
            }
        }

        // 1件分の表示内容をキューに追加し、表示処理が止まっていれば開始する。
        // title: アイテム名。
        // rarity: レアリティ文字列。
        // icon: 表示するアイコン。
        // accentColor: アクセント色。
        private void Enqueue(
            string title,
            string rarity,
            Sprite icon,
            Color accentColor)
        {
            _messages.Enqueue(new PopupMessage
            {
                title = title,
                rarity = rarity,
                icon = icon,
                accentColor = accentColor
            });

            if (_displayCoroutine == null)
            {
                _displayCoroutine =
                    StartCoroutine(DisplayMessages());
            }
        }

        // キューに入った取得通知を順番に表示してフェードアウトする。
        private IEnumerator DisplayMessages()
        {
            while (_messages.Count > 0)
            {
                var message =
                    _messages.Dequeue();

                _titleText.text = message.title;
                _rarityText.text = message.rarity;
                _rarityText.color = message.accentColor;
                _iconImage.sprite = message.icon;
                _iconImage.color =
                    message.icon == null ? Color.clear : Color.white;
                _accentImage.color = message.accentColor;

                yield return FadeTo(1f);
                yield return new WaitForSeconds(VisibleSeconds);
                yield return FadeTo(0f);
            }

            _displayCoroutine = null;
        }

        // CanvasGroupの透明度を指定値まで滑らかに変化させる。
        // targetAlpha: 最終的な透明度。
        private IEnumerator FadeTo(float targetAlpha)
        {
            var startAlpha =
                _canvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < FadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        targetAlpha,
                        elapsed / FadeSeconds);

                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
        }

        // ポップアップ表示に必要なCanvas、背景、アイコン、テキストをコードから作成する。
        private void BuildUi()
        {
            var canvas =
                gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler =
                gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            var panel =
                new GameObject("Popup Panel");
            panel.transform.SetParent(transform, false);

            var panelRect =
                panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-24f, -24f);
            panelRect.sizeDelta = new Vector2(360f, 92f);

            var background =
                panel.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.07f, 0.88f);

            _canvasGroup =
                panel.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            _accentImage =
                CreateAccent(panel.transform);
            _iconImage =
                CreateIcon(panel.transform);
            _titleText =
                CreateText(panel.transform, "Title", 18, FontStyles.Bold);
            _rarityText =
                CreateText(panel.transform, "Rarity", 14, FontStyles.Bold);
        }

        // パネル左側のアクセント帯Imageを作成する。
        // parent: 配置先のTransform。
        // 戻り値: 作成したImage。
        private static Image CreateAccent(Transform parent)
        {
            var accent =
                new GameObject("Accent");
            accent.transform.SetParent(parent, false);

            var rect =
                accent.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(6f, 0f);

            return accent.AddComponent<Image>();
        }

        // アイテムアイコン表示用Imageを作成する。
        // parent: 配置先のTransform。
        // 戻り値: 作成したImage。
        private static Image CreateIcon(Transform parent)
        {
            var icon =
                new GameObject("Icon");
            icon.transform.SetParent(parent, false);

            var rect =
                icon.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(46f, 0f);
            rect.sizeDelta = new Vector2(48f, 48f);

            var image =
                icon.AddComponent<Image>();
            image.preserveAspect = true;
            image.color = Color.clear;

            return image;
        }

        // アイテム名またはレアリティ用のTextMeshProテキストを作成する。
        // parent: 配置先のTransform。
        // name: オブジェクト名。Titleなら上段、それ以外なら下段に配置する。
        // fontSize: 文字サイズ。
        // fontStyle: 文字スタイル。
        // 戻り値: 作成したTMP_Text。
        private static TMP_Text CreateText(
            Transform parent,
            string name,
            int fontSize,
            FontStyles fontStyle)
        {
            var textObject =
                new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var rect =
                textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            if (name == "Title")
            {
                rect.offsetMin = new Vector2(82f, 42f);
                rect.offsetMax = new Vector2(-16f, -12f);
            }
            else
            {
                rect.offsetMin = new Vector2(82f, 14f);
                rect.offsetMax = new Vector2(-16f, -48f);
            }

            var text =
                textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.overflowMode = TextOverflowModes.Ellipsis;

            return text;
        }

        // レアリティに対応するポップアップのアクセント色を返す。
        // rarity: アイテムのレアリティ。
        // 戻り値: 表示に使う色。
        private static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Rare:
                    return new Color(0.2f, 0.55f, 1f);
                case Rarity.Epic:
                    return new Color(0.72f, 0.35f, 1f);
                case Rarity.Legendary:
                    return new Color(1f, 0.66f, 0.16f);
                default:
                    return new Color(0.72f, 0.78f, 0.82f);
            }
        }
    }
}
