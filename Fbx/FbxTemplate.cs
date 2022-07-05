using System;
using System.Collections.Generic;
using System.IO;
using Fbx.Data;
using Fbx.Data.Animation;
using Fbx.Data.Times;
using Fbx.PropertyBlocks;

namespace Fbx
{
	/// <summary>
	/// Template for writing valid FBX files. The intention is to set it up to recreate a very basic reference file
	/// and then gradually make parts of that file dynamic and support real data.
	/// </summary>
	public class FbxTemplate
	{
		private const string ApplicationName = "FbxWriter-ForAnimations";
		private const string ApplicationVersion = "000001";
		private const string VendorName = "YourNameHere";

		private const TimeModes TimeMode = TimeModes.Frames30;
		private const TimeProtocols TimeProtocol = TimeProtocols.DefaultProtocol;
		
		private readonly FbxId animationStackId = FbxId.GetNewId();
		private readonly FbxId baseLayerId = FbxId.GetNewId();

		private readonly string path;
		private readonly FbxDocument fbxDocument;
		
		private readonly List<Joint> joints = new List<Joint>();
		private readonly List<Curve> curves = new List<Curve>();
		
		private readonly List<FbxConnection> connections = new List<FbxConnection>();

		private bool IsAnimated => curves.Count > 0;

		/// <summary>
		/// Create a new FBX Template.
		/// </summary>
		/// <param name="path">The file path ending with .fbx that the data will be written to.</param>
		public FbxTemplate(string path)
		{
			this.path = path;

			// Create a document.
			fbxDocument = new FbxDocument { Version = FbxVersion.v7_5 };
		}

		/// <summary>
		/// Add a joint to 
		/// </summary>
		/// <param name="joint"></param>
		/// <returns></returns>
		public Joint AddJoint(Joint joint)
		{
			joints.Add(joint);
			return joint;
		}

		public Curve AddCurve(Curve curve)
		{
			curves.Add(curve);
			return curve;
		}

		private void AddConnection(ConnectionTypes type,
			FbxId fromId, string fromType, string fromName,
			FbxId toId, string toType, string toName,
			string property = null)
		{
			FbxConnection connection = new FbxConnection(
				type, fromId, fromType, fromName, toId, toType, toName, property);
			connections.Add(connection);
		}

		/// <summary>
		/// Writes the data to an FBX file.
		/// </summary>
		public void Write()
		{
			// Open an FBX writer.
			FileStream fileStream = new FileStream(path, FileMode.Create);
			FbxAsciiWriter writer = new FbxAsciiWriter(fileStream);

			CreateHeader();
			CreateGlobalSettings();
			CreateDocumentsDescription();
			CreateDocumentReferences();
			CreateObjectDefinitions();
			CreateObjectProperties();
			CreateObjectConnections();
			CreateTakes();

			// Close the FBX writer.
			writer.Write(fbxDocument);
			fileStream.Close();
		}

		private void CreateHeader()
		{
			// Add a header node.
			FbxNode headerNode = fbxDocument.Add("FBXHeaderExtension");

			headerNode.Add("FBXHeaderVersion", 1003);
			headerNode.Add("FBXVersion", 7500);

			FbxNode creationTimeStamp = headerNode.Add("CreationTimeStamp");
			creationTimeStamp.Add("Version", 1000);
			DateTime currentTime = DateTime.Now;
			creationTimeStamp.Add("Year", currentTime.Year);
			creationTimeStamp.Add("Month", currentTime.Month);
			creationTimeStamp.Add("Day", currentTime.Day);
			creationTimeStamp.Add("Hour", currentTime.Hour);
			creationTimeStamp.Add("Minute", currentTime.Minute);
			creationTimeStamp.Add("Second", currentTime.Second);
			creationTimeStamp.Add("Millisecond", currentTime.Millisecond);

			headerNode.Add("Creator", "FbxWriter-ForAnimations");

			FbxNode sceneInfo = headerNode.Add("SceneInfo", "SceneInfo::GlobalInfo", "UserData");
			sceneInfo.Add("Type", "UserData");
			sceneInfo.Add("Version", 100);

			FbxNode metaData = sceneInfo.Add("MetaData");
			metaData.Add("Version", 100);
			metaData.Add("Title", "");
			metaData.Add("Subject", "");
			metaData.Add("Author", "");
			metaData.Add("Keywords", "");
			metaData.Add("Revision", "");
			metaData.Add("Comment", "");

			PropertyBlock properties = new PropertyBlock(sceneInfo);
			properties.AddString("DocumentUrl", $@"{path}", StringTypes.Url);
			properties.AddString("SrcDocumentUrl", $@"{path}", StringTypes.Url);

			CompoundProperty originalCompound = properties.AddCompound("Original");
			originalCompound.AddString("ApplicationVendor", VendorName);
			originalCompound.AddString("ApplicationName", ApplicationName);
			originalCompound.AddString("ApplicationVersion", ApplicationVersion);
			originalCompound.AddDateTime("DateTime_GMT", currentTime);
			originalCompound.AddString("FileName", path);

			CompoundProperty lastSavedCompound = properties.AddCompound("LastSaved");
			lastSavedCompound.AddString("ApplicationVendor", VendorName);
			lastSavedCompound.AddString("ApplicationName", ApplicationName);
			lastSavedCompound.AddString("ApplicationVersion", ApplicationVersion);
			lastSavedCompound.AddDateTime("DateTime_GMT", currentTime);

			originalCompound.AddString("ApplicationActiveProject", Path.GetDirectoryName(path));
		}

