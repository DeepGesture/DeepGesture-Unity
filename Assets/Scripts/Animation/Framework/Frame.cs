﻿#if UNITY_EDITOR
using UnityEngine;

namespace AI4Animation {
	[System.Serializable]
	public class Frame {
		public MotionAsset Asset;
		public int Index;
		public float Timestamp;
		public Matrix4x4[] Transformations;

		public Frame(MotionAsset asset, int index, float timestamp, Matrix4x4[] matrices) {
			Asset = asset;
			Index = index;
			Timestamp = timestamp;
			Transformations = (Matrix4x4[])matrices.Clone();
		}

		public Frame GetFirstFrame() {
			return Asset.Frames[0];
		}

		public Frame GetLastFrame() {
			return Asset.Frames[Asset.Frames.Length-1];
		}

		public Matrix4x4 GetSmoothedBoneTransformation(string bone, bool mirrored, int padding) {
			Frame[] frames = Asset.GetFrames(Index-padding, Index+padding);
			Matrix4x4[] matrices = new Matrix4x4[frames.Length];
			for(int i=0; i<matrices.Length; i++) {
				matrices[i] = frames[i].GetBoneTransformation(bone, mirrored);
			}
			return Matrix4x4.TRS(
				matrices.GetPositions().Gaussian(),
				matrices.GetRotations().Gaussian(),
				Vector3.one
			);
		}

		public Matrix4x4[] GetBoneTransformations(bool mirrored) {
			Matrix4x4[] values = new Matrix4x4[Transformations.Length];
			for(int i=0; i<Transformations.Length; i++) {
				values[i] = GetBoneTransformation(i, mirrored);
			}
			return values;
		}

		public Matrix4x4[] GetBoneTransformations(string[] bones, bool mirrored) {
			Matrix4x4[] values = new Matrix4x4[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneTransformation(bones[i], mirrored);
			}
			return values;
		}

		public Matrix4x4[] GetBoneTransformations(int[] bones, bool mirrored) {
			Matrix4x4[] values = new Matrix4x4[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneTransformation(bones[i], mirrored);
			}
			return values;
		}

		public Matrix4x4 GetBoneTransformation(string bone, bool mirrored, bool local=false) {
			return GetBoneTransformation(Asset.Source.FindBone(bone).Index, mirrored, local);
		}

