using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using Cinemachine;
using UnityEngine.VFX;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GenererArchipel : MonoBehaviour
{
    // Sérialiser le prefab Ile (gameObject prefab qui contient le script générateur d'île auto)
    [SerializeField] Memoire _memoire;
    [SerializeField] private GenerateurIleAuto _ilePrefab;
    [SerializeField] GameObject _ame;
    [SerializeField] ParticleSystem _feu;
    [SerializeField] GameObject _prefabCube;
    // Sérialiser la propriété int _nombresDIles dans un interval possible de 1 à 30 îles. Déclaré à 1 par défaut
    [Range(1, 30)][SerializeField] private int _nbIles = 1;
    // Sérialiser les deux propriétés int _tailleMin et _variationsMaxTaille pour avoir des îles de taille aléatoire comprises entre _tailleMin et _tailleMin additionnée de _variationsMaxTaille. Déclaré à 10 par défaut
    [SerializeField] private int _tailleMin = 10;
    [SerializeField] private int _variationsMaxTaille = 10;
    // Sérialiser u int _padding qui ajoute un espace entre deux îles (valeur int libre). Déclaré à 10 par défaut
    [SerializeField] private int _padding = 10;
    // Sérialiser un booléen _carteBiome qui va dire aux îles créées si les biomes utilisent la map perlin de base (hauteur des cubes) ou une map perlin distnicte. Déclaré à false par défaut
    [SerializeField] private bool _carteBiome = false;
    // Sérialiser un booléen _circulaire qui va dire aux îles créées si elles seront circulaiers ou rectangulaires. Déclaré à false par défaut
    [SerializeField] private bool _circulaire = false;
    [SerializeField] float _seuilDestruction = 80;
    [Header("UI")]
    [SerializeField] private Image _iconePouvoirActuel;
    [SerializeField] private Image _iconeFeu;
    [SerializeField] private Image _iconeFleur;
    [SerializeField] private Sprite[] _iconesFeu;
    [SerializeField] private Sprite[] _iconesFleur;
    [SerializeField] private TextMeshProUGUI _champAmes;
    [SerializeField] private TextMeshProUGUI _champPouvoirs;
    [SerializeField] private ParticleSystem _flammeUI;
    [SerializeField] private Image _sliderDestruction;
    [SerializeField] private CanvasGroup _explication;
    [SerializeField] private TextMeshProUGUI _champExplications;
    [SerializeField] private TextMeshProUGUI _champTouches;
    [SerializeField] CanvasGroup _fadeCanvas; // Canvas pour le fade

    [Header("Camera")]
    [SerializeField] private CinemachineFreeLook _camera;
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Minimap _minimap;
    [SerializeField] private Perso _perso;
    [SerializeField] float _nbEnnemis = 1;
    [SerializeField][Range(2, 100)] float _forcePerlin = 10f;
    // _forcePerlin : la force appliquée au bruit de Perlin pour la génération du relief, bornée entre 2 et 100
    [SerializeField][Range(2, 100)] float _forcePerlinVariantes = 10f;
    // _forcePerlin : la force appliquée au bruit de Perlin pour la génération du relief, bornée entre 2 et 100
    [SerializeField][Range(2, 100)] float _forcePerlinBiomes = 10f;
    // _penteDegrade : la pente de la fonction sigmoïde pour gérer la transition en dégradé, bornée entre 1 et 100
    [SerializeField][Range(1, 100)] int _penteDegrade = 10;
    // _centreDegrade : détermine la position centrale du dégradé, bornée entre 0 et 1
    [SerializeField][Range(.6f, 1f)] float _pourcentHorsEau = .8f;
    [SerializeField][Range(0, 100)] int _fertile = 0;
    [SerializeField] Material _skybox;
    [SerializeField] GameObject _ocean;
    [SerializeField] ParticleSystem _cendres;

    [Header("Couleurs")]
    [SerializeField] Color _couleurSkyBase;
    [SerializeField] Color _couleurSkyDetruit;
    [SerializeField] Color _couleurOceanBase;
    [SerializeField] Color _couleurRippleBase;
    [SerializeField] Color _couleurOceanDetruit;
    [SerializeField] Color _couleurRippleDetruit;
    [SerializeField] Color _couleurSec;
    [SerializeField] Color _couleurVivantMin;
    [SerializeField] Color _couleurVivantMax;

    [Header("Volcan")]
    [SerializeField] Volcan _volcan;
    [SerializeField] PartFin _particulesFin; // Préfab à instancier
    int _nbPoints; // Nombre de points à répartir sur le périmètre
    float _hauteurVolcan;
    bool _aEteInstancie = false;
    List<Vector3> _pointsPerimetre = new(); // Liste pour stocker les objets instanciés

    private List<Vector3> _contourPositions = new(); // Liste des positions qui composent le périmètre

    [Header("Sons")]
    [SerializeField] AudioClip _sonFeu;
    [SerializeField] AudioClip _sonFeuApparait;
    [SerializeField] AudioClip _sonDestructionItem;
    [SerializeField] AudioClip _sonPlantePousse;
    [SerializeField] AudioClip _sonOcean;

    List<List<BiomesEtatsManager>> _biomesListes = new();
    List<BiomesEtatsManager> _biomes = new();
    Perso _persoInstance;
    Volcan _volcanInstance;


    private HashSet<Vector3Int> _cubesPresents = new HashSet<Vector3Int>();
    public List<BiomesEtatsManager> Biomes { get => _biomes; set => _biomes = value; }
    public bool AEteInstancie { get => _aEteInstancie; }

    void Awake()
    {
        // On appelle la méthode GenererIles avec le nombre d'îles défini dans l'éditeur
        GenererIles(2);
        StartCoroutine(FadeOutOfBlack());
        ReinitialiserInfosMonde();
        Vector2 tailleIconeFleur = _iconesFleur[0].rect.size;
        tailleIconeFleur.x *= 100 / tailleIconeFleur.y;
        tailleIconeFleur.y = 100;
        _iconeFleur.rectTransform.sizeDelta = tailleIconeFleur;
        Vector2 tailleIconeFeu = _iconesFeu[0].rect.size;
        tailleIconeFeu.x *= 100 / tailleIconeFeu.y;
        tailleIconeFeu.y = 100;
        _iconeFeu.rectTransform.sizeDelta = tailleIconeFeu;
        // _memoire.nbIles++;
    }

    void GenererIles(int nbIles)
    {
        // On déclare deux float offsetx et offsetz qu'on met à 0 par défaut pour que l'île 1 soit générée à la position 0,0,0
        float offsetx = 0;
        float offsetz = 0;

        // On crée un int tailleIlePrecedente (défaut à 0) pour y stocker la taille de l'île que l'on vient de créer et utiliser cette valeur lors de la création de l'île suivante pour définir le offset entre les deux îles (évite le chevauchement entre deux îles)
        int tailleIlePrecedente = 0;
        int ileVolcanRand = Random.Range(0, _nbIles);
        int ilePersoRand = Random.Range(0, _nbIles);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        // On loop entre 0 et notre nombre d'île définie dans l'éditeur avec notre slider entre 1 et 30
        for (int i = 0; i < nbIles; i++)
        {
            // Choix de la taille d'île aléatoire, on défini la valeur du int tailleIle entre _tailleMin et _tailleMin additionnée de _variationsMaxTaille
            int tailleIle = Random.Range(_tailleMin, _tailleMin + _variationsMaxTaille);

            // On défini la taille réelle de l'île en multipliant sa taille par son pourcentage hors de l'eau
            int tailleIleReelle = (int)(tailleIle * _pourcentHorsEau);

            // Le offset de l'île à créer est l'addition des deux tailles d'îles (précédente et courante) divisé par 2
            int offset = (tailleIle + tailleIlePrecedente) / 2;

            // Une chance sur 2 d'ajouter ce offset ainsi que le padding au x de l'île sinon, ajout au z de l'île
            if (Random.value > 0.5f)
            {
                offsetx += offset + _padding;
            }
            else
            {
                offsetz += offset + _padding;
            }

            // On instancie le prefab de notre île avec les coordonnées définies précédemment (l'île reste 0 en y)
            GenerateurIleAuto uneIle = Instantiate(_ilePrefab, new Vector3(offsetx, 0, offsetz), Quaternion.identity);
            uneIle.transform.parent = transform;


            float coefficientH = Random.Range(_forcePerlin / 4, _forcePerlin * .8f);

            // On appelle la méthode publique CreerIle de notre île instanciée en lui passant tous les paramètres requis
            // CreerIle(largeur, profondeur, coeff hauteur cubes, zoom perlin, zoom perlin biomes, zoom perlin variants, pourcentage erosion ile, pourcentage hors eau, carteBiome, circulaire)
            uneIle.CreerIle(tailleIle, tailleIle, coefficientH, _forcePerlin, _forcePerlinBiomes, _forcePerlinVariantes, _penteDegrade, _pourcentHorsEau, _carteBiome, _circulaire, _prefabCube);
            _biomesListes.Add(uneIle.EnvoieBiomes());
            for (int j = 0; j < _biomesListes[i].Count; j++)
            {
                _cubesPresents.Add(Vector3Int.FloorToInt(_biomesListes[i][j].transform.position));
            }
            for (int j = 0; j < _biomesListes[i].Count; j++)
            {
                _biomes.Add(_biomesListes[i][j]);
                _biomesListes[i][j].infos.Add("cubesPresents", _cubesPresents);
                _biomesListes[i][j].infos.Add("archipel", this);
                _biomesListes[i][j].infos.Add("perso", _persoInstance);
                _biomesListes[i][j].infos.Add("ame", _ame);
                _biomesListes[i][j].infos.Add("fertile", _fertile);
                _biomesListes[i][j].infos.Add("feu", _feu);
                _biomesListes[i][j].infos.Add("aUnItem", false);
                _biomesListes[i][j].infos.Add("genFeu", 0);
                _biomesListes[i][j].infos.Add("couleurSec", _couleurSec);
                _biomesListes[i][j].infos.Add("couleurVivantMin", _couleurVivantMin);
                _biomesListes[i][j].infos.Add("couleurVivantMax", _couleurVivantMax);
                _biomesListes[i][j].infos.Add("cendres", _cendres);
                _biomesListes[i][j].infos.Add("sonFeu", _sonFeu);
                _biomesListes[i][j].infos.Add("sonDestructionItem", _sonDestructionItem);
                _biomesListes[i][j].infos.Add("sonPlantePousse", _sonPlantePousse);
                _biomesListes[i][j].infos.Add("sonFeuApparait", _sonFeuApparait);
            }

            // Mise à jour des positions minimales et maximales
            minX = Mathf.Min(minX, offsetx);
            maxX = Mathf.Max(maxX, offsetx);
            minZ = Mathf.Min(minZ, offsetz);
            maxZ = Mathf.Max(maxZ, offsetz);

            // On stocke la taille de l'île actuelle * son pourcentage hors de l'eau dans tailleIlePrecedente avant de repartir la boucle et créer l'île suivante
            tailleIlePrecedente = tailleIleReelle;

            if (ilePersoRand == i)
            {
                Vector3 posPersoRand = new Vector3(Random.Range(10, tailleIleReelle / 3), 60, Random.Range(10, tailleIleReelle / 3));
                _persoInstance = Instantiate(_perso, new Vector3(uneIle.transform.position.x + posPersoRand.x, posPersoRand.y, uneIle.transform.position.z + posPersoRand.z), Quaternion.identity);
                _camera.Follow = _persoInstance.LookAt;
                _camera.LookAt = _persoInstance.LookAt;
                _camera.transform.position = _persoInstance.LookAt.position + new Vector3(0, 10, -10);
                _persoInstance.Cam = _camera;
                _persoInstance.MainCam = _mainCam;
                _persoInstance.ChampAmes = _champAmes;
                _persoInstance.ChampPouvoirs = _champPouvoirs;
                _persoInstance.IconePouvoirActuel = _iconePouvoirActuel;
                _persoInstance.transform.parent = transform;
                _persoInstance.Archipel = this;
            }

            if (i == ileVolcanRand)
            {
                _volcanInstance = Instantiate(_volcan, new Vector3(uneIle.transform.position.x, 0, uneIle.transform.position.z), Quaternion.identity);
                _volcanInstance.transform.parent = transform;
                _volcanInstance.TailleIle = tailleIleReelle;
                _volcanInstance.Archipel = this;
            }
        }

        _persoInstance.Volcan = _volcanInstance;
        _volcanInstance.GenererVolcan();
        _hauteurVolcan = _volcanInstance.HauteurVolcan;
        _persoInstance.Biome = _biomes[0];

        _minimap.CamPerso.transform.parent = _persoInstance.transform;
        _minimap.CamPerso.transform.localPosition = new Vector3(0, 500, 0);
        StartCoroutine(InfosMonde());
        CalculerPerimetre();
        _nbPoints = _nbIles * 10;
        PlacerPoints();
        // Invoke("PlacerEnnemis", 3);
    }

    void CalculerPerimetre()
    {
        _contourPositions.Clear();


        foreach (BiomesEtatsManager bloc in _biomes)
        {
            // Vérifie si le bloc est en bordure de l'île (pas complètement entouré)
            if (EstEnPerimetre(bloc.transform.position))
            {
                _contourPositions.Add(bloc.transform.position);
            }
        }

        // Trier les positions pour former un contour continu (optionnel si nécessaire)
        _contourPositions = TrierPositionsContour(_contourPositions);
    }

    bool EstEnPerimetre(Vector3 position)
    {
        // Vérifie si un bloc autour est vide (c'est un bloc de bordure)
        Vector3[] directions = {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right
        };

        foreach (Vector3 dir in directions)
        {
            if (!Physics.CheckBox(position + dir, Vector3.one * 0.4f))
            {
                return true;
            }
        }

        return false;
    }

    void PlacerPoints()
    {
        if (_contourPositions.Count == 0)
        {
            Debug.LogWarning("Le périmètre est vide !");
            return;
        }

        // Calcule l'intervalle entre chaque point à instancier
        float intervalle = _contourPositions.Count / (float)_nbPoints;

        for (int i = 0; i < _nbPoints; i++)
        {
            // Trouve la position correspondante le long du périmètre
            int index = Mathf.RoundToInt(i * intervalle) % _contourPositions.Count;
            Vector3 position = _contourPositions[index];
            // SoundManager.Instance.JouerSon(_sonOcean, position, 0, 3f);
            _pointsPerimetre.Add(position);
        }
    }

    List<Vector3> TrierPositionsContour(List<Vector3> positions)
    {
        // Implémente un algorithme de tri pour organiser les positions en un contour continu
        // Exemple : Recherche de la position la plus proche de manière itérative
        List<Vector3> sortedPositions = new List<Vector3> { positions[0] };
        positions.RemoveAt(0);

        while (positions.Count > 0)
        {
            Vector3 dernier = sortedPositions[sortedPositions.Count - 1];
            Vector3 plusProche = positions[0];
            float distanceMin = Vector3.Distance(dernier, plusProche);

            foreach (Vector3 pos in positions)
            {
                float distance = Vector3.Distance(dernier, pos);
                if (distance < distanceMin)
                {
                    distanceMin = distance;
                    plusProche = pos;
                }
            }

            sortedPositions.Add(plusProche);
            positions.Remove(plusProche);
        }

        return sortedPositions;
    }

    public List<BiomesEtatsManager> ChercheBiomes(string info, dynamic valeur)
    {
        List<BiomesEtatsManager> listeTemp = new();
        foreach (List<BiomesEtatsManager> listebiomes in _biomesListes)
        {
            foreach (BiomesEtatsManager biome in listebiomes)
                if (biome.infos.ContainsKey(info) && biome.infos[info].Equals(valeur))
                {
                    listeTemp.Add(biome);
                }
        }
        return listeTemp;
    }

    private float PourcentageBiomes(string typeBiome)
    {
        return Mathf.Round((float)ChercheBiomes("etat", typeBiome).Count / (float)_biomes.Count * 1000) / 10;
    }

    IEnumerator InfosMonde()
    {
        List<float> startLifetime = new List<float>();
        List<float> startSize = new List<float>();
        // Variables pour gérer les paliers de destruction
        int dernierPalier = 0; // Dernier seuil de 20% atteint
        const int intervallePalier = 20; // Intervalle en pourcentage pour faire apparaître un ennemi

        foreach (Transform child in _flammeUI.transform)
        {
            ParticleSystem childParticleSystem = child.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule childMain = childParticleSystem.main;
            startLifetime.Add(childMain.startLifetime.constant);
            startSize.Add(childMain.startSize.constant);
        }
        yield return new WaitForSeconds(1);
        _aEteInstancie = true;
        float destruction;
        

            Debug.Log("_seuilDestruction" + _seuilDestruction);
        while (true)
        {
            destruction = PourcentageBiomes("Mort") + PourcentageBiomes("Lave") + PourcentageBiomes("Roche");
            _volcanInstance.VarierVolcanAvecMonde(destruction, _seuilDestruction, false);
            _sliderDestruction.fillAmount = destruction / _seuilDestruction;

            Vector2 tailleIconeFleur = _iconesFleur[Mathf.FloorToInt(destruction / _seuilDestruction * (_iconesFleur.Length - 1))].rect.size;
            tailleIconeFleur.x *= 100 / tailleIconeFleur.y;
            tailleIconeFleur.y = 100;
            _iconeFleur.rectTransform.sizeDelta = tailleIconeFleur;
            _iconeFleur.sprite = _iconesFleur[Mathf.FloorToInt(destruction / _seuilDestruction * (_iconesFleur.Length - 1))];
            Vector2 tailleIconeFeu = _iconesFeu[Mathf.FloorToInt(destruction / _seuilDestruction * (_iconesFeu.Length - 1))].rect.size;
            tailleIconeFeu.x *= 100 / tailleIconeFeu.y;
            tailleIconeFeu.y = 100;
            _iconeFeu.rectTransform.sizeDelta = tailleIconeFeu;
            _iconeFeu.sprite = _iconesFeu[Mathf.FloorToInt(destruction / _seuilDestruction * (_iconesFeu.Length - 1))];

            _ocean.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.Lerp(_couleurOceanBase, _couleurOceanDetruit, destruction / _seuilDestruction));
            _ocean.GetComponent<Renderer>().material.SetColor("_RippleColor", Color.Lerp(_couleurRippleBase, _couleurRippleDetruit, destruction / _seuilDestruction));
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(_couleurSkyBase, _couleurSkyDetruit, destruction / _seuilDestruction));
            GererFeuUi(startLifetime, startSize);

            // Vérifie si un nouveau palier de 20% a été atteint
            int palierActuel = Mathf.FloorToInt(destruction / intervallePalier);
            if (palierActuel > dernierPalier)
            {
                dernierPalier = palierActuel;
                PlacerEnnemis(); // Fonction pour gérer l'apparition d'un ennemi
            }

            if (destruction >= _seuilDestruction)
            {
                _volcanInstance.VarierVolcanAvecMonde(destruction, _seuilDestruction, true);
                FinPartie();
                yield break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    void ReinitialiserInfosMonde()
    {
        _sliderDestruction.fillAmount = 0;
        _ocean.GetComponent<Renderer>().material.SetColor("_BaseColor", _couleurOceanBase);
        _ocean.GetComponent<Renderer>().material.SetColor("_RippleColor", _couleurRippleBase);
        RenderSettings.skybox.SetColor("_Tint", _couleurSkyBase);
    }

    public IEnumerator CoroutineExplications()
    {
        // Faire fade in les explications
        while (_explication.alpha < 1)
        {
            _explication.alpha += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5);

        // Faire fade out les explications
        while (_explication.alpha > 0)
        {
            _explication.alpha -= Time.deltaTime;
            yield return null;
        }

        _champExplications.text = "Maintenir     pour charger le pouvoir";
        _champTouches.rectTransform.anchoredPosition = new Vector3(-148, 0, 0);
        yield return new WaitForSeconds(1);

        // Faire fade in les explications
        while (_explication.alpha < 1)
        {
            _explication.alpha += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5);

        // Faire fade out les explications
        while (_explication.alpha > 0)
        {
            _explication.alpha -= Time.deltaTime;
            yield return null;
        }

        _champExplications.text = "Utiliser     pour alterner entre les pouvoirs";
        _champTouches.text = "Q";
        _champTouches.rectTransform.anchoredPosition = new Vector3(-259, 0, 0);
        yield return new WaitForSeconds(1);

        while (_explication.alpha < 1)
        {
            _explication.alpha += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5);

        // Faire fade out les explications
        while (_explication.alpha > 0)
        {
            _explication.alpha -= Time.deltaTime;
            yield return null;
        }
    }

    void GererFeuUi(List<float> startLifetimeBase, List<float> startSizeBase)
    {
        foreach (Transform child in _flammeUI.transform)
        {
            ParticleSystem childParticleSystem = child.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule childMain = childParticleSystem.main;
            float startLifetime = _persoInstance.NbAmes * .04f * startLifetimeBase[child.GetSiblingIndex()];
            float startSize = _persoInstance.NbAmes * .04f * startSizeBase[child.GetSiblingIndex()];
            startSize = Mathf.Clamp(startSize, 0, startLifetimeBase[child.GetSiblingIndex()] * 5);
            startLifetime = Mathf.Clamp(startLifetime, 0, startSizeBase[child.GetSiblingIndex()] * 5);
            childMain.startLifetime = startLifetime;
            childMain.startSize = startSize;
        }
    }

    void PlacerEnnemis()
    {
        for (int i = 0; i < _nbEnnemis; i++)
        {
            BiomesEtatsManager unCube = _biomes[Random.Range(0, _biomes.Count)];
            GameObject unEnnemi = Instantiate((GameObject)Resources.Load("Ennemi/Ennemi1"), new Vector3(unCube.transform.position.x, unCube.transform.position.y, unCube.transform.position.z), Quaternion.identity);
            unEnnemi.GetComponentInChildren<EnnemiEtatsManager>().infos.Add("maison", unCube);
            unEnnemi.GetComponentInChildren<EnnemiEtatsManager>().infos.Add("achipel", this);
            unEnnemi.GetComponentInChildren<EnnemiEtatsManager>().infos.Add("perso", _persoInstance);
            unEnnemi.GetComponentInChildren<EnnemiEtatsManager>().infos.Add("danger", null);
            unEnnemi.transform.parent = transform;
        }
    }

    List<List<T>> Shuffle<T>(List<List<T>> list)
    {
        foreach (List<T> sublist in list)
        {
            for (int i = 0; i < sublist.Count; i++)
            {
                T temp = sublist[i];
                int randomIndex = Random.Range(i, sublist.Count);
                sublist[i] = sublist[randomIndex];
                sublist[randomIndex] = temp;
            }
        }
        return list;
    }

    void FinPartie()
    {
        _persoInstance.IleDetruite = true;
        foreach (BiomesEtatsManager biome in _biomes)
        {
            if (biome.infos["etat"] == "Vivant" || biome.infos["etat"] == "Sec") StartCoroutine(FoncerBiomesFin(biome));
            else if (biome.infos.ContainsKey("ameInstance")) StartCoroutine(DetruireAmes(biome));
        }
        StartCoroutine(_persoInstance.VibrationFin());
        StartCoroutine(FaireApparraitrePartFin());
    }

    IEnumerator FaireApparraitrePartFin()
    {
        while (true)
        {
            foreach (Vector3 point in _pointsPerimetre)
            {
                PartFin particulesFin = Instantiate(_particulesFin, point, Quaternion.identity);
                particulesFin.transform.parent = transform;
                Vector3 posCratere = _volcanInstance.transform.position + Vector3.up * (_hauteurVolcan + 5);
                particulesFin.VolcanTarget = posCratere;
            }
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator FoncerBiomesFin(BiomesEtatsManager biome)
    {
        float tempsFoncer = 2f;
        float tempsRestant = 0;
        Color couleurSol = biome.GetComponent<Renderer>().material.color;

        while (tempsRestant < tempsFoncer)
        {
            tempsRestant += Time.deltaTime;
            biome.GetComponent<Renderer>().material.color = new Color(couleurSol.r - tempsRestant / tempsFoncer, couleurSol.g - tempsRestant / tempsFoncer, couleurSol.b - tempsRestant / tempsFoncer);
            yield return null;
        }
        biome.ChangerEtat(biome.Mort);
        if (biome.infos.ContainsKey("ameInstance")) StartCoroutine(DetruireAmes(biome));
    }

    IEnumerator DetruireAmes(BiomesEtatsManager biome)
    {
        float temps = 2f;
        float tempsRestant = 0;
        while (biome.infos["ameInstance"] && tempsRestant < temps)
        {
            tempsRestant += Time.deltaTime;
            biome.infos["ameInstance"].transform.localScale = Vector3.Lerp(biome.infos["ameInstance"].transform.localScale, Vector3.zero, tempsRestant / temps);
            yield return null;
        }
        Destroy(biome.infos["ameInstance"]);
    }

    private IEnumerator FadeOutOfBlack()
    {
        yield return new WaitForSeconds(1);
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            _fadeCanvas.alpha = alpha;
            yield return null;
        }
    }

    public IEnumerator FadeToBlack()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime;
            _fadeCanvas.alpha = alpha;
            yield return null;
        }
    }
}