		private void CreateGlobalSettings()
		{
			FbxNode globalSettings = fbxDocument.Add("GlobalSettings");
			globalSettings.Add("Version", 1000);

			PropertyBlock properties = new PropertyBlock(globalSettings);
			properties.AddInteger("UpAxis", 1);
			properties.AddInteger("UpAxisSign", 1);
			properties.AddInteger("FrontAxis", 2);
			properties.AddInteger("FrontAxisSign", 1);
			properties.AddInteger("CoordAxis", 0);
			properties.AddInteger("CoordAxisSign", 1);
			properties.AddInteger("OriginalUpAxis", 1);
			properties.AddInteger("OriginalUpAxisSign", 1);
			properties.AddDouble("UnitScaleFactor", 1);
			properties.AddDouble("OriginalUnitScaleFactor", 1);
			properties.AddColorRGB("AmbientColor", new ColorRGB(0, 0, 0));
			properties.AddString("DefaultCamera", "Producer Perspective");
			properties.AddEnum("TimeMode", TimeMode);
			properties.AddEnum("TimeProtocol", TimeProtocol);
			properties.AddEnum("SnapOnFrameMode", 0);
			properties.AddTime("TimeSpanStart", new DateTime(1539538600));
			properties.AddTime("TimeSpanStop", new DateTime(92372316000));
			properties.AddDouble("CustomFrameRate", -1);
			properties.AddCompound("TimeMarker");
			properties.AddInteger("CurrentTimeMarker", -1);
		}

		private void CreateDocumentsDescription()
		{
			fbxDocument.AddComment("Documents Description", CommentTypes.Header);
			FbxNode documents = fbxDocument.Add("Documents");
			documents.Add("Count", 1);

			FbxNode document = documents.Add("Document", 1710616802960, "", "Scene");

			PropertyBlock properties = new PropertyBlock(document);
			properties.AddObject("SourceObject", null);
			properties.AddString("ActiveAnimStackName", "Take 001");

			document.Add("RootNode", 0);
		}

		private void CreateDocumentReferences()
		{
			fbxDocument.AddComment("Document References", CommentTypes.Header);
			FbxNode references = fbxDocument.Add("References");
			references.Add(null);
		}

