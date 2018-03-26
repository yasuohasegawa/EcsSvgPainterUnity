using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace SVGPainterUnity
{
    public enum PainterState
    {
        None,
        Animating,
        Complete
    }

    public class SVGPainter : MonoBehaviour
    {
        private SVGPainterSystem painterSystem;

        private PainterState state = PainterState.None;

        private System.Action onComplete = null;
        private System.Action onRewindComplete = null;

        // Use this for initialization
        void Start()
        {
            painterSystem = World.Active.GetExistingManager<SVGPainterSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            if(painterSystem != null){
                if (state == PainterState.Complete)
                {
                    return;
                }

                if (painterSystem.painters.Count >= 1)
                {
                    int checkCompleteCount = 0;
                    for (int i = 0; i < painterSystem.painters.Count; i++)
                    {
                        painterSystem.painters[i].UpdateLine();
                        PainterAnimationState pstate = painterSystem.painters[i].GetState();
                        if (pstate == PainterAnimationState.Complete)
                        {
                            checkCompleteCount++;
                        }
                    }
                    if (checkCompleteCount >= painterSystem.painters.Count)
                    {
                        state = PainterState.Complete;
                        if (onComplete != null)
                        {
                            onComplete();
                            onComplete = null;
                        }

                        if (onRewindComplete != null)
                        {
                            onRewindComplete();
                            onRewindComplete = null;
                        }
                    }
                }
            }
        }

        public void Play(float duration = 3f, System.Func<float, float, float, float, float> _easing = null, System.Action callback = null)
        {
            state = PainterState.Animating;
            onComplete = callback;
            for (int i = 0; i < painterSystem.painters.Count; i++)
            {
                painterSystem.painters[i].duration = duration;
                painterSystem.painters[i].Play(0f, _easing);
            }
        }

        public void Rewind(float duration = 3f, System.Func<float, float, float, float, float> _easing = null, System.Action callback = null)
        {
            state = PainterState.Animating;
            onRewindComplete = callback;
            for (int i = 0; i < painterSystem.painters.Count; i++)
            {
                painterSystem.painters[i].duration = duration;
                painterSystem.painters[i].Rewind(0f, _easing);
            }
        }

        public bool IsActive() {
            if(painterSystem != null){
                if(painterSystem.painters.Count>=1){
                    return true;
                }
            }
            return false;
        }
	}
}