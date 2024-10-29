using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.Collections;

public class VideoRecorder : MonoBehaviour
{
    private RecorderController recorderController;

    // Replace with your audio clip
    public AudioClip audioClip;

    private void Start()
    {
        // Initialize the Recorder
        SetupRecorder();
    }

    private void SetupRecorder()
    {
        RecorderControllerSettings settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

        // Create a new Recorder Controller
        recorderController = new RecorderController(settings);

        // Set up recording settings
        MovieRecorderSettings recorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        
        // recorderSettings.SetCodec(MovieRecorderSettings.Codec.H264);
        // recorderSettings.SetFormat(MovieRecorderSettings.Format.MP4);
        // recorderSettings.SetOutputPath("Assets/Recordings");
        // recorderSettings.SetFileName("RecordedVideo");
        // recorderSettings.SetAudioSampleRate(44100); // Set sample rate for audio

        // // Create the recorder
        // var recorder = new MovieRecorder(); // recorderSettings

        // // Set the camera to capture from
        // recorder.AddInput(new CameraInput());
        // // recorderSettings, Camera.main

        // // Set up audio input
        // AudioInput audioInput = new AudioInput();
        // // audioInput.settings = settings;
        // // recorderSettings, audioClip
        // recorder.AddInput(audioInput);

        // // Add the recorder to the controller
        // recorderController.AddRecorder(recorder);
    }

    private void Update()
    {
        // Start recording when the R key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRecording();
        }

        // Stop recording when the S key is pressed
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (recorderController != null && !recorderController.IsRecording())
        {
            recorderController.PrepareRecording();
            recorderController.StartRecording();
            Debug.Log("Recording started");
        }
    }

    private void StopRecording()
    {
        if (recorderController != null && recorderController.IsRecording())
        {
            recorderController.StopRecording();
            Debug.Log("Recording stopped");
        }
    }
}
