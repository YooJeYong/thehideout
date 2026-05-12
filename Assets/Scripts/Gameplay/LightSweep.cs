using UnityEngine;

namespace Hidenet.Gameplay
{
    [RequireComponent(typeof(Light))]
    public class LightSweep : MonoBehaviour
    {
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 endPosition;
        [SerializeField] private float duration = 3f;

        [Header("Intensity")]
        [SerializeField] private float startIntensity = 1f;
        [SerializeField] private float peakIntensity = 3f;

        private Light _light;
        private float _elapsed;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = (_elapsed % duration) / duration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            _light.intensity = Mathf.Lerp(startIntensity, peakIntensity, t);
        }
    }
}
