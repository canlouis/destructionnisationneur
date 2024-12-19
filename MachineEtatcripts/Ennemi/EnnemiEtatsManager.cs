using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnnemiEtatsManager : MonoBehaviour
{
    private EnnemiEtatsBase _etatActuel;
    [SerializeField] private EnnemiEtatRepos _repos = new();
    [SerializeField] private EnnemiEtatFuite _fuite = new();
    [SerializeField] private EnnemiEtatRaviveBloc _raviveBloc = new();

    public NavMeshAgent Agent { get; private set; }
    public Dictionary<string, dynamic> infos { get; private set; } = new();
    private Dictionary<Renderer, Color> couleursInitiales = new Dictionary<Renderer, Color>();

    public EnnemiEtatRepos Repos => _repos;
    public EnnemiEtatFuite Fuite => _fuite;
    public EnnemiEtatRaviveBloc RaviveBloc => _raviveBloc;
    public EnnemiEtatsBase EtatActuel { get => _etatActuel; set => _etatActuel = value; }

    private Vector3 initialScale;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 10f;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        initialScale = transform.localScale;

        // Sauvegarder les couleurs initiales des enfants
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            couleursInitiales[rend] = rend.material.color;
        }

        StartCoroutine(Apparition());
        Invoke("ChargerInfos", 1f);
    }

    private void ChargerInfos()
    {
        infos["vision"] = 15f;
        infos["perso"] = GameObject.FindGameObjectWithTag("Perso");
        _etatActuel = _repos;
        _etatActuel.InitEtat(this);
    }

    public void ChangerEtat(EnnemiEtatsBase etat)
    {
        _etatActuel.ExitEtat(this);
        _etatActuel = etat;
        Debug.Log("Changement d'état: " + _etatActuel.GetType().Name);
        _etatActuel.InitEtat(this);
    }

    public void Degat()
    {
        StartCoroutine(CoroutineMort());
    }

    IEnumerator CoroutineMort()
    {
        Agent.isStopped = true;
        float time = 0f;
        float deathDuration = 0.2f; // Durée pour changer la couleur
        float scaleDuration = 0.5f; // Durée pour réduire l'échelle
        Color targetColor = Color.black; // Couleur cible : noir

        // Transition de couleur vers le noir
        while (time < deathDuration)
        {
            time += Time.deltaTime;
            float lerpTime = Mathf.Clamp01(time / deathDuration);

            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                if (couleursInitiales.TryGetValue(rend, out Color initialColor))
                {
                    rend.material.color = Color.Lerp(initialColor, targetColor, lerpTime); // Transition vers le noir
                }
            }
            yield return null;
        }

        // Réduction de l'échelle
        time = 0f;
        while (time < scaleDuration)
        {
            time += Time.deltaTime;
            float lerpTime = Mathf.Clamp01(time / scaleDuration);
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, lerpTime);
            yield return null;
        }

        Explosion();
        Destroy(gameObject);
    }


    void Explosion()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            BiomesEtatsManager biome = hitCollider.GetComponent<BiomesEtatsManager>();
            if (biome != null)
            {
                biome.Explose();
            }
        }

        // Ajouter des effets visuels ici si nécessaire
    }

    IEnumerator Apparition()
    {
        float time = 0f;
        float apparitionDuration = 5f;

        while (time < apparitionDuration)
        {
            time += Time.deltaTime;
            float lerpTime = Mathf.Clamp01(time / apparitionDuration);

            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                if (couleursInitiales.TryGetValue(rend, out Color initialColor))
                {
                    rend.material.color = Color.Lerp(Color.black, initialColor, lerpTime);
                }
            }

            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, lerpTime);
            yield return null;
        }
    }
}
