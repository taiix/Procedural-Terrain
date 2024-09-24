using Cinemachine;
using UnityEngine;


public class CustomAmbientZone : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachinePath path;

    float positionOnThePath;
    private CinemachinePathBase.PositionUnits m_PositionUnits
          = CinemachinePathBase.PositionUnits.PathUnits;

    private void Update()
    {
        SetCartPosition(path.FindClosestPoint(player.transform.position, 0, -1, 10));
        if (CalculateDot() < 0) transform.position = new Vector3(player.transform.position.x,
            player.transform.position.y + 1,
            player.transform.position.z);
    }

    private void SetCartPosition(float findClosestPoint)
    {
        positionOnThePath = path.StandardizeUnit(findClosestPoint, m_PositionUnits);
        transform.position = path.EvaluatePositionAtUnit(findClosestPoint, m_PositionUnits);
        transform.rotation = path.EvaluateOrientationAtUnit(findClosestPoint, m_PositionUnits);
    }

    private float CalculateDot()
    {
        Vector3 forward = transform.right;
        Vector3 dir = Vector3.Normalize(transform.position - player.transform.position);

        return Vector3.Dot(forward, dir);
    }
}
