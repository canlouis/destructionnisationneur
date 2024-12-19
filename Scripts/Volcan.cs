using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Volcan : MonoBehaviour
{
    [SerializeField] Light _lumVolcanPrefab;
    [SerializeField] GameObject _projectileVolcan;
    [SerializeField] int _tailleFumeeMin = 5;
    [SerializeField] int _tailleFumeeMax = 25;
    [SerializeField] float _intensiteLum = 100;
    [SerializeField] GameObject _cubeVolcan;
    [SerializeField] GameObject _laveVolcan;
    [SerializeField] VisualEffect _fumeePrefab;
    [SerializeField] GameObject _iconeVolcan;
    [SerializeField] Sprite _iconeVolcanFin;
    [SerializeField] AudioClip _sonGronder;
    [SerializeField] AudioClip _sonEruption;
    [SerializeField] ParticleSystem _effetEruption;
    Sprite _iconeVolcanBase;
    int _rayonBase;
    float _hauteurVolcan;
    float _rayonSommet;
    float _rayonCratere;
    Vector3 _posRocheBase;
    VisualEffect _fumee;
    Light _lumVolcan;
    GenererArchipel _archipel;
    GradientColorKey[] _colorKeys;
    int _tailleIle;
    ParticleSystem _effetEruptionInstance;
    public GenererArchipel Archipel { get => _archipel; set => _archipel = value; }
    public int TailleIle { get => _tailleIle; set => _tailleIle = value; }
    public float HauteurVolcan { get => _hauteurVolcan;}

    public void GenererVolcan()
    {
        // Debug.Log(SoundManager.Instance.JouerSon);
        // SoundManager.Instance.JouerSon(_sonGronder, transform.position, 0, 2, true);
        _iconeVolcanBase = _iconeVolcan.GetComponent<SpriteRenderer>().sprite;
        _iconeVolcan.transform.parent = transform;
        _rayonBase = _tailleIle / 5;
        _hauteurVolcan = _rayonBase * 0.75f;
        _rayonSommet = _rayonBase / 3;
        _rayonCratere = _rayonSommet * 0.75f;

        _effetEruptionInstance = Instantiate(_effetEruption, transform.position + new Vector3(0, _hauteurVolcan + 3, 0), Quaternion.Euler(-90, 0, 0));
        _effetEruptionInstance.transform.parent = transform;
        _effetEruptionInstance.transform.localScale = Vector3.one * 2;
        // Ensemble des positions déjà générées
        HashSet<Vector3> positionsGenerees = new();
        List<BiomesEtatsManager> biomes = _archipel.Biomes;
        List<BiomesEtatsManager> biomesASupprimer = new List<BiomesEtatsManager>();

        List<GameObject> volcanCubes = new();
        List<float> couches = new();
        for (int y = 0; y <= _hauteurVolcan; y++)
        {
            float interpolation = (float)y / _hauteurVolcan;
            int rayonExterieur = Mathf.RoundToInt(Mathf.Lerp(_rayonBase, _rayonSommet, interpolation));
            couches.Add(rayonExterieur);

            for (int x = -rayonExterieur; x <= rayonExterieur; x++)
            {
                for (int z = -rayonExterieur; z <= rayonExterieur; z++)
                {
                    float distanceAuCentre = Mathf.Sqrt(x * x + z * z);

                    if (distanceAuCentre <= rayonExterieur && distanceAuCentre > _rayonCratere)
                    {
                        Vector3 positionAbsolue = transform.position + new Vector3(x, y, z);
                        // Ajoute la position générée dans l'ensemble
                        positionsGenerees.Add(positionAbsolue);
                    }
                }
            }
        }

        foreach (Vector3 positonGeneree in positionsGenerees)
        {
            if (EstCubeVisible(positonGeneree, positionsGenerees) && positonGeneree.y > 0)
            {
                GameObject cubeVolcan = Instantiate(_cubeVolcan, positonGeneree, Quaternion.identity);
                cubeVolcan.transform.parent = transform;
                volcanCubes.Add(cubeVolcan);
            }
        }

        foreach (BiomesEtatsManager biome in biomes)
        {
            if (Vector3.Distance(biome.transform.position, transform.position) < _rayonBase)
            {
                for (int i = 0; i < couches.Count; i++)
                {
                    if (Mathf.FloorToInt(biome.transform.position.y) == i &&
                        couches[i] - 1 >= Vector3.Distance(Vector3Int.FloorToInt(biome.transform.position), Vector3Int.FloorToInt(transform.position)))
                    {
                        biomesASupprimer.Add(biome);
                        break;
                    }
                }
            }
        }

        // Supprimer les éléments après la boucle
        foreach (BiomesEtatsManager biome in biomesASupprimer)
        {
            _archipel.Biomes.Remove(biome);
            Destroy(biome.gameObject);
        }

        _posRocheBase = transform.position + new Vector3(0, _hauteurVolcan * 0.75f, 0);
        GameObject laveVolcan = Instantiate(_laveVolcan, _posRocheBase, Quaternion.identity);
        laveVolcan.transform.localScale = new Vector3(_rayonSommet * 2, 0.1f, _rayonSommet * 2);
        laveVolcan.transform.parent = transform;
        _lumVolcan = Instantiate(_lumVolcanPrefab, _posRocheBase, Quaternion.identity);
        _lumVolcan.transform.parent = transform;
        _fumee = Instantiate(_fumeePrefab, transform.position + new Vector3(0, _hauteurVolcan / 2, 0), Quaternion.Euler(-64, 0, 0));
        _fumee.transform.parent = transform;
        _colorKeys = _fumee.GetGradient("CouleurFumee").colorKeys;
        ReinitialiserVariationsVolcan();
    }


    bool EstCubeVisible(Vector3 position, HashSet<Vector3> positionsGenerees)
    {
        // Liste des directions pour vérifier les voisins adjacents
        Vector3[] directions = {
        Vector3.forward, // (0, 0, 1)
        Vector3.back,    // (0, 0, -1)
        Vector3.left,    // (-1, 0, 0)
        Vector3.right,   // (1, 0, 0)
        Vector3.up,      // (0, 1, 0)
        Vector3.down     // (0, -1, 0)
        };

        // Vérifie si un des voisins n'est pas dans positionsGenerees
        foreach (Vector3 direction in directions)
        {
            Vector3 voisin = position + direction;

            // Si un voisin n'est pas généré, le cube est visible
            if (!positionsGenerees.Contains(voisin))
            {
                return true;
            }
        }

        // Si tous les voisins sont présents, le cube est invisible
        return false;
    }

    public void LancerRochesEnflammees(Vector3 cible, float puissance, float tailleRocheBase)
    {
        // Déterminer une position cible aléatoire
        Vector3 positionCible = TrouverTrajectoireCible(cible);
        Vector3 direction = CalculerLancementDirection(transform.position, positionCible);

        // Lancer une roche vers cette position
        LancerRoche(direction, _posRocheBase, puissance, tailleRocheBase);
    }

    Vector3 TrouverTrajectoireCible(Vector3 cible)
    {
        float angle = Random.Range(0, Mathf.PI * 2); // Angle en radians
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)); // Direction sur le plan XZ
        Vector3 pos = cible + direction;
        pos.y = 0; // S'assurer que la position est au niveau du sol
        return pos;
    }
    
    Vector3 CalculerLancementDirection(Vector3 startPoint, Vector3 endPoint)
    {
        float angleLancement = 70f; // Angle souhaité en degrés
        float angleEnRadians = angleLancement * Mathf.Deg2Rad;
        float gravite = Mathf.Abs(Physics.gravity.y);

        // Calcul des distances
        Vector3 distanceH = new Vector3(endPoint.x - startPoint.x, 0, endPoint.z - startPoint.z); // Distance horizontale
        float distanceHorizontale = distanceH.magnitude;
        float hauteurRelative = endPoint.y - startPoint.y; // Différence de hauteur

        // Vérification des conditions physiques
        if (gravite <= 0 || distanceHorizontale == 0)
        {
            return Vector3.zero;
        }

        // Calcul de la vitesse initiale requise (v0)
        float term1 = gravite * distanceHorizontale * distanceHorizontale;
        float term2 = 2 * Mathf.Pow(Mathf.Cos(angleEnRadians), 2) * (distanceHorizontale * Mathf.Tan(angleEnRadians) - hauteurRelative);

        if (term2 <= 0)
        {
            return Vector3.zero;
        }

        float v0 = Mathf.Sqrt(term1 / term2);

        // Calcul de la vitesse initiale horizontale et verticale
        Vector3 velociteH = distanceH.normalized * v0 * Mathf.Cos(angleEnRadians);
        float velociteV = v0 * Mathf.Sin(angleEnRadians);

        // Combine la vitesse horizontale et verticale
        return velociteH + Vector3.up * velociteV;
    }


    void LancerRoche(Vector3 direction, Vector3 posBase, float puissance, float tailleRocheBase)
    {
        _effetEruptionInstance.Play();
        SoundManager.Instance.JouerSonGlobal(_sonEruption, puissance * 0.05f + 3);
        GameObject roche = Instantiate(_projectileVolcan, posBase, Quaternion.identity);
        Rigidbody rb = roche.GetComponent<Rigidbody>();
        roche.transform.localScale = Vector3.one * Mathf.Clamp(puissance * .5f + tailleRocheBase, 0, _rayonCratere * 2.5f);
        roche.transform.parent = transform;

        // Appliquer une force vers la cible
        rb?.AddForce(direction, ForceMode.VelocityChange);
    }


    public void VarierVolcanAvecMonde(float destruction, float seuilDestruction, bool actif)
    {
        if(actif) _iconeVolcan.GetComponent<SpriteRenderer>().sprite = _iconeVolcanFin;
        _lumVolcan.intensity = destruction * _intensiteLum / seuilDestruction;
        _fumee.transform.localScale = Vector3.one * Mathf.Clamp(destruction * _tailleFumeeMax / seuilDestruction, _tailleFumeeMin, _tailleFumeeMax);
        _colorKeys[^1].time = Mathf.Clamp(1 - destruction / seuilDestruction, 0.01f, 1f);
        _fumee.SetGradient("CouleurFumee", new Gradient { colorKeys = _colorKeys });
        SoundManager.Instance.VariationVolumeSonVolcan(destruction / seuilDestruction * 5f);
    }
    void ReinitialiserVariationsVolcan()
    {
        _iconeVolcan.GetComponent<SpriteRenderer>().sprite = _iconeVolcanBase;
        _lumVolcan.intensity = _intensiteLum;
        _fumee.transform.localScale = Vector3.one * _tailleFumeeMin;
        _colorKeys[^1].time = 1f;
        _fumee.SetGradient("CouleurFumee", new Gradient { colorKeys = _colorKeys });
    }
}