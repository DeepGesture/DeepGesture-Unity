# if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using OpenHuman;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DeepGesture {

    public class GesturePlayer : MonoBehaviour {

        public bool Mirror = false;
        private int[] BoneMapping = null;
        public float Framerate = 60f;
        public MotionAsset Asset;
        private Actor Actor = null;
        private float Timescale = 1f;
        private float Timestamp = 0f;

        public float Zoom = 1f;

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private EditorCoroutines.EditorCoroutine Coroutine = null;

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        void Start() {
            // Debug.Log("GesturePlayer Start");
            Actor = GetComponentInChildren<Actor>();
        }

        void OnDestroy() {
        }

        public bool IsPlaying() {
            return Coroutine != null;
        }

        public void InitializeDance(params object[] objects) {
            MotionAsset asset = (MotionAsset)objects[0];
            float timestamp = (float)objects[1];
            bool mirrored = (bool)objects[2];
            Matrix4x4 reference = asset.GetModule<RootModule>().GetRootTransformation(timestamp, mirrored);
            Matrix4x4 root = reference;
            Actor.GetRoot().transform.position = reference.GetPosition();
            Actor.GetRoot().transform.rotation = reference.GetRotation();
            Actor.SetBoneTransformations(asset.GetFrame(timestamp).GetBoneTransformations(Actor.GetBoneNames(), mirrored).TransformationsFromTo(reference, root, true));
            Actor.SetBoneVelocities(asset.GetFrame(timestamp).GetBoneVelocities(Actor.GetBoneNames(), mirrored).DirectionsFromTo(reference, root, true));
        }

        public void Update() {
            if (Application.isPlaying) {
                LoadFrame(Mathf.Repeat(Timestamp + Time.deltaTime, Asset.Frames.Last().Timestamp));
            }
        }

        public void Play(bool value) {
            if (value && IsPlaying()) {
                return;
            }
            if (!value && !IsPlaying()) {
                return;
            }

            if (value) {
                Coroutine = EditorCoroutines.StartCoroutine(OnPlay(), this);
            }
            else {
                EditorCoroutines.StopCoroutine(OnPlay(), this);
                Coroutine = null;
            }
        }

        private IEnumerator OnPlay() {
            DateTime previous = Utility.GetTimestamp();
            while (Asset != null) {
                float delta = Timescale * (float)Utility.GetElapsedTime(previous);
                if (Timescale == 0f || delta > 1f / Framerate) {
                    previous = Utility.GetTimestamp();
                    LoadFrame(Mathf.Repeat(Timestamp + delta, Asset.Frames.Last().Timestamp));
                }
                yield return new WaitForSeconds(0f);
            }
        }

        public Actor GetActor() {
            return GetComponentInChildren<Actor>();
        }

        public void OnPlayWithParam(object[] parameters) {
            float timestamp = (float)parameters[0];
            Debug.Log("OnPlayWithParam.timestamp: " + timestamp);
            LoadFrame(timestamp);
            // Vector3 rootPosition = Actor.transform.position;
            // BoneMapping = Asset.Source.GetBoneIndices(Actor.GetBoneNames());
            // //Apply posture on character
            // for (int i = 0; i < Actor.Bones.Length; i++) {
            //     if (BoneMapping[i] != -1) {
            //         Matrix4x4 boneMatrix = Asset.GetFrame(timestamp).GetBoneTransformation(BoneMapping[i], Mirror);
            //         boneMatrix.m03 = boneMatrix.m03 + rootPosition.x;
            //         boneMatrix.m13 = boneMatrix.m13 + rootPosition.y;
            //         boneMatrix.m23 = boneMatrix.m23 + rootPosition.z;

            //         Actor.Bones[i].SetTransformation(boneMatrix);
            //         Actor.Bones[i].SetVelocity(
            //             Asset.GetFrame(timestamp).GetBoneVelocity(BoneMapping[i], Mirror)
            //         );
            //         // Actor.Bones[i].SetAcceleration(
            //         // 	Asset.GetFrame(Editor.GetTimestamp()).GetBoneAcceleration(BoneMapping[i], Mirror)
            //         // );
            //     }
            // }
        }

        public void LoadFrame(float timestamp) {
            if (Asset == null) {
                return;
            }

            Timestamp = timestamp;
            LoadFrame();
        }

        public void LoadFrame() {
            Actor actor = GetActor();
            Vector3 rootPosition = actor.transform.position;
            BoneMapping = Asset.Source.GetBoneIndices(actor.GetBoneNames());

            // Apply posture on character
            for (int i = 0; i < actor.Bones.Length; i++) {
                if (BoneMapping[i] != -1) {
                    Matrix4x4 boneMatrix = Asset.GetFrame(GetTimestamp()).GetBoneTransformation(BoneMapping[i], Mirror);
                    boneMatrix.m03 = boneMatrix.m03 + rootPosition.x;
                    boneMatrix.m13 = boneMatrix.m13 + rootPosition.y;
                    boneMatrix.m23 = boneMatrix.m23 + rootPosition.z;

                    actor.Bones[i].SetTransformation(boneMatrix);
                    actor.Bones[i].SetVelocity(
                        Asset.GetFrame(GetTimestamp()).GetBoneVelocity(BoneMapping[i], Mirror)
                    );
                    // actor.Bones[i].SetAcceleration(
                    // 	Asset.GetFrame(Editor.GetTimestamp()).GetBoneAcceleration(BoneMapping[i], Mirror)
                    // );
                }
            }

            //Send callbacks to all modules
            // Asset.Callback(e);
        }

        public Frame GetCurrentFrame() {
            return Asset.GetFrame(GetTimestamp());
        }

        public float GetTimestamp() {
            return Timestamp;
        }

        void OnGUI() {
        }

        void OnRenderObject() {

        }

        // [CustomEditor(typeof(GesturePlayer))]
        // public class GesturePlayer_Editor : Editor {

        // }
    }



    [CustomEditor(typeof(GesturePlayer))]
    public class GesturePlayer_Editor : Editor {
        public GesturePlayer Target;

        void Awake() {
            // Timestamp = Utility.GetTimestamp();
            Target = (GesturePlayer)target;
        }

        public void OnEnable() {
            Target = (GesturePlayer)target;
        }

        public override void OnInspectorGUI() {
            Frame frame = Target.GetCurrentFrame();

            Target.Asset = EditorGUILayout.ObjectField("Motion Asset", Target.Asset, typeof(MotionAsset), true) as MotionAsset;
            Target.Mirror = EditorGUILayout.Toggle("Mirror", Target.Mirror);
            Target.Framerate = EditorGUILayout.FloatField("Framerate", Target.Framerate);

            using (new EditorGUILayout.VerticalScope("Box")) {
                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                if (Target.IsPlaying()) {
                    // || - Pause
                    if (Utility.GUIButton("⌧", Color.red, Color.white, 50f, 40f)) {
                        Target.Play(false);
                    }
                }
                else {
                    // |> - Play
                    if (Utility.GUIButton("▶", Color.green, Color.white, 50f, 40f)) {
                        Target.Play(true);
                    }
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                int index = EditorGUILayout.IntSlider(frame.Index, 1, Target.Asset.GetTotalFrames());
                if (index != frame.Index) {
                    Target.LoadFrame(Target.Asset.GetFrame(index).Timestamp);
                }
                Target.Zoom = EditorGUILayout.Slider(Target.Zoom, 0f, 1f);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

        }
    }
}

#endif