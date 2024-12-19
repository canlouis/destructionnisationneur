using System.Collections;
using UnityEngine;

public class BiomesEtatSec : BiomesEtatsBase
{
    public override void InitEtat(BiomesEtatsManager biome)
    {
        ExitEtat(biome);
        biome.StartCoroutine(CoroutineAssecher(biome));
        CreerAmes(biome);
    }
    public override void UpdateEtat(BiomesEtatsManager biome)
    {

    }
    public override void TriggerEnterEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Feu"))
        {
            biome.ChangerEtat(biome.Feu);
        }

        if (other.CompareTag("Lave"))
        {
            biome.ChangerEtat(biome.Lave);
        }

        if (other.CompareTag("Ennemi"))
        {
            biome.ChangerEtat(biome.Vivant);
        }

        if (other.CompareTag("Explosion"))
        {
            biome.gameObject.GetComponent<Renderer>().material = biome.infos["mort"][0];
            biome.ChangerEtat(biome.Mort);
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
        int chanceAmes = 10;
        if (nbAmesRandom <= chanceAmes)
        {
            GameObject ame = GameObject.Instantiate(biome.infos["ame"], posRand, Quaternion.identity);
            ame.transform.parent = biome.transform;
            biome.infos.Add("ameInstance", ame);
        }
    }

    IEnumerator CoroutineAssecher(BiomesEtatsManager biome)
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
                biome.infos["itemCube"].transform.localScale = Vector3.Lerp(biome.infos["itemCube"].transform.localScale, scaleItem * .6f, timer / duree);
                biome.infos["itemCube"].GetComponent<Renderer>().material.color = Color.Lerp(biome.infos["itemCube"].GetComponent<Renderer>().material.color, biome.infos["couleurSec"], timer / duree);
            }
            biome.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(biome.gameObject.GetComponent<Renderer>().material.color, biome.infos["couleurSec"], timer / duree);
            yield return null;
        }
    }

    public override void ExitEtat(BiomesEtatsManager biome)
    {
        biome.StopAllCoroutines();
    }
}
