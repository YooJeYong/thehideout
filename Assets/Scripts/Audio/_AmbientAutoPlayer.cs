using UnityEngine;

namespace Hidenet.Audio
{
    /// <summary>
    /// 임시 테스트용. 씬 시작 시 지정한 Ambient 사운드를 자동 재생한다.
    /// 검증 후 삭제 권장.
    /// </summary>
    public class _AmbientAutoPlayer : MonoBehaviour
    {
        [Header("재생할 SoundDataSO 드래그")]
        [SerializeField] private SoundDataSO ambientSound;

        [Header("Resources 경로 사용 (SoundDataSO 비어있을 때)")]
        [SerializeField] private string resourcePath = "Ambient/AMB_Sewer_Drips";

        [Header("재생 시작 지연 (초)")]
        [SerializeField] private float startDelay = 0f;

        private void Start()
        {
            if (startDelay > 0f)
            {
                Invoke(nameof(Play), startDelay);
            }
            else
            {
                Play();
            }
        }

        private void Play()
        {
            if (SoundManager.Instance == null)
            {
                Debug.LogWarning("[_AmbientAutoPlayer] SoundManager.Instance is null.");
                return;
            }

            if (ambientSound != null)
            {
                SoundManager.Instance.PlayAmbient(ambientSound);
                Debug.Log($"[_AmbientAutoPlayer] Played: {ambientSound.name}");
            }
            else if (!string.IsNullOrEmpty(resourcePath))
            {
                SoundManager.Instance.PlayAmbient(resourcePath);
                Debug.Log($"[_AmbientAutoPlayer] Played from Addressables: {resourcePath}");
            }
            else
            {
                Debug.LogWarning("[_AmbientAutoPlayer] No SoundDataSO or resource path set.");
            }
        }
    }
}
