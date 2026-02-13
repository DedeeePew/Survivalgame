using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Player
{
    /// <summary>
    /// Realistic first-person arms/hands inspired by ARK: Survival Evolved.
    /// Creates detailed procedural arm meshes with proper skin material,
    /// sleeve textures, anatomically correct fingers that can grip items.
    /// Supports grip states: Open, Fist (tools), Palm (resources), Relaxed (idle).
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        public enum GripState { Open, Fist, Palm, Relaxed }

        [Header("Arm Settings")]
        [SerializeField] private float _armBobSpeed = 8f;
        [SerializeField] private float _armBobAmount = 0.02f;
        [SerializeField] private float _armSwayAmount = 0.01f;

        private Transform _cameraHolder;
        private GameObject _leftArm;
        private GameObject _rightArm;
        private GameObject _leftHand;
        private GameObject _rightHand;
        private GameObject _body;

        // Finger references for grip animation
        private Transform[] _rightFingers;
        private Transform _rightThumb;
        private Transform[] _leftFingers;
        private Transform _leftThumb;

        private Vector3 _leftArmBasePos;
        private Vector3 _rightArmBasePos;
        private float _bobTimer;
        private PlayerController _controller;

        // Current grip state
        private GripState _rightGripState = GripState.Relaxed;
        private GripState _leftGripState = GripState.Relaxed;
        private float _gripLerp = 0f;
        private float _targetGripLerp = 0f;

        // Materials
        private Material _skinMat;
        private Material _sleeveMat;
        private Material _bodyMat;
        private Material _nailMat;

        /// <summary>The grip point on the right hand where items attach.</summary>
        public Transform RightGripPoint { get; private set; }

        /// <summary>The grip point on the left hand.</summary>
        public Transform LeftGripPoint { get; private set; }

        private void Start()
        {
            _controller = GetComponent<PlayerController>();
            _cameraHolder = GetComponentInChildren<Camera>()?.transform;

            if (_cameraHolder == null)
            {
                Debug.LogError("[PlayerVisuals] No camera found!");
                return;
            }

            CreateMaterials();
            CreateArms();
            CreateBody();
        }

        private void CreateMaterials()
        {
            // Realistic skin color with subtle texture
            var skinTex = GenerateSkinTexture(64);
            _skinMat = ProceduralTextureGenerator.CreateMaterial(skinTex);
            if (_skinMat.HasProperty("_Smoothness"))
                _skinMat.SetFloat("_Smoothness", 0.25f);

            // Survival jacket sleeve - dark green with fabric texture
            var sleeveTex = GenerateSleeveTexture(64);
            _sleeveMat = ProceduralTextureGenerator.CreateMaterial(sleeveTex);
            if (_sleeveMat.HasProperty("_Smoothness"))
                _sleeveMat.SetFloat("_Smoothness", 0.05f);

            // Body material
            _bodyMat = ProceduralTextureGenerator.CreateColorMaterial(new Color(0.22f, 0.32f, 0.16f));

            // Fingernail material
            _nailMat = ProceduralTextureGenerator.CreateColorMaterial(new Color(0.9f, 0.82f, 0.75f));
            if (_nailMat.HasProperty("_Smoothness"))
                _nailMat.SetFloat("_Smoothness", 0.6f);
        }

        private void CreateArms()
        {
            // Right arm (dominant, tool hand) - more visible, positioned right-lower
            _rightArm = CreateArmGroup("RightArm",
                new Vector3(0.28f, -0.28f, 0.35f),
                new Vector3(0, 0, -5f),
                true);

            // Left arm - less prominent, support hand
            _leftArm = CreateArmGroup("LeftArm",
                new Vector3(-0.28f, -0.28f, 0.35f),
                new Vector3(0, 0, 5f),
                false);

            _rightArmBasePos = _rightArm.transform.localPosition;
            _leftArmBasePos = _leftArm.transform.localPosition;
        }

        private GameObject CreateArmGroup(string name, Vector3 localPos, Vector3 localRot, bool isRight)
        {
            var armRoot = new GameObject(name);
            armRoot.transform.SetParent(_cameraHolder);
            armRoot.transform.localPosition = localPos;
            armRoot.transform.localRotation = Quaternion.Euler(localRot);

            // ── Upper Arm (Sleeve) ──
            var upperArm = CreateMeshPart("UpperArm", armRoot.transform,
                new Vector3(0, 0.01f, -0.12f),
                new Vector3(0.085f, 0.065f, 0.22f),
                Quaternion.Euler(12, 0, 0),
                _sleeveMat);

            // Sleeve cuff detail
            var cuff = CreateMeshPart("Cuff", armRoot.transform,
                new Vector3(0, 0.005f, -0.01f),
                new Vector3(0.09f, 0.068f, 0.04f),
                Quaternion.identity,
                _sleeveMat);

            // ── Forearm (Skin) ──
            var forearm = CreateMeshPart("Forearm", armRoot.transform,
                new Vector3(0, -0.005f, 0.08f),
                new Vector3(0.068f, 0.055f, 0.2f),
                Quaternion.Euler(-3, 0, 0),
                _skinMat);

            // ── Wrist ──
            var wrist = CreateMeshPart("Wrist", armRoot.transform,
                new Vector3(0, -0.01f, 0.175f),
                new Vector3(0.058f, 0.042f, 0.04f),
                Quaternion.identity,
                _skinMat);

            // ── Hand (Palm) ──
            var hand = new GameObject("Hand");
            hand.transform.SetParent(armRoot.transform);
            hand.transform.localPosition = new Vector3(0, -0.015f, 0.21f);
            hand.transform.localRotation = Quaternion.Euler(5, 0, 0);

            var palm = CreateMeshPart("Palm", hand.transform,
                Vector3.zero,
                new Vector3(0.065f, 0.028f, 0.075f),
                Quaternion.identity,
                _skinMat);

            // ── Grip Point (where items attach) ──
            var gripPoint = new GameObject("GripPoint");
            gripPoint.transform.SetParent(hand.transform);
            gripPoint.transform.localPosition = new Vector3(0, 0.01f, 0.02f);
            gripPoint.transform.localRotation = Quaternion.identity;

            if (isRight)
            {
                RightGripPoint = gripPoint.transform;
                _rightHand = hand;
            }
            else
            {
                LeftGripPoint = gripPoint.transform;
                _leftHand = hand;
            }

            // ── Fingers ──
            var fingers = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                float xOffset = -0.022f + i * 0.0148f;
                float fingerLen = (i == 1 || i == 2) ? 0.05f : 0.042f; // Middle/ring longer

                var finger = CreateFinger($"Finger_{i}", hand.transform,
                    new Vector3(xOffset, -0.003f, 0.04f),
                    fingerLen, 0.011f, isRight);
                fingers[i] = finger.transform;
            }

            // ── Thumb ──
            var thumb = CreateThumb("Thumb", hand.transform,
                new Vector3(isRight ? -0.035f : 0.035f, 0.008f, 0.015f),
                isRight);

            if (isRight)
            {
                _rightFingers = fingers;
                _rightThumb = thumb.transform;
            }
            else
            {
                _leftFingers = fingers;
                _leftThumb = thumb.transform;
            }

            return armRoot;
        }

        private GameObject CreateFinger(string name, Transform parent, Vector3 pos, float length, float width, bool isRight)
        {
            var fingerRoot = new GameObject(name);
            fingerRoot.transform.SetParent(parent);
            fingerRoot.transform.localPosition = pos;
            fingerRoot.transform.localRotation = Quaternion.Euler(8, 0, 0); // Slight natural curl

            // Proximal phalanx
            var seg1 = CreateMeshPart("Proximal", fingerRoot.transform,
                new Vector3(0, 0, length * 0.22f),
                new Vector3(width, width * 0.85f, length * 0.45f),
                Quaternion.identity,
                _skinMat);

            // Distal phalanx (tip)
            var seg2 = CreateMeshPart("Distal", fingerRoot.transform,
                new Vector3(0, -0.001f, length * 0.6f),
                new Vector3(width * 0.88f, width * 0.78f, length * 0.4f),
                Quaternion.Euler(5, 0, 0),
                _skinMat);

            // Fingernail
            var nail = CreateMeshPart("Nail", fingerRoot.transform,
                new Vector3(0, 0.004f, length * 0.82f),
                new Vector3(width * 0.7f, 0.002f, width * 0.5f),
                Quaternion.identity,
                _nailMat);

            return fingerRoot;
        }

        private GameObject CreateThumb(string name, Transform parent, Vector3 pos, bool isRight)
        {
            var thumbRoot = new GameObject(name);
            thumbRoot.transform.SetParent(parent);
            thumbRoot.transform.localPosition = pos;
            thumbRoot.transform.localRotation = Quaternion.Euler(0, isRight ? 40 : -40, isRight ? -15 : 15);

            // Thumb base (metacarpal)
            var base1 = CreateMeshPart("Base", thumbRoot.transform,
                new Vector3(0, 0, 0.01f),
                new Vector3(0.014f, 0.013f, 0.025f),
                Quaternion.identity,
                _skinMat);

            // Thumb tip
            var tip = CreateMeshPart("Tip", thumbRoot.transform,
                new Vector3(0, 0, 0.032f),
                new Vector3(0.012f, 0.011f, 0.022f),
                Quaternion.Euler(5, 0, 0),
                _skinMat);

            // Thumbnail
            var nail = CreateMeshPart("Nail", thumbRoot.transform,
                new Vector3(0, 0.005f, 0.04f),
                new Vector3(0.009f, 0.002f, 0.008f),
                Quaternion.identity,
                _nailMat);

            return thumbRoot;
        }

        private GameObject CreateMeshPart(string name, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot, Material mat)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = pos;
            part.transform.localScale = scale;
            part.transform.localRotation = rot;
            part.GetComponent<Renderer>().material = mat;
            Object.Destroy(part.GetComponent<Collider>());
            return part;
        }

        private void CreateBody()
        {
            _body = new GameObject("PlayerBody");
            _body.transform.SetParent(transform);
            _body.transform.localPosition = Vector3.zero;

            // Torso
            CreateMeshPart("Torso", _body.transform,
                Vector3.zero, new Vector3(0.4f, 0.5f, 0.25f),
                Quaternion.identity, _bodyMat);

            // Legs
            var pantsMat = ProceduralTextureGenerator.CreateColorMaterial(new Color(0.25f, 0.2f, 0.15f));
            CreateMeshPart("LeftLeg", _body.transform,
                new Vector3(-0.1f, -0.55f, 0), new Vector3(0.15f, 0.6f, 0.15f),
                Quaternion.identity, pantsMat);
            CreateMeshPart("RightLeg", _body.transform,
                new Vector3(0.1f, -0.55f, 0), new Vector3(0.15f, 0.6f, 0.15f),
                Quaternion.identity, pantsMat);

            // Boots
            var bootMat = ProceduralTextureGenerator.CreateColorMaterial(new Color(0.2f, 0.15f, 0.08f));
            CreateMeshPart("LeftBoot", _body.transform,
                new Vector3(-0.1f, -0.88f, 0.03f), new Vector3(0.16f, 0.08f, 0.22f),
                Quaternion.identity, bootMat);
            CreateMeshPart("RightBoot", _body.transform,
                new Vector3(0.1f, -0.88f, 0.03f), new Vector3(0.16f, 0.08f, 0.22f),
                Quaternion.identity, bootMat);
        }

        // ══════════════════════════════════════
        // GRIP CONTROL
        // ══════════════════════════════════════

        /// <summary>Set the right hand grip state for item holding.</summary>
        public void SetRightGrip(GripState state)
        {
            if (_rightGripState == state) return;
            _rightGripState = state;
            ApplyGripState(_rightFingers, _rightThumb, state, true);
        }

        /// <summary>Set the left hand grip state.</summary>
        public void SetLeftGrip(GripState state)
        {
            if (_leftGripState == state) return;
            _leftGripState = state;
            ApplyGripState(_leftFingers, _leftThumb, state, false);
        }

        private void ApplyGripState(Transform[] fingers, Transform thumb, GripState state, bool isRight)
        {
            if (fingers == null || thumb == null) return;

            float fingerCurl, thumbCurl;
            switch (state)
            {
                case GripState.Fist:
                    fingerCurl = 75f;   // Fingers curled tight around handle
                    thumbCurl = 45f;
                    break;
                case GripState.Palm:
                    fingerCurl = 20f;   // Fingers slightly curved, holding on palm
                    thumbCurl = 10f;
                    break;
                case GripState.Open:
                    fingerCurl = 0f;
                    thumbCurl = 0f;
                    break;
                default: // Relaxed
                    fingerCurl = 15f;   // Natural slight curl
                    thumbCurl = 8f;
                    break;
            }

            for (int i = 0; i < fingers.Length; i++)
            {
                // Vary curl per finger (ring/pinky curl more)
                float multiplier = (i >= 2) ? 1.15f : 1f;
                fingers[i].localRotation = Quaternion.Euler(fingerCurl * multiplier, 0, 0);
            }

            float thumbDir = isRight ? 1f : -1f;
            if (state == GripState.Fist)
            {
                thumb.localRotation = Quaternion.Euler(thumbCurl, (isRight ? 40 : -40) - thumbDir * 15f, isRight ? -15 : 15);
            }
            else
            {
                thumb.localRotation = Quaternion.Euler(thumbCurl * 0.3f, isRight ? 40 : -40, isRight ? -15 : 15);
            }
        }

        // ══════════════════════════════════════
        // ANIMATION
        // ══════════════════════════════════════

        private void LateUpdate()
        {
            if (_controller == null || _rightArm == null) return;

            float speed = _controller.CurrentSpeed;
            bool isMoving = speed > 0.5f;
            bool isSprinting = _controller.IsSprinting;

            float bobSpeed = isSprinting ? _armBobSpeed * 1.5f : _armBobSpeed;
            float bobAmount = isSprinting ? _armBobAmount * 1.5f : _armBobAmount;

            if (isMoving)
            {
                _bobTimer += Time.deltaTime * bobSpeed;
            }
            else
            {
                _bobTimer += Time.deltaTime * 1.5f;
                bobAmount *= 0.3f;
            }

            float bobY = Mathf.Sin(_bobTimer) * bobAmount;
            float bobX = Mathf.Cos(_bobTimer * 0.5f) * _armSwayAmount;

            // Smooth breathing-like idle motion
            float breathY = Mathf.Sin(Time.time * 1.2f) * 0.002f;
            float breathX = Mathf.Sin(Time.time * 0.8f) * 0.001f;

            if (_rightArm != null)
            {
                _rightArm.transform.localPosition = _rightArmBasePos + new Vector3(bobX + breathX, bobY + breathY, 0);
            }
            if (_leftArm != null)
            {
                _leftArm.transform.localPosition = _leftArmBasePos + new Vector3(-bobX - breathX, -bobY * 0.8f + breathY, 0);
            }

            // Body leg animation
            if (_body != null && isMoving)
            {
                var leftLeg = _body.transform.Find("LeftLeg");
                var rightLeg = _body.transform.Find("RightLeg");
                if (leftLeg != null && rightLeg != null)
                {
                    float legSwing = Mathf.Sin(_bobTimer) * 15f;
                    leftLeg.localRotation = Quaternion.Euler(legSwing, 0, 0);
                    rightLeg.localRotation = Quaternion.Euler(-legSwing, 0, 0);
                }
            }
        }

        // ══════════════════════════════════════
        // PROCEDURAL SKIN TEXTURE
        // ══════════════════════════════════════

        private static Texture2D GenerateSkinTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Base skin tone with subtle variation
                    float n1 = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float n2 = Mathf.PerlinNoise(x * 0.25f + 50, y * 0.25f + 50) * 0.15f;
                    float pore = Mathf.PerlinNoise(x * 0.6f + 100, y * 0.6f + 100) * 0.05f;

                    float v = n1 + n2 - pore;
                    float r = Mathf.Lerp(0.78f, 0.88f, v);
                    float g = Mathf.Lerp(0.58f, 0.72f, v);
                    float b = Mathf.Lerp(0.42f, 0.55f, v);

                    // Subtle vein-like coloring
                    float vein = Mathf.PerlinNoise(x * 0.15f + 200, y * 0.04f + 200);
                    if (vein > 0.65f)
                    {
                        float veinStr = (vein - 0.65f) * 0.3f;
                        r -= veinStr * 0.08f;
                        g -= veinStr * 0.03f;
                        b += veinStr * 0.05f;
                    }

                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b)
                    ));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateSleeveTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Fabric weave pattern
                    float weave1 = Mathf.Abs(Mathf.Sin(x * 1.2f)) * 0.05f;
                    float weave2 = Mathf.Abs(Mathf.Sin(y * 1.2f)) * 0.05f;
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                    float dirt = Mathf.PerlinNoise(x * 0.05f + 80, y * 0.05f + 80) * 0.1f;

                    float v = noise + weave1 + weave2 - dirt;

                    // Dark olive green
                    float r = Mathf.Lerp(0.15f, 0.28f, v);
                    float g = Mathf.Lerp(0.22f, 0.38f, v);
                    float b = Mathf.Lerp(0.08f, 0.18f, v);

                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(r),
                        Mathf.Clamp01(g),
                        Mathf.Clamp01(b)
                    ));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
