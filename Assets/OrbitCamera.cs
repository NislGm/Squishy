using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform _focus = default;
    [SerializeField, Range(1f, 20f)] float _distance = 5f;
    [SerializeField, Range(0.1f, 1f)] float _distanceNearScale = .66f;
    [SerializeField, Min(0f)] private float _focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] private float _focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f)] private float _rotationSpeed = 90f;

    [Header("Contraints")]
    [SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -30f;
    [SerializeField, Range(-89f, 89f)] private float maxVerticalAngle = 60f;
    [SerializeField, Range(0f, 1f)] private float _verticalScale = 0.5f;

    [Header("Realign")]
    [Space][SerializeField, Min(0f)] private float alignDelay = 5f;
    [SerializeField, Range(0f, 90f)] private float alignSmoothRange = 45f;

    private Vector2 orbitAngles = new Vector2(25f, 0f);
    private Vector3 focusPoint, previousFocusPoint;

    private Vector2 _input;
    private float lastManualRotationTime;
    private float _distanceDefault;
    private float _focusRadiusDefault;

    private bool _toggleNearDistance = false;

    void Awake()
    {
        focusPoint = _focus.position;
        _distanceDefault = _distance;
        _focusRadiusDefault = _focusRadius;
    }
    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    void Update()
    {
        var gamepad = Gamepad.current;
        
        if (gamepad != null && gamepad.rightStickButton.wasPressedThisFrame)
        {
            _toggleNearDistance = !_toggleNearDistance;
            _distance = _toggleNearDistance ? _distanceDefault * _distanceNearScale : _distanceDefault;
            _focusRadius = _toggleNearDistance ? _focusRadiusDefault * _distanceNearScale : _focusRadius;
        }
    }

    void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;

        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * _distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;

        Vector3 targetPoint = _focus.position;
        if (_focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && _focusCentering > 0f)
            {
                t = Mathf.Pow(1f - _focusCentering, Time.deltaTime);
            }
            if (distance > _focusRadius)
            {
                t = Mathf.Pow(1f - _focusCentering, Time.unscaledDeltaTime);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    private void ConstrainAngles()
    {
        orbitAngles.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    private bool ManualRotation()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return false; // No gamepad connected.

        _input.y = gamepad.rightStick.ReadValue().x;
        _input.x = -gamepad.rightStick.ReadValue().y * _verticalScale;

        const float e = 0.001f;
        if (_input.x < -e || _input.x > e || _input.y < -e || _input.y > e)
        {
            orbitAngles += _rotationSpeed * Time.unscaledDeltaTime * _input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    private bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }

        Vector2 movement = new Vector2(
            focusPoint.x - previousFocusPoint.x,
            focusPoint.z - previousFocusPoint.z
        );

        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f)
        {
            return false;
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = _rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    private static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }
}