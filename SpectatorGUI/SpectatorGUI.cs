using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Player = GorillaLocomotion.Player;
using SpectatorGUI.Resources;
namespace SpectatorGUI
{
    class SpectatorGUI : MonoBehaviour
    {
        public static SpectatorGUI Instance;

        public Canvas statsCanvas, playerCanvas;
        public Text velText, stepText, sizeText, debugText;
        public Player player;
        public List<PlayerLine> playerLines = new List<PlayerLine>();
        public Vector2 imageSize = new Vector2(10, 10);
        private float margin = 2.5f;
        public Tracker velocityAverager, stepTracker, stepAverager;
        public Sprite mutedSprite, speakingSprite;
        private bool wasLeftTouching, wasRightTouching;
        private List<Text> statTexts;

        public void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            try
            {
                speakingSprite = AssetUtils.LoadSprite("SpectatorGUI/Resources/speaking.png");
                mutedSprite = AssetUtils.LoadSprite("SpectatorGUI/Resources/muted.png");
                if (speakingSprite is null)
                    throw new NullReferenceException("Speaking sprite is null");
                if (mutedSprite is null)
                    throw new NullReferenceException("Muted sprite is null");

                int display = 0;
                foreach (var camera in GameObject.FindObjectsOfType<Camera>())
                    if (camera.name == "Shoulder Camera")
                        display = camera.targetDisplay;

                var player = Player.Instance.gameObject;
                statsCanvas = CreateCanvas(display, 3);
                {
                    statTexts = new List<Text>();
                    velText = CreateText("VELOCITY:", new Vector2(margin, margin), Vector2.zero, statsCanvas);
                    velocityAverager = player.AddComponent<Tracker>();
                    velocityAverager.window = 5f;
                    velocityAverager.collector += (_player) =>
                    {
                        return _player.currentVelocity.magnitude;
                    };
                    statTexts.Add(velText);

                    stepText = CreateText("STEPS PER SECOND:", new Vector2(margin, margin + 10), Vector2.zero, statsCanvas);
                    stepTracker = player.AddComponent<Tracker>();
                    stepTracker.window = 1f;
                    stepAverager = player.AddComponent<Tracker>();
                    stepAverager.window = 5f;
                    stepAverager.collector += (_player) =>
                    {
                        return stepTracker.Count;
                    };

                    statTexts.Add(stepText);

                    sizeText = CreateText("SIZE: 1", new Vector2(margin, margin + 20), Vector2.zero, statsCanvas);
                    statTexts.Add(velText);
                    SetStatTextPosition(0);

                    //debugText = CreateText("Hello", new Vector2(margin, -margin), Vector2.up, statsCanvas);
                    //debugText.alignment = TextAnchor.UpperLeft;
                }

                playerCanvas = CreateCanvas(display, 2);
                {
                    int lines = 10;
                    for (int i = 0; i < lines; i++)
                    {
                        Vector2 position = new Vector2(5, (i - lines / 2) * -12);
                        var swatch = CreateImage(position, playerCanvas);

                        position += new Vector2(imageSize.x + margin, 0);
                        var speaker = CreateImage(position, playerCanvas);
                        speaker.sprite = speakingSprite;
                        speaker.enabled = false;

                        position += new Vector2(imageSize.x + margin, 0);
                        var text = CreateText($"Player {i}", position, new Vector2(0f, 0.5f), playerCanvas);
                        swatch.enabled = false;
                        text.text = "";
                        playerLines.Add(new PlayerLine() {
                            speaker = speaker,
                            swatch = swatch,
                            text = text
                        });
                    }
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.WriteLine(ex.StackTrace); }

        }

        public TextAnchor GetTextAnchor(Vector2 v)
        {
            string x, y;
            x = v.x == 0 ? "Left" : "Right";
            y = v.y == 0 ? "Lower" : "Upper";
            return (TextAnchor)Enum.Parse(typeof(TextAnchor), y + x);
        }

        public Vector2 statsPivot = new Vector2();
        public Vector2 statsOffsetMult = new Vector2(1, 1);
        public TextAnchor SetStatTextPosition(int direction)
        {
            // 0, 1, 2, 3
            // L, R, D, U

            switch(direction)
            {
                case 0:
                    statsPivot.x = 0;
                    statsOffsetMult.x = 1; 
                    break;
                case 1:
                    statsPivot.x = 1;
                    statsOffsetMult.x = -1;
                    break;
                case 2:
                    statsPivot.y = 0;
                    statsOffsetMult.y = 1;
                    break;
                case 3:
                    statsPivot.y = 1;
                    statsOffsetMult.y = -1;
                    break;
            }

            var anchor = GetTextAnchor(statsPivot);

            foreach (Text text in statTexts)
            {
                float 
                    x = Mathf.Abs(text.rectTransform.anchoredPosition.x) * statsOffsetMult.x,
                    y = Mathf.Abs(text.rectTransform.anchoredPosition.y) * statsOffsetMult.y;

                text.alignment = anchor;
                text.rectTransform.anchorMin = statsPivot;
                text.rectTransform.anchorMax = statsPivot;
                text.rectTransform.pivot = statsPivot;
                text.rectTransform.anchoredPosition = new Vector2(x, y);
            }
            return anchor;
        }

