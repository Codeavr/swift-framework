﻿using System.Collections;
using UnityEngine;
using SwiftFramework.Core;
using UnityEngine.UI;

namespace SwiftFramework.Utils.UI
{
    public class BounceAnimation : MonoBehaviour
    {
        [SerializeField] private float animationDuration = .15f;
        [SerializeField] private float scaleDownPercent = .9f;
        [SerializeField] private AudioClipLink clickSound = null;

        private Coroutine currentAnimation;
        private bool clicked;
        private Vector3 startScale;
        private ISoundManager soundManager;

        private void Awake()
        {
            startScale = transform.localScale;
        }

        public void Click()
        {

            App.Core.Coroutine.Begin(PressAnimationRoutine(), ref currentAnimation);
            
            if (clickSound != null && clickSound.HasValue)
            {
                App.Core.GetCachedModule(ref soundManager);
                soundManager.PlayOnce(clickSound, SoundType.SFX);
            }

            clicked = true;
        }

        public void Release()
        {
            if (clicked == false)
            {
                return;
            }

            App.Core.Coroutine.Begin(BounceAnimationRoutine(), ref currentAnimation);
            
            clicked = false;
        }

        private IEnumerator BounceAnimationRoutine()
        {
            float t = 0f;
            while (t < 1)
            {
                transform.localScale = Vector3.Lerp(startScale * scaleDownPercent, startScale, t);
                t += Time.unscaledDeltaTime / animationDuration;
                yield return null;
            }
            transform.localScale = startScale;
        }

        private IEnumerator PressAnimationRoutine()
        {
            float t = 0f;
            while (t < 1)
            {
                transform.localScale = Vector3.Lerp(startScale, startScale * scaleDownPercent, t);
                t += Time.unscaledDeltaTime / animationDuration;
                yield return null;
            }
        }

    }
}