# if UNITY_EDITOR
using UnityEngine;
using OpenHuman;
using UltimateIK;
using Unity.Barracuda;

namespace DeepGesture {

    public class GesturePlayer : MonoBehaviour {

        public bool Mirror = false;
        private int[] BoneMapping = null;
        public float Framerate = 60f;
        public MotionAsset Asset;
        private float Timestamp = 0f;
        private Actor Actor = null;

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        void Start() {
            Debug.Log("GesturePlayer Start");
            Actor = GetComponent<Actor>();

        }

        void OnDestroy() {
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

        public void AnimateGesture(object[] parameters) {
        }

        public void Update() {
            // AudioSpectrum.PlayMusic(Timestamp, true);
        }

        public void OnPlayWithParam(object[] parameters) {
            float timestamp = (float)parameters[1];
            // Actor.DrawRoot = true;
            Vector3 rootPosition = Actor.transform.position;
            // Actor.transform
            BoneMapping = Asset.Source.GetBoneIndices(Actor.GetBoneNames());
            //Apply posture on character
            for (int i = 0; i < Actor.Bones.Length; i++) {
                if (BoneMapping[i] != -1) {
                    Matrix4x4 boneMatrix = Asset.GetFrame(timestamp).GetBoneTransformation(BoneMapping[i], Mirror);
                    Matrix4x4 rootMatrix = Matrix4x4.TRS(rootPosition, Quaternion.identity, Vector3.zero);
                    boneMatrix.TransformationFrom(rootMatrix);
                    Actor.Bones[i].SetTransformation(
                        boneMatrix
                    );
                    Actor.Bones[i].SetVelocity(
                        Asset.GetFrame(timestamp).GetBoneVelocity(BoneMapping[i], Mirror)
                    );
                    // Actor.Bones[i].SetAcceleration(
                    // 	Asset.GetFrame(Editor.GetTimestamp()).GetBoneAcceleration(BoneMapping[i], Mirror)
                    // );
                }
            }
        }


        void OnGUI() {
        }

        void OnRenderObject() {

        }

        // [CustomEditor(typeof(GesturePlayer))]
        // public class GesturePlayer_Editor : Editor {

        // }
    }


}

#endif