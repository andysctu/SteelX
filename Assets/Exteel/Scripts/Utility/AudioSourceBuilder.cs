using UnityEngine;

namespace Utility {
    class AudioSourceBuilder {
        public static AudioSource Build(GameObject g, float volume = 1, bool loop = false, float dopplerLevel = 0, float minDistance = 20, float maxDistance = 250, float spatialBlend = 1) {
            AudioSource audioSource = g.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.dopplerLevel = dopplerLevel;
            audioSource.maxDistance = maxDistance;
            audioSource.minDistance = minDistance;
            audioSource.spatialBlend = spatialBlend;

            return audioSource;
        }
    }
}
