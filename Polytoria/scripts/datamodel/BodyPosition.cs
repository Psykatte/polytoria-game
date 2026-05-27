// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// BodyPosition are objects that apply a force to their parent until they reach the target position.
/// </summary>
[Instantiable]
[DocCategory("physics")]
public partial class BodyPosition : Instance
{
	private Vector3 _targetPosition = new(0, 0, 0);
	private float _force = 0;
	private float _acceptanceDistance = 2;

	/// <summary>
	/// Determines the target position that the body applies forces to get to.
	/// </summary>
	[Editable, ScriptProperty]
	public Vector3 TargetPosition
	{
		get => _targetPosition;
		set
		{
			_targetPosition = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines how much force the body applies.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(0)]
	public float Force
	{
		get => _force;
		set
		{
			_force = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines how close the body has to be to the target position to stop applying forces to it.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(2)]
	public float AcceptanceDistance
	{
		get => _acceptanceDistance;
		set
		{
			_acceptanceDistance = value;
			OnPropertyChanged();
		}
	}

	public override void Init()
	{
		SetPhysicsProcess(true);
		base.Init();
	}

	public override void PhysicsProcess(double delta)
	{
		Vector3 gdPos = _targetPosition;
		if (Parent != null)
		{
			if (Parent is NPC npc)
			{
				if (npc.GetGlobalPosition().DistanceTo(gdPos) > AcceptanceDistance)
				{
					Vector3 currentPos = npc.Position;
					Vector3 direction = (TargetPosition - currentPos).Normalized();
					float distance = currentPos.DistanceTo(TargetPosition);

					float forceMagnitude = Mathf.Min(Force * distance, Force);
					Vector3 forceVector = direction * forceMagnitude * (float)delta;

					npc.CharacterVelocity += forceVector;
				}
			}
			else if (Parent.GDNode is RigidBody3D rigid3D)
			{
				Vector3 currentPos = rigid3D.GlobalPosition;

				if (currentPos.DistanceTo(gdPos) > AcceptanceDistance)
				{
					Vector3 dir = gdPos - currentPos;
					rigid3D.LinearVelocity = dir * Force;
				}
			}
		}
		base.PhysicsProcess(delta);
	}
}