		private void CreateObjectDefinitions()
		{
			fbxDocument.AddComment("Object definitions", CommentTypes.Header);
			FbxNode definitions = fbxDocument.Add("Definitions");
			definitions.Add("Version", 100);

			// Every joint has its own node and an attribute node.
			// Also if this scene is animated, there's a few extra nodes for every animatable property.
			int jointNodeCount = 2;
			if (IsAnimated)
				jointNodeCount += Joint.AnimatablePropertyFields.Count;
			
			// The count is the total number of objects including *their own* counts.
			int count =
					1 + // GlobalSettings
					1 + // Animation Stack
					1 + // Animation Layer
					jointNodeCount * joints.Count +
					curves.Count
				;
			definitions.Add("Count", count);

			FbxNode globalSettings = definitions.Add("ObjectType", "GlobalSettings");
			globalSettings.Add("Count", 1);

			FbxNode animationStack = definitions.Add("ObjectType", "AnimationStack");
			animationStack.Add("Count", 1);
			FbxNode propertyTemplate = animationStack.Add("PropertyTemplate", "FbxAnimStack");
			PropertyBlock propertyBlock = new PropertyBlock(propertyTemplate);
			propertyBlock.AddString("Description", "");
			propertyBlock.AddTime("LocalStart", DateTime.MinValue);
			propertyBlock.AddTime("LocalStop", DateTime.MinValue);
			propertyBlock.AddTime("ReferenceStart", DateTime.MinValue);
			propertyBlock.AddTime("ReferenceStop", DateTime.MinValue);

			FbxNode animationLayer = definitions.Add("ObjectType", "AnimationLayer");
			animationLayer.Add("Count", 1);
			propertyTemplate = animationLayer.Add("PropertyTemplate", "FbxAnimLayer");
			propertyBlock = new PropertyBlock(propertyTemplate);
			propertyBlock.AddNumber("Weight", 100);
			propertyBlock.AddBool("Mute", false);
			propertyBlock.AddBool("Solo", false);
			propertyBlock.AddBool("Lock", false);
			propertyBlock.AddColorRGB("Color", new ColorRGB(0.8f, 0.8f, 0.8f));
			propertyBlock.AddEnum("BlendMode", 0);
			propertyBlock.AddEnum("RotationAccumulationMode", 0);
			propertyBlock.AddEnum("ScaleAccumulationMode", 0);
			propertyBlock.AddULongLong("BlendModeBypass", 0);

			FbxNode nodeAttribute = definitions.Add("ObjectType", "NodeAttribute");
			nodeAttribute.Add("Count", joints.Count);
			propertyTemplate = nodeAttribute.Add("PropertyTemplate", "FbxSkeleton");
			propertyBlock = new PropertyBlock(propertyTemplate);
			propertyBlock.AddColorRGB("Color", new ColorRGB(0.8f, 0.8f, 0.8f));
			propertyBlock.AddDouble("Size", 100);
			propertyBlock.AddDouble("LimbLength", 100, DoubleTypes.H);

			FbxNode model = definitions.Add("ObjectType", "Model");
			model.Add("Count", joints.Count);
			propertyTemplate = model.Add("PropertyTemplate", "FbxNode");
			propertyBlock = new PropertyBlock(propertyTemplate);
			propertyBlock.AddEnum("QuaternionInterpolate", 0);
			propertyBlock.AddVector3D("RotationOffset", Vector3D.Zero);
			propertyBlock.AddVector3D("RotationPivot", Vector3D.Zero);
			propertyBlock.AddVector3D("ScalingOffset", Vector3D.Zero);
			propertyBlock.AddVector3D("ScalingPivot", Vector3D.Zero);
			propertyBlock.AddBool("TranslationActive", false);
			propertyBlock.AddVector3D("TranslationMin", Vector3D.Zero);
			propertyBlock.AddVector3D("TranslationMax", Vector3D.Zero);
			propertyBlock.AddBool("TranslationMinX", false);
			propertyBlock.AddBool("TranslationMinY", false);
			propertyBlock.AddBool("TranslationMinZ", false);
			propertyBlock.AddBool("TranslationMaxX", false);
			propertyBlock.AddBool("TranslationMaxY", false);
			propertyBlock.AddBool("TranslationMaxZ", false);
			propertyBlock.AddEnum("RotationOrder", 0);
			propertyBlock.AddBool("RotationSpaceForLimitOnly", false);
			propertyBlock.AddDouble("RotationStiffnessX", 0);
			propertyBlock.AddDouble("RotationStiffnessY", 0);
			propertyBlock.AddDouble("RotationStiffnessZ", 0);
			propertyBlock.AddDouble("AxisLen", 10);
			propertyBlock.AddVector3D("PreRotation", Vector3D.Zero);
			propertyBlock.AddVector3D("PostRotation", Vector3D.Zero);
			propertyBlock.AddBool("RotationActive", false);
			propertyBlock.AddVector3D("RotationMin", Vector3D.Zero);
			propertyBlock.AddVector3D("RotationMax", Vector3D.Zero);
			propertyBlock.AddBool("RotationMinX", false);
			propertyBlock.AddBool("RotationMinY", false);
			propertyBlock.AddBool("RotationMinZ", false);
			propertyBlock.AddBool("RotationMaxX", false);
			propertyBlock.AddBool("RotationMaxY", false);
			propertyBlock.AddBool("RotationMaxZ", false);
			propertyBlock.AddEnum("InheritType", 0);
			propertyBlock.AddBool("ScalingActive", false);
			propertyBlock.AddVector3D("ScalingMin", Vector3D.Zero);
			propertyBlock.AddVector3D("ScalingMax", Vector3D.One);
			propertyBlock.AddBool("ScalingMinX", false);
			propertyBlock.AddBool("ScalingMinY", false);
			propertyBlock.AddBool("ScalingMinZ", false);
			propertyBlock.AddBool("ScalingMaxX", false);
			propertyBlock.AddBool("ScalingMaxY", false);
			propertyBlock.AddBool("ScalingMaxZ", false);
			propertyBlock.AddVector3D("GeometricTranslation", Vector3D.Zero);
			propertyBlock.AddVector3D("GeometricRotation", Vector3D.Zero);
			propertyBlock.AddVector3D("GeometricScaling", Vector3D.One);
			propertyBlock.AddDouble("MinDampRangeX", 0);
			propertyBlock.AddDouble("MinDampRangeY", 0);
			propertyBlock.AddDouble("MinDampRangeZ", 0);
			propertyBlock.AddDouble("MaxDampRangeX", 0);
			propertyBlock.AddDouble("MaxDampRangeY", 0);
			propertyBlock.AddDouble("MaxDampRangeZ", 0);
			propertyBlock.AddDouble("MinDampStrengthX", 0);
			propertyBlock.AddDouble("MinDampStrengthY", 0);
			propertyBlock.AddDouble("MinDampStrengthZ", 0);
			propertyBlock.AddDouble("MaxDampStrengthX", 0);
			propertyBlock.AddDouble("MaxDampStrengthY", 0);
			propertyBlock.AddDouble("MaxDampStrengthZ", 0);
			propertyBlock.AddDouble("PreferedAngleX", 0); // There's a typo in here but that's part of the format...
			propertyBlock.AddDouble("PreferedAngleY", 0); // There's a typo in here but that's part of the format...
			propertyBlock.AddDouble("PreferedAngleZ", 0); // There's a typo in here but that's part of the format...
			propertyBlock.AddObject("LookAtProperty", null);
			propertyBlock.AddObject("UpVectorProperty", null);
			propertyBlock.AddBool("Show", true);
			propertyBlock.AddBool("NegativePercentShapeSupport", true);
			propertyBlock.AddInteger("DefaultAttributeIndex", -1);
			propertyBlock.AddBool("Freeze", false);
			propertyBlock.AddBool("LODBox", false);
			propertyBlock.AddLclTranslation("Lcl Translation", Vector3D.Zero);
			propertyBlock.AddLclTranslation("Lcl Rotation", Vector3D.Zero);
			propertyBlock.AddLclTranslation("Lcl Scaling", Vector3D.One);
			propertyBlock.AddVisibility("Visibility", true);
			propertyBlock.AddVisibilityInheritance("Visibility Inheritance", true);

			if (IsAnimated)
			{
				FbxNode animationCurveNode = definitions.Add("ObjectType", "AnimationCurveNode");
				animationCurveNode.Add("Count", joints.Count * Joint.AnimatablePropertyFields.Count);
				propertyTemplate = animationCurveNode.Add("PropertyTemplate", "FbxAnimCurveNode");
				propertyBlock = new PropertyBlock(propertyTemplate);
				propertyBlock.AddCompound("d");
				
				FbxNode animationCurve = definitions.Add("ObjectType", "AnimationCurve");
				animationCurve.Add("Count", curves.Count);
			}
		}

