# if UNITY_EDITOR
using UnityEngine;
using OpenHuman;
using UltimateIK;
using Unity.Barracuda;

namespace DeepGesture {

    public class GroundTruthController : MonoBehaviour {
        public MotionAsset Asset;
        [Range(0f, 5f)] public float Timescale = 0f;
        public float Framerate = 60f;
        public bool Mirror = false;
        private int[] BoneMapping = null;

        public bool DrawGUI = true;
        public bool DrawDebug = true;
        public bool DrawAudio = false;
        public bool DrawNoise = false;

        public int Channels = 8;
        public int NoiseSamples = 8;
        [Range(0f, 5f)] public float NoiseStrength = 0f;
        [Range(0f, 5f)] public float NoiseReset = 1f;
        [Range(0f, 1f)] public float MinAmplitude = 0f;

        public float FootSafetyDistance = 0f;

        [Range(0f, 1f)] public float PhaseStability = 0.5f;

        public bool Postprocessing = true;
        public float ContactPower = 3f;
        public float ContactThreshold = 2f / 3f;

        public Actor Actor = null;

        private TimeSeries GestureSeries;
        private TimeSeries SpeechSeries;

        private RootModule.Series RootSeries;
        private ContactModule.Series ContactSeries;
        private DeepPhaseModule.Series PhaseSeries;
        private AudioSpectrumModule.Series SpectrumSeries;

        private IK LeftFootIK;
        private IK RightFootIK;

        private float[] NoiseA = null;
        private float[] NoiseB = null;
        private float[] Noise = null;
        private float NoiseTimer = 1f;

        public RootModule.Series GetRootSeries() {
            return RootSeries;
        }

        void Start() {
            // Debug.Log("GestureController Start");
            Actor = GetComponentInChildren<Actor>();
            // Actor = GetComponent<Actor>();

            GestureSeries = new TimeSeries(6, 6, 1f, 1f, 1);
            SpeechSeries = new TimeSeries(6, 6, 1f, 1f, 1);
            // SpeechSeries = new TimeSeries(20, 20, 1f, 1f, 3);

            RootSeries = new RootModule.Series(GestureSeries, transform);
            ContactSeries = new ContactModule.Series(GestureSeries, "LeftFoot", "RightFoot");
            PhaseSeries = new DeepPhaseModule.Series(GestureSeries, Channels);
            SpectrumSeries = new AudioSpectrumModule.Series(SpeechSeries);

            LeftFootIK = IK.Create(Actor.FindTransform("LeftUpLeg"), Actor.GetBoneTransforms("LeftToeBase"));
            RightFootIK = IK.Create(Actor.FindTransform("RightUpLeg"), Actor.GetBoneTransforms("RightToeBase"));

            NoiseA = new float[NoiseSamples];
            NoiseB = new float[NoiseSamples];
            Noise = new float[NoiseSamples];

            RootSeries.DrawGUI = DrawGUI;
            ContactSeries.DrawGUI = DrawGUI;
            PhaseSeries.DrawGUI = DrawGUI;
            SpectrumSeries.DrawGUI = DrawGUI;
            RootSeries.DrawScene = DrawDebug;
            ContactSeries.DrawScene = DrawDebug;
            PhaseSeries.DrawScene = DrawDebug;
            SpectrumSeries.DrawScene = DrawDebug;

        }

        void OnDestroy() {
        }

        string GetTag() {
            return Channels + "Channels";
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
            RootSeries = asset.GetModule<RootModule>().ExtractSeries(GestureSeries, timestamp, mirrored) as RootModule.Series;
            RootSeries.TransformFromTo(reference, root);

            ContactSeries = asset.GetModule<ContactModule>().ExtractSeries(GestureSeries, timestamp, mirrored) as ContactModule.Series;
            PhaseSeries = asset.GetModule<DeepPhaseModule>(GetTag()).ExtractSeries(GestureSeries, timestamp, mirrored) as DeepPhaseModule.Series;
            SpectrumSeries = asset.GetModule<AudioSpectrumModule>().ExtractSeries(SpeechSeries, timestamp, mirrored) as AudioSpectrumModule.Series;
            foreach (Objective o in LeftFootIK.Objectives) {
                o.TargetPosition = LeftFootIK.Joints[o.Joint].Transform.position;
                o.TargetRotation = LeftFootIK.Joints[o.Joint].Transform.rotation;
            }
            foreach (Objective o in RightFootIK.Objectives) {
                o.TargetPosition = RightFootIK.Joints[o.Joint].Transform.position;
                o.TargetRotation = RightFootIK.Joints[o.Joint].Transform.rotation;
            }
        }

        public void AnimateGroundTruth(object[] parameters) {
            // SpeechControl(parameters);
            // Feed();
            // Read();
            PlayAnimation(parameters);
        }

