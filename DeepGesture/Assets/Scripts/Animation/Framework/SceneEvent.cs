#if UNITY_EDITOR
using UnityEngine;

namespace OpenHuman {
	public abstract class SceneEvent : MonoBehaviour {
		public abstract void Callback(MotionEditor editor);
	}
}

#endif