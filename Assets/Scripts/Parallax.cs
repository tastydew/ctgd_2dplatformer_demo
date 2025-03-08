using System;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;

    public Transform player;

    private Vector2 startPosition;
    private float startZ;

    private Vector2 travel => (Vector2)cam.transform.position - startPosition;

    float distanceFromPlayer => transform.position.z - player.position.z;
    float clippingPlane => (cam.transform.position.z + (distanceFromPlayer > 0 ? cam.farClipPlane : cam.nearClipPlane));

    float parallaxFactor => Mathf.Abs(distanceFromPlayer) / clippingPlane;

    public void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    private void Update()
    {
        Vector2 newPos = startPosition + travel * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }
}
