using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Tiles")]
    [SerializeField] public ObjectPooler centerTile;
    [SerializeField] public ObjectPooler edgeTile;
    [SerializeField] public ObjectPooler cornerTile;
    [SerializeField] public ObjectPooler soloTile;
}
