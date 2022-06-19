using Fbx.PropertyBlocks;

namespace Fbx.Data
{
	/// <summary>
	/// Represents a single joint. The intended workflow is that you create a joint hierarchy and animation data,
	/// add it to an FbxTemplate, then write that to an FBX. This allows you to work on a simple and intuitive level
	/// which is translated to the complicated FBX syntax under the hood. 
	/// </summary>
	public class Joint
	{
		private string name;
		public string Name => name;

		private FbxNodeId id;
		public FbxNodeId Id => id;

		private FbxNodeId attributesNodeId;
		public FbxNodeId AttributesNodeId => attributesNodeId;

		private FbxNodeId animCurveNodeId;
		public FbxNodeId AnimCurveNodeId => animCurveNodeId;

		private Vector3D position;
		public Vector3D Position => position;
		
		private Vector3D rotation;
		public Vector3D Rotation => rotation;
		
		private Vector3D scaling;
		public Vector3D Scaling => scaling;

		/// <summary>
		/// Creates a new joint.
		/// </summary>
		/// <param name="name">Name of the joint.</param>
		/// <param name="position">Start position. TODO: Support animations.</param>
		/// <param name="rotation">Start rotation.</param>
		/// <param name="scaling">Start scaling.</param>
		public Joint(string name, Vector3D position, Vector3D rotation, Vector3D scaling)
		{
			this.name = name;
			id = FbxNodeId.GetNewId();
			attributesNodeId = FbxNodeId.GetNewId();
			animCurveNodeId = FbxNodeId.GetNewId();
			this.position = position;
			this.rotation = rotation;
			this.scaling = scaling;
		}

		/// <summary>
		/// Creates a new joint.
		/// </summary>
		/// <param name="name">Name of the joint.</param>
		/// <param name="position">Start position. TODO: Support animations.</param>
		public Joint(string name, Vector3D position, Vector3D rotation)
			: this(name, position, rotation, Vector3D.One)
		{
		}

		/// <summary>
		/// Creates a new joint.
		/// </summary>
		/// <param name="name">Name of the joint.</param>
		/// <param name="position">Start position. TODO: Support animations.</param>
		public Joint(string name, Vector3D position)
			: this(name, position, Vector3D.Zero)
		{
		}
	}
}
