using UnityEngine;

namespace Hidenet.NPCs
{
    [RequireComponent(typeof(Collider))]
    public class JaneNPC : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[Jane] Collision: Awwwwwwwww{collision.gameObject.name}");
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Jane] Trigger: Awwwwwwwww {other.name}");
        }
    }
}
