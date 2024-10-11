using UnityEngine;

public class TimeSeries {

	public abstract class Component : TimeSeries {
		public bool DrawGUI = true;
		public bool DrawScene = true;
		public Component(TimeSeries global) : base(global) { }
		public abstract void Increment(int start, int end);
		public abstract void Interpolate(int start, int end);
		public abstract void GUI();
		public abstract void Draw();
	}

	public int PastKeys { get; private set; } // 6 key
	public int FutureKeys { get; private set; } // 6 key
	public float PastWindow { get; private set; } // 1f - 1 second
	public float FutureWindow { get; private set; } // 1f - 1 second
	public int Resolution { get; private set; } // 10
	public Sample[] Samples { get; private set; } // 121

	public int Pivot => PastSampleCount; // 60
	public int SampleCount => PastSampleCount + FutureSampleCount + 1; // 121
	public int PastSampleCount => PastKeys * Resolution; // 6 * 10 = 60
	public int FutureSampleCount => FutureKeys * Resolution; // 6 * 10 = 60

	public int PivotKey => PastKeys; // 6
	public int KeyCount => PastKeys + FutureKeys + 1; // 13
	public float Window => PastWindow + FutureWindow; // 2f - 2 seconds

	public float DeltaTime => Window / SampleCount; // 0.01652893

	public float MaximumFrequency => 0.5f * KeyCount / Window; //Shannon-Nyquist Sampling Theorem fMax <= 0.5*fSignal

	public class Sample {
		public int Index;
		public float Timestamp;
		public Sample(int index, float timestamp) {
			Index = index;
			Timestamp = timestamp;
		}
	}

	public TimeSeries(int pastKeys, int futureKeys, float pastWindow, float futureWindow, int resolution) {
		PastKeys = pastKeys;
		FutureKeys = futureKeys;
		PastWindow = pastWindow;
		FutureWindow = futureWindow;
		Resolution = resolution;
		Samples = new Sample[SampleCount]; // 121

		// 0 to 60 - [0, 59]
		for (int i = 0; i < Pivot; i++) {
			Samples[i] = new Sample(i, -PastWindow + i * PastWindow / PastSampleCount);
		}
		
		// 60
		Samples[Pivot] = new Sample(Pivot, 0f);

		// 61 to 121 - [61, 120]
		for (int i = Pivot + 1; i < Samples.Length; i++) {
			Samples[i] = new Sample(i, (i - Pivot) * FutureWindow / FutureSampleCount);
		}
	}

	protected TimeSeries(TimeSeries global) {
		SetTimeSeries(global);
	}

	public void SetTimeSeries(TimeSeries global) {
		PastKeys = global.PastKeys;
		FutureKeys = global.FutureKeys;
		PastWindow = global.FutureWindow;
		FutureWindow = global.FutureWindow;
		Resolution = global.Resolution;
		Samples = global.Samples;
	}

	public float[] GetTimestamps() {
		float[] timestamps = new float[Samples.Length];
		for (int i = 0; i < timestamps.Length; i++) {
			timestamps[i] = Samples[i].Timestamp;
		}
		return timestamps;
	}

	public float GetTemporalScale(float value) {
		// return value;
		return Window / KeyCount * value;
	}

	public Vector2 GetTemporalScale(Vector2 value) {
		// return value;
		return Window / KeyCount * value;
	}

	public Vector3 GetTemporalScale(Vector3 value) {
		// return value;
		return Window / KeyCount * value;
	}

	public Sample GetPivot() {
		return Samples[Pivot]; // 60
	}

	public Sample GetKey(int index) {
		if (index < 0 || index >= KeyCount) {
			Debug.Log("Given key was " + index + " but must be within 0 and " + (KeyCount - 1) + ".");
			return null;
		}
		return Samples[index * Resolution]; // 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120
	}

	public Sample GetPreviousKey(int sample) {
		if (sample < 0 || sample >= Samples.Length) {
			Debug.Log("Given index was " + sample + " but must be within 0 and " + (Samples.Length - 1) + ".");
			return null;
		}
		return GetKey(sample / Resolution); // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 11, 12
	}

	public Sample GetNextKey(int sample) {
		if (sample < 0 || sample >= Samples.Length) {
			Debug.Log("Given index was " + sample + " but must be within 0 and " + (Samples.Length - 1) + ".");
			return null;
		}
		if (sample % Resolution == 0) {
			return GetKey(sample / Resolution); // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
		}
		else {
			return GetKey(sample / Resolution + 1); // 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
		}
	}

	public float GetControl(int index, float bias, float min = 0f, float max = 1f) {
		return index.Ratio(Pivot, Samples.Length - 1).ActivateCurve(bias, min, max);
	}

	public float GetCorrection(int index, float bias, float max = 1f, float min = 0f) {
		return index.Ratio(Pivot, Samples.Length - 1).ActivateCurve(bias, max, min);
	}
}