		public Matrix4x4 GetBoneTransformation(int index, bool mirrored, bool local=false) {
			Matrix4x4 m = mirrored ? Transformations[Asset.Symmetry[index]].GetMirror(Asset.MirrorAxis) : Transformations[index];
			if(Asset.Source.Bones[index].Alignment != Vector3.zero) {
				Matrix4x4 update =  mirrored ? Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Asset.Source.Bones[Asset.Symmetry[index]].Alignment), Vector3.one).GetMirror(Asset.MirrorAxis) : Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Asset.Source.Bones[index].Alignment), Vector3.one);
				m *= update;
			}
			if(mirrored && Asset.Source.Bones[index].Correction != Vector3.zero) {
				m *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Asset.Source.Bones[index].Correction), Vector3.one);
			}
			if(Asset.Translation != Vector3.zero || Asset.Rotation != Vector3.zero || Asset.Scale != 1f) {
				Matrix4x4 update = Matrix4x4.TRS(
					mirrored ? Asset.Translation.GetMirror(Asset.MirrorAxis) : Asset.Translation,
					mirrored ? Quaternion.Euler(Asset.Rotation).GetMirror(Asset.MirrorAxis) : Quaternion.Euler(Asset.Rotation),
					Asset.Scale * Vector3.one
				);
				m = update * m;
			}
			if(local) {
				return Asset.Source.Bones[index].Parent == -1 ? m : m.TransformationTo(GetBoneTransformation(Asset.Source.Bones[index].Parent, mirrored));
			} else {
				return m;
			}
		}

		public Vector3[] GetBoneVelocities(bool mirrored) {
			Vector3[] values = new Vector3[Asset.Source.Bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneVelocity(i, mirrored);
			}
			return values;
		}

		public Vector3[] GetBoneVelocities(string[] bones, bool mirrored) {
			Vector3[] values = new Vector3[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneVelocity(bones[i], mirrored);
			}
			return values;
		}

		public Vector3[] GetBoneVelocities(int[] bones, bool mirrored) {
			Vector3[] values = new Vector3[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneVelocity(bones[i], mirrored);
			}
			return values;
		}

		public Vector3 GetBoneVelocity(string bone, bool mirrored) {
			return GetBoneVelocity(Asset.Source.FindBone(bone).Index, mirrored);
		}

		public Vector3 GetBoneVelocity(int index, bool mirrored) {
			if(Timestamp - Asset.GetDeltaTime() < 0f) {
				return (Asset.GetFrame(Timestamp + Asset.GetDeltaTime()).GetBoneTransformation(index, mirrored).GetPosition() - GetBoneTransformation(index, mirrored).GetPosition()) / Asset.GetDeltaTime();
			} else {
				return (GetBoneTransformation(index, mirrored).GetPosition() - Asset.GetFrame(Timestamp - Asset.GetDeltaTime()).GetBoneTransformation(index, mirrored).GetPosition()) / Asset.GetDeltaTime();
			}
		}

		public float[] GetAngularVelocities(bool mirrored) {
			float[] values = new float[Asset.Source.Bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetAngularVelocity(i, mirrored);
			}
			return values;
		}

		public float[] GetAngularVelocities(string[] bones, bool mirrored) {
			float[] values = new float[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetAngularVelocity(bones[i], mirrored);
			}
			return values;
		}

		public float[] GetAngularVelocities(int[] bones, bool mirrored) {
			float[] values = new float[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetAngularVelocity(bones[i], mirrored);
			}
			return values;
		}

		public float GetAngularVelocity(string bone, bool mirrored) {
			return GetAngularVelocity(Asset.Source.FindBone(bone).Index, mirrored);
		}

		public float GetAngularVelocity(int index, bool mirrored) {
			if(Timestamp - Asset.GetDeltaTime() < 0f) {
				return Quaternion.Angle(GetBoneTransformation(index, mirrored).GetRotation(), Asset.GetFrame(Timestamp + Asset.GetDeltaTime()).GetBoneTransformation(index, mirrored).GetRotation()) / Asset.GetDeltaTime();
			} else {
				return Quaternion.Angle(Asset.GetFrame(Timestamp - Asset.GetDeltaTime()).GetBoneTransformation(index, mirrored).GetRotation(), GetBoneTransformation(index, mirrored).GetRotation()) / Asset.GetDeltaTime();
			}
		}

		public Vector3[] GetBoneAccelerations(bool mirrored) {
			Vector3[] values = new Vector3[Asset.Source.Bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneVelocity(i, mirrored);
			}
			return values;
		}

		public Vector3[] GetBoneAccelerations(string[] bones, bool mirrored) {
			Vector3[] values = new Vector3[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneAcceleration(bones[i], mirrored);
			}
			return values;
		}

		public Vector3[] GetBoneAccelerations(int[] bones, bool mirrored) {
			Vector3[] values = new Vector3[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneAcceleration(bones[i], mirrored);
			}
			return values;
		}

		public Vector3 GetBoneAcceleration(string bone, bool mirrored) {
			return GetBoneAcceleration(Asset.Source.FindBone(bone).Index, mirrored);
		}

		public Vector3 GetBoneAcceleration(int index, bool mirrored) {
			if(Timestamp - Asset.GetDeltaTime() < 0f) {
				return (Asset.GetFrame(Timestamp + Asset.GetDeltaTime()).GetBoneVelocity(index, mirrored) - GetBoneVelocity(index, mirrored)) / Asset.GetDeltaTime();
			} else {
				return (GetBoneVelocity(index, mirrored) - Asset.GetFrame(Timestamp - Asset.GetDeltaTime()).GetBoneVelocity(index, mirrored)) / Asset.GetDeltaTime();
			}
		}

		public Vector3[] GetBoneJerks(bool mirrored) {
			Vector3[] values = new Vector3[Asset.Source.Bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneVelocity(i, mirrored);
			}
			return values;
		}

		public Vector3[] GetBoneJerks(string[] bones, bool mirrored) {
			Vector3[] values = new Vector3[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneJerk(bones[i], mirrored);
			}
			return values;
		}

		public Vector3[] GetBoneJerks(int[] bones, bool mirrored) {
			Vector3[] values = new Vector3[bones.Length];
			for(int i=0; i<values.Length; i++) {
				values[i] = GetBoneJerk(bones[i], mirrored);
			}
			return values;
		}

		public Vector3 GetBoneJerk(string bone, bool mirrored) {
			return GetBoneJerk(Asset.Source.FindBone(bone).Index, mirrored);
		}

		public Vector3 GetBoneJerk(int index, bool mirrored) {
			if(Timestamp - Asset.GetDeltaTime() < 0f) {
				return (Asset.GetFrame(Timestamp + Asset.GetDeltaTime()).GetBoneVelocity(index, mirrored) - GetBoneVelocity(index, mirrored)) / Asset.GetDeltaTime();
			} else {
				return (GetBoneVelocity(index, mirrored) - Asset.GetFrame(Timestamp - Asset.GetDeltaTime()).GetBoneVelocity(index, mirrored)) / Asset.GetDeltaTime();
			}
		}
	}
}
#endif