using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    float _rayon;
    // BiomesEtatsManager _biome;
    public float Rayon { get => _rayon; set => _rayon = value; }
    // public BiomesEtatsManager Biome { get => _biome; set => _biome = value; }
    void Start()
    {
        StartCoroutine(CoroutineExplosion());
    }
    // void GenererExplosion(Vector3 centreExplosion)
    // {
    //     // Récupère tous les objets touchés par l'explosion
    //     Collider[] objetsTouches = Physics.OverlapSphere(centreExplosion, _rayon);

    //     HashSet<Vector3Int> positionsTraitees = new HashSet<Vector3Int>(); // Garde une trace des positions déjà traitées

    //     foreach (Collider objet in objetsTouches)
    //     {
    //         if (objet.CompareTag("FuturCube"))
    //         {
    //             Vector3Int positionFuturCube = Vector3Int.RoundToInt(objet.transform.position);
    //             float distance = Vector3.Distance(centreExplosion, objet.transform.position);

    //             // Si le cube est proche du bord ou en dessous d'une certaine hauteur, on ajoute un nouveau cube
    //             if (_rayon - distance < .9f || positionFuturCube.y <= 0)
    //             {
    //                 // Ajoute un cube à cette position si nécessaire
    //                 if (!positionsTraitees.Contains(positionFuturCube))
    //                 {
    //                     AjouterDesCubes(_biome, positionFuturCube);
    //                     positionsTraitees.Add(positionFuturCube);
    //                 }
    //             }

    //             // Avant de détruire le FuturCube, vérifiez si le sol doit être comblé
    //             VerifierEtComblerSol(positionFuturCube);

    //             // Détruit le FuturCube
    //             Destroy(objet.gameObject);
    //         }
    //     }
    // }

    // void VerifierEtComblerSol(Vector3Int position)
    // {
    //     // Vérifie les positions sous le cube pour éviter les trous
    //     for (int y = position.y - 1; y >= 0; y--)
    //     {
    //         Vector3Int positionVerifiee = new Vector3Int(position.x, y, position.z);

    //         // Vérifie s'il y a déjà un cube à cette position
    //         if (!EstCubePresent(positionVerifiee))
    //         {
    //             // Ajoute un cube pour combler le trou
    //             AjouterDesCubes(_biome, positionVerifiee);
    //         }
    //         else
    //         {
    //             // Si un cube est trouvé, arrêtez la vérification
    //             break;
    //         }
    //     }
    // }

    // bool EstCubePresent(Vector3Int position)
    // {
    //     // Vérifie s'il existe déjà un objet (cube ou autre) à cette position
    //     Collider[] objets = Physics.OverlapBox(position, Vector3.one * 0.4f); // Ajustez la taille pour éviter les erreurs de précision
    //     foreach (Collider objet in objets)
    //     {
    //         if (objet.CompareTag("Cube") || objet.CompareTag("FuturCube"))
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    // void AjouterDesCubes(BiomesEtatsManager biome, Vector3Int positionVoisine)
    // {
    //     // Instancier un nouveau cube
    //     BiomesEtatsManager nouveauCube = BiomesEtatsManager.Instantiate(biome, positionVoisine, Quaternion.identity);
    //     if (nouveauCube.GetComponent<BiomesEtatsManager>().enabled == false)
    //     {
    //         nouveauCube.GetComponent<BiomesEtatsManager>().enabled = true;
    //     }
    //     nouveauCube.infos.Add("cubesPresents", biome.infos["cubesPresents"]);
    //     nouveauCube.infos.Add("archipel", biome.infos["archipel"]);
    //     nouveauCube.infos.Add("perso", biome.infos["perso"]);
    //     nouveauCube.infos.Add("ame", biome.infos["ame"]);
    //     nouveauCube.infos.Add("fertile", biome.infos["fertile"]);
    //     nouveauCube.infos.Add("feu", biome.infos["feu"]);
    //     nouveauCube.infos.Add("vivant", biome.infos["vivant"]);
    //     nouveauCube.infos.Add("mort", biome.infos["mort"]);
    //     nouveauCube.infos.Add("lave", biome.infos["lave"]);
    //     nouveauCube.infos.Add("roche", biome.infos["roche"]);
    //     nouveauCube.infos.Add("sable", biome.infos["sable"]);
    //     nouveauCube.infos.Add("items", biome.infos["items"]);
    //     nouveauCube.infos.Add("item", biome.infos["item"]);
    //     nouveauCube.infos.Add("sonFeu", biome.infos["sonFeu"]);
    //     nouveauCube.infos.Add("sonDestructionItem", biome.infos["sonDestructionItem"]);
    //     nouveauCube.infos.Add("sonPlantePousse", biome.infos["sonPlantePousse"]);
    //     nouveauCube.infos.Add("sonFeuApparait", biome.infos["sonFeuApparait"]);
    //     nouveauCube.infos.Add("aUnItem", false);
    //     nouveauCube.infos.Add("genFeu", 0);
    //     nouveauCube.infos.Add("couleurSec", biome.infos["couleurSec"]);
    //     nouveauCube.infos.Add("couleurVivantMin", biome.infos["couleurVivantMin"]);
    //     nouveauCube.infos.Add("couleurVivantMax", biome.infos["couleurVivantMax"]);
    //     nouveauCube.infos.Add("cendres", biome.infos["cendres"]);
    //     nouveauCube.transform.parent = biome.transform.parent;
    //     nouveauCube.gameObject.GetComponent<Renderer>().material = biome.infos["mort"][0];
    //     nouveauCube.infos["etatInitial"] = "Mort";
    // }

    private IEnumerator CoroutineExplosion()
    {
        Debug.Log("Explosion");
        transform.localScale = Vector3.zero;

        while (transform.localScale.x < _rayon)
        {
            transform.localScale += Vector3.one * Time.fixedDeltaTime * 100;
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ennemi"))
        {
            EnnemiEtatsManager ennemi = other.GetComponent<EnnemiEtatsManager>();
            if (ennemi != null)
            {
                ennemi.Degat();
            }
        }
    }
}
