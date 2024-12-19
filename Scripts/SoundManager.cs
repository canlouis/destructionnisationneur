using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    // Singleton
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
            }
            return _instance;
        }
    }

    public AudioClip musique1;
    public AudioClip musique2;

    GameObject _audioManager;
    AudioSource _tempAudioChargement;
    AudioSource _audioVolcan;
    bool _enChargement = false;

    void Awake()
    {
                Debug.Log(_instance);
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        _audioManager = new GameObject("AudioManager");
        // DontDestroyOnLoad(_audioManager);
    }

    void Start()
    {
        // Lancer la séquence musicale
        if (SceneManager.GetActiveScene().name != "Menu") StartCoroutine(JouerMusique());
    }

    private IEnumerator JouerMusique()
    {
        AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
        tempAudioSource.clip = musique1;
        tempAudioSource.Play();
        yield return new WaitForSeconds(musique1.length - 0.7f); // Attendre que la musique se termine

        // Jouer la deuxième musique
        tempAudioSource.clip = musique2;
        tempAudioSource.loop = true;
        tempAudioSource.Play();
    }

    public void JouerSon(AudioClip clip, Vector3 position, float tempsDestruction, float volume = 1.0f, bool sonVolcan = false)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip est null !");
            return;
        }

        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        tempAudio.transform.parent = _audioManager.transform;

        AudioSource tempAudioSource = tempAudio.AddComponent<AudioSource>();
        tempAudioSource.clip = clip;
        tempAudioSource.pitch = Random.Range(0.8f, 1.2f); // Variation de pitch
        tempAudioSource.spatialBlend = 1.0f; // 3D spatial sound
        tempAudioSource.volume = volume;
        tempAudioSource.Play();

        if (tempsDestruction > 0) Destroy(tempAudio, tempsDestruction);
        else tempAudioSource.loop = true;

        if (sonVolcan)
        {
            _audioVolcan = tempAudioSource;
        }
    }

    public void JouerSonGlobal(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip est null !");
            return;
        }

        AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
        tempAudioSource.clip = clip;
        if (pitch == 1.0f) pitch = Random.Range(.8f, 1.2f);
        tempAudioSource.pitch = pitch; // Variation de pitch
        tempAudioSource.volume = volume;
        tempAudioSource.Play();

        Destroy(tempAudioSource, clip.length);
    }

    public IEnumerator JouerSonChargement(AudioClip clip, float tempsChargement, float volume = 1.0f)
    {
        _enChargement = true;
        if (clip == null)
        {
            Debug.LogError("AudioClip est null !");
            yield break;
        }

        _tempAudioChargement = gameObject.AddComponent<AudioSource>();
        _tempAudioChargement.clip = clip;
        _tempAudioChargement.volume = volume;
        _tempAudioChargement.Play();
        _tempAudioChargement.loop = true;


        float tempsEcoule = 0;

        while (tempsEcoule < tempsChargement && _enChargement)
        {
            tempsEcoule += Time.fixedDeltaTime;
            _tempAudioChargement.pitch = Mathf.Lerp(0.8f, 1.2f, tempsEcoule / tempsChargement);
            yield return null;
        }
    }

    public void VariationVolumeSonVolcan(float volume)
    {
        if (_audioVolcan != null)
        {
            _audioVolcan.volume = volume;
        }
    }

    public void DetruireSonChargement()
    {
        if (_tempAudioChargement != null)
        {
            _enChargement = false;
            Destroy(_tempAudioChargement);
        }
    }
}
