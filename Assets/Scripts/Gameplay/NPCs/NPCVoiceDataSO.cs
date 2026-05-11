using Hidenet.Audio;
using UnityEngine;

namespace Hidenet.NPCs
{
    [CreateAssetMenu(fileName = "NPCVoiceData", menuName = "Hidenet/NPC/Voice Data", order = 0)]
    public class NPCVoiceDataSO : ScriptableObject
    {
        public SoundDataSO[] lines;

        public SoundDataSO GetLine(int step)
        {
            if (lines == null || lines.Length == 0) return null;
            if (step < 0 || step >= lines.Length) return null;
            return lines[step];
        }
    }
}
