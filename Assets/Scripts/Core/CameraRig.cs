using UnityEngine;
using UnityEngine.InputSystem;
using Arkham.Map;
using Arkham.Players;

namespace Arkham.Core
{
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private MapGraph graph;

        [Header("Edge pan")]
        [SerializeField] private float panSpeed = 12f;
        [SerializeField] private float edgeThicknessPixels = 15f;

        [Header("Recenter")]
        [SerializeField] private Key recenterKey = Key.Space;
        [SerializeField] private float recenterTime = 0.25f;

        [Header("Bounds (XZ, in world units)")]
        [SerializeField] private float boundsPadding = 2f;

        [Header("Iso angle (2.5D table view)")]
        [Tooltip("Down-tilt. ~30 = 2:1 dimetric board-game iso; higher = more top-down.")]
        [SerializeField] private float pitchDegrees = 30f;
        [Tooltip("Rotation around vertical. 45 views the board from a corner (classic isometric).")]
        [SerializeField] private float yawDegrees = 45f;
        [Tooltip("If true, applies pitch+yaw on Awake. Disable to keep the camera transform's authored rotation.")]
        [SerializeField] private bool applyTiltOnAwake = true;

        private Camera _cam;
        private Vector3 _recenterVelocity;
        private Vector3 _recenterTarget;
        private bool _recentering;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (applyTiltOnAwake)
                transform.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
        }

        private void Update()
        {
            HandleRecenterInput();
            if (_recentering) UpdateRecenter();
            else HandleEdgePan();
        }

        private void HandleEdgePan()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 m = mouse.position.ReadValue();
            float w = Screen.width;
            float h = Screen.height;
            if (m.x < 0f || m.y < 0f || m.x > w || m.y > h) return;

            float dx = 0f, dz = 0f;
            if (m.x <= edgeThicknessPixels) dx = -1f;
            else if (m.x >= w - edgeThicknessPixels) dx = 1f;
            if (m.y <= edgeThicknessPixels) dz = -1f;
            else if (m.y >= h - edgeThicknessPixels) dz = 1f;
            if (dx == 0f && dz == 0f) return;

            Vector3 right = Flatten(transform.right);
            Vector3 forward = Flatten(transform.forward);
            Vector3 move = (right * dx + forward * dz).normalized * (panSpeed * Time.deltaTime);

            transform.position = Clamp(transform.position + move);
        }

        private void HandleRecenterInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || player == null) return;
            if (!kb[recenterKey].wasPressedThisFrame) return;

            _recenterTarget = Clamp(ComputeCameraPosFor(player.transform.position));
            _recenterVelocity = Vector3.zero;
            _recentering = true;
        }

        private void UpdateRecenter()
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, _recenterTarget, ref _recenterVelocity, recenterTime);
            if ((transform.position - _recenterTarget).sqrMagnitude < 0.0001f)
            {
                transform.position = _recenterTarget;
                _recentering = false;
            }
        }

        // Where should the camera sit so that `worldTarget` lands at screen center?
        // Casts a ray through the viewport center onto the ground plane (y=0) and
        // shifts the camera by the XZ delta between that look-point and the target.
        private Vector3 ComputeCameraPosFor(Vector3 worldTarget)
        {
            Ray r = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (!ground.Raycast(r, out float dist)) return transform.position;
            Vector3 lookPoint = r.GetPoint(dist);
            Vector3 delta = worldTarget - lookPoint;
            delta.y = 0f;
            return transform.position + delta;
        }

        // Clamp by where the camera is LOOKING on the ground, not where it physically sits.
        // The camera body is typically set back/above the grid, so clamping its position
        // would lock it to the grid edge and prevent panning outward.
        private Vector3 Clamp(Vector3 desiredCamPos)
        {
            if (graph == null || graph.Nodes.Count == 0) return desiredCamPos;
            Ray r = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (!ground.Raycast(r, out float dist)) return desiredCamPos;

            Vector3 currentLook = r.GetPoint(dist);
            Vector3 lookOffset = currentLook - transform.position;
            Vector3 desiredLook = desiredCamPos + lookOffset;

            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            foreach (var node in graph.Nodes)
            {
                if (node.WorldPos.x < minX) minX = node.WorldPos.x;
                if (node.WorldPos.x > maxX) maxX = node.WorldPos.x;
                if (node.WorldPos.z < minZ) minZ = node.WorldPos.z;
                if (node.WorldPos.z > maxZ) maxZ = node.WorldPos.z;
            }
            desiredLook.x = Mathf.Clamp(desiredLook.x, minX - boundsPadding, maxX + boundsPadding);
            desiredLook.z = Mathf.Clamp(desiredLook.z, minZ - boundsPadding, maxZ + boundsPadding);

            return desiredLook - lookOffset;
        }

        private static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude < 0.0001f ? Vector3.zero : v.normalized;
        }
    }
}
