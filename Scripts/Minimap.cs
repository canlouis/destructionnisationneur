using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField] Camera _camPerso;
    public Camera CamPerso { get => _camPerso; set => _camPerso = value; }

    void Update()
    {
        _camPerso.transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
