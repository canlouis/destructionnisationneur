using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnnemiEtatFuite : EnnemiEtatsBase
{
    public override void InitEtat(EnnemiEtatsManager ennemi)
    {
        ennemi.StartCoroutine(Fuir(ennemi));
    }

    public override void ExitEtat(EnnemiEtatsManager ennemi)
    {
        ennemi.StopAllCoroutines();
    }

    private IEnumerator Fuir(EnnemiEtatsManager ennemi)
    {
        float vitesseNormale = ennemi.Agent.speed;
        ennemi.Agent.speed = vitesseNormale * 3f;

        float fuiteTimeMax = 2.5f;
        float fuiteTimer = 0f;

        GameObject perso = ennemi.infos["perso"];
        GameObject danger = ennemi.infos.ContainsKey("danger") ? ennemi.infos["danger"] : null;

        while (ennemi.EtatActuel == ennemi.Fuite)
        {
            float distanceJoueur = Vector3.Distance(ennemi.transform.position, perso.transform.position);
            float distanceDanger = danger != null ? Vector3.Distance(ennemi.transform.position, danger.transform.position) : float.MaxValue;

            if (distanceJoueur < ennemi.infos["vision"] || distanceDanger < ennemi.infos["vision"])
            {
                Vector3 directionFuite = (distanceJoueur <= distanceDanger)
                    ? ennemi.transform.position - perso.transform.position
                    : ennemi.transform.position - danger.transform.position;

                Vector3 nouvelleDestination = ennemi.transform.position + directionFuite.normalized * ennemi.infos["vision"];
                if (NavMesh.SamplePosition(nouvelleDestination, out NavMeshHit hit, ennemi.infos["vision"], NavMesh.AllAreas))
                {
                    ennemi.Agent.SetDestination(hit.position);
                }

                fuiteTimer = 0f;
            }
            else if (distanceJoueur > ennemi.infos["vision"] * 1.5f && distanceDanger > ennemi.infos["vision"] * 1.5f)
            {
                ennemi.Agent.speed = vitesseNormale;
                ennemi.ChangerEtat(ennemi.Repos);
                yield break;
            }

            fuiteTimer += Time.deltaTime;
            if (fuiteTimer >= fuiteTimeMax)
            {
                ennemi.Agent.speed = vitesseNormale;
                ennemi.ChangerEtat(ennemi.Repos);
                yield break;
            }

            yield return null;
        }

        ennemi.Agent.speed = vitesseNormale;
    }

}
