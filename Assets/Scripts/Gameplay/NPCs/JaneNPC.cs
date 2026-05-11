using Hidenet.Audio;
using Hidenet.Core;
using UnityEngine;

namespace Hidenet.NPCs
{
    [RequireComponent(typeof(Collider))]
    public class JaneNPC : MonoBehaviour
    {
        [SerializeField] private NPCVoiceDataSO voiceData;

        private bool hasPlayed;

        private void OnEnable()
        {
            StoryManager.Instance.OnStepChanged += OnStoryChanged;
        }

        private void OnDisable()
        {
            if (StoryManager.HasInstance)
                StoryManager.Instance.OnStepChanged -= OnStoryChanged;
        }

        private void OnStoryChanged(StoryStep _)
        {
            hasPlayed = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasPlayed) return;
            if (!collision.gameObject.CompareTag("Player")) return;
            hasPlayed = true;

            StoryStep step = StoryManager.Instance.CurrentStep;
            var line = voiceData?.GetLine((int)step);

            Debug.Log($"[Jane] step={step} voice={(line != null ? line.name : "없음")}");
            if (line != null)
                SoundManager.Instance.PlayVoice(line);

            StoryManager.Instance.NextStep();
        }
    }
}
