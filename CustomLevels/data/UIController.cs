using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

namespace QuadrataPatcher
{
    internal class UIController
    {
        public static void ChangeButtonSize(Transform rectTransform, Vector2 target, float time = 0f, Ease ease = Ease.Linear, float delay = 0f)
        {
            rectTransform.DOScale(target, time).SetEase(ease).SetDelay(delay).Play();
        }
        public static void TweenMenuElements(bool show, float duration = 0.3f, params Transform[] targets)
        {
            Vector2 targetScale = show ? Vector2.one : Vector2.zero;

            foreach (var target in targets)
            {
                if (target != null)
                    ChangeButtonSize(target, targetScale, duration, Ease.OutBack);
            }
        }

        public static void DelayedTweenMenuElements(MonoBehaviour context, bool show, float delay = 0.5f, float duration = 0.3f, params Transform[] targets)
        {
            context.StartCoroutine(DelayedTweenCoroutine(show, delay, duration, targets));
        }

        private static IEnumerator DelayedTweenCoroutine(bool show, float delay, float duration, Transform[] targets)
        {
            yield return new WaitForSecondsRealtime(delay);
            TweenMenuElements(show, duration, targets);
        }

    }
}
