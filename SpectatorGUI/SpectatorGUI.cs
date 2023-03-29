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
        public Text velText, stepText, debugText;
        public Player player;
        public List<PlayerLine> playerLines = new List<PlayerLine>();
        public Vector2 imageSize = new Vector2(10, 10);
        private float margin = 2.5f;
        public Tracker velocityAverager, stepTracker, stepAverager;
        public Sprite mutedSprite, speakingSprite;
        private bool wasLeftTouching, wasRightTouching;

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
                    velText = CreateText("VELOCITY:", new Vector2(margin, 5), Vector2.zero, statsCanvas);
                    velText.alignment = TextAnchor.LowerLeft;
                    velocityAverager = player.AddComponent<Tracker>();
                    velocityAverager.window = 5f;
                    velocityAverager.collector += (_player) =>
                    {
                        return _player.currentVelocity.magnitude;
                    };

                    stepText = CreateText("STEPS PER SECOND:", new Vector2(margin, 15), Vector2.zero, statsCanvas);
                    stepText.alignment = TextAnchor.LowerLeft;
                    stepTracker = player.AddComponent<Tracker>();
                    stepTracker.window = 1f;
                    stepAverager = player.AddComponent<Tracker>();
                    stepAverager.window = 5f;
                    stepAverager.collector += (_player) =>
                    {
                        return stepTracker.Count;
                    };

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

        void FixedUpdate()
        {
            try
            {
                CheckIfPlayerTookAStep();
                velText.text = $"VELOCITY: {velocityAverager.Average():f2}";
                stepText.text = $"STEPS PER SECOND: {stepAverager.Average():f2}";
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
            canvasObj.AddComponent<CanvasScaler>().scaleFactor = scaleFactor;
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


