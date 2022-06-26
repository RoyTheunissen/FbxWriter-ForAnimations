using System;
using System.Collections.Generic;
using System.Reflection;
using Fbx.Data.Animation;
using Fbx.PropertyBlocks;

namespace Fbx.Data
{
	/// <summary>
	/// Represents a single joint. The intended workflow is that you create a joint hierarchy and animation data,
	/// add it to an FbxTemplate, then write that to an FBX. This allows you to work on a simple and intuitive level
	/// which is translated to the complicated FBX syntax under the hood. 
	/// </summary>
	public class Joint : IAnimatablePropertyOwner
	{
		private string name;
		public string Name => name;

		private FbxId id;
		public FbxId Id => id;

		private FbxId attributesNodeId;
		public FbxId AttributesNodeId => attributesNodeId;

		private AnimatableProperty<short> filmboxTypeID;
		public AnimatableProperty<short> FilmboxTypeId => filmboxTypeID;

		private AnimatableProperty<Vector3D> translation;
		public AnimatableProperty<Vector3D> Translation => translation;
		
		private AnimatableProperty<Vector3D> rotation;
		public AnimatableProperty<Vector3D> Rotation => rotation;
		
		private AnimatableProperty<Vector3D> scaling;
		public AnimatableProperty<Vector3D> Scaling => scaling;

		private Joint parent;
		public Joint Parent => parent;
		
		private List<AnimatablePropertyBase> animatableProperties;

		/// <summary>
		/// Creates a new joint.
		/// </summary>
		/// <param name="name">Name of the joint.</param>
		/// <param name="translation">Start position. TODO: Support animations.</param>
		/// <param name="rotation">Start rotation.</param>
		/// <param name="scaling">Start scaling.</param>
		/// <param name="parent">The parent joint that this joint is a child of.</param>
		public Joint(string name, Vector3D translation, Vector3D rotation, Vector3D scaling, Joint parent = null)
		{
			this.name = name;
			id = FbxId.GetNewId();
			attributesNodeId = FbxId.GetNewId();
			
			// NOTE: Not sure what this means exactly, but it seems to always be 5.
			filmboxTypeID = new AnimatableProperty<short>(AnimatablePropertyTypes.FilmboxTypeID, "filmboxTypeID", "filmboxTypeID", 5);
			
			this.translation = new AnimatableProperty<Vector3D>(AnimatablePropertyTypes.Translation, "T", "Lcl Translation", translation);
			this.rotation = new AnimatableProperty<Vector3D>(AnimatablePropertyTypes.Rotation, "R", "Lcl Rotation", rotation);
			this.scaling = new AnimatableProperty<Vector3D>(AnimatablePropertyTypes.Scaling, "S", "Lcl Scaling", scaling);
			
			this.parent = parent;

			// Initialize the animatable properties with an owner.
			List<AnimatablePropertyBase> animatableProperties = GetAnimatableProperties();
			foreach (AnimatablePropertyBase animatableProperty in animatableProperties)
			{
				animatableProperty.Initialize(this);
			}
		}

		/// <summary>
		/// Creates a new joint.
		/// </summary>
		/// <param name="name">Name of the joint.</param>
		/// <param name="translation">Start position. TODO: Support animations.</param>
		public Joint(string name, Vector3D translation, Vector3D rotation)
			: this(name, translation, rotation, Vector3D.One)
		{
		}

		/// <summary>
		/// Creates a new joint.
		/// </summary>
		/// <param name="name">Name of the joint.</param>
		/// <param name="translation">Start position. TODO: Support animations.</param>
		public Joint(string name, Vector3D translation)
			: this(name, translation, Vector3D.Zero)
		{
		}

		/// <summary>
		/// Gets all the animatable properties defined in this joint.
		/// </summary>
		/// <returns>List of all animatable properties.</returns>
		public List<AnimatablePropertyBase> GetAnimatableProperties()
		{
			if (animatableProperties != null)
				return animatableProperties;
			
			animatableProperties = new List<AnimatablePropertyBase>();
			FieldInfo[] allFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			Type animatablePropertyType = typeof(AnimatablePropertyBase);
			
			// Find all the animatable properties using reflection.
			foreach (FieldInfo fieldInfo in allFields)
			{
				if (!animatablePropertyType.IsAssignableFrom(fieldInfo.FieldType))
					continue;

				AnimatablePropertyBase animatableProperty = (AnimatablePropertyBase)fieldInfo.GetValue(this);
				animatableProperties.Add(animatableProperty);
			}

			return animatableProperties;
		}
	}
}
