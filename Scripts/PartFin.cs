using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartFin : MonoBehaviour
{
    Vector3 _volcanTarget; // Le point cible (centre du cratère)
    float _vitesse = 10f; // Vitesse de déplacement
    float _delaiDestruction = 1f; // Temps avant destruction une fois dans le cratère
    public Vector3 VolcanTarget { set => _volcanTarget = value; } // Propriété pour le point cible

    void Start()
    {
        // _volcanTarget.position += Vector3.up * 30; // Ajoute une hauteur
        StartCoroutine(AllerVersVolcan());
    }

    IEnumerator AllerVersVolcan()
    {
        float _hauteurDebut = 5f;
        while(transform.position.y < _hauteurDebut)
        {
            transform.position += Vector3.up * _vitesse * Time.deltaTime;
            yield return null;
        }

        // Déplacement fluide vers la cible
        Vector3 direction = (_volcanTarget - transform.position).normalized;

        while (Vector3.Distance(transform.position, _volcanTarget) > 2f)
        {
            transform.position += direction * _vitesse * Time.deltaTime;
            yield return null;
        }

        while (true)
        {
            transform.position += Vector3.down * _vitesse * Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Détection du cratère avec un trigger
        if (other.CompareTag("LaveVolcan"))
        {
            // Détruit l'objet avec un délai
            Destroy(gameObject, _delaiDestruction);
        }
    }
}
