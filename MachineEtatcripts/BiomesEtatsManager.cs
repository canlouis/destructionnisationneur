using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomesEtatsManager : MonoBehaviour
{
    BiomesEtatsBase _etatActuel;
    [SerializeField] BiomesEtatVivant _vivant = new();
    [SerializeField] BiomesEtatSec _sec = new();
    [SerializeField] BiomesEtatMort _mort = new();
    [SerializeField] BiomesEtatLave _lave = new();
    [SerializeField] BiomesEtatRoche _roche = new();
    [SerializeField] BiomesEtatFeu _feu = new();

    public Dictionary<string, dynamic> infos { get; set; } = new();

    // Getter / Setter pour les états
    public BiomesEtatVivant Vivant { get => _vivant; set => _vivant = value; }
    public BiomesEtatSec Sec { get => _sec; set => _sec = value; }
    public BiomesEtatMort Mort { get => _mort; set => _mort = value; }
    public BiomesEtatLave Lave { get => _lave; set => _lave = value; }
    public BiomesEtatRoche Roche { get => _roche; set => _roche = value; }
    public BiomesEtatFeu Feu { get => _feu; set => _feu = value; }

    List<List<Material>> _biomesMats = new();
    List<List<GameObject>> _biomesItems = new();
    public List<List<Material>> BiomesMats { get => _biomesMats; set => _biomesMats = value; }
    public List<List<GameObject>> BiomesItems { get => _biomesItems; set => _biomesItems = value; }

    // Start is called before the first frame update
    void Start()
    {
        // Vérifier si une condition spécifique est remplie pour démarrer à l'état "Mort"
        if (infos.ContainsKey("etatInitial") && infos["etatInitial"] == "Mort")
        {
            ChangerEtat(Mort);
        }
        else
        {
            ChangerEtat(Vivant); // Par défaut, l'état est "Vivant"
        }
    }
    public void Explose()
    {
        GameObject biome = gameObject;
        biome.GetComponent<Renderer>().material.color = Color.black;
        ChangerEtat(Mort);
    }


    public void ChangerEtat(BiomesEtatsBase etat)
    {
        etat.ExitEtat(this);
        _etatActuel = etat;
        infos["etat"] = _etatActuel.GetType().Name.Replace("BiomesEtat", "");
        _etatActuel.InitEtat(this);
    }

    // Update is called once per frame
    void Update()
    {
        _etatActuel.UpdateEtat(this);
    }

    void OnTriggerEnter(Collider other)
    {
        _etatActuel.TriggerEnterEtat(this, other);
    }

    void OnTriggerExit(Collider other)
    {
        _etatActuel.TriggerExitEtat(this, other);
    }
    void OnTriggerStay(Collider other)
    {
        _etatActuel.TriggerStayEtat(this, other);
    }
}
