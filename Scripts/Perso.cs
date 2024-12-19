using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Perso : MonoBehaviour
{
    [SerializeField] private Transform _transformSphere;
    [SerializeField] private GameObject _iconeMinimap;
    [SerializeField] private GameObject _sphereFeu;
    // Création d'une variable de type float pour stocker la vitesse de mouvement du personnage
    [SerializeField] private float _vitesseMouvMin = 5f;
    [SerializeField] private float _vitesseMouvMax = 10f;
    // Création d'une variable de type float pour stocker la vitesse de rotation du personnage
    private float _vitesseRotation = 3f;
    // Création d'une variable de type float pour stocker la force de l'impulsion du saut du personnage
    [SerializeField] private float _impulsionSaut = 30f;
    // Création d'une variable de type float pour stocker la gravité du personnage
    [SerializeField] private float _gravite = .2f;
    // Création d'une variable de type float pour stocker la taille de la sphère
    [SerializeField] private float _tailleSphereMax = 6f;
    [SerializeField] private float _tailleSphereMin = 2f;
    [SerializeField][Range(0f, 1f)] private float _ratioAmelioration = .2f;
    [SerializeField] private int _dureeFeu = 6;
    [SerializeField] private ParticleSystem _poussiere;
    [SerializeField] private Light _lumFeu;
    [SerializeField] Transform _lookAt;

    [Header("Pouvoirs")]
    [SerializeField] private Sprite[] _iconesPouvoirs;
    [SerializeField] private ParticleSystem _feuPerso;
    [SerializeField] private ParticleSystem _chuteEnflammee;
    [SerializeField] private ParticleSystem _partVitesse;
    [SerializeField] private Explosion _explosion;
    [SerializeField] private Light _lumPouvoirs;
    [SerializeField] private GameObject _murFeu;
    [SerializeField] VisualEffect _effetChargement;

    [Header("Sons")]
    [SerializeField] private AudioClip[] _sonPas;
    [SerializeField] private AudioClip _sonSaut;
    [SerializeField] private AudioClip _sonAtterrissage;
    [SerializeField] private AudioClip _sonAtterrissageViolent;
    [SerializeField] private AudioClip _sonChargement;
    [SerializeField] private AudioClip _sonPouvoir;
    [SerializeField] private AudioClip _sonPasAssezAmes;
    [SerializeField] private AudioClip _sonRepositionnement;
    [SerializeField] private AudioClip _sonPropulsion;
    [SerializeField] private AudioClip _sonChangementPouvoir;

    [SerializeField] private Memoire _memoire;

    private float _vitesseSaut;
    private CinemachineFreeLook _cam;
    private Camera _mainCam;
    private TextMeshProUGUI _champAmes;
    private TextMeshProUGUI _champPouvoirs;
    // Création d'une variable de type Vector3 pour stocker la direction de mouvement du personnage
    private Vector3 _directionsMouvement = Vector3.zero;
    // Création d'une variable de type Animator pour stocker l'animator du personnage
    private Animator _animator;
    // Création d'une variable de type CharacterController pour stocker le controller du personnage
    private CharacterController _controller;
    private int _nbAmes = 0;
    float _vitesseMouv;
    float _tailleSphere;
    bool _estEnFeu = true;
    int _lumFeuIntensite = 50;
    bool _ileDetruite = false;
    float _longueurMurFeuBase = 5;
    float _vitesseMurFeu = 10;
    float _tailleExplosionBase = 5;
    float _tailleRocheBase = 5;
    List<float> _startLifetimeFeuPerso = new List<float>();
    Volcan _volcan;
    float _chargementActuel = 0f;
    bool estEnTrainDeCharger = false;
    float _tempsDepuisDerniereConsommation = 0f; // Temps écoulé depuis la dernière consommation
    float _intervalleConsommation = 0.02f; // Intervalle entre chaque consommation d'âme (en secondes)
    int _indexPouvoirActuel = 0;
    float _tempsDerniereSelection = 0f;
    float _delaiEntreSelections = 0.2f;
    Image _iconePouvoirActuel;
    List<Pouvoir> _listePouvoirs = new List<Pouvoir>();
    bool _persoPeutCharger = false;
    Vector3 _dernierePositionSurIle;
    bool _estEnRepositionnement = false;
    bool _peutSeDeplacer = false;
    List<float> _startLifetimeChuteEnflammee = new();
    List<float> _startLifetimePartVitesse = new();
    GameObject _iconeMinimapInstance;
    BiomesEtatsManager _biome;
    bool _estAuSol = false;
    bool _peutJouerPas = true;
    bool _aJoueExplications = false;
    bool _estAtterriViolemment = false;
    GenererArchipel _archipel;
    private CinemachineBasicMultiChannelPerlin[] _noiseComponents; // Pour accéder aux couches de bruit
    [Header("Couleurs")]
    [SerializeField] Color _couleurPartSec;
    [SerializeField] Color _couleurPartMort;
    [SerializeField] Color _couleurPartLave;
    [SerializeField] Color _couleurPartRoche;
    public BiomesEtatsManager Biome { get => _biome; set => _biome = value; }
    public bool IleDetruite { get => _ileDetruite; set => _ileDetruite = value; }
    public CinemachineFreeLook Cam { get => _cam; set => _cam = value; }
    public Camera MainCam { get => _mainCam; set => _mainCam = value; }
    public TextMeshProUGUI ChampAmes { get => _champAmes; set => _champAmes = value; }
    public TextMeshProUGUI ChampPouvoirs { get => _champPouvoirs; set => _champPouvoirs = value; }
    public Image IconePouvoirActuel { get => _iconePouvoirActuel; set => _iconePouvoirActuel = value; }
    public int NbAmes { get => _nbAmes; set => _nbAmes = value; }
    public bool EstEnFeu { get => _estEnFeu; set => _estEnFeu = value; }
    public Transform LookAt { get => _lookAt; }
    public Volcan Volcan { get => _volcan; set => _volcan = value; }
    public GenererArchipel Archipel { get => _archipel; set => _archipel = value; }

    void Start()
    {
        // Obtenir les composants de bruit des rigs de CinemachineFreeLook
        _noiseComponents = new CinemachineBasicMultiChannelPerlin[3];
        for (int i = 0; i < 3; i++)
        {
            _noiseComponents[i] = _cam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        InitialiserParticulesLifeTime();
        _lumFeu.intensity = _lumFeuIntensite;
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _vitesseMouv = _vitesseMouvMin;
        _tailleSphere = _tailleSphereMin;
        Cursor.lockState = CursorLockMode.Locked;
        RemplirPouvoirs();
        _animator.SetBool("enSaut", true);
        _champPouvoirs.text = _listePouvoirs[_indexPouvoirActuel].CoutAmes.ToString();
        EffetsAmes();
        Vector2 tailleIcone = _listePouvoirs[_indexPouvoirActuel].Icone.rect.size;
        _iconePouvoirActuel.rectTransform.sizeDelta = tailleIcone;
        _iconePouvoirActuel.sprite = _listePouvoirs[_indexPouvoirActuel].Icone;
        StartCoroutine(AttendreSol());
        _iconeMinimapInstance = Instantiate(_iconeMinimap, new Vector3(transform.position.x, 100, transform.position.z), Quaternion.Euler(-90, 0, 0));
        if (_memoire.nbIles > 1) _aJoueExplications = true;
    }

    void FixedUpdate()
    {
        if (_estEnRepositionnement) return; // Ignore tout le reste si en repositionnement

        // J'aimerais donner ma vitesse verticale à l'animator
        _animator.SetFloat("VitesseV", _controller.velocity.y);
        _animator.SetFloat("VitesseH", _vitesseMouv);


        if (_controller.isGrounded && _animator.GetBool("enSaut"))
        {
            SoundManager.Instance.JouerSonGlobal(_sonAtterrissage, 3);
        }

        if (_controller.isGrounded)
        {
            _estAuSol = true;
            _animator.SetBool("enSaut", false);
        }
        else if (_estAuSol) StartCoroutine(DesactiverEstAuSol());

        _iconeMinimapInstance.transform.position = new Vector3(transform.position.x, 100, transform.position.z);
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;


        // Gestion des mouvements verticaux (saut et gravité)
        if (Input.GetButtonDown("Jump") && _estAuSol)
        {
            SoundManager.Instance.JouerSonGlobal(_sonSaut, 3);
            _animator.SetBool("enSaut", true);
            _vitesseSaut = _impulsionSaut;
        }
        _directionsMouvement.y = +_vitesseSaut;
        if (!_estAuSol) _vitesseSaut -= _gravite;

        _controller.Move(_directionsMouvement * Time.deltaTime);

        if (_estEnFeu && !_estAuSol)
        {
            // Vérifie si le personnage tombe (vitesse verticale négative)
            if (_controller.velocity.y < 0)
            {
                if (!_chuteEnflammee.isPlaying)
                {
                    _estAtterriViolemment = false;
                    _chuteEnflammee.Play(); // Assure que les particules jouent
                }

                // Ajuste la durée de vie des particules en fonction de la vitesse de chute
                float vitesseChute = Mathf.Abs(_controller.velocity.y); // Vitesse verticale absolue
                ParticleSystem.MainModule mainChute = _chuteEnflammee.main;
                mainChute.startLifetime = _startLifetimeChuteEnflammee[0] + vitesseChute * 0.02f;

                // Ajuste les particules enfants
                foreach (Transform child in _chuteEnflammee.transform)
                {
                    ParticleSystem.MainModule childMain = child.GetComponent<ParticleSystem>().main;
                    childMain.startLifetime = _startLifetimeChuteEnflammee[child.GetSiblingIndex()] + vitesseChute * 0.02f;
                }
            }
            else
            {
                // Arrête les particules si le personnage ne tombe plus (vitesse verticale non négative)
                if (_controller.velocity.y >= 0 && _chuteEnflammee.isPlaying)
                {
                    _chuteEnflammee.Stop();
                }
            }
        }
        else if (_chuteEnflammee.isPlaying && !_estAtterriViolemment)
        {
            _estAtterriViolemment = true;
            // Arrête les particules si le personnage n'est plus en feu ou est au sol
            StartCoroutine(VibrationRoutine(transform.position, 40));
            SoundManager.Instance.JouerSonGlobal(_sonAtterrissageViolent, 1);
            _chuteEnflammee.Stop();
        }

        if (!_peutSeDeplacer) return;
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _vitesseRotation, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _controller.Move(moveDir.normalized * _vitesseMouv * Time.deltaTime);
        }

        _animator.SetBool("enCourse", direction.magnitude != 0 && _estAuSol);
        if (_animator.GetBool("enCourse"))
        {
            _poussiere.Play();
            _partVitesse.Play();
            if (_peutJouerPas) StartCoroutine(JouerBruitagePas());

            ParticleSystem.MainModule mainPartVitesse = _partVitesse.main;
            mainPartVitesse.startLifetime = _startLifetimePartVitesse[0] + _vitesseMouv / _vitesseMouvMin * 0.1f;

            // Ajuste les particules enfants
            foreach (Transform child in _partVitesse.transform)
            {
                ParticleSystem.MainModule childMain = child.GetComponent<ParticleSystem>().main;
                childMain.startLifetime = _startLifetimePartVitesse[child.GetSiblingIndex()] + _vitesseMouv / _vitesseMouvMin * 0.1f;
            }
        }
        else
        {
            _poussiere.Stop();
            _partVitesse.Stop();
        }

        if (_listePouvoirs[_indexPouvoirActuel].CoutAmes > _nbAmes)
        {
            _iconePouvoirActuel.color = Color.grey;
        }
        else
        {
            _iconePouvoirActuel.color = Color.white;
        }

        // Ajustement de la sphère selon la vitesse
        float vitesseAvant = Input.GetAxis("Vertical") * _vitesseMouv;
        float vitesseCotes = Input.GetAxis("Horizontal") * _vitesseMouv;
        float vitesse = Mathf.Max(Mathf.Abs(vitesseAvant), Mathf.Abs(vitesseCotes));
        _transformSphere.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * _tailleSphere, Mathf.Abs(vitesse) / _vitesseMouv);

        _cam.m_Lens.FarClipPlane = transform.position.y + 100;
    }


    void Update()
    {
        // Gestion de la sélection du pouvoir avec Q
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= _tempsDerniereSelection + _delaiEntreSelections)
        {
            SoundManager.Instance.JouerSonGlobal(_sonChangementPouvoir, 3);
            _tempsDerniereSelection = Time.time; // Met à jour le temps de la dernière sélection
            _indexPouvoirActuel++;

            // Revenir au premier pouvoir si on dépasse la liste
            if (_indexPouvoirActuel >= _listePouvoirs.Count)
            {
                _indexPouvoirActuel = 0;
            }

            Vector2 tailleIcone = _listePouvoirs[_indexPouvoirActuel].Icone.rect.size;
            _iconePouvoirActuel.rectTransform.sizeDelta = tailleIcone;
            _iconePouvoirActuel.sprite = _listePouvoirs[_indexPouvoirActuel].Icone;
            _champPouvoirs.text = _listePouvoirs[_indexPouvoirActuel].CoutAmes.ToString();
        }

        if (!_aJoueExplications && _nbAmes >= _listePouvoirs[_indexPouvoirActuel].CoutAmes)
        {
            StartCoroutine(Archipel.CoroutineExplications());
            _aJoueExplications = true;
        }

        // Gestion du chargement avec E
        if (Input.GetKeyDown(KeyCode.E))
        {
            estEnTrainDeCharger = true;
            _chargementActuel = 0f; // Réinitialise le temps de charge
            _persoPeutCharger = PersoAAssezAmes(_listePouvoirs[_indexPouvoirActuel].CoutAmes);
            _animator.SetBool("enChargement", true);
            if (!_persoPeutCharger) SoundManager.Instance.JouerSonGlobal(_sonPasAssezAmes, 3);
            else
            {
                StartCoroutine(SoundManager.Instance.JouerSonChargement(_sonChargement, _intervalleConsommation * _listePouvoirs[_indexPouvoirActuel].ChargementMax));

            }
        }

        if (Input.GetKey(KeyCode.E) && estEnTrainDeCharger)
        {
            if (_persoPeutCharger)
            {
                _effetChargement.gameObject.SetActive(true); // Activer l'effet de chargement
                // Faire grossir l'effet de chargement en fonction du chargement actuel à partir de 0
                _effetChargement.transform.localScale = Vector3.one * _chargementActuel * .03f;
                _tempsDepuisDerniereConsommation += Time.fixedDeltaTime;

                // Consommer des âmes pour charger
                if (_tempsDepuisDerniereConsommation >= _intervalleConsommation && _nbAmes > 0 && _chargementActuel < _listePouvoirs[_indexPouvoirActuel].ChargementMax)
                {
                    _nbAmes--; // Retirer une âme
                    _chargementActuel += 1; // Ajouter une unité au chargement
                    _tempsDepuisDerniereConsommation = 0f; // Réinitialiser le compteur

                    // Mettre à jour l'affichage des âmes
                    _champAmes.text = _nbAmes.ToString();
                }

                // Limiter le chargement au maximum
                if (_chargementActuel >= _listePouvoirs[_indexPouvoirActuel].ChargementMax)
                {
                    _chargementActuel = _listePouvoirs[_indexPouvoirActuel].ChargementMax;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.E) && estEnTrainDeCharger)
        {
            _animator.SetBool("enChargement", false);
            SoundManager.Instance.DetruireSonChargement();
            SoundManager.Instance.JouerSonGlobal(_sonPouvoir, 3);
            estEnTrainDeCharger = false;
            StartCoroutine(CoroutineFinChargement());

            // Activer le pouvoir sélectionné
            Pouvoir pouvoir = _listePouvoirs[_indexPouvoirActuel];
            if (_persoPeutCharger) ActiverPouvoir(pouvoir, _chargementActuel);
        }

        // Gestion des effets de feu
        if (_estEnFeu && _estAuSol)
        {
            StartCoroutine(CoroutineEnFeu());
        }
    }

    void InitialiserParticulesLifeTime()
    {
        _startLifetimePartVitesse.Add(_partVitesse.main.startLifetime.constant);
        foreach (Transform child in _partVitesse.transform)
        {
            ParticleSystem.MainModule childMain = child.GetComponent<ParticleSystem>().main;
            _startLifetimePartVitesse.Add(childMain.startLifetime.constant);
        }

        foreach (Transform child in _feuPerso.transform)
        {
            ParticleSystem childParticleSystem = child.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule childMain = childParticleSystem.main;
            _startLifetimeFeuPerso.Add(childMain.startLifetime.constant);
        }

        _chuteEnflammee.Play();
        _startLifetimeChuteEnflammee.Add(_chuteEnflammee.main.startLifetime.constant);
        foreach (Transform child in _chuteEnflammee.transform)
        {
            ParticleSystem.MainModule childMain = child.GetComponent<ParticleSystem>().main;
            _startLifetimeChuteEnflammee.Add(childMain.startLifetime.constant);
        }
    }

    IEnumerator DesactiverEstAuSol()
    {
        float tempsAttente = 0.5f;
        float timer = 0f;
        while (timer < tempsAttente)
        {
            timer += Time.deltaTime;
            if (_controller.isGrounded) yield break;
            yield return null;
        }
        _estAuSol = false;
    }

    void RemplirPouvoirs()
    {
        // Paramètre 1 : nom du pouvoir, Paramètre 2 : coût en âmes, Paramètre 3 : maxChargementAmes
        _listePouvoirs.Add(new Pouvoir("Roche Enflammée", 10, 100, _iconesPouvoirs[2]));
        _listePouvoirs.Add(new Pouvoir("Explosion", 20, 100, _iconesPouvoirs[1]));
        _listePouvoirs.Add(new Pouvoir("Mur de feu", 30, 100, _iconesPouvoirs[0]));
    }

    public void AjouterAmes()
    {
        _nbAmes++;
        _champAmes.text = _nbAmes.ToString();
        EffetsAmes();
    }

    void EffetsAmes()
    {
        _vitesseMouv = _nbAmes * _ratioAmelioration + _vitesseMouvMin;
        _vitesseMouv = Mathf.Clamp(_vitesseMouv, _vitesseMouvMin, _vitesseMouvMax);
        _tailleSphere = _nbAmes * _ratioAmelioration + _tailleSphereMin;
        _tailleSphere = Mathf.Clamp(_tailleSphere, _tailleSphereMin, _tailleSphereMax);
        _lumPouvoirs.intensity = Mathf.Clamp(_nbAmes, 0, 15);
        foreach (Transform child in _feuPerso.transform)
        {
            ParticleSystem childParticleSystem = child.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule childMain = childParticleSystem.main;
            float startLifetime = _startLifetimeFeuPerso[_feuPerso.transform.childCount - 1];
            startLifetime *= _nbAmes * .02f;
            startLifetime = Mathf.Clamp(startLifetime, 0, _startLifetimeFeuPerso[_feuPerso.transform.childCount - 1] * 2);
            childMain.startLifetime = startLifetime;
        }

    }

    private void ActiverPouvoir(Pouvoir pouvoir, float puissance)
    {
        Debug.Log($"Pouvoir {pouvoir.Nom} activé !");

        // Ajouter ici les effets ou animations spécifiques du pouvoir
        if (pouvoir.Nom == "Explosion")
        {
            Explosion explosion = Instantiate(_explosion, transform.position, Quaternion.identity);
            explosion.Rayon = puissance * .3f + _tailleExplosionBase;
        }
        else if (pouvoir.Nom == "Mur de feu")
        {
            StartCoroutine(CoroutineMurFeu(puissance));
        }
        else if (pouvoir.Nom == "Roche Enflammée")
        {
            _volcan.LancerRochesEnflammees(transform.position, puissance, _tailleRocheBase);
            StartCoroutine(VibrationRoutine(_volcan.transform.position, puissance));
        }
    }


    public IEnumerator CoroutineEnFeu()
    {
        _lumFeu.intensity = _lumFeuIntensite;
        _estEnFeu = false;
        _sphereFeu.SetActive(true);
        float timer = 0;
        while (timer < _dureeFeu)
        {
            timer += Time.deltaTime;
            _lumFeu.intensity = Mathf.Lerp(_lumFeuIntensite, 0, timer / _dureeFeu);
            yield return null;
        }
        _sphereFeu.SetActive(false);
    }

    private IEnumerator VibrationRoutine(Vector3 posObjet, float puissance)
    {
        // Calcul de la distance entre la caméra et l'objet
        float distance = Vector3.Distance(transform.position, posObjet);

        // Calcul d'un facteur basé sur la distance (plus on est loin, plus l'amplitude est faible)
        float distanceFactor = Mathf.Clamp01(1 - (distance / 100f)); // 10f = distance maximale pour effet

        // Appliquer les vibrations initiales
        foreach (var noise in _noiseComponents)
        {
            if (noise != null)
            {
                noise.m_AmplitudeGain = distanceFactor * puissance / 8; // Amplitude basée sur le facteur de distance
                noise.m_FrequencyGain = distanceFactor * puissance / 8; // Fréquence également
            }
        }

        // Maintenir les vibrations pendant une courte période
        yield return new WaitForSeconds(0.2f);

        // Diminuer progressivement les vibrations
        while (_noiseComponents[0].m_AmplitudeGain > 0)
        {
            foreach (var noise in _noiseComponents)
            {
                if (noise != null)
                {
                    noise.m_AmplitudeGain = Mathf.Max(0, noise.m_AmplitudeGain - Time.deltaTime * 5); // Réduction graduelle
                    noise.m_FrequencyGain = Mathf.Max(0, noise.m_FrequencyGain - Time.deltaTime * 5); // Réduction graduelle
                }
            }

            yield return null; // Attendre une frame avant de continuer
        }
    }

    public IEnumerator VibrationFin()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, _volcan.transform.position);
            // Calcul d'un facteur basé sur la distance (plus on est loin, plus l'amplitude est faible)
            float distanceFactor = Mathf.Clamp01(1 - (distance / 100f)); // 10f = distance maximale pour effet
            foreach (var noise in _noiseComponents)
            {
                if (noise != null)
                {
                    noise.m_AmplitudeGain = distanceFactor * 2; // Amplitude basée sur le facteur de distance
                    noise.m_FrequencyGain = distanceFactor * 2; // Amplitude basée sur le facteur de distance
                }
            }
            yield return null;
        }

    }


    IEnumerator CoroutineMurFeu(float puissance)
    {
        GameObject mur = Instantiate(_murFeu, transform.position, Quaternion.identity);
        mur.transform.rotation = Quaternion.LookRotation(transform.forward);
        float longueurMurFeu = mur.transform.localScale.z;
        while (longueurMurFeu <= puissance / 2 + _longueurMurFeuBase)
        {
            longueurMurFeu += Time.deltaTime * _vitesseMurFeu;
            mur.transform.localScale = new Vector3(1, 20, longueurMurFeu);
            yield return null;
        }
        Destroy(mur);
    }

    IEnumerator CoroutineFinChargement()
    {
        while (_effetChargement.transform.localScale.x < 10f)
        {
            _effetChargement.transform.localScale *= 1.5f;
            yield return null;
        }
        _effetChargement.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LaveVolcan"))
        {
            _animator.SetBool("enSaut", true);
            SoundManager.Instance.JouerSonGlobal(_sonPropulsion, 8f);
            StartCoroutine(DevenirEnFeu());
            _vitesseSaut = other.transform.position.y * 2;
            _lumFeu.intensity = _lumFeuIntensite;
            if (_ileDetruite)
            {
                _vitesseSaut = other.transform.position.y * 3;
                _ileDetruite = false;
                StartCoroutine(CoroutineFin());
            }
        }

        if (other.CompareTag("Eau"))
        {
            // Si le personnage entre en contact avec un objet ayant le tag "Eau"
            if (other.CompareTag("Eau"))
            {
                ReplacerSurIle();
            }
        }
    }

    IEnumerator DevenirEnFeu()
    {
        yield return new WaitForSeconds(.5f);
        _estEnFeu = true;
        StopCoroutine(CoroutineEnFeu());
    }

    IEnumerator CoroutineFin()
    {
        yield return new WaitForSeconds(1);
        _cam.Follow = null;
        yield return new WaitForSeconds(1);
        yield return GetComponentInParent<GenererArchipel>().FadeToBlack();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    bool PersoAAssezAmes(int coutAmes)
    {
        if (_nbAmes >= coutAmes)
        {
            _nbAmes -= coutAmes;
            _champAmes.text = _nbAmes.ToString();
            EffetsAmes();
            return true;
        }
        return false;
    }

    IEnumerator JouerBruitagePas()
    {
        _peutJouerPas = false;
        float tempsEntrePas = 0.27f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            float pitch = _vitesseMouv / _vitesseMouvMax + .5f;
            if (hit.collider.CompareTag("Cube"))
            {
                _dernierePositionSurIle = hit.collider.transform.position;
                string etatBloc = hit.collider.GetComponent<BiomesEtatsManager>().infos["etat"];
                ParticleSystem.MainModule main = _poussiere.main;

                switch (etatBloc)
                {
                    case "Sec":
                    case "Vivant":
                        main.startColor = _couleurPartSec;
                        SoundManager.Instance.JouerSonGlobal(_sonPas[0], 4f, pitch);
                        break;

                    case "Mort":
                        main.startColor = _couleurPartMort;
                        SoundManager.Instance.JouerSonGlobal(_sonPas[1], 2f, pitch);
                        break;

                    case "Lave":
                        main.startColor = _couleurPartLave;
                        SoundManager.Instance.JouerSonGlobal(_sonPas[2], 2f, pitch);
                        break;

                    case "Roche":
                        main.startColor = _couleurPartRoche;
                        SoundManager.Instance.JouerSonGlobal(_sonPas[3], 2f, pitch);
                        break;
                }
            }
            else if (hit.collider.CompareTag("CubeVolcan"))
            {
                SoundManager.Instance.JouerSonGlobal(_sonPas[3], 2f, pitch);
            }
        }
        yield return new WaitForSeconds(tempsEntrePas);
        _peutJouerPas = true;
    }

    private void ReplacerSurIle()
    {
        if (_dernierePositionSurIle != Vector3.zero && !_estEnRepositionnement)
        {
            SoundManager.Instance.JouerSonGlobal(_sonRepositionnement, 3);
            _estEnRepositionnement = true;

            // Replace le joueur à la dernière position connue
            transform.position = _dernierePositionSurIle + Vector3.up;

            // Délai pour stabiliser avant de réactiver les contrôles
            StartCoroutine(ReactiverControle());
        }
    }

    IEnumerator ReactiverControle()
    {
        yield return new WaitForSeconds(0.1f); // Attendre un court instant pour stabiliser
        _estEnRepositionnement = false; // Réactive les contrôles
    }

    private IEnumerator AttendreSol()
    {
        // Attendre que le personnage touche le sol
        while (!_controller.isGrounded)
        {
            yield return null;  // Attendre jusqu'à la prochaine frame
        }

        // Une fois au sol, permettre le déplacement
        _peutSeDeplacer = true;
        StartCoroutine(CoroutineEnFeu());
    }

}