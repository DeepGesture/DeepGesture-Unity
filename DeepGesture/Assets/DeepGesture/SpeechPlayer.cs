# if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using OpenHuman;

namespace DeepGesture {
    public class SpeechPlayer : MonoBehaviour {

        public AudioSpectrum AudioSpectrum;

        public GestureController Controller;

        public bool RealTime = true;

        public float Framerate = 60f;
        public float Pitch = 1f;

        public float Timestamp = 0f;

        void Update() {
            Utility.SetFPS(Mathf.RoundToInt(Framerate));
            if (AudioSpectrum == null) {
                Debug.Log("AudioSpectrum is null");
                return;
            }

            if (RealTime) {
                Timestamp = AudioSpectrum.GetTimestamp();
            }
            else {
                Timestamp += Pitch / Framerate;
                if (GetTimeDifference() > 0.25f) {
                    AudioSpectrum.PlayMusic(Timestamp, true);
                }
            }
            AudioSpectrum.PlayMusic(Timestamp, false);
            AudioSpectrum.ApplyPitch(Pitch);

            Controller.SendMessage("AnimateGesture", new object[] {
                AudioSpectrum,
                Timestamp,
                Pitch
            });
        }

        float GetTimeDifference() {
            if (AudioSpectrum == null) {
                return 0f;
            }
            return AudioSpectrum.GetTimestamp() - Timestamp;
        }

        public void SetTimestamp(float timestamp) {
            if (Timestamp != timestamp) {
                AudioSpectrum.PlayMusic(timestamp, true);
                Timestamp = timestamp;
            }
        }

        public void SetAudioSpectrum(AudioSpectrum spectrum) {
            if (AudioSpectrum != spectrum) {
                if (Application.isPlaying) {
                    AudioSpectrum.StopMusic();
                }
                AudioSpectrum = spectrum;
                if (Application.isPlaying) {
                    AudioSpectrum.PlayMusic(0f, false);
                    Timestamp = 0f;
                }
            }
        }

        [CustomEditor(typeof(SpeechPlayer))]
        public class SpeechPlayer_Editor : Editor {

            public SpeechPlayer Target;

            void Awake() {
                Target = (SpeechPlayer)target;
            }

            public override void OnInspectorGUI() {
                Target.SetAudioSpectrum(EditorGUILayout.ObjectField("Audio Spectrum", Target.AudioSpectrum, typeof(AudioSpectrum), true) as AudioSpectrum);
                Target.Controller = EditorGUILayout.ObjectField("Controller", Target.Controller, typeof(GestureController), true) as GestureController;
                Target.RealTime = EditorGUILayout.Toggle("Real Time", Target.RealTime);
                Target.Framerate = EditorGUILayout.FloatField("Framerate", Target.Framerate);
                Target.Pitch = EditorGUILayout.Slider("Pitch", Target.Pitch, 0.5f, 1.5f);
                if (Target.AudioSpectrum != null) {
                    Target.SetTimestamp(EditorGUILayout.Slider("Timestamp", Target.Timestamp, 0f, Target.AudioSpectrum.GetLength()));
                    EditorGUILayout.HelpBox("Time Difference: " + Target.GetTimeDifference(), MessageType.Info);
                }
            }

        }
    }

}

#endif