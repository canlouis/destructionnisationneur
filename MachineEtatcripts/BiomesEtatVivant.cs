using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BiomesEtatVivant : BiomesEtatsBase
{
    Color _couleurVivant;
    public override void InitEtat(BiomesEtatsManager biome)
    {
        ExitEtat(biome);
        // Appliquer la couleur en fonction de la hauteur normalisée
        biome.gameObject.GetComponent<Renderer>().material = biome.infos["vivant"][0];
        _couleurVivant = Color.Lerp(biome.infos["couleurVivantMin"], biome.infos["couleurVivantMax"], biome.transform.position.y / biome.infos["coefficientH"]);
        CreerVegetation(biome);
    }
    public override void UpdateEtat(BiomesEtatsManager biome)
    {

    }
    public override void TriggerEnterEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Perso") || other.CompareTag("Feu"))
        {
            biome.ChangerEtat(biome.Sec);
        }

        if (other.CompareTag("Lave"))
        {
            biome.ChangerEtat(biome.Lave);
        }

        if (other.CompareTag("Explosion"))
        {
            // biome.gameObject.GetComponent<Renderer>().material = biome.infos["mort"][0];
            biome.ChangerEtat(biome.Sec);
        }
    }

    public override void TriggerStayEtat(BiomesEtatsManager biome, Collider other)
    {

    }

    public override void TriggerExitEtat(BiomesEtatsManager biome, Collider other)
    {

    }

    public override void CreerAmes(BiomesEtatsManager biome)
    {
        int nbAmesRandom = Random.Range(1, 100);
        float posRandX = Random.Range(biome.transform.position.x, biome.transform.position.x + 1);
        float posRandZ = Random.Range(biome.transform.position.z, biome.transform.position.z + 1);
        Vector3 posRand = new Vector3(posRandX, biome.transform.position.y + .5f, posRandZ);
        int chanceAmes = 20;
        if (nbAmesRandom <= chanceAmes)
        {
            GameObject ame = GameObject.Instantiate(biome.infos["ame"], posRand, Quaternion.identity);
            ame.transform.parent = biome.transform;
        }
    }

    void CreerVegetation(BiomesEtatsManager biome)
    {
        // Vérifie la fertilité pour décider si un item peut être créé
        if (Random.value * 100 <= biome.infos["fertile"])
        {
            // Charge les données de l'item à instancier
            int varianteRand = Random.Range(1, biome.infos["items"][biome.infos["item"]].Count + 1);
            string cheminItem = $"Items/i{biome.infos["item"] + 1}_{varianteRand}";

            // Vérifie si l'item à instancier est un "Arbre"
            GameObject prefabItem = (GameObject)Resources.Load(cheminItem);
            if (prefabItem == null)
            {
                return;
            }

            bool estArbre = prefabItem.CompareTag("Arbre");
            if (estArbre && ExisteArbreProche(biome))
            {
                return;
            }
            float taille;
            if (estArbre) taille = Random.Range(.2f, .6f);
            else taille = Random.Range(.1f, .3f);

            float rotRand = Random.Range(0, 360);
            if(biome.infos["archipel"].AEteInstancie) SoundManager.Instance.JouerSon(biome.infos["sonPlantePousse"], biome.transform.position, biome.infos["sonPlantePousse"].length, 2f);
            GameObject unItem = GameObject.Instantiate(prefabItem,
                new Vector3(biome.transform.position.x, biome.transform.position.y + .9f, biome.transform.position.z),
                Quaternion.Euler(-90, rotRand, 0));

            // Configure l'item
            unItem.transform.localScale = Vector3.one * taille;
            Color couleur = unItem.GetComponent<Renderer>().material.color;
            float couleurRand = Random.Range(-.3f, .3f);
            unItem.GetComponent<Renderer>().material.color = new Color(couleur.r + couleurRand, couleur.g + couleurRand, couleur.b + couleurRand);

            if (biome.infos.ContainsKey("itemCube")) biome.infos.Remove("itemCube");
            biome.infos.Add("itemCube", unItem);
            biome.infos["aUnItem"] = true;

            // Lie l'item comme enfant du biome
            unItem.transform.parent = biome.transform;

        }
        biome.StartCoroutine(CoroutinePousserPlante(biome));
    }

    bool ExisteArbreProche(BiomesEtatsManager biome)
    {
        // Rayon de recherche pour détecter les cubes voisins
        float rayon = 3.5f; // Ajuste en fonction de la taille de tes cubes
        Collider[] voisins = Physics.OverlapSphere(biome.transform.position, rayon);

        foreach (Collider voisin in voisins)
        {
            // Vérifie si le voisin possède un item
            BiomesEtatsManager voisinBiome = voisin.GetComponent<BiomesEtatsManager>();
            if (voisinBiome != null && voisinBiome.infos.ContainsKey("itemCube"))
            {
                GameObject item = voisinBiome.infos["itemCube"] as GameObject;
                if (item != null && item.CompareTag("Arbre"))
                {
                    // Un arbre a été trouvé à proximité
                    return true;
                }
            }
        }

        // Aucun arbre trouvé dans le rayon
        return false;
    }

    IEnumerator CoroutinePousserPlante(BiomesEtatsManager biome)
    {
        float timer = 0;
        float duree = 1;
        Vector3 scaleItem = Vector3.zero;
        if (biome.infos["aUnItem"])
        {
            scaleItem = biome.infos["itemCube"].transform.localScale;
        }

        while (timer < duree)
        {
            timer += Time.deltaTime;
            if (biome.infos["aUnItem"])
            {
                biome.infos["itemCube"].transform.localScale = Vector3.Lerp(Vector3.zero, scaleItem, timer / duree);
            }
            biome.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(biome.gameObject.GetComponent<Renderer>().material.color, _couleurVivant, timer / duree);
            yield return null;
        }
    }

    public override void ExitEtat(BiomesEtatsManager biome)
    {
        biome.StopAllCoroutines();
    }
}
