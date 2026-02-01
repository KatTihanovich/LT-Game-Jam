using UnityEngine;

namespace Game.Audio
{
    public interface ISoundManager
    {
        void PlaySound(string soundName);
        AudioSource PlaySound(string soundName, bool loop);
        void StopSound(AudioSource source);
    }
}
