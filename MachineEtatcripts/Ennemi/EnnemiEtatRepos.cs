using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnnemiEtatRepos : EnnemiEtatsBase
{
    public override void InitEtat(EnnemiEtatsManager ennemi)
    {
        ennemi.StartCoroutine(Repos(ennemi));
    }

    public override void ExitEtat(EnnemiEtatsManager ennemi)
    {
        ennemi.StopAllCoroutines();
    }

    private IEnumerator Repos(EnnemiEtatsManager ennemi)
    {
        while (ennemi.EtatActuel == ennemi.Repos)
        {
            GameObject perso = ennemi.infos["perso"];
            float distanceJoueur = Vector3.Distance(ennemi.transform.position, perso.transform.position);

            if (distanceJoueur < ennemi.infos["vision"] * 0.9f) // Seuil pour entrer en état de fuite
            {
                ennemi.ChangerEtat(ennemi.Fuite);
                yield break;
            }

            Collider[] blocsMorts = Physics.OverlapSphere(ennemi.transform.position, ennemi.infos["vision"]);
            foreach (Collider bloc in blocsMorts)
            {
                BiomesEtatsManager blocEtat = bloc.GetComponent<BiomesEtatsManager>();
                if (blocEtat != null && blocEtat.infos.ContainsKey("etat"))
                {
                    if (blocEtat.infos["etat"] == "Feu" || blocEtat.infos["etat"] == "Lave")
                    {
                        ennemi.infos["danger"] = bloc.gameObject;
                        ennemi.ChangerEtat(ennemi.Fuite);
                        yield break;
                    }
                    if (blocEtat.infos["etat"] == "Mort" || blocEtat.infos["etat"] == "Sec") // Ajouter les tags des blocs morts ici
                    {
                        ennemi.infos["blocCible"] = bloc.gameObject;
                        ennemi.ChangerEtat(ennemi.RaviveBloc);
                        yield break;
                    }
                }

            }

            Vector3 randomDirection = Random.insideUnitSphere * ennemi.infos["vision"];
            randomDirection += ennemi.transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, ennemi.infos["vision"], NavMesh.AllAreas))
            {
                ennemi.Agent.SetDestination(hit.position);
            }

            float tempsAttente = Random.Range(1f, 3.5f);
            yield return new WaitForSeconds(tempsAttente);
        }
    }

}
