using UnityEngine;

public class AudioZone : MonoBehaviour
{
    [SerializeField] private Transform location;
    [SerializeField] private GameObject audioSource;

    private void Start()
    {
        
    }

    private void Update()
    {
        Vector3 closestPoint = GetCollider().ClosestPoint(location.position);
        audioSource.transform.position = closestPoint;
    }

    Collider GetCollider() {
        var collider = GetComponent<Collider>();

        if (!collider)
        {
            return null; // nothing to do without a collider
        }

        return collider;
    }
}
