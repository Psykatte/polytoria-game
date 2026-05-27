// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Godot.Collections;
using Polytoria.Attributes;
using Polytoria.Client;
using Polytoria.Networking;
using Polytoria.Scripting;
using Polytoria.Shared;
using Polytoria.Utils;

namespace Polytoria.Datamodel;

/// <summary>
/// NPC (non-player character) is an object similar to a Player but that can be controlled by code. Like players, it can walk and jump, and its body part colors can be customized.
/// </summary>
[Instantiable]
[DocCategory("world")]
public partial class NPC : Physical
{
	private const float CoyoteTime = 0.15f;
	private const float NavigationDistance = 1f;
	public const float BodyRotateLerp = 10f;
	private const float StepHeight = 1.5f;
	private Tool? _holdingTool;
	private Seat? _sittingIn;
	private CharacterModel? _character;
	private Dynamic? _moveTarget;

	public CharacterBody3D CharBody3D = null!;
	public const float ForwardRaycastRange = 1;
	public const float StairForwardRaycastRange = 4;
	public const float NameTagHeightMinus = 3f;
	private Vector3 _seatOffset = new(0, 1.5f, 0);
	private float _health = 100;
	private RemoteTransform3D? _toolRemoteTransform;
	private float _maxHealth = 100;
	private float _jumpPower = 36;
	private float _walkSpeed = 16;
	private string _displayName = "";
	protected RayCast3D FootFwdRaycast = null!;
	private Sound? _jumpSound;
	private bool _lastOnFloorState = false;
	private float _timeSinceGrounded = 0f;
	private bool _coyoteUsed = false;
	private Node3D? _navAgentContainer;
	private NavigationAgent3D? _navAgent;

	private Vector3 _nametagOffset = Vector3.Zero;
	private Vector3 _fixedNametagOffset = new(0, 3, 0);
	private float _nametagVisibleRadius = 40;
	private bool _useNametag = true;
	private Nametag _nametag = null!;

	// Pending properties to apply to character
	private Color? _pendingHeadColor;
	private Color? _pendingTorsoColor;
	private Color? _pendingLeftArmColor;
	private Color? _pendingRightArmColor;
	private Color? _pendingLeftLegColor;
	private Color? _pendingRightLegColor;
	private int? _pendingFaceID;

	protected override float PositionSyncThreshold => 0.1f;
	protected override float RotationSyncThreshold => 1f;

	/// <summary>
	/// Determines the linear velocity of this NPC.
	/// </summary>
	[Editable, ScriptProperty, SyncVar(Unreliable = true, AllowAuthorWrite = true)]
	public override Vector3 Velocity
	{
		get
		{
			return CharacterVelocity;
		}
		set
		{
			if (this is Player plr)
			{
				plr.LastVelocity = value;
				plr.ExternalVelocity = value;
			}

			CharacterVelocity = value;

			OnPropertyChanged();
		}
	}

