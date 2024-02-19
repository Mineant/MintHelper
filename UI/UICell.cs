using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Mineant
{
    public class UICell : MonoBehaviour
    {
        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }


        public Animator Animator;
        protected float _currentPosition;

        public void UpdatePosition(float position)
        {
            _currentPosition = position;

            if (Animator.isActiveAndEnabled)
            {
                Animator.Play(AnimatorHash.Scroll, -1, position);
            }

            Animator.speed = 0;
        }

        void OnEnable()
        {
            UpdatePosition(_currentPosition);
        }

    }

}
