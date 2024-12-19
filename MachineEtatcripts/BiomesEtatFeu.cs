using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomesEtatFeu : BiomesEtatsBase
{
    ParticleSystem _feu;

    public override void InitEtat(BiomesEtatsManager biome)
    {
        ExitEtat(biome);
        biome.StartCoroutine(CoroutineFeu(biome));
    }
    public override void UpdateEtat(BiomesEtatsManager biome)
    {

    }
    public override void TriggerEnterEtat(BiomesEtatsManager biome, Collider other)
    {
        if (other.CompareTag("Ennemi"))
        {
            other.GetComponent<EnnemiEtatsManager>().Degat();
        }

        if (other.CompareTag("Lave"))
        {
            _feu.Stop();
            biome.ChangerEtat(biome.Lave);
        }

        if (other.CompareTag("Explosion"))
        {
            biome.gameObject.GetComponent<Renderer>().material = biome.infos["mort"][0];
            _feu.Stop();
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

    IEnumerator CoroutineFeu(BiomesEtatsManager biome)
    {
        int genFeuMax = 3;
        float tempsFeu = Random.Range(2f, 8f);
        float tempsRestant = 0;
        Collider[] voisins = Physics.OverlapSphere(biome.transform.position, 1.5f); // Rayon ajustable.
        _feu = ParticleSystem.Instantiate(biome.infos["feu"], biome.transform.position, Quaternion.Euler(-90, 0, 0));
        _feu.transform.parent = biome.transform;

        foreach (Transform child in _feu.transform)
        {
            ParticleSystem childParticleSystem = child.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule childMain = childParticleSystem.main;
            float startLifetime = childMain.startLifetime.constant;
            startLifetime -= biome.infos["genFeu"] * childMain.startLifetime.constant * .3f;
            childMain.startLifetime = startLifetime;
        }
        SoundManager.Instance.JouerSon(biome.infos["sonFeuApparait"], biome.transform.position, biome.infos["sonFeuApparait"].length, 3f);
        SoundManager.Instance.JouerSon(biome.infos["sonFeu"], biome.transform.position, tempsFeu, 3f);
        while (tempsRestant < tempsFeu)
        {
            tempsRestant += Time.deltaTime;
            if (biome.infos["aUnItem"])
            {
                foreach (Material mat in biome.infos["itemCube"].GetComponent<Renderer>().materials)
                {
                    mat.color = new Color(mat.color.r - tempsRestant / tempsFeu, mat.color.g - tempsRestant / tempsFeu, mat.color.b - tempsRestant / tempsFeu);
                }
            }
            Color couleurSol = biome.GetComponent<Renderer>().material.color;
            biome.GetComponent<Renderer>().material.color = new Color(couleurSol.r - tempsRestant / tempsFeu, couleurSol.g - tempsRestant / tempsFeu, couleurSol.b - tempsRestant / tempsFeu);

            if (tempsRestant >= tempsFeu / 2)
            {
                foreach (Collider voisin in voisins)
                {
                    if (voisin)
                    {
                        BiomesEtatsManager biomeVoisin = voisin.GetComponent<BiomesEtatsManager>();
                        if (biomeVoisin != null && biomeVoisin.infos["etat"] == "Sec" && biome.infos["genFeu"] < genFeuMax)
                        {
                            biomeVoisin.infos["genFeu"] = biome.infos["genFeu"] + 1;
                            biomeVoisin.ChangerEtat(biomeVoisin.Feu);
                        }
                        else if (biomeVoisin != null && biomeVoisin.infos["etat"] == "Vivant")
                        {
                            biomeVoisin.ChangerEtat(biomeVoisin.Sec);
                        }
                    }
                }
            }
            yield return null;
        }
        _feu.Stop();

        biome.ChangerEtat(biome.Mort);
    }

    public override void ExitEtat(BiomesEtatsManager biome)
    {
        biome.StopAllCoroutines();
    }
}
