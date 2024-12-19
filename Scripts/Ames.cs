using System.Collections;
using TMPro;
using UnityEngine;

public class Ames : MonoBehaviour
{
    [SerializeField] AudioClip _sonRamasser;
    [SerializeField] AudioClip _sonApparition;
    float _vitesseOscillation;
    float _timer = 0;
    float _posYMax = 1.5f;
    float _posYMin = .5f;
    float _posY;
    float _scaleMax = 4f;
    float _scaleMin = 2f;
    bool _cree = false;
    float _rotRand;
    bool estRamasser = false;
    TrailRenderer _trail;

    // Start is called before the first frame update
    void Awake()
    {
        SoundManager.Instance.JouerSon(_sonApparition, transform.position, _sonApparition.length, 5f);
        _vitesseOscillation = Random.Range(3f, 8f);
        _rotRand = Random.Range(-1f, 1f);
        transform.localScale = Vector3.zero;
        _posY = transform.position.y + _posYMax;
        GetComponent<SphereCollider>().enabled = false;
        _trail = GetComponent<TrailRenderer>();
        _trail.enabled = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!estRamasser)
        {
            _timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, _posY, transform.position.z), _timer); // Smooth lerp for vertical position
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(_scaleMax, _scaleMax, _scaleMax), _timer * 0.5f); // Smooth scale lerp

            if (transform.position.y >= _posY - .1) _cree = true;

            if (_cree)
            {
                GetComponent<SphereCollider>().enabled = true;
                float oscillationPos = Mathf.Cos(_timer * _vitesseOscillation / Mathf.PI) * (_posY - (_posY - _posYMin)) / 2 + (_posY + (_posY - _posYMin)) / 2;
                transform.position = new Vector3(transform.position.x, oscillationPos, transform.position.z);
                float oscillationScale = Mathf.Cos(_timer * _vitesseOscillation / Mathf.PI) * (_scaleMax - _scaleMin) / 2 + (_scaleMax + _scaleMin) / 2;
                transform.localScale = new Vector3(oscillationScale, oscillationScale, oscillationScale);
                transform.Rotate(Vector3.up, _rotRand);
            }
        }
        else
        {
            // Une fois que l'âme a été ramassée, on arrête l'oscillation et on fixe la taille et la position
            transform.localScale = new Vector3(_scaleMax, _scaleMax, _scaleMax); // Taille stable de l'âme
            // L'âme reste à sa position finale et ne fait plus d'oscillation
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SphereRecolte")
        {
            // other.GetComponentInParent<Perso>().AjouterAmes();
            Perso perso = other.GetComponentInParent<Perso>();
            TextMeshProUGUI champAmes = other.GetComponentInParent<Perso>().ChampAmes;
            if (estRamasser)
            {
                perso.AjouterAmes();
                SoundManager.Instance.JouerSon(_sonRamasser, transform.position, _sonRamasser.length);
                Destroy(gameObject);
            }
            estRamasser = true;
            StartCoroutine(RamasserAmes(perso, champAmes));
        }
    }

    /// <summary>
    /// Ramasse les âmes avec une animation et les ajoute au joueur lorsqu'il entre en collision avec elles
    /// </summary>
    /// <param name="perso"></param>
    /// <param name="champAmes"></param>
    /// <returns></returns>
    IEnumerator RamasserAmes(Perso perso, TextMeshProUGUI champAmes)
    {
        GameObject target = perso.gameObject; // Où veut-on que les particules se déplacent
        float temp = 1f;
        Vector3 startPosition = transform.position;
        Vector3 midPosition = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
        float journeyLength = Vector3.Distance(startPosition, midPosition);
        float startTime = Time.time;

        // Se déplacer vers la position intermédiaire
        while (Vector3.Distance(transform.position, midPosition) > 0.1f)
        {
            float distCovered = (Time.time - startTime) * temp;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, midPosition, fractionOfJourney);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        _trail.enabled = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        // Utilisation de MoveTowards pour une approche plus fluide
        while (Vector3.Distance(transform.position, target.transform.position) > 0.1f)
        {
            Vector3 positionAAller = new Vector3(target.transform.position.x, target.transform.position.y + 2f, target.transform.position.z); // Ajuste la hauteur du joueur
            transform.position = Vector3.MoveTowards(transform.position, positionAAller, temp * Time.deltaTime * 15f); // Ajuste la vitesse de transition
            yield return null;
        }
        Destroy(gameObject);
    }
}