		private void CreateObjectProperties()
		{
			fbxDocument.AddComment("Object properties", CommentTypes.Header);
			FbxNode objects = fbxDocument.Add("Objects");
			PropertyBlock propertyBlock;
			
			FbxNode animationStack = objects.Add("AnimationStack", animationStackId, "AnimStack::Take 001", "");
			propertyBlock = new PropertyBlock(animationStack);
			propertyBlock.AddTime("LocalStart", new DateTime(1539538600));
			propertyBlock.AddTime("LocalStop", new DateTime(46186158000));
			propertyBlock.AddTime("ReferenceStart", new DateTime(1539538600));
			propertyBlock.AddTime("ReferenceStop", new DateTime(46186158000));
			
			FbxNode animationLayer = objects.Add("AnimationLayer", baseLayerId, "AnimLayer::BaseLayer", "");
			animationLayer.Add(null);
			
			foreach (Joint joint in joints)
			{
				// Create a node for the joint itself.
				FbxNode node = objects.Add("Model", joint.Id, "Model::" + joint.Name, "LimbNode");
				node.Add("Version", 232);
				propertyBlock = new PropertyBlock(node);
				propertyBlock.AddBool("RotationActive", true);
				propertyBlock.AddEnum("InheritType", 1);
				propertyBlock.AddVector3D("ScalingMax", Vector3D.Zero);
				propertyBlock.AddInteger("DefaultAttributeIndex", 0);
				
				if (joint.Translation != Vector3D.Zero)
					propertyBlock.AddLclTranslation("Lcl Translation", joint.Translation);
				if (joint.Rotation != Vector3D.Zero)
					propertyBlock.AddLclRotation("Lcl Rotation", joint.Rotation);
				if (joint.Scaling != Vector3D.One)
					propertyBlock.AddLclScaling("Lcl Scaling", joint.Scaling);
				propertyBlock.AddShort("filmboxTypeID", 5, ShortTypes.APlusUH);
				
				node.Add("Shading", 'Y');
				node.Add("Culling", "CullingOff");
				
				// Create an attribute node.
				FbxNode limbNode = objects.Add("NodeAttribute", joint.AttributesNodeId, "NodeAttribute::", "LimbNode");
				propertyBlock = new PropertyBlock(limbNode);
				propertyBlock.AddDouble("Size", 333.333333333333);
				limbNode.Add("TypeFlags", "Skeleton");
				
				// Only necessary if this scene is animated.
				if (IsAnimated)
				{
					List<AnimatablePropertyBase> animatableProperties = joint.GetAnimatableProperties();

					// Create an AnimationCurveNode for every animatable property.
					foreach (AnimatablePropertyBase animatableProperty in animatableProperties)
					{
						string nodeName = animatableProperty.NodeName;

						FbxId animationCurveNodeId = FbxId.GetNewId();
						FbxNode animationCurveNode = objects.Add("AnimationCurveNode", animationCurveNodeId, $"AnimCurveNode::{nodeName}", "");
						Type valueType = animatableProperty.GetValueType();
						
						// Handle the various property types that can exist. Currently only short and Vector3D.
						propertyBlock = new PropertyBlock(animationCurveNode);
						if (valueType == typeof(short))
							propertyBlock.AddShort($"d|{nodeName}", (short)animatableProperty.GetValueRaw());
						else if (valueType == typeof(Vector3D))
						{
							Vector3D value = (Vector3D)animatableProperty.GetValueRaw();
							propertyBlock.AddNumber("d|X", value.X);
							propertyBlock.AddNumber("d|Y", value.Y);
							propertyBlock.AddNumber("d|Z", value.Z);
						}
						else
							throw new NotImplementedException($"Tried to create AnimationCurveNode for unsupported value type '{valueType}'");

						// Also assign it back to the animatable property so we can reference it later.
						animatableProperty.AnimationCurveNodeId = animationCurveNodeId;
					}
				}
			}

			foreach (Curve curve in curves)
			{
				FbxNode animationCurve = objects.Add("AnimationCurve", curve.Id, "AnimCurve::", "");
				animationCurve.Add("Default", 0);
				animationCurve.Add("KeyVer", 4009);
				
				long[] times = new long[curve.Count];
				float[] values = new float[curve.Count];
				int[] attributeFlags = new int[curve.Count];
				const int AttrDataPerKey = 4; // Pretty sure this depends on things like velocity mode...
				long[] attrDataFloat = new long[curve.Count * AttrDataPerKey];
				int[] attrRefCount = new int[curve.Count];
				for (int i = 0; i < curve.Count; i++)
				{
					Key key = curve[i];
					times[i] = key.Time.TimeInInternalFormat;
					values[i] = key.Value;
					attributeFlags[i] = key.AttributeFlags;
					
					// There seem to be four per key?
					for (int j = 0; j < AttrDataPerKey; j++)
					{
						// TODO: Figure out what the values of this field actually mean.
						attrDataFloat[i * AttrDataPerKey + j] = 0;
					}

					// TODO: Figure out what the values of this field actually mean. 
					attrRefCount[i] = 1;
				}
				
				animationCurve.Add("KeyTime", times);
				animationCurve.Add("KeyValueFloat", values);
				
				// TODO: Describe the flags with text...
				animationCurve.Add("KeyAttrFlags", attributeFlags);
				
				// TODO: Describe the data with text...
				// Right, so on a nice linear animation this field looks like this:
				// ;KeyAttrDataFloat: RightSlope:-3000, NextLeftSlope:-3000, RightWeight:0.333333, NextLeftWeight:0.333333, RightVelocity:0, NextLeftVelocity:0; RightSlope:0, NextLeftSlope:0, RightWeight:0.333333, NextLeftWeight:0.333333, RightVelocity:0, NextLeftVelocity:0
				// Now, remarkably this produces an array of only 8 values:
				// a: -985956352,-985956352,218434821,0,0,0,218434821,0
				// That's misleading, huh? Well, I think that depending on the settings some of these fields
				// *are not used*. For example, if we leave out all of the velocity parameters (which I suspect happens
				// when the velocity mode is NONE) we end up with 4 float values per key, and 2 keys, so 8 values.
				// Which works out exactly.
				animationCurve.Add("KeyAttrDataFloat", attrDataFloat);
				
				// Still a mystery what this means.
				animationCurve.Add("KeyAttrRefCount", attrRefCount);
			}
		}

