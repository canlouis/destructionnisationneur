using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RotationParVitesse : MonoBehaviour
{
    public ParticleSystem _particules; // Référence à l'objet particules
    public AudioClip sonDeDeplacement; // Clip audio pour le déplacement
    private Transform _modele; // Référence au modèle FBX
    private NavMeshAgent _agent; // Référence à l'agent NavMesh
    public float facteurRotation = 50f; // Facteur de multiplication pour la rotation
    public Vector3 axeRotation = Vector3.right; // Par défaut, l'axe de rotation est X
    bool _peutRejouerSon = true; // Indique si le son peut être rejoué

    private void Start()
    {
        // Récupère l'agent NavMesh 
        _agent = GetComponent<NavMeshAgent>();

        // Récupère l'enfant
        _modele = transform.GetChild(0);

        // Vérification
        if (_modele == null)
        {
            Debug.LogWarning("Aucun modèle trouvé en tant qu'enfant de cet objet.");
        }
        if (_agent == null)
        {
            Debug.LogError("Aucun NavMeshAgent trouvé sur cet objet !");
        }
    }

    private void FixedUpdate()
    {
        if (_modele != null && _agent != null)
        {
            // Vérifie si l'agent se déplace
            if (_agent.velocity.magnitude > 0.1f)
            {
                // Calcule la rotation en fonction de la vitesse
                float rotationVitesse = _agent.velocity.magnitude * facteurRotation;

                // Crée un effet de particules selon la vitesse de déplacement
                if (_particules != null)
                {
                    if (!_particules.isPlaying)
                    {
                        _particules.Play();
                    }
                    var emission = _particules.emission;
                    emission.rateOverTime = rotationVitesse * 0.5f;
                }

                // Applique la rotation au modèle
                _modele.Rotate(axeRotation, rotationVitesse * Time.deltaTime, Space.Self);

                if (sonDeDeplacement != null && _peutRejouerSon)
                {
                    SoundManager.Instance.JouerSon(sonDeDeplacement, transform.position, sonDeDeplacement.length);
                    _peutRejouerSon = false;
                    StartCoroutine(RejouerSon());
                }
            }
            else
            {
                // Arrête les particules si l'agent est immobile
                if (_particules != null && _particules.isPlaying)
                {
                    _particules.Stop();
                }
            }
        }
    }

    IEnumerator RejouerSon()
    {
        yield return new WaitForSeconds(sonDeDeplacement.length);
        _peutRejouerSon = true;
    }
}
