﻿#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FM: Class to use only in editor, it creates bones with preview static mesh then skin it to skinned mesh renderer
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("FImpossible Creations/Tail Animator/Utilities/FTail Editor Skinner")]
    public class FTail_Editor_Skinner : MonoBehaviour
    {
        [Header("SKIN STATIC MESHES INSIDE UNITY", order = 0)]
        [Space(3f, order = 1)]
        [BackgroundColor(0.75f, 0.75f, 1.0f, 0.7f)]
        [Header("Auto bone markers settings", order = 2)]
        [Space(3f, order = 3)]
        public int AutoMarkersCount = 5;
        public float DistanceValue = 0.3f;
        public Vector3 positionOffset = new Vector3(0, 0f);
        public Vector2 startDirection = new Vector2(-90, 0f);
        public Vector2 rotationOffset = new Vector2(0f, 0f);

        [Range(0f, 5f)]
        public float HelpScaleValue = 1f;

        [BackgroundColor(0.85f, 0.85f, 1.0f, 0.85f)]
        public AnimationCurve DistancesFaloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        public AnimationCurve RotationsFaloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [BackgroundColor(0.5f, 1f, 0.5f, 0.8f)]
        [Space(10f, order = 0)]
        [Header("Left empty if you don't use custom markers", order = 1)]
        [Space(-7f, order = 2)]
        [Header("Moving custom markers will not trigger realtime update", order = 3)]
        public Transform[] CustomBoneMarkers;

        [Space(7f, order = 0)]
        [Header("Weights Spread Settings", order = 1)]
        [Space(3f, order = 2)]
        [Range(0f, 1f)]
        public float SpreadValue = 0.8f;
        [Range(0f, 1f)]
        public float SpreadPower = .185f;
        [Tooltip("Offsetting spreading area, For example 0,0,1 and recommended values from 0 to 2 not bigger")]
        public Vector3 SpreadOffset = Vector3.zero;
        [Range(1, 2)]
        public int LimitBoneWeightCount = 2;

        [BackgroundColor(0.4f, 0.8f, 0.8f, 0.8f)]
        [Space(7f, order = 0)]
        [Header("Additional Variables", order = 1)]
        [Space(3f, order = 2)]
        [Range(0f, 5f)]
        public float GizmoSize = 0.1f;
        [Range(0f, 1f)]
        public float GizmoAlpha = 1f;

        [BackgroundColor()]
        [Tooltip("If your model have many vertices, turn it only when neccesary")]
        public bool RealtimeUpdate = true;
        public bool ShowPreview = true;
        public bool DebugMode = false;



        // ----- Private stuff variables

        /// <summary> Base Mesh </summary>
        private Mesh baseMesh;

        [HideInInspector]
        public List<Color32> baseVertexColor;

        private MeshRenderer meshRenderer;

        /// <summary> Offsets to define bone influence area </summary>
        private Vector3[] boneAreas;

        /// <summary> Fake bones list before creating true skeleton for mesh </summary>
        private Transform[] ghostBones;

        /// <summary> Vertex datas used for setting weights precisely</summary>
        private VertexData[] vertexDatas;

        /// <summary> Generated marker points for automatic bone points </summary>
        private Transform[] autoMarkers;

        // Hide in inspector because when variables are private, they're resetted to null every time code compiles
        /// <summary> Because we can't destroy gameObjects in OnValidate, we do something similar to object pools </summary>
        [HideInInspector]
        public List<Transform> allMarkersTransforms = new List<Transform>();

        /// <summary> Transform with components helping drawing how weights are spread on model </summary>
        [HideInInspector]
        public Transform weightPreviewTransform;

        [HideInInspector]
        public bool popupShown = false;

        private Material weightPreviewMaterial;
        private Mesh weightPreviewMesh;


        // ----- DEAR CALCULATIONS! LETS BEGIN!

        /// <summary>
        /// When something changes in inspector, let's recalculate parameters
        /// </summary>
        private void OnValidate()
        {
            if (!GetBaseMesh()) return;

            if (CustomBoneMarkers == null) CustomBoneMarkers = new Transform[0]; // Prevent error log when adding component

            // Use only custom markers if they're assigned
            if (CustomBoneMarkers.Length > 0)
            {
                ghostBones = CustomBoneMarkers;
            }
            else // Use auto markers
            {
                CalculateAutoMarkers();
                ghostBones = autoMarkers;
            }


            if (RealtimeUpdate)
            {
                CalculateVertexDatas();
                UpdatePreviewMesh();
            }
        }

        /// <summary>
        /// Drawing helper stuff
        /// </summary>
        private void OnDrawGizmos()
        {
            if (CustomBoneMarkers == null) return;

            if (CustomBoneMarkers.Length < 1)
                DrawMarkers(autoMarkers);
            else
                DrawMarkers(CustomBoneMarkers);
        }


        /// <summary>
        /// Updating preview mesh to view weights correctly
        /// </summary>
        private void UpdatePreviewMesh()
        {
            #region Creation of new preview mesh when needed

            if (weightPreviewTransform == null)
            {
                weightPreviewTransform = new GameObject(name + "[preview mesh]").transform;
                weightPreviewTransform.SetParent(transform);
                weightPreviewTransform.localPosition = Vector3.zero;
                weightPreviewTransform.localRotation = Quaternion.identity;
                weightPreviewTransform.localScale = Vector3.one;

                weightPreviewTransform.gameObject.AddComponent<MeshFilter>().mesh = baseMesh;

                Material[] newMaterials = new Material[meshRenderer.sharedMaterials.Length];

                for (int i = 0; i < newMaterials.Length; i++) newMaterials[i] = new Material(Shader.Find("Particles/FVertexLit Blended"));
                weightPreviewTransform.gameObject.AddComponent<MeshRenderer>().materials = newMaterials;
            }

            #endregion

            if (ShowPreview)
            {
                meshRenderer.enabled = false;
                weightPreviewTransform.gameObject.SetActive(true);
                List<Color> vColors = new List<Color>();
                for (int i = 0; i < vertexDatas.Length; i++) vColors.Add(vertexDatas[i].GetWeightColor());
                baseMesh.SetColors(vColors);
                weightPreviewTransform.gameObject.GetComponent<MeshFilter>().mesh = baseMesh;
            }
            else
            {
                meshRenderer.enabled = true;
                if (baseVertexColor != null) if (baseMesh) if (baseMesh.vertexCount == baseVertexColor.Count) baseMesh.SetColors(baseVertexColor);
                weightPreviewTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Calculating auto markers transforms
        /// </summary>
        private void CalculateAutoMarkers()
        {
            #region Creation of markers' transforms

            if (autoMarkers == null) autoMarkers = new Transform[0];

            if (allMarkersTransforms.Count < AutoMarkersCount)
            {
                for (int i = autoMarkers.Length; i < AutoMarkersCount; i++)
                {
                    GameObject newMarker = new GameObject(name + "-SkinMarker " + i);
                    newMarker.transform.SetParent(transform, true);
                    allMarkersTransforms.Add(newMarker.transform);
                }
            }

            if (autoMarkers.Length != AutoMarkersCount)
            {
                autoMarkers = new Transform[AutoMarkersCount];
                for (int i = 0; i < AutoMarkersCount; i++)
                {
                    autoMarkers[i] = allMarkersTransforms[i];
                }
            }

            #endregion

            autoMarkers[0].position = transform.position + positionOffset;
            autoMarkers[0].rotation = Quaternion.Euler(startDirection + rotationOffset);

            float step = 1f / (float)AutoMarkersCount;

            for (int i = 1; i < AutoMarkersCount; i++)
            {
                float forwardMultiplier = DistanceValue;
                forwardMultiplier *= DistancesFaloff.Evaluate(i * step);
                forwardMultiplier *= HelpScaleValue;
                Vector3 targetPosition = autoMarkers[i - 1].position + autoMarkers[i - 1].rotation * Vector3.forward * forwardMultiplier;

                Vector3 newRot = startDirection + rotationOffset * (i + 1) * RotationsFaloff.Evaluate(i * step);

                autoMarkers[i].position = targetPosition;
                autoMarkers[i].rotation = Quaternion.Euler(newRot);
            }
        }

        /// <summary>
        /// Getting base mesh variable, depends if it's skinned mesh or static mesh
        /// </summary>
        private Mesh GetBaseMesh()
        {
            if (baseMesh == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter) baseMesh = meshFilter.sharedMesh;
            }
            else return baseMesh;

            if (!baseMesh)
            {
                if (!popupShown)
                {
                    EditorUtility.DisplayDialog("Tail Skinner Error", "[Tail Skinner] No base mesh! (mesh filter and mesh renderer)", "Ok");
                    popupShown = true;
                }

                Debug.LogError("No BaseMesh!");
            }

            if (baseMesh)
            {
                if (baseVertexColor == null) baseVertexColor = new List<Color32>();
                if (baseVertexColor.Count != baseMesh.vertexCount)
                {
                    baseVertexColor.Clear();
                    baseMesh.GetColors(baseVertexColor);
                }
            }

            return baseMesh;
        }

        /// <summary>
        /// Calculating base vertices datas for current bones setup
        /// </summary>
        private void CalculateVertexDatas()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            int vertCount = baseMesh.vertexCount;
            vertexDatas = new VertexData[vertCount];

            boneAreas = new Vector3[ghostBones.Length];
            for (int i = 0; i < ghostBones.Length - 1; i++) boneAreas[i] = ghostBones[i + 1].localPosition - ghostBones[i].localPosition;
            if (boneAreas.Length > 1) boneAreas[boneAreas.Length - 1] = boneAreas[boneAreas.Length - 2];

            try
            {
                for (int i = 0; i < vertCount; i++)
                {
                    vertexDatas[i] = new VertexData(baseMesh.vertices[i]);

                    vertexDatas[i].CalculateVertexParameters(ghostBones, boneAreas, LimitBoneWeightCount, SpreadValue, SpreadOffset, SpreadPower);

                    // Displaying progress bar when iteration takes too much time
                    if (watch.ElapsedMilliseconds > 1500)
                        if (i % 10 == 0)
                        {
                            EditorUtility.DisplayProgressBar("Analizing mesh vertices...", "Analizing Vertices (" + i + "/" + vertCount + ")", ((float)i / (float)vertCount));
                        }
                }

                EditorUtility.ClearProgressBar();

            }
            catch (System.Exception exc)
            {
                Debug.LogError(exc);
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Skinning mesh to new skinned mesh renderer with choosed weight markers settings
        /// </summary>
        public void SkinMesh(bool addTailAnimator = false)
        {
            CalculateVertexDatas();

            Vector3 prePos = transform.position;
            Quaternion preRot = transform.rotation;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            GameObject newSkinObject = new GameObject(name + " [FSKINNED]");
            newSkinObject.transform.localScale = transform.localScale;

            SkinnedMeshRenderer newSkinnedMesh = newSkinObject.AddComponent<SkinnedMeshRenderer>();
            Mesh newMesh = Instantiate(GetBaseMesh());
            newMesh.name = baseMesh.name + " [FSKINNED]";
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
            newMesh.RecalculateTangents();

            if (baseVertexColor != null)
                if (newMesh.vertexCount == baseVertexColor.Count) newMesh.SetColors(baseVertexColor);

            MeshRenderer meshRend = GetComponent<MeshRenderer>();

            if (meshRend)
            {
                newSkinnedMesh.materials = meshRend.sharedMaterials;
                newSkinnedMesh.sharedMaterials = meshRend.sharedMaterials;
            }

            Transform[] bones = new Transform[ghostBones.Length];
            Matrix4x4[] bindPoses = new Matrix4x4[ghostBones.Length];

            string nameString = "";
            if (baseMesh.name.Length < 6) nameString = baseMesh.name; else nameString = baseMesh.name.Substring(0, 5);

            for (int i = 0; i < ghostBones.Length; i++)
            {
                bones[i] = new GameObject("BoneF-" + nameString + "[" + i + "]").transform;
                if (i == 0) bones[i].SetParent(newSkinObject.transform, true); else bones[i].SetParent(bones[i - 1], true);

                bones[i].transform.position = ghostBones[i].position;
                bones[i].transform.rotation = ghostBones[i].rotation;

                bindPoses[i] = bones[i].worldToLocalMatrix * transform.localToWorldMatrix;
            }

            BoneWeight[] weights = new BoneWeight[newMesh.vertexCount];
            for (int v = 0; v < weights.Length; v++) weights[v] = new BoneWeight();

            // Calculating and applying weights for verices
            for (int i = 0; i < vertexDatas.Length; i++)
            {
                for (int w = 0; w < vertexDatas[i].weights.Length; w++)
                {
                    weights[i] = SetWeightIndex(weights[i], w, vertexDatas[i].bonesIndexes[w]);
                    weights[i] = SetWeightToBone(weights[i], w, vertexDatas[i].weights[w]);
                }
            }

            newMesh.bindposes = bindPoses;
            newMesh.boneWeights = weights;

            newSkinnedMesh.sharedMesh = newMesh;// (Mesh)AssetDatabase.LoadAssetAtPath(newMeshPath, typeof(Mesh));

            newSkinnedMesh.rootBone = bones[0];
            newSkinnedMesh.bones = bones;

            transform.position = prePos;
            transform.rotation = preRot;

            newSkinObject.transform.SetParent(transform.parent, true);
            newSkinObject.transform.position = prePos + Vector3.right;
            newSkinObject.transform.rotation = preRot;

            switch (LimitBoneWeightCount)
            {
                case 1: newSkinnedMesh.quality = SkinQuality.Bone1; break;
                case 2: newSkinnedMesh.quality = SkinQuality.Bone2; break;
                case 4: newSkinnedMesh.quality = SkinQuality.Bone4; break;
                default: newSkinnedMesh.quality = SkinQuality.Auto; break;
            }

            if (addTailAnimator)
            {
                FTail_Animator t = bones[0].gameObject.AddComponent<FTail_Animator>();
                t.LookUpMethod = FTail_AnimatorBase.FELookUpMethod.CrossUp;
                t.OrientationReference = newSkinObject.transform;
            }

            // Create asset for new model so it not disappear when we create prefab from this gameObject
            string newMeshPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(baseMesh));
            AssetDatabase.CreateAsset(newMesh, newMeshPath + "/" + newMesh.name + ".mesh");
            AssetDatabase.SaveAssets();

            Debug.Log("New skinned mesh '" + newMesh.name + ".mesh" + "' saved under path: '" + newMeshPath + "'");
        }

        /// <summary>
        /// Method which is returning certain weight variable from BoneWeight struct
        /// </summary>
        public float GetWeightAtIndex(BoneWeight weight, int bone = 0)
        {
            switch (bone)
            {
                case 1: return weight.weight1;
                case 2: return weight.weight2;
                case 3: return weight.weight3;
                default: return weight.weight0;
            }
        }

        /// <summary>
        /// Method which is setting certain weight variable from BoneWeight struct
        /// </summary>
        public BoneWeight SetWeightIndex(BoneWeight weight, int bone = 0, int index = 0)
        {
            switch (bone)
            {
                case 1: weight.boneIndex1 = index; break;
                case 2: weight.boneIndex2 = index; break;
                case 3: weight.boneIndex3 = index; break;
                default: weight.boneIndex0 = index; break;
            }

            return weight;
        }

        /// <summary>
        /// Method which is setting certain weight variable from BoneWeight struct
        /// </summary>
        public BoneWeight SetWeightToBone(BoneWeight weight, int bone = 0, float value = 1f)
        {
            switch (bone)
            {
                case 1: weight.weight1 = value; break;
                case 2: weight.weight2 = value; break;
                case 3: weight.weight3 = value; break;
                default: weight.weight0 = value; break;
            }

            return weight;
        }

        /// <summary>
        /// Make sure everything which was created by this script is destroyed
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < allMarkersTransforms.Count; i++) if (allMarkersTransforms[i] != null) DestroyImmediate(allMarkersTransforms[i].gameObject);
            if (weightPreviewTransform != null) DestroyImmediate(weightPreviewTransform.gameObject);

            if (baseMesh == null) return;
            meshRenderer.enabled = true;
        }

        /// <summary>
        /// Simple helper class to store vertices parameters in reference to bones
        /// </summary>
        [System.Serializable]
        public class VertexData
        {
            // Assigned in constructor
            public Vector3 position;

            public Transform[] bones;

            /// <summary> Indexes for helpers in visualization </summary>
            public int[] bonesIndexes;
            public int allMeshBonesCount;

            // Assigned during custom weight calculations
            public float[] weights;


            /// <summary>
            /// Set main data
            /// </summary>
            public VertexData(Vector3 pos)
            {
                position = pos;
            }


            /// <summary>
            /// Distance to bone area
            /// </summary>
            public float DistanceToLine(Vector3 pos, Vector3 lineStart, Vector3 lineEnd)
            {
                Vector3 dirVector1 = pos - lineStart;
                Vector3 dirVector2 = (lineEnd - lineStart).normalized;

                float distance = Vector3.Distance(lineStart, lineEnd);
                float dot = Vector3.Dot(dirVector2, dirVector1);

                if (dot <= 0) return Vector3.Distance(pos, lineStart);

                if (dot >= distance) return Vector3.Distance(pos, lineEnd);

                Vector3 dotVector = dirVector2 * dot;

                Vector3 closestPoint = lineStart + dotVector;

                return Vector3.Distance(pos, closestPoint);
            }


            /// <summary>
            /// Calculating vertex's distances to 4 nearest bones (4 bone weights is maximum count in Unity)
            /// for further custom weight calculations
            /// </summary>
            public void CalculateVertexParameters(Transform[] allSkinnedBones, Vector3[] boneAreas, int maxWeightedBones, float spread, Vector3 spreadOffset, float spreadPower = 1f)
            {
                allMeshBonesCount = allSkinnedBones.Length;

                // Using Vector2 for simple two float values in one variable, x = bone index y = distance of vertex to this bone, later we will sort list using distances
                List<Vector2> calculatedDistances = new List<Vector2>();

                // Check later if we don't need to transpone points to model space scale
                for (int i = 0; i < allSkinnedBones.Length; i++)
                {
                    Vector3 boneEnd;
                    if (i != allSkinnedBones.Length - 1)
                        boneEnd = Vector3.Lerp(allSkinnedBones[i].localPosition, allSkinnedBones[i + 1].localPosition, 0.9f);
                    else
                        boneEnd = Vector3.Lerp(allSkinnedBones[i].localPosition, allSkinnedBones[i].localPosition + (allSkinnedBones[i].localPosition - allSkinnedBones[i - 1].localPosition), 0.9f);

                    boneEnd += allSkinnedBones[i].TransformDirection(spreadOffset);

                    float distance = DistanceToLine(position, allSkinnedBones[i].localPosition, boneEnd);

                    // Making bone offset to behave like bone area
                    calculatedDistances.Add(new Vector2(i, distance));
                }

                // Sorting by nearest all bones
                calculatedDistances.Sort((a, b) => a.y.CompareTo(b.y));

                // Limiting vertex weight up to 4 bones
                int maxBones = (int)Mathf.Min(maxWeightedBones, allSkinnedBones.Length);

                // Assigning max 4 nearest bones and their distances to this vertex
                bonesIndexes = new int[maxBones];
                float[] nearestDistances = new float[maxBones];
                Transform[] nearestBones = new Transform[maxBones];

                for (int i = 0; i < maxBones; i++)
                {
                    bonesIndexes[i] = (int)calculatedDistances[i].x;
                    nearestBones[i] = allSkinnedBones[bonesIndexes[i]];
                    nearestDistances[i] = calculatedDistances[i].y;
                }

                bones = nearestBones;

                // Basing on spread value we spreading weight to nearest bones
                // Calculating percentage distances to bones
                float[] boneWeightsForVertex = new float[maxBones];


                AutoSetBoneWeights(boneWeightsForVertex, nearestDistances, spread, spreadPower, boneAreas);


                float weightLeft = 1f; // Must amount of weight which needs to be assigned
                weights = new float[maxBones]; // New weight parameters

                // Applying weights to each bone assigned to vertex
                for (int i = 0; i < maxBones; i++)
                {
                    if (spread == 0) if (i > 0) break;

                    if (weightLeft <= 0f) // No more weight to apply
                    {
                        weights[i] = 0f;
                        continue;
                    }

                    float targetWeight = boneWeightsForVertex[i];

                    weightLeft -= targetWeight;

                    if (weightLeft <= 0f) targetWeight += weightLeft; else { if (i == maxBones - 1) targetWeight += weightLeft; } // Using weight amount which is left to assign

                    weights[i] = targetWeight;
                }
            }

            public float[] debugDists;
            public float[] debugDistWeights;
            public float[] debugWeights;

            /// <summary>
            /// Spreading weights over bones for current vertex
            /// </summary>
            private void AutoSetBoneWeights(float[] weightForBone, float[] distToBone, float spread, float spreadPower, Vector3[] boneAreas)
            {
                int bonesC = weightForBone.Length;
                float[] boneLengths = new float[bonesC]; for (int i = 0; i < boneLengths.Length; i++) boneLengths[i] = boneAreas[i].magnitude;
                float[] normalizedDistanceWeights = new float[bonesC];
                for (int i = 0; i < weightForBone.Length; i++) weightForBone[i] = 0f;

                float normalizeDistance = 0f;
                for (int i = 0; i < bonesC; i++) normalizeDistance += distToBone[i];
                for (int i = 0; i < bonesC; i++) normalizedDistanceWeights[i] = 1f - distToBone[i] / normalizeDistance; // Reversing weight power - nearest (smallest distance) must have biggest weight value

                debugDists = distToBone;

                if (bonesC == 1 || spread == 0f) // Simpliest ONE BONE -------------------------------------------------------------
                {
                    // [0] - nearest bone
                    weightForBone[0] = 1f; // Just one weight - spread does not change anything

                }



                else if (bonesC == 2) // Simple TWO BONES -------------------------------------------------------------
                {
                    float normalizer = 1f;
                    weightForBone[0] = 1f;


                    // distToBone[0] is zero, max spread distance is length of bone / 3 
                    float distRange = Mathf.InverseLerp(distToBone[0] + (boneLengths[0] / 1.25f) * spread, distToBone[0], distToBone[1]);
                    debugDists[0] = distRange;

                    // 0 -> full nearest bone weight
                    // 1 -> half nearest half second bone weight
                    //float value = Mathf.Pow(Mathf.Lerp(0f, 1f, distRange), Mathf.Lerp(1.5f, 6f, spreadPower)); // dist out
                    float value = DistributionIn(Mathf.Lerp(0f, 1f, distRange), Mathf.Lerp(1.5f, 16f, spreadPower));
                    //float value = Mathf.Lerp(0f, 1f, distRange);
                    

                    weightForBone[1] = value;
                    normalizer += value;

                    debugDistWeights = new float[weightForBone.Length];

                    weightForBone.CopyTo(debugDistWeights, 0);

                    for (int i = 0; i < bonesC; i++) weightForBone[i] /= normalizer;

                    debugWeights = weightForBone;
                }

                else // Complex > TWO BONES -------------------------------------------------------------
                {

                    float reffVal = boneLengths[0] / 10f;
                    float refLength = boneLengths[0] / 2f;
                    float normalizer = 0f;

                    for (int i = 0; i < bonesC; i++)
                    {
                        float weight = Mathf.InverseLerp(0f, reffVal + refLength * (spread), distToBone[i]);
                        float value = Mathf.Lerp(1f, 0f, weight);
                        if (i == 0) if (value == 0f) value = 1f;

                        weightForBone[i] = value;

                        normalizer += value;
                    }

                    debugDistWeights = new float[weightForBone.Length];
                    weightForBone.CopyTo(debugDistWeights, 0);

                    for (int i = 0; i < bonesC; i++) weightForBone[i] /= normalizer;

                    debugWeights = weightForBone;

                }

            }

            /// <summary>
            /// Returns average color value for weight idicator for this vertex
            /// </summary>
            public Color GetWeightColor()
            {
                Color lerped = GetBoneIndicatorColor(bonesIndexes[0], allMeshBonesCount, 1f, 1f);

                for (int i = 1; i < bones.Length; i++)
                {
                    lerped = Color.Lerp(lerped, GetBoneIndicatorColor(bonesIndexes[i], allMeshBonesCount, 1f, 1f), weights[i]);
                }

                return lerped;
            }
        }

        public static float DistributionIn(float k, float power)
        {
            return Mathf.Pow(k, power + 1f);
        }

        /// <summary>
        /// Drawing markers to be visible in editor window to help place bones correctly
        /// </summary>
        public void DrawMarkers(Transform[] markers)
        {
            if (markers == null) return;

            for (int i = 0; i < markers.Length; i++)
            {
                Gizmos.color = FColorMethods.ChangeColorAlpha(GetBoneIndicatorColor(i, markers.Length), GizmoAlpha);

                Vector3 targetPosition = markers[i].position;
                Vector3 previousPos = targetPosition;
                if (i > 0) previousPos = markers[i - 1].position;

                Gizmos.DrawWireSphere(targetPosition, GizmoSize);

                Gizmos.color = FColorMethods.ChangeColorAlpha(GetBoneIndicatorColor(i, markers.Length, 1f, 1f), GizmoAlpha * 0.8f);
                Gizmos.DrawSphere(targetPosition, GizmoSize * 0.7f);

                Gizmos.DrawRay(targetPosition, markers[i].up * GizmoSize * 1.1f);
                Gizmos.DrawRay(targetPosition, -markers[i].up * GizmoSize * 1.1f);
                Gizmos.DrawRay(targetPosition, markers[i].right * GizmoSize * 1.1f);
                Gizmos.DrawRay(targetPosition, -markers[i].right * GizmoSize * 1.1f);

                Vector3 targetPoint;
                if (i < markers.Length - 1) targetPoint = markers[i + 1].position;
                else
                    targetPoint = markers[i].position + (markers[i].position - markers[i - 1].position);

                Gizmos.DrawLine(targetPosition + markers[i].up * GizmoSize * 1.1f, targetPoint);
                Gizmos.DrawLine(targetPosition - markers[i].up * GizmoSize * 1.1f, targetPoint);
                Gizmos.DrawLine(targetPosition + markers[i].right * GizmoSize * 1.1f, targetPoint);
                Gizmos.DrawLine(targetPosition - markers[i].right * GizmoSize * 1.1f, targetPoint);

                previousPos = targetPosition;
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (!DebugMode) return;
            if (vertexDatas == null) return;
            if (vertexDatas.Length == 0) return;
            if (vertexDatas[0].bonesIndexes == null) return;

            for (int i = 0; i < vertexDatas.Length; i++)
            {
                Handles.color = GetBoneIndicatorColor(vertexDatas[i].bonesIndexes[0], vertexDatas[i].bones.Length) * new Color(1f, 1f, 1f, GizmoAlpha);
                Gizmos.color = Handles.color;

                Handles.Label(transform.TransformPoint(vertexDatas[i].position), "[" + i + "]");
                //Handles.Label(transform.TransformPoint(vertexDatas[i].position), "[" + i + "]\n" + Math.Round(vertexDatas[i].debugDists[0], 3) + "\n" + Math.Round(vertexDatas[i].debugDists[1], 3));
                Gizmos.DrawSphere(transform.TransformPoint(vertexDatas[i].position), 0.125f * GizmoSize);
            }
        }


        /// <summary>
        /// Returning helper color for bone
        /// </summary>
        public static Color GetBoneIndicatorColor(int boneIndex, int bonesCount, float s = 0.9f, float v = 0.9f)
        {
            float h = ((float)(boneIndex) * 1.125f) / bonesCount;
            h += 0.125f * boneIndex;
            h += 0.3f;
            h %= 1f;

            return Color.HSVToRGB(h, s, v);
        }
    }

    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FTail_Editor_Skinner))]
    public class FTail_Editor_SkinnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FTail_Editor_Skinner targetScript = (FTail_Editor_Skinner)target;
            DrawDefaultInspector();

            GUILayout.Space(10f);

            if (GUILayout.Button("Skin It")) targetScript.SkinMesh();
            if (GUILayout.Button("Skin and add Tail Animator")) targetScript.SkinMesh(true);
        }
    }

}

#endif
