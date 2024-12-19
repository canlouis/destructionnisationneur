using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class VolcanMenu : MonoBehaviour
{
    [SerializeField] GameObject _projectileVolcan;
    [SerializeField] AudioClip _sonEruption;
    [SerializeField] ParticleSystem _effetEruption;
    Vector3 _posRocheBase;
    float _tailleCratere = 15;
    float _tailleRocheBase = 4;
    MenuAnimation _menuAnimation;
    public MenuAnimation MenuAnimation { get => _menuAnimation; set => _menuAnimation = value; }
    void Start()
    {
        _posRocheBase = new Vector3(transform.position.x, 18, transform.position.z);
        StartCoroutine(LancerRochesEnflammeesRoutine());
    }

    void LancerRochesEnflammees(Vector3 cible)
    {
        // Déterminer une position cible aléatoire
        Vector3 trajectoireCible = TrouverTrajectoireCible(cible);
        Vector3 direction = CalculerLancementDirection(transform.position, trajectoireCible);

        // Lancer une roche vers cette position
        LancerRoche(direction);
    }


    Vector3 TrouverTrajectoireCible(Vector3 cible)
    {
        float angle = Random.Range(0, Mathf.PI * 2); // Angle en radians
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)); // Direction sur le plan XZ
        Vector3 pos = cible + direction;
        pos.y = 0; // S'assurer que la position est au niveau du sol
        return pos;
    }
    Vector3 CalculerLancementDirection(Vector3 startPoint, Vector3 endPoint)
    {
        // Paramètres
        float angleLancement = 70f; // Angle souhaité en degrés
        float angleEnRadians = angleLancement * Mathf.Deg2Rad;
        float gravite = Mathf.Abs(Physics.gravity.y);

        // Calcul des distances
        Vector3 distanceH = new Vector3(endPoint.x - startPoint.x, 0, endPoint.z - startPoint.z); // Distance horizontale
        float distanceHorizontale = distanceH.magnitude;
        float hauteurRelative = endPoint.y - startPoint.y; // Différence de hauteur

        // Vérification des conditions physiques
        if (gravite <= 0 || distanceHorizontale == 0)
        {
            Debug.LogError("Paramètres invalides : gravité ou distance horizontale nulle.");
            return Vector3.zero;
        }

        // Calcul de la vitesse initiale requise (v0)
        float term1 = gravite * distanceHorizontale * distanceHorizontale;
        float term2 = 2 * Mathf.Pow(Mathf.Cos(angleEnRadians), 2) * (distanceHorizontale * Mathf.Tan(angleEnRadians) - hauteurRelative);

        if (term2 <= 0)
        {
            Debug.LogError("Impossible de calculer une trajectoire valide vers la cible.");
            return Vector3.zero;
        }

        float v0 = Mathf.Sqrt(term1 / term2);

        // Calcul de la vitesse initiale horizontale et verticale
        Vector3 velociteH = distanceH.normalized * v0 * Mathf.Cos(angleEnRadians);
        float velociteV = v0 * Mathf.Sin(angleEnRadians);

        // Combine la vitesse horizontale et verticale
        return velociteH + Vector3.up * velociteV;
    }

    void LancerRoche(Vector3 direction)
    {
        _effetEruption.Play();
        SoundManager.Instance.JouerSonGlobal(_sonEruption);
        StartCoroutine(_menuAnimation.VibrationRoutine());
        GameObject roche = Instantiate(_projectileVolcan, _posRocheBase, Quaternion.identity);
        Rigidbody rb = roche.GetComponent<Rigidbody>();
        roche.transform.localScale = Vector3.one * Random.Range(_tailleRocheBase, _tailleCratere);
        roche.transform.parent = transform;

        // Appliquer une force vers la cible
        rb?.AddForce(direction, ForceMode.VelocityChange);
    }

    public IEnumerator LancerRochesEnflammeesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4, 8));
            float posX = Random.Range(-40, 40);
            float posZ = Random.Range(-40, 40);
            if(posX < 0) posZ = Mathf.Abs(posX);
            LancerRochesEnflammees(new Vector3(transform.position.x + posX, 0, transform.position.z + posZ));
        }
    }
}