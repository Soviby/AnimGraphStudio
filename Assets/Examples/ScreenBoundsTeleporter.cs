using UnityEngine;

public sealed class ScreenBoundsTeleporter : MonoBehaviour
{
    [SerializeField] private BoxCollider _Collider;

    private void Update()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        var position = camera.transform.position;
        position.z = 0;
        transform.position = position;

        var topLeft = camera.ScreenPointToRay(Vector3.zero).origin;
        _Collider.size = (position - topLeft) * 2;
    }


    private void OnTriggerExit(Collider collider)
    {
        var position = collider.transform.position;
        position.x -= (position.x - transform.position.x) * 2;
        collider.transform.position = position;
    }
}

