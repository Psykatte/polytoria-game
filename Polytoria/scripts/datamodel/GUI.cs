// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
#if CREATOR
using Polytoria.Datamodel.Creator;
#endif

namespace Polytoria.Datamodel;

/// <summary>
/// GUI is a class that is used to create a GUI.
/// </summary>
[Instantiable]
[DocCategory("ui")]
public partial class GUI : Instance
{
	private Control _control = null!;
	private bool _visible = true;
	private int _zIndex = 0;

	/// <summary>
	/// Determines whether the GUI is visible or not.
	/// </summary>
	[Editable, ScriptProperty]
	public bool Visible
	{
		get => _visible;
		set
		{
			_visible = value;
			_control.Visible = _visible;
			RecomputeVisible();
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the ZIndex (layer) of the GUI.
	/// </summary>
	[Editable, ScriptProperty]
	public int ZIndex
	{
		get => _zIndex;
		set
		{
			_zIndex = value;
			_control.ZIndex = value;
			OnPropertyChanged();
		}
	}

	public Vector2 AbsoluteSize => _control.Size;

	public override Node CreateGDNode()
	{
		_control = new() { MouseFilter = Control.MouseFilterEnum.Ignore };
		return _control;
	}

	public override void Init()
	{
		base.Init();

		// Make fullrect
		_control.SetAnchorsPreset(Control.LayoutPreset.FullRect, true);
		_control.Resized += RecomputeChildTransforms;
	}

	public override void EnterTree()
	{
		RecomputeVisible();
		base.EnterTree();
	}

	public void RecomputeVisible()
	{
		bool isValidParent = Parent is PlayerGUI or GUI3D
#if CREATOR
		or CreatorGUI
#endif
			;

		_control.Visible = _visible && isValidParent;
	}

	public override void PreDelete()
	{
		_control.Resized -= RecomputeChildTransforms;
		base.PreDelete();
	}

	protected void RecomputeChildTransforms()
	{
		foreach (Instance item in GetChildren())
		{
			if (item is UIField uifield)
			{
				uifield.RecomputeTransform();
			}
		}
	}
}
