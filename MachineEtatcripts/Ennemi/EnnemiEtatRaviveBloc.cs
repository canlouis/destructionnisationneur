using System.Collections;
using UnityEngine;

public class EnnemiEtatRaviveBloc : EnnemiEtatsBase
{
    public override void InitEtat(EnnemiEtatsManager ennemi)
    {
        // Debug.Log("RaviveBloc");

        // Vérifie si la clé "blocCible" existe et est valide
        if (ennemi.infos.ContainsKey("blocCible") && ennemi.infos["blocCible"] != null)
        {
            ennemi.Agent.SetDestination(ennemi.infos["blocCible"].transform.position);
            // Debug.Log("Bloc cible: " + ennemi.infos["blocCible"].transform.position);
            ennemi.StartCoroutine(RaviverBloc(ennemi));
        }
        else
        {
            Debug.LogWarning("Bloc cible non défini ou invalide !");
            ennemi.ChangerEtat(ennemi.Repos);
        }
    }

    public override void ExitEtat(EnnemiEtatsManager ennemi)
    {
        ennemi.StopAllCoroutines();
    }

    private IEnumerator RaviverBloc(EnnemiEtatsManager ennemi)
    {
        while (true)
        {
            // Vérifie la distance à la cible
            if (Vector3.Distance(ennemi.transform.position, ennemi.infos["blocCible"].transform.position) < 3f)
            {
                ennemi.ChangerEtat(ennemi.Repos);
                yield break;
            }
            yield return null;
        }
    }
}
