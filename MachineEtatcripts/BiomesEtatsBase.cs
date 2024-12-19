using System.Collections;
using UnityEngine;

public abstract class BiomesEtatsBase
{
    public abstract void InitEtat(BiomesEtatsManager biome);
    public abstract void ExitEtat(BiomesEtatsManager biome);
    public abstract void UpdateEtat(BiomesEtatsManager biome);
    public abstract void TriggerEnterEtat(BiomesEtatsManager biome, Collider other);
    public abstract void TriggerExitEtat(BiomesEtatsManager biome, Collider other);
    public abstract void TriggerStayEtat(BiomesEtatsManager biome, Collider other);
    public abstract void CreerAmes(BiomesEtatsManager biome);
}