	internal void ApplyInternalVelocity(Vector3 velocity)
	{
		UpdateVelocityInternal(velocity);
		CharacterVelocity = velocity;
		OnPropertyChanged(nameof(Velocity));
	}


	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character"), CloneIgnore]
	public Color HeadColor
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.HeadColor : _pendingHeadColor ?? new Color();
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.HeadColor = value;
				_pendingHeadColor = null;
			}
			else
			{
				_pendingHeadColor = value;
			}
		}
	}

	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character instead"), CloneIgnore]
	public Color TorsoColor
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.TorsoColor : _pendingTorsoColor ?? new Color();
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.TorsoColor = value;
				_pendingTorsoColor = null;
			}
			else
			{
				_pendingTorsoColor = value;
			}
		}
	}

	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character instead"), CloneIgnore]
	public Color LeftArmColor
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.LeftArmColor : _pendingLeftArmColor ?? new Color();
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.LeftArmColor = value;
				_pendingLeftArmColor = null;
			}
			else
			{
				_pendingLeftArmColor = value;
			}
		}
	}

	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character instead"), CloneIgnore]
	public Color RightArmColor
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.RightArmColor : _pendingRightArmColor ?? new Color();
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.RightArmColor = value;
				_pendingRightArmColor = null;
			}
			else
			{
				_pendingRightArmColor = value;
			}
		}
	}

	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character instead"), CloneIgnore]
	public Color LeftLegColor
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.LeftLegColor : _pendingLeftLegColor ?? new Color();
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.LeftLegColor = value;
				_pendingLeftLegColor = null;
			}
			else
			{
				_pendingLeftLegColor = value;
			}
		}
	}

	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character instead"), CloneIgnore]
	public Color RightLegColor
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.RightLegColor : _pendingRightLegColor ?? new Color();
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.RightLegColor = value;
				_pendingRightLegColor = null;
			}
			else
			{
				_pendingRightLegColor = value;
			}
		}
	}

	[Editable, ScriptProperty, NoSync, Attributes.Obsolete("Apply them to Character instead"), CloneIgnore]
	public int FaceID
	{
		get => (Character is PolytorianModel polytorian) ? polytorian.FaceID : _pendingFaceID ?? 0;
		set
		{
			if (Character is PolytorianModel polytorian)
			{
				polytorian.FaceID = value;
				_pendingFaceID = null;
			}
			else
			{
				_pendingFaceID = value;
			}
		}
	}

	/// <summary>
	/// The offset to the seat at which the NPC is positioned when sitting.
	/// </summary>
	[Editable, ScriptProperty]
	public Vector3 SeatOffset
	{
		get => _seatOffset;
		set
		{
			_seatOffset = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The current health of the NPC.
	/// </summary>
	[Editable, ScriptProperty]
	public float Health
	{
		get => _health;
		set
		{
			if (this is Player plr && !plr.IsReady) return;
			_health = value;
			if (_health <= 0 && !IsDead)
			{
				TriggerNPCDead();
			}
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The maximum health of the NPC.
	/// </summary>
	[Editable, ScriptProperty]
	public float MaxHealth
	{
		get => _maxHealth;
		set
		{
			_maxHealth = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the jump power of the NPC.
	/// </summary>
	[Editable, ScriptProperty]
	public float JumpPower
	{
		get => _jumpPower;
		set
		{
			_jumpPower = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the walking speed of the NPC.
	/// </summary>
	[Editable, ScriptProperty]
	public float WalkSpeed
	{
		get => _walkSpeed;
		set
		{
			_walkSpeed = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines whether the NPC uses a nametag.
	/// </summary>
	[Editable, ScriptProperty]
	public bool UseNametag
	{
		get => _useNametag;
		set
		{
			_useNametag = value;
			_nametag?.UpdateNameTag();
			OnPropertyChanged();
		}
	}


	/// <summary>
	/// Determines the offset position of the NPC's nametag.
	/// </summary>
	[Editable, ScriptProperty]
	public Vector3 NametagOffset
	{
		get => _nametagOffset;
		set
		{
			_nametagOffset = value;
			RecalculateNametagOffset();
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the visibility radius of the NPC's nametag.
	/// </summary>
	[Editable, ScriptProperty]
	public float NametagVisibleRadius
	{
		get => _nametagVisibleRadius;
		set
		{
			_nametagVisibleRadius = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the display name of the NPC.
	/// </summary>
	[Editable, ScriptProperty]
	public string DisplayName
	{
		get => _displayName;
		set
		{
			_displayName = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the sound played when the NPC jumps.
	/// </summary>
	[Editable, ScriptProperty]
	public Sound? JumpSound
	{
		get => _jumpSound;
		set
		{
			_jumpSound = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Indicates whether the NPC is currently sitting.
	/// </summary>
	[SyncVar, ScriptProperty]
	public bool IsSitting { get; internal set; } = false;

	/// <summary>
	/// Indicates whether the NPC is currently dead.
	/// </summary>
	[SyncVar, ScriptProperty]
	public bool IsDead { get; internal set; } = false;

	/// <summary>
	/// Indicates the tool currently held by the NPC.
	/// </summary>
	[SyncVar, ScriptProperty]
	public Tool? HoldingTool
	{
		get
		{
			if (_holdingTool != null && _holdingTool.IsDeleted)
			{
				_holdingTool = null;
			}
			return _holdingTool;
		}
		internal set => _holdingTool = value;
	}

	/// <summary>
	/// Indicates the seat in which the NPC is currently sitting.
	/// </summary>
	[SyncVar, ScriptProperty]
	public Seat? SittingIn
	{
		get
		{
			if (_sittingIn != null && _sittingIn.IsDeleted)
			{
				_sittingIn = null;
			}
			return _sittingIn;
		}
		internal set => _sittingIn = value;
	}

	/// <summary>
	/// The character model associated with the NPC.
	/// </summary>
	[Editable, ScriptProperty, SyncVar]
	public CharacterModel? Character
	{
		get
		{
			if (_character != null && _character.IsDeleted)
			{
				_character = null;
			}
			return _character;
		}
		internal set => _character = value;
	}

	/// <summary>
	/// Determines the instance the NPC should walk towards.
	/// </summary>
	[SyncVar, ScriptProperty]
	public Dynamic? MoveTarget
	{
		get
		{
			if (_moveTarget != null && _moveTarget.IsDeleted)
			{
				_moveTarget = null;
			}
			return _moveTarget;
		}
		set => _moveTarget = value;
	}

	/// <summary>
	/// Indicates if NPC is standing on ground.
	/// </summary>
	[ScriptProperty, ScriptLegacyProperty("Grounded")]
	public bool IsOnGround => CharBody3D.IsOnFloor();

	/// <summary>
	/// Indicates if NPC is on the ceiling.
	/// </summary>
	[ScriptProperty]
	public bool IsOnCeiling => CharBody3D.IsOnCeiling();

	/// <summary>
	/// Indicates the distance to the navigation destination.
	/// </summary>
	[ScriptProperty] public float NavDestinationDistance => _navAgent == null ? Mathf.Inf : _navAgent.DistanceToTarget();
	/// <summary>
	/// Indicates whether the NPC has reached its navigation destination.
	/// </summary>
	[ScriptProperty] public bool NavDestinationReached => _navAgent != null && _navAgent.IsTargetReached();
	/// <summary>
	/// Indicates whether the navigation destination is valid.
	/// </summary>
	[ScriptProperty] public bool NavDestinationValid => _navAgent != null && _navAgent.IsTargetReachable();

	public Vector3 CharacterVelocity = Vector3.Zero;

	/// <summary>
	/// Triggered when the NPC dies.
	/// </summary>
	[ScriptProperty]
	public PTSignal Died { get; private set; } = new();

	/// <summary>
	/// Triggered when the NPC lands on the ground after a jump or fall.
	/// </summary>
	[ScriptProperty]
	public PTSignal Landed { get; private set; } = new();

	/// <summary>
	/// Triggered when the NPC finishes navigating to a destination.
	/// </summary>
	[ScriptProperty]
	public PTSignal NavFinished { get; private set; } = new();

	public override Node CreateGDNode()
	{
		return new CharacterBody3D() { FloorMaxAngle = Mathf.DegToRad(80f) };
	}

	public override void InitGDNode()
	{
		CharBody3D = (CharacterBody3D)GDNode;
		base.InitGDNode();
	}

	public override void Init()
	{
		base.Init();
		EnsureTouchArea();
		OverridePhysicsProcess = true;

		// Create nametag
		_nametag = new()
		{
			Target = this
		};
		GDNode3D.AddChild(_nametag);
		excludedBoundNodes.Add(_nametag);

		FootFwdRaycast = new();
		GDNode3D.AddChild(FootFwdRaycast, false, Node.InternalMode.Front);
		FootFwdRaycast.Position = new Vector3(0, -3, 0);
		FootFwdRaycast.TargetPosition = new Vector3(0, 0, ForwardRaycastRange);

		ChildAdded.Connect(OnChildAdded);
		ChildRemoved.Connect(OnChildRemoved);

		RecalculateNametagOffset();

		// Force these to always be on
		SetProcess(true);
		SetPhysicsProcess(true);
	}

	public override void InitOverrides()
	{
		Anchored = false;
	}

	public override void PreDelete()
	{
		ChildAdded.Disconnect(OnChildAdded);
		ChildRemoved.Disconnect(OnChildRemoved);
		_navAgent?.NavigationFinished -= OnNavFinished;
		base.PreDelete();
	}

	private void OnChildAdded(Instance n)
	{
		if (n is Tool t)
		{
			InternalAttachTool(t);
		}
	}

	private void OnChildRemoved(Instance n)
	{
		if (n is Tool)
		{
			InternalDetachTool();
		}
	}

	public override void Ready()
	{
		if (Root.IsLegacyWorld && Character == null && !PendingProps.Contains(nameof(Character)))
		{
			// Create default character on legacy world. If character is not set
			Root.Insert.InitializeDefaultNPC(this);

			if (Character is PolytorianModel polytorian)
			{
				if (_pendingHeadColor.HasValue)
				{
					polytorian.HeadColor = _pendingHeadColor.Value;
					_pendingHeadColor = null;
				}
				if (_pendingTorsoColor.HasValue)
				{
					polytorian.TorsoColor = _pendingTorsoColor.Value;
					_pendingTorsoColor = null;
				}
				if (_pendingLeftArmColor.HasValue)
				{
					polytorian.LeftArmColor = _pendingLeftArmColor.Value;
					_pendingLeftArmColor = null;
				}
				if (_pendingRightArmColor.HasValue)
				{
					polytorian.RightArmColor = _pendingRightArmColor.Value;
					_pendingRightArmColor = null;
				}
				if (_pendingLeftLegColor.HasValue)
				{
					polytorian.LeftLegColor = _pendingLeftLegColor.Value;
					_pendingLeftLegColor = null;
				}
				if (_pendingRightLegColor.HasValue)
				{
					polytorian.RightLegColor = _pendingRightLegColor.Value;
					_pendingRightLegColor = null;
				}
				if (_pendingFaceID.HasValue)
				{
					polytorian.FaceID = _pendingFaceID.Value;
					_pendingFaceID = null;
				}
			}
		}

		if (IsSitting && SittingIn != null)
		{
			InternalSit(SittingIn);
		}

		if (HoldingTool != null)
		{
			InternalAttachTool(HoldingTool);
		}

		RecalculateNametagOffset();
		base.Ready();
	}

#if CREATOR
	public override void CreatorInserted()
	{
		Root.Insert.InitializeDefaultNPC(this);
		base.CreatorInserted();
	}
#endif

	private void RecalculateNametagOffset()
	{
		if (!_nametag.IsInsideTree()) { return; }
		_nametag.Position = NametagOffset + _fixedNametagOffset;
	}

	public override void Process(double delta)
	{
		base.Process(delta);

		if (Root == null) return;
		if (Anchored || IsHidden) return;
		if (!Root.IsLoaded) return;

		// Only enable physics in client mode
		if (Root.SessionType != World.SessionTypeEnum.Client) return;

		// Kill player if fall off the map
		if (Position.Y < Root.Environment.PartDestroyHeight)
		{
			Kill();
		}

		if (IsSitting)
		{
			if (!Root.Network.IsServer && SittingIn != null)
			{
				Velocity = Vector3.Zero;
				Position = SittingIn.Position + SeatOffset * Up;
				Rotation = SittingIn.Rotation;
				Character?.PlayIdle();
			}
			return;
		}

		if (this is Player plr)
		{
			if (!plr.IsLocal)
			{
				return;
			}
		}

		if (Root.Network.LocalPeerID != NetworkAuthority && ExistInNetwork) return;

		bool isOnFloor = CharBody3D.IsOnFloor();
		bool isOnCeiling = CharBody3D.IsOnCeiling();
		bool playerNPCOverride = this is Player p && !p.CanMove;

		CharacterModel.CharacterModelStateEnum finalState = CharacterModel.CharacterModelStateEnum.Idle;
		Vector3? walkTarget = null;
		float animSpeed = 1;

		if (MoveTarget != null)
		{
			walkTarget = MoveTarget.GetGlobalPosition();
		}

		if (_navAgent != null)
		{
			walkTarget = _navAgent.GetNextPathPosition();

			// Adjust Nav agent position in-case of unstable Y position changes
			_navAgentContainer?.GlobalPosition = _navAgentContainer.GlobalPosition with { Y = walkTarget.Value.Y };
		}

		if (walkTarget.HasValue)
		{
			Vector3 velo = GetGlobalPosition().DirectionTo(walkTarget.Value with { Y = Position.Y });
			CharacterVelocity = new(velo.X * WalkSpeed, CharacterVelocity.Y, velo.Z * WalkSpeed);
			GDNode3D.GlobalRotationDegrees = new Vector3(Rotation.X, Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(Rotation.Y), Mathf.Atan2(CharacterVelocity.X, CharacterVelocity.Z), MathUtils.ExpDecay((float)delta, BodyRotateLerp))), Rotation.Z);

			float distanceToTarget = GetGlobalPosition().DistanceTo(walkTarget.Value);

			if (distanceToTarget > 0.5f)
			{
				finalState = CharacterModel.CharacterModelStateEnum.Walking;
				animSpeed = WalkSpeed / 8;
				TryStepUp();
			}
		}
		else if (this is not Player || playerNPCOverride)
		{
			CharacterVelocity = new(0, CharacterVelocity.Y, 0);
		}

		if (!isOnFloor)
		{
			finalState = CharacterModel.CharacterModelStateEnum.Jumping;
		}

		if (this is not Player || playerNPCOverride)
		{
			Character?.SetState(finalState);
			Character?.SetAnimSpeed(animSpeed);
		}

		// Apply gravity
		if (!isOnFloor)
		{
			CharacterVelocity.Y += Root.Environment.Gravity.Y * (float)delta;
		}
		else if (isOnFloor && CharacterVelocity.Y < 0)
		{
			// Cancel downward velocity when on floor
			CharacterVelocity.Y = 0;
		}

		// Prevent sticking
		if (isOnCeiling && CharacterVelocity.Y > 0)
		{
			CharacterVelocity.Y = 0;
		}

		UpdateVelocityInternal(CharacterVelocity);
		if (this is not Player)
		{
			CharBody3D.Velocity = Velocity;
			CharBody3D.MoveAndSlide();
		}

		if (isOnFloor != _lastOnFloorState)
		{
			_lastOnFloorState = isOnFloor;

			// On floor change
			if (isOnFloor)
			{
				_coyoteUsed = false;
				Landed.Invoke();
			}
		}
	}

	public override void PhysicsProcess(double delta)
	{
		if (CharBody3D != null)
		{
			bool isOnFloor = CharBody3D.IsOnFloor();

			if (isOnFloor)
			{
				_timeSinceGrounded = 0f;
			}
			else
			{
				_timeSinceGrounded += (float)delta;
			}
		}
		base.PhysicsProcess(delta);
	}

	/// <summary>
	/// Move this NPC while respecting collisions.
	/// </summary>
	[ScriptMethod]
	public void Move(Vector3 velo)
	{
		CharacterVelocity = velo;
		UpdateVelocityInternal(CharacterVelocity);
		CharBody3D.Velocity = Velocity;
		CharBody3D.MoveAndSlide();
	}

	/// <summary>
	/// Kills the NPC.
	/// </summary>
	[ScriptMethod]
	public void Kill()
	{
		Health = 0;
		RpcId(1, nameof(NetKill));
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetKill()
	{
		Health = 0;
	}

	private void TriggerNPCDead()
	{
		if (IsDead) return;
		if (Root.SessionType != World.SessionTypeEnum.Client) return;
		Anchored = true;
		OverrideCanCollide = true;
		OverrideCanCollideTo = false;
		Unsit(false);
		UpdateCollision();

		Character?.Animator?.StopAnimation();
		Character?.Animator?.StopOneShotAnimation();

		if (Character is PolytorianModel ptmodel)
		{
			ptmodel.StartRagdoll(Velocity);
		}
		IsDead = true;
		Died.Invoke();
	}

	/// <summary>
	/// Try to detect stairs and step up. Returns true if the NPC has stepped up.
	/// </summary>
	[ScriptMethod]
	public bool TryStepUp()
	{
		if (CharBody3D == null)
		{
			return false;
		}

		if (!CharBody3D.IsOnFloor())
		{
			return false;
		}

		int slideCount = CharBody3D.GetSlideCollisionCount();

		if (slideCount <= 0)
		{
			return false;
		}

		Vector3 desiredVelocity = Velocity;
		Vector3 desiredXZ = new(desiredVelocity.X, 0f, desiredVelocity.Z);
		if (desiredXZ.LengthSquared() < 0.0001f)
		{
			return false;
		}

		float groundY;
		{
			var downHit = new KinematicCollision3D();
			bool hasGround = CharBody3D.TestMove(CharBody3D.GlobalTransform, Vector3.Down * (StepHeight + 0.05f), downHit, 0.001f, true);
			if (!hasGround)
			{
				return false;
			}

			groundY = downHit.GetPosition().Y;
		}

		const float stepSearchOvershoot = 0.05f;

		var spaceState = World.Current!.World3D.DirectSpaceState;

		for (int i = 0; i < slideCount; i++)
		{
			KinematicCollision3D stepTest = CharBody3D.GetSlideCollision(i);
			Vector3 n = stepTest.GetNormal();
			Vector3 p = stepTest.GetPosition();

			if (Mathf.Abs(n.Y) >= 0.01f)
			{
				continue;
			}

			if (!(p.Y - groundY < StepHeight))
			{
				continue;
			}

			float stepHeight = p.Y + StepHeight + 0.0001f;
			Vector3 stepTestInvDir = new Vector3(-n.X, 0, -n.Z).Normalized();
			Vector3 origin = new Vector3(p.X, stepHeight, p.Z) + (stepTestInvDir * stepSearchOvershoot);
			Vector3 direction = Vector3.Down * StepHeight;

			Dictionary result = spaceState.IntersectRay(new PhysicsRayQueryParameters3D()
			{
				From = origin,
				To = origin + direction,
				Exclude = [CharBody3D.GetRid()],
				CollideWithAreas = false,
				CollideWithBodies = true,
			});

			if (result.Count == 0)
			{
				continue;
			}

			Vector3 hitPos = result["position"].AsVector3();

			Vector3 stepUpPoint = new Vector3(p.X, hitPos.Y + 0.01f, p.Z) + (stepTestInvDir * stepSearchOvershoot);
			Vector3 stepUpPointOffset = stepUpPoint - new Vector3(p.X, groundY, p.Z);

			CharBody3D.GlobalPosition += stepUpPointOffset;
			CharBody3D.Velocity = desiredVelocity;

			return true;
		}

		return false;
	}

	/// <summary>
	/// Makes the NPC jump.
	/// </summary>
	[ScriptMethod]
	public virtual void Jump()
	{
		bool canJump = CharBody3D.IsOnFloor() || (!_coyoteUsed && _timeSinceGrounded <= CoyoteTime);
		bool playJumpSound = false;
		if (canJump)
		{
			_coyoteUsed = true;
			CharacterVelocity.Y = JumpPower;
			playJumpSound = true;
		}
		if (IsSitting)
		{
			playJumpSound = true;
			Unsit();
		}
		if (playJumpSound && JumpSound != null && !JumpSound.Playing)
		{
			JumpSound?.Play();
		}
	}

	/// <summary>
	/// Makes the NPC sit on a specified seat.
	/// </summary>
	[ScriptMethod]
	public void Sit(Seat seat)
	{
		Rpc(nameof(NetSit), seat.NetworkedObjectID);
	}

	/// <summary>
	/// Unsits the NPC from the current seat.
	/// </summary>
	[ScriptMethod]
	public void Unsit(bool addForce = true)
	{
		Rpc(nameof(NetJumpFromSeat));

		// Reset rotation
		Rotation = new(0, Rotation.Y, 0);

		if (addForce)
		{
			Position += SeatOffset * 2;
		}
	}

	[NetRpc(AuthorityMode.Server, TransferMode = TransferMode.Reliable, CallLocal = true)]
	private async void NetSit(string seatID)
	{
		Seat? seat = (Seat?)await Root.WaitForNetObjectAsync(seatID);

		if (seat != null)
		{
			InternalSit(seat);
		}
	}

	private void InternalSit(Seat seat)
	{
		IsSitting = true;
		OverrideNetworkTransform = true;
		SittingIn = seat;
		seat.Occupant = this;
		seat.InvokeSat(this);
		Character?.SetBlendValue(CharacterModel.CharacterModelBlendEnum.Sitting, 1);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable, CallLocal = true)]
	private void NetJumpFromSeat()
	{
		if (IsSitting)
		{
			// Unsit the NPC
			IsSitting = false;
			OverrideNetworkTransform = false;

			if (SittingIn != null)
			{
				SittingIn.Occupant = null;
				SittingIn.InvokeVacated(this);
				SittingIn = null;
			}

			Character?.SetBlendValue(CharacterModel.CharacterModelBlendEnum.Sitting, 0);
		}
	}

	/// <summary>
	/// Equips the NPC with a specified tool.
	/// </summary>
	[ScriptMethod]
	public void EquipTool(Tool tool)
	{
		if (IsDead) return;
		// Check if tool is already held
		if (HoldingTool != null)
		{
			if (this is Player plr)
			{
				plr.UnequipTool();
			}
			else
			{
				DropTool();
			}
		}

		Rpc(nameof(NetEquipTool), tool.NetworkedObjectID);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable, CallLocal = true)]
	private async void NetEquipTool(string networkID)
	{
		NetworkedObject? netObj = await Root.WaitForNetObjectAsync(networkID);

		if (netObj == null) { return; }

		Tool tool = (Tool)netObj;

		if (tool != null)
		{
			HoldingTool = tool;

			// If is authority, attach the tool
			if (HasAuthority)
			{
				tool.Holder = this;
				tool.Parent = this;
			}

			tool.InvokeEquipped();
		}
	}

	/// <summary>
	/// Attach tool to hand
	/// </summary>
	/// <param name="tool"></param>
	private async void InternalAttachTool(Tool tool)
	{
		tool.Holder = this;

		if (_toolRemoteTransform != null && Node.IsInstanceValid(_toolRemoteTransform))
		{
			_toolRemoteTransform.QueueFree();
		}

		_toolRemoteTransform = new()
		{
			UpdatePosition = true,
			UpdateRotation = true,
			UpdateScale = false
		};

		if (Character != null)
		{
			Dynamic attachment = Character.GetAttachment(CharacterModel.CharacterAttachmentEnum.HandRight);
			attachment.GDNode.AddChild(_toolRemoteTransform, @internal: Node.InternalMode.Back);
		}

		// stick and stones
		// this is needed because GetPath doesn't update when it entered tree
		await Globals.Singleton.WaitFrame();
		_toolRemoteTransform.Position = new Vector3(0, 0, 0);
		_toolRemoteTransform.RotationDegrees = new Vector3(0, -90, -90);
		_toolRemoteTransform.UpdateScale = false;
		_toolRemoteTransform.RemotePath = _toolRemoteTransform.GetPathTo(tool.GDNode);
	}

	internal void InternalDetachTool()
	{
		if (_toolRemoteTransform != null && Node.IsInstanceValid(_toolRemoteTransform))
		{
			_toolRemoteTransform?.QueueFree();
		}

		Character?.SetBlendValue(CharacterModel.CharacterModelBlendEnum.ToolHoldRight, 0);
	}

	/// <summary>
	/// Unequips the currently equipped tool from the NPC.
	/// </summary>
	[ScriptMethod, ScriptLegacyMethod("DropTools")]
	public void DropTool()
	{
		if (HoldingTool != null)
		{
			Tool tool = HoldingTool;
			if (this is Player plr)
			{
				plr.UnequipTool();
			}
			Rpc(nameof(NetDropTool), tool.NetworkedObjectID);
		}
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable, CallLocal = true)]
	private async void NetDropTool(string id)
	{
		Tool? tool = (Tool?)await Root.WaitForNetObjectAsync(id);

		if (tool != null && tool.Droppable)
		{
			tool.Reparent(Root.Environment);
			InternalDetachTool();
			tool.InvokeDropped();
		}
	}

	/// <summary>
	/// Loads the appearance of the NPC based on a user ID.
	/// </summary>
	[ScriptMethod]
	public void LoadAppearance(int userID)
	{
		if (Character is PolytorianModel ptm)
		{
			ptm.LoadAppearance(userID, Root.PlayerDefaults.LoadAppearanceTools);
		}
	}

	/// <summary>
	/// Clears the NPC's current appearance.
	/// </summary>
	[ScriptMethod]
	public void ClearAppearance()
	{
		if (Character is PolytorianModel ptm)
		{
			ptm.ClearAppearance();
		}
	}

	/// <summary>
	/// Determines the position the NPC should walk towards.
	/// </summary>
	[ScriptMethod]
	public void SetNavDestination(Vector3 pos)
	{
		MoveTarget = null;
		if (_navAgent == null)
		{
			_navAgentContainer = new();
			_navAgent = new()
			{
				PathDesiredDistance = NavigationDistance,
				TargetDesiredDistance = 0.5f,
				PathHeightOffset = -(CalculateBounds().Size.Y / 2),
				PathMaxDistance = 3f,
			};

			_navAgentContainer.AddChild(_navAgent);
			GDNode3D.AddChild(_navAgentContainer);
			if (Globals.IsInGDEditor)
			{
				_navAgent.DebugEnabled = true;
			}

			_navAgent.NavigationFinished += OnNavFinished;
		}
		_navAgent.TargetPosition = pos;
	}

	private void OnNavFinished()
	{
		_navAgentContainer?.QueueFree();
		_navAgent = null;
		NavFinished.Invoke();
	}

	/// <summary>
	/// Respawns the NPC.
	/// </summary>
	[ScriptMethod]
	public void Respawn()
	{
		Health = MaxHealth;
		Anchored = false;
		IsDead = false;

		if (Character is PolytorianModel ptmodel)
		{
			ptmodel.StopRagdoll();
		}
		CharacterVelocity = Vector3.Zero;

		OverrideCanCollide = false;
		UpdateCollision();
	}

	/// <summary>
	/// Applies damage to the NPC.
	/// </summary>
	[ScriptMethod]
	public void TakeDamage(float dmg)
	{
		Health -= dmg;
	}

	/// <summary>
	/// Heals the NPC by a specified amount.
	/// </summary>
	[ScriptMethod]
	public void Heal(float amount)
	{
		Health += amount;
	}
}
