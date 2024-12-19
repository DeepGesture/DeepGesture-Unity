# if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using OpenHuman;

namespace DeepGesture {
    public class GestureSpeechPlayer : MonoBehaviour {

        // public AudioSpectrum AudioSpectrum;
        public AudioClip AudioClip;
        public AudioSource AudioSource;

        public float AudioPosition = 0f;

        public bool RealTime = true;
        private EditorCoroutines.EditorCoroutine Coroutine = null;

        public float Framerate = 60f;
        public float Pitch = 1f;

        public float Timestamp = 0f;

        public void Update() {
            // if (AudioSource != null && AudioSource.isPlaying) {
            //     Debug.Log("sdfsdf");
            //     AudioPosition = AudioSource.time; // Sync with AudioSource's playback time
            // }
            // Utility.SetFPS(Mathf.RoundToInt(Framerate));
            // if (AudioSpectrum == null) {
            //     Debug.Log("AudioSpectrum is null");
            //     return;
            // }

            // if (RealTime) {
            //     Timestamp = AudioSpectrum.GetTimestamp();
            // }
            // else {
            //     Timestamp += Pitch / Framerate;
            //     if (GetTimeDifference() > 0.25f) {
            //         AudioSpectrum.PlayMusic(Timestamp, true);
            //     }
            // }
            // AudioSpectrum.PlayMusic(Timestamp, false);
            // AudioSpectrum.ApplyPitch(Pitch);

            // GesturePredict.SendMessage("PlayAnimation", new object[] {});

        }

        public void UpdateAudioPosition(float newPosition) {
            if (AudioSource != null) {
                AudioSource.time = newPosition;
            }
        }

        public void PlayAnimation() {
            if (AudioSource != null) {
                AudioSource.time = Timestamp;
                AudioSource.Play();
            }
            else {
                AudioSource = gameObject.AddComponent<AudioSource>();
                AudioSource.clip = AudioClip;
                AudioSource.time = Timestamp;
                AudioSource.Play();
            }

            GesturePlayer[] gestures = GetComponentsInChildren<GesturePlayer>();
            // GesturePlayer[] gestures = FindObjectsOfType<GesturePlayer>();
            foreach (GesturePlayer gesture in gestures) {
                gesture.PlayAnimation();
            }
        }

        public void StopAnimation() {
            if (AudioSource != null) {
                AudioSource.Stop();
            }
            GesturePlayer[] gestures = GetComponentsInChildren<GesturePlayer>();
            // GesturePlayer[] gestures = FindObjectsOfType<GesturePlayer>();
            foreach (GesturePlayer gesture in gestures) {
                gesture.StopAnimation();
            }
        }

        public bool IsPlaying() {
            if (AudioSource == null) {
                return false;
            }

            return AudioSource.isPlaying;
        }

        // public void Play(bool value) {
        //     if (value && IsPlaying()) {
        //         return;
        //     }
        //     if (!value && !IsPlaying()) {
        //         return;
        //     }

        //     if (value) {
        //         if (AudioSource != null) {
        //             AudioSource.time = Timestamp;
        //             AudioSource.Play();
        //         }
        //         else {
        //             AudioSource = gameObject.AddComponent<AudioSource>();
        //             AudioSource.clip = AudioClip;
        //             AudioSource.time = Timestamp;
        //             AudioSource.Play();
        //         }
        //         // Coroutine = EditorCoroutines.StartCoroutine(OnPlay(), this);
        //     }
        //     else {
        //         if (AudioSource != null) {
        //             AudioSource.Stop();
        //         }
        //         // EditorCoroutines.StopCoroutine(OnPlay(), this);
        //         Coroutine = null;
        //     }
        // }

        // float GetTimeDifference() {
        //     if (AudioSpectrum == null) {
        //         return 0f;
        //     }
        //     return AudioSpectrum.GetTimestamp() - Timestamp;
        // }

        // public void SetTimestamp(float timestamp) {
        //     if (Timestamp != timestamp) {
        //         AudioSpectrum.PlayMusic(timestamp, true);
        //         Timestamp = timestamp;
        //     }
        // }

        // public void SetAudioSpectrum(AudioSpectrum spectrum) {
        //     if (AudioSpectrum != spectrum) {
        //         if (Application.isPlaying) {
        //             AudioSpectrum.StopMusic();
        //         }
        //         AudioSpectrum = spectrum;
        //         if (Application.isPlaying) {
        //             AudioSpectrum.PlayMusic(0f, false);
        //             Timestamp = 0f;
        //         }
        //     }
        // }

        [CustomEditor(typeof(GestureSpeechPlayer))]
        public class GestureSpeechPlayer_Editor : Editor {

            public GestureSpeechPlayer Target;

            void Awake() {
                Target = (GestureSpeechPlayer)target;
            }

            public override void OnInspectorGUI() {
                // Target.SetAudioSpectrum(EditorGUILayout.ObjectField("Audio Spectrum", Target.AudioSpectrum, typeof(AudioSpectrum), true) as AudioSpectrum);
                Target.RealTime = EditorGUILayout.Toggle("Real Time", Target.RealTime);
                Target.AudioClip = EditorGUILayout.ObjectField("Audio Clip", Target.AudioClip, typeof(AudioClip), true) as AudioClip;

                // public AudioClip AudioClip;
                Target.Framerate = EditorGUILayout.FloatField("Framerate", Target.Framerate);
                // Target.Pitch = EditorGUILayout.Slider("Pitch", Target.Pitch, 0.5f, 1.5f);
                // if (Target.AudioSpectrum != null) {
                //     Target.SetTimestamp(EditorGUILayout.Slider("Timestamp", Target.Timestamp, 0f, Target.AudioSpectrum.GetLength()));
                //     EditorGUILayout.HelpBox("Time Difference: " + Target.GetTimeDifference(), MessageType.Info);
                // }


                using (new EditorGUILayout.VerticalScope("Box")) {
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    if (Target.IsPlaying()) {
                        // || - Pause
                        if (Utility.GUIButton("⌧", Color.red, Color.white, 50f, 40f)) {
                            Target.StopAnimation();
                        }
                    }
                    else {
                        // |> - Play
                        if (Utility.GUIButton("▶", Color.green, Color.white, 50f, 40f)) {
                            Target.PlayAnimation();
                        }
                    }

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginVertical();

                    // Target.AudioPosition = EditorGUILayout.Slider("Audio Position", Target.AudioPosition, 0f, Target.AudioSource.clip.length);

                    // if (Target.AudioSource != null) {
                    //     // Add a slider to control the audio position
                    //     float newTime = EditorGUILayout.Slider("Audio Position", Target.audioPosition, 0f, Target.AudioSource.clip.length);

                    //     // If the position has changed, update the audio
                    //     if (Mathf.Abs(newTime - Target.audioPosition) > 0.01f) {
                    //         Target.audioPosition = newTime;
                    //         Target.UpdateAudioPosition(newTime);
                    //     }
                    // }
                    // else {
                    //     EditorGUILayout.HelpBox("No AudioSource assigned!", MessageType.Warning);
                    // }

                    // Frame frame = Target.GetCurrentFrame();
                    // int index = EditorGUILayout.IntSlider(frame.Index, 1, Target.Asset.GetTotalFrames());
                    // if (index != frame.Index) {
                    //     Target.LoadFrame(Target.Asset.GetFrame(index).Timestamp);
                    // }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                }

            }

        }
    }

}

#endif