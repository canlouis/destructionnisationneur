using System.Collections;
using UnityEngine;

public class BiomesEtatRoche : BiomesEtatsBase
{
    public override void InitEtat(BiomesEtatsManager biome)
    {
        ExitEtat(biome);
        biome.gameObject.GetComponent<Renderer>().material = biome.infos["roche"][0];
        biome.StopAllCoroutines();
    }
    public override void UpdateEtat(BiomesEtatsManager biome)
    {

    }
    public override void TriggerEnterEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Lave"))
        {
            biome.ChangerEtat(biome.Lave);
            Debug.Log("Lave");
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
        int chanceAmes = 20;
        if (nbAmesRandom <= chanceAmes)
        {
            GameObject ame = GameObject.Instantiate(biome.infos["ame"], posRand, Quaternion.identity);
            ame.transform.parent = biome.transform;
        }
    }

    public override void ExitEtat(BiomesEtatsManager biome)
    {
        biome.StopAllCoroutines();
    }
}
