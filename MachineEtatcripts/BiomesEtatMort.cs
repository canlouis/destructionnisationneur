using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BiomesEtatMort : BiomesEtatsBase
{
    public override void InitEtat(BiomesEtatsManager biome)
    {
        ExitEtat(biome);
        biome­.infos["genFeu"] = 0;
        CreerAmes(biome);
        if (biome.infos["aUnItem"])
        {
            SoundManager.Instance.JouerSon(biome.infos["sonDestructionItem"], biome.transform.position, biome.infos["sonDestructionItem"].length, 1);
            GameObject.Destroy(biome.infos["itemCube"]);
            ParticleSystem cendres = ParticleSystem.Instantiate(biome.infos["cendres"], new Vector3(biome.transform.position.x, biome.transform.position.y + 1, biome.transform.position.z), Quaternion.identity);
            cendres.transform.parent = biome.transform;
            biome.infos["aUnItem"] = false;
        }
    }
    public override void UpdateEtat(BiomesEtatsManager biome)
    {

    }

    public override void TriggerEnterEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Lave"))
        {
            biome.ChangerEtat(biome.Lave);
        }

        if (other.CompareTag("Ennemi"))
        {
            biome.ChangerEtat(biome.Vivant);
        }
    }

    public override void TriggerStayEtat(BiomesEtatsManager biome, Collider other)
    {

    }


    public override void TriggerExitEtat(BiomesEtatsManager biome, Collider other)
    {

    }

    

    public override void ExitEtat(BiomesEtatsManager biome)
    {
        biome.StopAllCoroutines();
    }

    public override void CreerAmes(BiomesEtatsManager biome)
    {
        int nbAmesRandom = Random.Range(1, 100);
        float posRandX = Random.Range(biome.transform.position.x, biome.transform.position.x + 1);
        float posRandZ = Random.Range(biome.transform.position.z, biome.transform.position.z + 1);
        Vector3 posRand = new Vector3(posRandX, biome.transform.position.y + .5f, posRandZ);
        int chanceAmes = 15;
        if (nbAmesRandom <= chanceAmes)
        {
            GameObject ame = GameObject.Instantiate(biome.infos["ame"], posRand, Quaternion.identity);
            ame.transform.parent = biome.transform;
            if (!biome.infos.ContainsKey("ameInstance"))
            {
                biome.infos.Add("ameInstance", ame);
            }
            else
            {
                biome.infos["ameInstance"] = ame;
            }
        }
    }
}
