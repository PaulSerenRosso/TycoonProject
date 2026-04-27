using System;
using UnityEngine;

public class PlayerStart : MonoBehaviour
{
 private void OnDrawGizmos()
 {
  Gizmos.color = Color.red;
  Gizmos.DrawWireSphere(transform.position, 10);
 }
}