		private void CreateObjectConnections()
		{
			fbxDocument.AddComment("Object connections", CommentTypes.Header);
			FbxNode connections = fbxDocument.Add("Connections");

			// Joint connections.
			FbxId rootNodeId = new FbxId();
			foreach (Joint joint in joints)
			{
				// Connections from the joints to the parent node.
				if (joint.Parent == null)
					AddConnection(ConnectionTypes.OO, joint.Id, "Model", joint.Name, rootNodeId, "Model", "RootNode");
				else
					AddConnection(ConnectionTypes.OO, joint.Id, "Model", joint.Name, joint.Parent.Id, "Model", joint.Parent.Name);

				// Connections from the joint's attribute node to the joint itself
				AddConnection(ConnectionTypes.OO, joint.AttributesNodeId, "NodeAttribute", "", joint.Id, "Model", joint.Name);
				
				// Connections from the joints' anim curve nodes to the base layer and also to the joint's property.
				if (IsAnimated)
				{
					List<AnimatablePropertyBase> animatableProperties = joint.GetAnimatableProperties();
					foreach (AnimatablePropertyBase animatableProperty in animatableProperties)
					{
						string nodeName = animatableProperty.NodeName;
						AddConnection(
							ConnectionTypes.OO, animatableProperty.AnimationCurveNodeId, "AnimCurveNode", nodeName,
							baseLayerId, "AnimLayer", "BaseLayer");

						string propertyName = animatableProperty.PropertyName;
						AddConnection(
							ConnectionTypes.OP, animatableProperty.AnimationCurveNodeId, "AnimCurveNode", nodeName,
							joint.Id, "Model", joint.Name, propertyName);
					}
				}
			}

			foreach (Curve curve in curves)
			{
				// TODO: Write a connection from the curve node to the AnimationCurveNode for the respective property,
				// and then reference the relevant component, like d|X. 
				AddConnection(
					ConnectionTypes.OP, curve.Id, "AnimCurve", "", curve.Property.AnimationCurveNodeId, "AnimCurveNode",
					curve.Property.NodeName, curve.Component);
			}

			// Connection from the base layer to the Take
			AddConnection(ConnectionTypes.OO, baseLayerId, "AnimLayer", "BaseLayer", animationStackId, "AnimStack", "Take 001");
			
			// Write all the connections.
			foreach (FbxConnection connection in this.connections)
			{
				connections.AddLineBreak();
				connections.AddComment(
					$"{connection.FromType}::{connection.FromName}, {connection.ToType}::{connection.ToName}",
					CommentTypes.Inline);

				connections.Add(
					"C", Shorten(connection.ConnectionType), connection.FromId, connection.ToId,
					connection.Property);
			}
		}

		private void CreateTakes()
		{
			fbxDocument.AddComment("Takes section", CommentTypes.Header);
			FbxNode takes = fbxDocument.Add("Takes");
			takes.Add("Current", "Take 001");
			FbxNode take001 = takes.Add("Take", "Take 001");
			take001.Add("FileName", "Take_001.tak");
			take001.Add("LocalTime", 1539538600, 46186158000);
			take001.Add("ReferenceTime", 1539538600, 46186158000);
		}
		
		private static string Shorten(ConnectionTypes connectionType)
		{
			switch (connectionType)
			{
				case ConnectionTypes.OP:
					return "OP";
				case ConnectionTypes.OO:
					return "OO";
				case ConnectionTypes.PP:
					return "PP";
				default:
					throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
			}
		}
	}
}
