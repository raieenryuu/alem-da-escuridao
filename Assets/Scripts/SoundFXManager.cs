using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    
    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundEffectClip(AudioClip audioClip, Transform spawnTransform, float volume, float clipLength = -1f)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        if (clipLength < 0f)
        {
            clipLength = audioSource.clip.length;
        }

        Destroy(audioSource.gameObject, clipLength);
    }
    
    public void PlayRandomSoundEffectClip(AudioClip[] audioClips, Transform spawnTransform, float volume)
    {
        int rand = Random.Range(0, audioClips.Length);
        
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClips[rand];

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}