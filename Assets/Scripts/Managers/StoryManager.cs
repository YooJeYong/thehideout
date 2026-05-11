using System;
using UnityEngine;

namespace Hidenet.Core
{
    public class StoryManager : MonoSingleton<StoryManager>
    {
        [SerializeField] private StoryStep currentStep = StoryStep.Jane_Greeting_0;

        public StoryStep CurrentStep => currentStep;

        public event Action<StoryStep> OnStepChanged;

        public void NextStep()
        {
            currentStep++;
            OnStepChanged?.Invoke(currentStep);
            Debug.Log($"[StoryManager] Step: {currentStep}");
        }

        public void SetStep(StoryStep step)
        {
            currentStep = step;
            OnStepChanged?.Invoke(currentStep);
            Debug.Log($"[StoryManager] Step: {currentStep}");
        }
    }
}
