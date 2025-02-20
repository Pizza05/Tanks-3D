using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // The damping time for smooth camera movement and zooming
    public float dampTime = 0.2f;

    // Additional space around the screen edges to prevent the camera from zooming in too close
    public float screenEdgeBuffer = 4f;

    // The minimum camera size to avoid excessive zooming
    public float minSize = 6.5f;

    // Array of target objects that the camera will follow
    public Transform[] targets;

    private Camera _camera; // Reference to the Camera component
    private float _zoomSpeed; // Speed of zooming (used with SmoothDamp)
    private Vector3 _moveVelocity; // Speed of movement (used with SmoothDamp)
    private Vector3 _desiredPosition; // The target position where the camera should move

    private void Awake()
    {
        // Get the Camera component from the child object of this GameObject
        _camera = GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        Move(); // Update the camera position
        Zoom(); // Update the camera zoom level
    }

    private void Move()
    {
        FindAveragePosition(); // Calculate the average position of all active targets
        // Smoothly move the camera to the calculated position
        transform.position = Vector3.SmoothDamp(transform.position, _desiredPosition, ref _moveVelocity, dampTime);
    }

    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3(); // Stores the average position of all targets
        int numTargets = 0; // Counts the number of active targets

        // Loop through all targets
        for (int i = 0; i < targets.Length; i++)
        {
            // Skip targets that are not active
            if (!targets[i].gameObject.activeSelf)
                continue;

            // Add the position of active targets to the average position
            averagePos += targets[i].position;
            numTargets++;
        }

        // If there are active targets, calculate the average position
        if (numTargets > 0)
            averagePos /= numTargets;

        // Keep the camera's Y position unchanged to prevent vertical movement
        averagePos.y = transform.position.y;

        // Set the desired camera position
        _desiredPosition = averagePos;
    }

    private void Zoom()
    {
        // Calculate the required zoom size
        float requiredSize = FindRequiredSize();
        // Smoothly adjust the camera's size to the required zoom level
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, requiredSize, ref _zoomSpeed, dampTime);
    }

    private float FindRequiredSize()
    {
        // Convert the desired camera position into local space
        Vector3 desiredLocalPos = transform.InverseTransformPoint(_desiredPosition);
        float size = 0f; // Initial camera size

        // Loop through all targets
        for (int i = 0; i < targets.Length; i++)
        {
            // Skip inactive targets
            if (!targets[i].gameObject.activeSelf)
                continue;

            // Convert the target's position into local space
            Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].position);

            // Calculate the distance between the target and the camera center
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // Find the maximum required size based on the Y-axis distance
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            // Find the maximum required size based on the X-axis distance, adjusted for camera aspect ratio
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / _camera.aspect);
        }

        // Add buffer space around the edges to prevent zooming too close
        size += screenEdgeBuffer;

        // Ensure the camera size does not go below the minimum size
        size = Mathf.Max(size, minSize);

        return size;
    }

    public void SetStartPositionAndSize()
    {
        FindAveragePosition(); // Calculate the initial camera position
        transform.position = _desiredPosition; // Set the camera's initial position
        _camera.orthographicSize = FindRequiredSize(); // Set the initial zoom level
    }
}
