using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEditor.Rendering;
using UnityEngine;

namespace Mineant.TwoD
{
    public class SpriteAnimator : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public string DefaultAnimation;
        public List<SpriteAnimationClip> Clips;

        private Coroutine _animationCoroutine;  // Used to animate the sprite renderer

        [Header("Debug")]
        public string DebugAnimation;

        [Button]
        void DebugPlayAnimation()
        {
            Play(DebugAnimation);
        }


        void Start()
        {
            Play(DefaultAnimation);
        }

        public void Play(string animation)
        {
            SpriteAnimationClip clip = FindClip(animation);

            if (clip == null)
            {
                Debug.LogWarning($"Cannot find clip {animation}.");
                return;
            }

            // Start to play animation
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(_Playing(clip));
        }

        private IEnumerator _Playing(SpriteAnimationClip clip)
        {
            // Store start play time, used this to determine when to play what frame
            float interval = 1f / clip.FrameRate;

            do
            {
                float startTime = Time.time;
                for (int i = 0; i < clip.Sprites.Count; i++)
                {
                    SpriteRenderer.sprite = clip.Sprites[i];

                    // When not time to the next sprite animation frame, just wait.
                    do
                    {
                        yield return null;
                    } while (Time.time < startTime + interval * (i + 1));
                }

                // Pingpong reverse loop
                if (clip.LoopMode == SpriteAnimationClip.LoopModes.PingPong)
                {
                    startTime = Time.time;
                    for (int i = clip.Sprites.Count - 2; i > 0; i--)
                    {
                        SpriteRenderer.sprite = clip.Sprites[i];

                        // When not time to the next sprite animation frame, just wait.
                        do
                        {
                            yield return null;
                        } while (Time.time < startTime + interval * (clip.Sprites.Count - i - 1));
                    }
                }
            } while (clip.Loop);

            if (clip.ReturnDefault) Play(DefaultAnimation);
        }


        private SpriteAnimationClip FindClip(string animation)
        {
            return Clips.FirstOrDefault(c => c.Name == animation);
        }
    }

    [System.Serializable]
    public class SpriteAnimationClip
    {
        public enum LoopModes { Restart, PingPong }
        public string Name;
        public bool Loop = false;
        public LoopModes LoopMode;
        public int FrameRate = 12;

        [Tooltip("After finishing this animation, will it return to the default animation from the sprite animator?")]
        public bool ReturnDefault;
        public List<Sprite> Sprites;

    }

}
