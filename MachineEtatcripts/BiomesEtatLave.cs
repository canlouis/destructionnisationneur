using System.Collections;
using UnityEngine;

public class BiomesEtatLave : BiomesEtatsBase
{
    public override void InitEtat(BiomesEtatsManager biome)
    {
        ExitEtat(biome);
        biome.gameObject.GetComponent<Renderer>().material = biome.infos["lave"][0];
        CreerAmes(biome);
        if (biome.infos["aUnItem"])
        {
            GameObject.Destroy(biome.infos["itemCube"]);
            biome.infos["aUnItem"] = false;
        }
        biome.StartCoroutine(CoroutineLave(biome));
    }
    public override void UpdateEtat(BiomesEtatsManager biome)
    {

    }
    public override void TriggerEnterEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            biome.gameObject.GetComponent<Renderer>().material = biome.infos["mort"][0];
            biome.ChangerEtat(biome.Mort);
        }
    }

    public override void TriggerExitEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Perso"))
        {
            other.GetComponentInParent<Perso>().EstEnFeu = true;
        }
    }

    public override void TriggerStayEtat(BiomesEtatsManager biome, Collider other)
    {
        
    }


    public override void CreerAmes(BiomesEtatsManager biome)
    {
        int nbAmesRandom = Random.Range(1, 100);
        float posRandX = Random.Range(biome.transform.position.x, biome.transform.position.x + 1);
        float posRandZ = Random.Range(biome.transform.position.z, biome.transform.position.z + 1);
        Vector3 posRand = new Vector3(posRandX, biome.transform.position.y + .5f, posRandZ);
        int chanceAmes = 30;
        if (nbAmesRandom <= chanceAmes)
        {
            GameObject ame = GameObject.Instantiate(biome.infos["ame"], posRand, Quaternion.identity);
            ame.transform.parent = biome.transform;
        }
    }

    IEnumerator CoroutineLave(BiomesEtatsManager biome)
    {
        float dureeLave = Random.Range(20f, 30f);
        float tempsRestant = 0;
        while (tempsRestant < dureeLave)
        {
            tempsRestant += Time.deltaTime;
            yield return null;
        }
        biome.ChangerEtat(biome.Roche);
    }

    public override void ExitEtat(BiomesEtatsManager biome)
    {
        biome.StopAllCoroutines();
    }
}