        public void PlayAnimation(object[] parameters) {
            float timestamp = (float)parameters[1];
            Actor.DrawRoot = true;
            Vector3 rootPosition = Actor.transform.position;
            // Actor.transform
            BoneMapping = Asset.Source.GetBoneIndices(Actor.GetBoneNames());
            //Apply posture on character
            for (int i = 0; i < Actor.Bones.Length; i++) {
                if (BoneMapping[i] != -1) {
                    Matrix4x4 boneMatrix = Asset.GetFrame(timestamp).GetBoneTransformation(BoneMapping[i], Mirror);
                    Matrix4x4 rootMatrix = Matrix4x4.TRS(rootPosition, Quaternion.identity, Vector3.zero);
                    // boneMatrix.TransformationFrom(rootMatrix);


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

            //Apply scene changes
            // foreach (GameObject instance in Asset.GetScene().GetRootGameObjects()) {
            //     instance.transform.localScale = Vector3.one.GetMirror(Mirror ? Asset.MirrorAxis : Axis.None);
            //     foreach (SceneEvent e in instance.GetComponentsInChildren<SceneEvent>(true)) {
            //         e.Callback(Editor);
            //     }
            // }

            //Send callbacks to all modules
            // Asset.Callback(Editor);
        }

        private void SpeechControl(object[] parameters) {
            AudioSpectrum spectrum = (AudioSpectrum)parameters[0];
            float timestamp = (float)parameters[1];
            float pitch = (float)parameters[2];

            // Set Speech
            SpectrumSeries.Increment(0, GestureSeries.Pivot);
            for (int i = 0; i < SpectrumSeries.Samples.Length; i++) {
                if (SpectrumSeries.Values[i] == null) {
                    SpectrumSeries.Values[i] = spectrum.GetFiltered(timestamp + pitch * SpectrumSeries.Samples[i].Timestamp, SpeechSeries.MaximumFrequency);
                }
            }
            for (int i = SpectrumSeries.Pivot; i < SpectrumSeries.Samples.Length; i++) {
                SpectrumSeries.Values[i] = spectrum.GetFiltered(timestamp + pitch * SpectrumSeries.Samples[i].Timestamp, SpeechSeries.MaximumFrequency);
            }

            // Noise Sampler
            // HandleNoise();
        }


        private float GetLerp() {
            float min = 0f;
            float max = NoiseReset;
            float lerp = Mathf.Clamp(NoiseTimer, min, max).Normalize(min, max, 1f, 0f);
            return lerp;
        }


        private void Read() {
            // Update Past States
            RootSeries.Increment(0, GestureSeries.Pivot);
            ContactSeries.Increment(0, GestureSeries.Pivot);
            PhaseSeries.Increment(0, GestureSeries.Pivot);

            // // Update Root State
            // Vector3 offset = NeuralNetwork.ReadVector3();

            // Matrix4x4 reference = Actor.GetRoot().GetWorldMatrix();
            // Matrix4x4 root = reference * Matrix4x4.TRS(new Vector3(offset.x, 0f, offset.z), Quaternion.AngleAxis(offset.y, Vector3.up), Vector3.one);
            // RootSeries.Transformations[GestureSeries.Pivot] = root;
            // RootSeries.Velocities[GestureSeries.Pivot] = NeuralNetwork.ReadXZ().DirectionFrom(root);

            // // Read Future States
            // for (int i = GestureSeries.PivotKey + 1; i < GestureSeries.KeyCount; i++) {
            //     int index = GestureSeries.GetKey(i).Index;
            //     RootSeries.Transformations[index] = Utility.Interpolate(
            //         RootSeries.Transformations[index],
            //         Matrix4x4.TRS(NeuralNetwork.ReadXZ().PositionFrom(reference), Quaternion.LookRotation(NeuralNetwork.ReadXZ().DirectionFrom(reference).normalized, Vector3.up), Vector3.one),
            //         1f,
            //         1f
            //     );
            //     RootSeries.Velocities[index] = NeuralNetwork.ReadXZ().DirectionFrom(reference);
            // }

            // // Read Posture
            // Vector3[] positions = new Vector3[Actor.Bones.Length];
            // Vector3[] forwards = new Vector3[Actor.Bones.Length];
            // Vector3[] upwards = new Vector3[Actor.Bones.Length];
            // Vector3[] velocities = new Vector3[Actor.Bones.Length];
            // for (int i = 0; i < Actor.Bones.Length; i++) {
            //     Vector3 position = NeuralNetwork.ReadVector3().PositionFrom(root);
            //     Vector3 forward = NeuralNetwork.ReadVector3().normalized.DirectionFrom(root);
            //     Vector3 upward = NeuralNetwork.ReadVector3().normalized.DirectionFrom(root);
            //     Vector3 velocity = NeuralNetwork.ReadVector3().DirectionFrom(root);
            //     velocities[i] = velocity;
            //     positions[i] = Vector3.Lerp(Actor.Bones[i].GetTransform().position + velocity / Framerate, position, 0.5f);
            //     forwards[i] = forward;
            //     upwards[i] = upward;
            // }

            // // Update Contacts
            // float[] contacts = NeuralNetwork.Read(ContactSeries.Bones.Length, 0f, 1f);
            // for (int i = 0; i < ContactSeries.Bones.Length; i++) {
            //     ContactSeries.Values[GestureSeries.Pivot][i] = contacts[i].SmoothStep(ContactPower, ContactThreshold);
            // }

            // // Update Phases
            // PhaseSeries.UpdateAlignment(NeuralNetwork.Read((1 + PhaseSeries.FutureKeys) * PhaseSeries.Channels * 4), PhaseStability, 1f / Framerate, MinAmplitude);

            // // Interpolate Timeseries
            // RootSeries.Interpolate(GestureSeries.Pivot, GestureSeries.Samples.Length);
            // PhaseSeries.Interpolate(GestureSeries.Pivot, GestureSeries.Samples.Length);

            // // Assign Posture
            // transform.position = RootSeries.GetPosition(GestureSeries.Pivot);
            // transform.rotation = RootSeries.GetRotation(GestureSeries.Pivot);
            // for (int i = 0; i < Actor.Bones.Length; i++) {
            //     Actor.Bones[i].SetVelocity(velocities[i]);
            //     Actor.Bones[i].SetPosition(positions[i]);
            //     Actor.Bones[i].SetRotation(Quaternion.LookRotation(forwards[i], upwards[i]));
            // }

            // // Correct Twist
            // Actor.RestoreAlignment();

            // // Process Contact States
            // ProcessFootIK(LeftFootIK, ContactSeries.Values[GestureSeries.Pivot][0]);
            // ProcessFootIK(RightFootIK, ContactSeries.Values[GestureSeries.Pivot][1]);

            // PhaseSeries.AddGatingHistory(NeuralNetwork.GetOutput("W").AsFloats());
        }

        private void ProcessFootIK(IK ik, float contact) {
            if (!Postprocessing) {
                return;
            }
            ik.Activation = UltimateIK.ACTIVATION.Linear;
            for (int i = 0; i < ik.Objectives.Length; i++) {
                Vector3 self = ik.Joints[ik.Objectives[i].Joint].Transform.position;
                Vector3 other = ik == LeftFootIK ? RightFootIK.Joints.Last().Transform.position : LeftFootIK.Joints.Last().Transform.position;
                // Vector3 target = other + Mathf.Max(FootSafetyDistance, Vector3.Distance(self, other)) * (self-other).normalized;
                Vector3 target = Vector3.Lerp(self, other + Mathf.Max(FootSafetyDistance, Vector3.Distance(self, other)) * (self - other).normalized, 1f - contact);
                ik.Objectives[i].SetTarget(Vector3.Lerp(ik.Objectives[i].TargetPosition, target, 1f - contact));
                ik.Objectives[i].SetTarget(ik.Joints[ik.Objectives[i].Joint].Transform.rotation);
            }
            ik.Iterations = 25;
            ik.Solve();
        }

        void OnGUI() {
            // RootSeries.DrawGUI = DrawGUI;
            // RootSeries.GUI();

            // ContactSeries.DrawGUI = DrawGUI;
            // ContactSeries.GUI();

            // PhaseSeries.DrawGUI = DrawGUI;
            // PhaseSeries.GUI();

            // if (DrawAudio) {
            //     SpectrumSeries.Draw();
            // }
        }

        void OnRenderObject() {
            // RootSeries.DrawScene = DrawDebug;
            // RootSeries.Draw();

            // ContactSeries.DrawScene = DrawDebug;
            // ContactSeries.Draw();

            // PhaseSeries.DrawScene = DrawDebug;
            // PhaseSeries.Draw();

            // UltiDraw.Begin();
            // List<float> values = new List<float>();
            // for(int i=PhaseSeries.Pivot; i<PhaseSeries.Samples.Length; i++) {
            //     float ratio = i.Ratio(PhaseSeries.Pivot-1, PhaseSeries.Samples.Length-1);
            //     float blend = ratio.SmoothStep(2f, 1f-TransitionBlend);
            //     values.Add(blend);
            // }
            // UltiDraw.PlotFunction(new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0.25f), values.ToArray(), 0f, 1f);
            // UltiDraw.End();

            // if (DrawNoise) {
            //     UltiDraw.Begin();
            //     UltiDraw.PlotBars(new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), Noise);
            //     UltiDraw.End();
            // }

            // if (DrawAudio) {
            //     SpectrumSeries.Draw();
            // }
        }
    }
}

#endif