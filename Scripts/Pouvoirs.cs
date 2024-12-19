using UnityEngine;

[System.Serializable]
public class Pouvoir
{
    string _nom;           // Nom du pouvoir
    int _coutAmes;         // Coût en âmes pour utiliser le pouvoir
    float _chargementMax; // Temps ou charge nécessaire pour activer
    Sprite _icone;        // Icône du pouvoir
    public string Nom { get { return _nom; } }
    public int CoutAmes { get { return _coutAmes; } }
    public float ChargementMax { get { return _chargementMax; } }
    public Sprite Icone { get { return _icone; } }

    public Pouvoir(string nom, int coutAmes, float chargementMax, Sprite icone)
    {
        _nom = nom;
        _coutAmes = coutAmes;
        _chargementMax = chargementMax;
        _icone = icone;
    }
}
