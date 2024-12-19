using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

// Définition d'une classe "GenerateurIle" qui hérite de MonoBehaviour, ce qui permet à cette classe d'être attachée à des objets Unity
public class GenerateurIleAuto : MonoBehaviour
{
    // _largeurIle : la largeur de l'île générée, avec des bornes comprises entre 10 et 1000
    int _largeurIle;
    // _profondeurIle : la profondeur de l'île générée, avec des bornes comprises entre 10 et 1000
    int _profondeurIle;
    // _forcePerlin : la force appliquée au bruit de Perlin pour la génération du relief, bornée entre 2 et 100
    float _forcePerlin;
    // _forcePerlin : la force appliquée au bruit de Perlin pour la génération du relief, bornée entre 2 et 100
    float _forcePerlinVariantes;
    // _forcePerlin : la force appliquée au bruit de Perlin pour la génération du relief, bornée entre 2 et 100
    float _forcePerlinBiomes;
    // _coefficientH : coefficient qui affecte la hauteur des éléments de l'île, bornée entre 2 et 100
    float _coefficientH;
    // _penteDegrade : la pente de la fonction sigmoïde pour gérer la transition en dégradé, bornée entre 1 et 100
    float _penteDegrade;
    // _centreDegrade : détermine la position centrale du dégradé, bornée entre 0 et 1
    float _centreDegrade;
    // _prefabCube : référence au prefab utilisé pour générer les cubes du terrain
    GameObject _prefabCube;
    List<List<Material>> _biomesMats = new();
    // _biomesItems : liste de listes d'objets pour les biomes
    List<List<GameObject>> _biomesItems = new();
    List<BiomesEtatsManager> _biomesListe = new();

    // Fonction Start, appelée automatiquement lors de l'initialisation de l'objet
    void Awake()
    {
        // Charge les ressources nécessaires pour la génération de l'île
        LoadResources();
        LoadItems();
    }

