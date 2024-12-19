using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuAnimation : MonoBehaviour
{
    [SerializeField] VolcanMenu _volcan; // Position finale du volcan
    [SerializeField] float _vitesse = 5f; // Vitesse de déplacement
    [SerializeField] float _forcePropulsion = 20f; // Force de la propulsion
    [SerializeField] ParticleSystem _partVitesse; // Force de la propulsion
    [SerializeField] CanvasGroup _fadeCanvas; // Canvas pour le fade
    [SerializeField] Button _bouton; // Canvas pour le fade
    [SerializeField] TextMeshProUGUI _titre; // Canvas pour le fade
    [SerializeField] Light _lumPerso; // Canvas pour le fade
    [SerializeField] CinemachineFreeLook _cam; // Canvas pour le fade
    [SerializeField] Transform _lookAt; // Canvas pour le fade
    [SerializeField] Memoire _memoire; // Canvas pour le fade
    [SerializeField] string _sceneJeu = "IleProcedurale"; // Nom de la scène de jeu
    [SerializeField] AudioClip _sonBouton; // Nom de la scène de jeu
    [SerializeField] AudioClip _sonEruption; // Nom de la scène de jeu
    [SerializeField] ParticleSystem _effetEruption; // Nom de la scène de jeu

    private CharacterController _characterController;
    private Animator _personnageAnimator; // Animator du personnage
    private bool _enCours = false;
    private Vector3 _gravityEffect = Vector3.zero; // Effet de gravité
    private float _gravity = -9.81f; // Valeur de la gravité
    private bool _estPropulse = false; // Indique si le personnage est propulsé
    private bool _arreterDeplacement = false; // Indique si le déplacement doit être arrêté
    private CinemachineBasicMultiChannelPerlin[] _noiseComponents; // Pour accéder aux couches de bruit

    private void Start()
    {
        _volcan.MenuAnimation = this;
        // Récupère les composants nécessaires
        _characterController = GetComponent<CharacterController>();
        _personnageAnimator = GetComponent<Animator>();
        _personnageAnimator.SetFloat("VitesseH", 10);
        if (_characterController == null)
        {
            Debug.LogError("Le personnage n'a pas de CharacterController !");
        }

        // Obtenir les composants de bruit des rigs de CinemachineFreeLook
        _noiseComponents = new CinemachineBasicMultiChannelPerlin[3];
        for (int i = 0; i < 3; i++)
        {
            _noiseComponents[i] = _cam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void Jouer()
    {
        if (!_enCours)
        {
            StopAllCoroutines();
            _partVitesse.Play();
            SoundManager.Instance.JouerSonGlobal(_sonBouton, 7f);
            _enCours = true;
            _memoire.nbIles = 1;
            StartCoroutine(SequenceJouer());
        }
    }

    private IEnumerator SequenceJouer()
    {
        _cam.LookAt = _lookAt;
        StartCoroutine(FadeOutRoutine());
        // Activer l'animation de course
        _personnageAnimator.SetBool("enCourse", true);

        // Déplacement vers le volcan
        yield return StartCoroutine(DeplacementVersDestination());

        _lumPerso.intensity = 2000;
        StartCoroutine(Propulsion());
        yield return new WaitForSeconds(2.5f);

        // Désactiver l'animation de course
        _personnageAnimator.SetBool("enCourse", false);

        // Transition : fade to black
        yield return StartCoroutine(FadeToBlack());

        // Charger la scène de jeu
        SceneManager.LoadScene(_sceneJeu);
    }

    private IEnumerator DeplacementVersDestination()
    {
        Vector3 destination = _volcan.transform.position + new Vector3(0, 20, 0);

        const float groundTolerance = 0.05f;

        while (Vector3.Distance(transform.position, destination) > 0.1f && !_arreterDeplacement)
        {
            _personnageAnimator.SetFloat("VitesseV", _characterController.velocity.y);
            destination = new Vector3(_volcan.transform.position.x, _characterController.transform.position.y, _volcan.transform.position.z);

            Vector3 direction = (destination - transform.position).normalized;

            if (_characterController.isGrounded)
            {
                if (!_estPropulse) 
                {
                    _gravityEffect.y = -groundTolerance; 
                }
            }
            else
            {
                _gravityEffect.y += _gravity * Time.deltaTime;
            }

            Vector3 mouvement = (direction * _vitesse) + _gravityEffect;
            _characterController.Move(mouvement * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }

            yield return null;
        }

        _gravityEffect.y = 0;
    }


    private IEnumerator FadeToBlack()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime;
            _fadeCanvas.alpha = alpha;
            yield return null;
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            _titre.alpha = alpha;
            _bouton.image.color = new Color(_bouton.image.color.r, _bouton.image.color.g, _bouton.image.color.b, alpha);
            _bouton.GetComponentInChildren<TextMeshProUGUI>().alpha = alpha;
            yield return null;
        }
    }

    IEnumerator Propulsion()
    {
        _effetEruption.Play();
        StartCoroutine(VibrationRoutine());
        SoundManager.Instance.JouerSonGlobal(_sonEruption, 6f);
        float tempsSaut = 3.5f;
        float tempsEcoule = 0f;
        _gravityEffect = Vector3.up * _forcePropulsion;
        while (tempsEcoule < tempsSaut)
        {
            tempsEcoule += Time.deltaTime;
            Vector3 mouvement = _gravityEffect;
            _characterController.Move(mouvement * Time.deltaTime);

            yield return null;
        }
    }

    public IEnumerator VibrationRoutine(float force = 5f)
    {
        // Activer les vibrations
        foreach (var noise in _noiseComponents)
        {
            if (noise != null)
            {
                noise.m_AmplitudeGain = force;
                noise.m_FrequencyGain = force;
            }
        }

        yield return new WaitForSeconds(.2f);

        while (_noiseComponents[0].m_AmplitudeGain > 0)
        {
            foreach (var noise in _noiseComponents)
            {
                if (noise != null)
                {
                    noise.m_AmplitudeGain -= Time.deltaTime * 5;
                    noise.m_FrequencyGain -= Time.deltaTime * 5;
                }
            }

            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LaveVolcan"))
        {
            _personnageAnimator.SetBool("enSaut", true);
            // Activer la propulsion
            _estPropulse = true;
            _arreterDeplacement = true;
            StopCoroutine(DeplacementVersDestination());
        }
    }
}