        void FixedUpdate()
        {
            try
            {
                CheckIfPlayerTookAStep();
                velText.text = $"VELOCITY: {velocityAverager.Average():f2}";
                stepText.text = $"STEPS PER SECOND: {stepAverager.Average():f2}";
                sizeText.text = $"SIZE: {Player.Instance.scale:f2}";
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.WriteLine(ex.StackTrace); }

        }

        public void CheckIfPlayerTookAStep()
        {
            var player = Player.Instance;
            if (
                (!wasLeftTouching && player.IsHandTouching(true)) ||
                (!wasRightTouching && player.IsHandTouching(false))
            )
            {
                stepTracker.AddEntry(0);
            }
            wasLeftTouching = player.IsHandTouching(true);
            wasRightTouching = player.IsHandTouching(false);
        }

        public void RefreshPlayerList(List<GorillaPlayerScoreboardLine> lines)
        {
            if (playerLines is null) return;
            for (int i = 0; i < playerLines.Count; i++)
            {
                try
                {
                    if (lines is null || i >= lines.Count || !lines[i].enabled)
                    {
                        playerLines[i].swatch.enabled = false;
                        playerLines[i].speaker.enabled = false;
                        playerLines[i].text.text = "";
                        continue;
                    }

                    var source = lines[i];
                    playerLines[i].swatch.enabled = true;
                    playerLines[i].text.text = source.linePlayer.NickName;

                    var swatch = playerLines[i].swatch;
                    swatch.sprite = source.images[0].sprite;
                    swatch.material = source.images[0].material;
                    swatch.color = source.images[0].color;
                    
                    var speaker = playerLines[i].speaker;
                    if (source.playerVRRig)
                    {
                        playerLines[i].speaker.sprite = source.playerVRRig.muted ? mutedSprite : speakingSprite;
                        playerLines[i].speaker.enabled = source.speakerIcon.activeSelf || source.playerVRRig.muted;
                    }

                }
                catch (Exception ex) { Console.WriteLine(ex.Message); Console.WriteLine(ex.StackTrace); }
            }
        }

        private Text CreateText(string __text, Vector2 position, Vector2 pivot, Canvas canvas)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(canvas.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            var _text = textObj.AddComponent<Text>();
            _text.text = __text;
            _text.alignment = TextAnchor.MiddleLeft;
            _text.font = FindObjectOfType<GorillaLevelScreen>().myText.font;

            textRect.anchorMin = pivot;
            textRect.anchorMax = pivot;
            textRect.pivot = pivot;
            textRect.anchoredPosition = position;
            textRect.sizeDelta = new Vector2(200, 50);
            return _text;
        }

        private Image CreateImage(Vector2 position, Canvas canvas)
        {
            GameObject imageObj = new GameObject("Swatch");
            var image = imageObj.AddComponent<Image>();
            image.transform.SetParent(canvas.transform);

            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0f, 0.5f);
            imageRect.anchorMax = new Vector2(0f, 0.5f);
            imageRect.pivot = new Vector2(0f, 0.5f);
            imageRect.sizeDelta = imageSize;
            imageRect.anchoredPosition = position;
            return image;
        }

        public Canvas CreateCanvas(int targetDisplay, float scaleFactor)
        {
            GameObject canvasObj = new GameObject("Canvas");
            var camvas = canvasObj.AddComponent<Canvas>();
            camvas.renderMode = RenderMode.ScreenSpaceOverlay;
            camvas.targetDisplay = targetDisplay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720) / scaleFactor;
            return camvas;
        }

        public void OnDestroy()
        {
            Console.WriteLine("Destroying everything");
            foreach (var tracker in FindObjectsOfType<Tracker>())
            {
                Destroy(tracker);
            }

            Destroy(playerCanvas);
            Destroy(statsCanvas);
            Console.WriteLine("Destroyed everything");
        }

        public struct PlayerLine
        {
            public Text text;
            public Image swatch, speaker;
        }
    }
}