    public void CreerIle(int il, int ip, float ch, float fp, float fpb, float fpv, int pd, float cd, bool carteBiome, bool circulaire, GameObject prefabCube)
    {
        _largeurIle = il;
        _profondeurIle = ip;
        _coefficientH = ch;
        _forcePerlin = fp;
        _forcePerlinBiomes = fpb;
        _forcePerlinVariantes = fpv;
        _penteDegrade = pd;
        _centreDegrade = cd;
        _prefabCube = prefabCube;
        GenererCarte(carteBiome, circulaire);
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    // Fonction GenererCarte pour générer l'île
    void GenererCarte(bool carteBiome, bool circulaire)
    {
        // Génère une carte de hauteur avec la fonction Terraforme
        float[,] uneCarte = Terraforme(_largeurIle, _profondeurIle, _forcePerlin);
        // Modifie la carte pour ajouter l'effet d'eau (centre dégradé)
        // Si circulaire est vrai, applique un dégradé circulaire, sinon un dégradé rectangulaire à l'île
        uneCarte = circulaire ? AquaformeC(uneCarte) : AquaformeR(uneCarte);
        // Génère une carte de biomes avec la fonction Terraforme si carteBiome est vrai, sinon utilise la carte de hauteur
        float[,] uneCarteBiomes = carteBiome ? Terraforme(_largeurIle, _profondeurIle, _forcePerlin * _forcePerlinBiomes) : uneCarte;
        float[,] uneCarteVariantes = Terraforme(_largeurIle, _profondeurIle, _forcePerlin * _forcePerlinVariantes);
        // Affiche la carte visuellement dans la scène
        AfficherIle(uneCarte, uneCarteVariantes, uneCarteBiomes);
        // Place les personnages sur la carte
        // PlacerPersos(uneCarte);
    }

    // Fonction Terraforme qui crée un terrain basé sur le bruit de Perlin
    float[,] Terraforme(int largeur, int profondeur, float fP)
    {
        // Crée un tableau de floats pour stocker les hauteurs du terrain
        float[,] terrain = new float[largeur, profondeur];
        // Génère un bruit de Perlin aléatoire pour varier la topologie
        float newNoise = UnityEngine.Random.Range(0, 100000);
        // Parcours chaque point de la carte pour générer le relief
        for (int z = 0; z < profondeur; z++)
        {
            for (int x = 0; x < largeur; x++)
            {
                // Calcule la valeur du bruit de Perlin pour chaque point
                float y = Mathf.PerlinNoise((x / fP) + newNoise, (z / fP) + newNoise);
                Mathf.Clamp01(y);
                // Stocke cette valeur dans le tableau terrain
                terrain[x, z] = y;
            }
        }
        // Retourne le tableau de hauteurs
        return terrain;
    }

    // Fonction AquaformeR qui applique un dégradé rectangulaire sur le terrain
    float[,] AquaformeR(float[,] terrain)
    {
        // Récupère les dimensions du terrain
        int l = terrain.GetLength(0);
        int p = terrain.GetLength(1);

        // Applique une transformation à chaque point pour l'atténuer selon sa distance des bords
        for (int z = 0; z < p; z++)
        {
            for (int x = 0; x < l; x++)
            {
                // Calcule la distance relative au centre en x et en z
                float dx = x / (float)l * 2 - 1;
                float dz = z / (float)p * 2 - 1;
                // Prend la valeur maximale entre dx et dz pour obtenir l'éloignement des bords
                float val = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz));
                // Applique une fonction sigmoïde pour adoucir le dégradé
                val = Sigmoide(val);
                // Modifie la hauteur en fonction du dégradé
                terrain[x, z] = Mathf.Clamp01(terrain[x, z] - val);
            }
        }
        // Retourne le terrain modifié
        return terrain;
    }

    // Fonction AquaformeC qui applique un dégradé circulaire sur le terrain
    float[,] AquaformeC(float[,] terrain)
    {
        // Récupère les dimensions du terrain
        int l = terrain.GetLength(0);
        int p = terrain.GetLength(1);

        // Applique une transformation à chaque point pour l'atténuer selon sa distance du centre
        for (int z = 0; z < p; z++)
        {
            for (int x = 0; x < l; x++)
            {
                // Calcule la distance relative au centre en x et en z
                float dx = x / (float)l * 2 - 1;
                float dz = z / (float)p * 2 - 1;
                // Calcule la distance radiale à partir du centre
                float val = Mathf.Sqrt(MathF.Pow(dx, 2) + MathF.Pow(dz, 2));
                // Applique une fonction sigmoïde pour adoucir le dégradé
                val = Sigmoide(val);
                // Modifie la hauteur en fonction du dégradé circulaire
                terrain[x, z] = Mathf.Clamp01(terrain[x, z] - val);
            }
        }
        // Retourne le terrain modifié
        return terrain;
    }

    // Fonction pour afficher visuellement l'île dans Unity
    void AfficherIle(float[,] carte, float[,] carteVariante, float[,] carteBiomes)
    {
        // Parcourt chaque point de la carte pour placer des cubes
        for (int z = 0; z < carte.GetLength(1); z++)
        {
            for (int x = 0; x < carte.GetLength(0); x++)
            {
                // Récupère la hauteur du terrain à cet emplacement
                float y = carte[x, z];
                // Calcule la hauteur du cube en fonction de la hauteur du terrain
                float hCube = y * _coefficientH;

                // Si la hauteur est positive, place un cube à cet endroit
                if (y > 0)
                {
                    // Instancie un cube à l'endroit correspondant
                    GameObject unCube = Instantiate(_prefabCube, transform.position + new Vector3(x, hCube, z) - new Vector3(_largeurIle / 2, 0, _profondeurIle / 2), Quaternion.identity);
                    // Le parent de l'île
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("vivant", _biomesMats[0]);
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("mort", _biomesMats[1]);
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("lave", _biomesMats[2]);
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("roche", _biomesMats[3]);
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("sable", _biomesMats[4]);
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("items", _biomesItems);
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("coefficientH", _coefficientH);
                    unCube.transform.parent = transform;
                    int quelItem = Mathf.RoundToInt(carteBiomes[x, z] * (_biomesItems.Count - 1));
                    unCube.GetComponent<BiomesEtatsManager>().infos.Add("item", quelItem);

                    _biomesListe.Add(unCube.GetComponent<BiomesEtatsManager>());
                }
            }
        }
    }

    // Fonction Sigmoide pour adoucir la transition entre les hauteurs du terrain
    float Sigmoide(float value)
    {
        // Fonction sigmoïde : 1 / (1 + e^(-k * (x - c))) où k est la pente et c le centre
        return 1 / (1 + Mathf.Exp(-_penteDegrade * (value - _centreDegrade)));
    }

    private void LoadResources()
    {
        int nbBiomes = 1;  // Initialisation du compteur de biomes
        int nbVariantes = 1;  // Initialisation du compteur de variantes pour chaque biome
        bool resteDesMats = true;  // Flag pour indiquer s'il reste des matériaux à charger

        List<Material> tpBiome = new List<Material>();  // Liste temporaire pour stocker les matériaux d'un biome

        do
        {
            UnityEngine.Object mats = Resources.Load($"Biomes/Mats/b{nbBiomes}_{nbVariantes}");  // Chargement d'un matériau spécifique en fonction du biome et de la variante

            if (mats)
            {
                tpBiome.Add((Material)mats);  // Ajout du matériau à la liste temporaire
                nbVariantes++;  // Passage à la variante suivante
            }
            else
            {
                if (nbVariantes == 1)
                {
                    resteDesMats = false;  // S'il n'y a pas de variante pour le biome, fin du chargement des matériaux
                }
                else
                {
                    _biomesMats.Add(tpBiome);  // Ajout de la liste des matériaux du biome dans _biomesMats
                    tpBiome = new List<Material>();  // Réinitialisation de la liste temporaire pour le prochain biome
                    nbBiomes++;  // Passage au biome suivant
                    nbVariantes = 1;  // Réinitialisation des variantes pour le nouveau biome
                }
            }
        } while (resteDesMats);  // Répéter tant qu'il y a des matériaux à charger
    }

    private void LoadItems()
    {
        int nbBiomes = 1;  // Initialisation du compteur de biomes
        int nbVariantes = 1;  // Initialisation du compteur de variantes pour chaque biome
        bool resteDesItems = true;  // Flag pour indiquer s'il reste des items à charger

        List<GameObject> tpBiome = new List<GameObject>();  // Liste temporaire pour stocker les items d'un biome

        do
        {
            UnityEngine.Object items = Resources.Load($"Items/i{nbBiomes}_{nbVariantes}");

            if (items)
            {
                tpBiome.Add((GameObject)items);  // Ajout de l'item à la liste temporaire
                nbVariantes++;  // Passage à la variante suivante
            }
            else
            {
                if (nbVariantes == 1)
                {
                    resteDesItems = false;  // S'il n'y a pas de variante pour le biome, fin du chargement des items
                }
                else
                {
                    _biomesItems.Add(tpBiome);  // Ajout de la liste des items du biome dans _biomesItems
                    tpBiome = new List<GameObject>();  // Réinitialisation de la liste temporaire pour le prochain biome
                    nbBiomes++;  // Passage au biome suivant
                    nbVariantes = 1;  // Réinitialisation des variantes pour le nouveau biome
                    // Affiche le nom de l'item chargé
                }
            }
        } while (resteDesItems);  // Répéter tant qu'il y a des items à charger
    }

    public List<BiomesEtatsManager> EnvoieBiomes()
    {
        return _biomesListe;
    }
}