// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Datamodel.Resources;
using Polytoria.Scripting;
using Polytoria.Shared;

namespace Polytoria.Datamodel;

/// <summary>
/// Explosion is a deadly explosion killing players and applying force to parts at the given position.
/// </summary>
[Instantiable]
public partial class Explosion : Dynamic
{
	private const float ExplosionParticleTimeSec = 10f;

	private GpuParticles3D _particle = null!;
	private float _radius = 10;
	private float _force = 5000;
	private bool _affectAnchored = false;
	private float _damage = 100000;

	/// <summary>
	/// Determines the radius of this explosion
	/// </summary>
	[Editable, ScriptProperty]
	public float Radius
	{
		get => _radius;
		set
		{
			_radius = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the force of this explosion that will be applied to affected hits
	/// </summary>
	[Editable, ScriptProperty]
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
	/// Determines if this explosion should affect anchored parts or not
	/// </summary>
	[Editable, ScriptProperty]
	public bool AffectAnchored
	{
		get => _affectAnchored;
		set
		{
			_affectAnchored = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Damage that is applied to the player
	/// </summary>
	[Editable, ScriptProperty]
	public float Damage
	{
		get => _damage;
		set
		{
			_damage = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// A predicate function deciding whether this part should be accepted or not Example usage: ``<c>lua explosion.AffectPredicate = function(hit) -- always explode return true end </c>``
	/// </summary>
	[ScriptProperty] public PTFunction? AffectPredicate { get; set; }

	/// <summary>
	/// Fires when this explosion affects an instance
	/// </summary>
	[ScriptProperty] public PTSignal<Instance> Touched { get; private set; } = new();

	public override Node CreateGDNode()
	{
		return Globals.LoadNetworkedObjectScene(ClassName)!;
	}

	public override void Init()
	{
		base.Init();
		_particle = GDNode.GetNode<GpuParticles3D>("Particles");
		_particle.Visible = false;
	}

	public override void Ready()
	{
		TryIgnite();
		base.Ready();
	}

	public override void EnterTree()
	{
		TryIgnite();
		base.EnterTree();
	}

	private async void TryIgnite()
	{
		if (!IsNetworkReady || IsHidden) return;
		_particle.Scale = Vector3.One * _radius / 15;
		_particle.Visible = true;
		_particle.Emitting = true;

		BuiltInAudioAsset audio = New<BuiltInAudioAsset>();
		audio.AudioPreset = BuiltInAudioAsset.BuiltInAudioPresetEnum.Explosion;

		Sound? s = null;

		if (!Root.Network.IsServer)
		{
			s = New<Sound>();
			s.Audio = audio;
			s.PlayInWorld = true;
			s.Parent = this;
			s.LocalPosition = Vector3.Zero;
		}

		Instance[] overlaps = Root.Environment.OverlapSphere(Position, Radius);

		foreach (Instance item in overlaps)
		{
			Touched.Invoke(item);

			if (AffectPredicate != null)
			{
				object?[] res = await AffectPredicate.Call(item);
				if (!(res.Length == 1 && res[0] is bool b && b))
				{
					continue;
				}
			}

			if (item is Entity e && !item.IsDescendantOfClass("Accessory"))
			{
				if (e.Anchored && !AffectAnchored && AffectPredicate == null) continue;

				RigidBody3D body = e.GDRigidBody;
				Vector3 direction = body.GlobalTransform.Origin - GetGlobalTransform().Origin;
				float distance = direction.Length();
				bool unanchor = true;

				direction = direction.Normalized();

				if ((e.Size.X > Radius * 1.3 || e.Size.Y > Radius * 1.3 || e.Size.Z > Radius * 1.3) && AffectPredicate == null)
				{
					unanchor = false;
				}

				if (unanchor)
				{
					e.Anchored = false;
				}

				float forceMagnitude = Force * (1 - (distance / Radius));
				Vector3 force = direction * forceMagnitude / 100;

				body.ApplyCentralImpulse(force);
			}
			else if (item is Player plr)
			{
				if (plr.IsDead) continue;

				plr.TakeDamage(Damage);
				AddPlrExplosionForce(plr);
			}
		}

		// Play sound on next frame, needed to be loaded
		if (s != null)
		{
			Callable.From(s.Play).CallDeferred();
		}

		await Globals.Singleton.WaitAsync(ExplosionParticleTimeSec);

		Delete();
	}

	private void AddPlrExplosionForce(Player player)
	{
		float force = Force * 0.02f;
		Vector3 dir = player.GetGlobalTransform().Origin - GetGlobalTransform().Origin;
		float wearoff = 1 - (dir.Length() / (Radius * 2f));
		wearoff = Mathf.Max(Mathf.Clamp(wearoff, 0, 1), 0.1f);
		Vector3 f = dir.Normalized() * force;
		f.X *= 1.5f;
		f.Z *= 1.5f;

		player.CharacterVelocity = f * wearoff;
	}
}
