using Godot;
using System;

public partial class CameraController : Node3D
{
	private Node3D _node;
	private Camera3D _camera;

	private Vector2 _mousePos;
	private bool _mouseMoveFlag = false;
	private bool _mouseRotateFlag = false;
	private float _mouseMoveSensitivity = 0.1f;
	private float _mouseRotationSensitivity = 0.2f;
	private float _mouseZoomSensitivity = 0.5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("CameraX/Camera3D");
		_node = GetNode<Node3D>("CameraX");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		UpdateCamera(@event);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
	}

	private void UpdateCamera(InputEvent @event)
	{
		float distanceToOrigin = 1.0f;

		// Translation: Hold Shift + Middle Mouse Button
		if (!_mouseRotateFlag && Input.IsMouseButtonPressed(MouseButton.Left) && Input.IsKeyPressed(Key.Shift))
		{
			if (!_mouseMoveFlag)
			{
				Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
				_mousePos = GetViewport().GetMousePosition();
				GD.Print(_mousePos);
				Input.MouseMode = Input.MouseModeEnum.Captured;
				_mouseMoveFlag = true;
			}
		}

		if (_mouseMoveFlag)
		{
			if (@event is InputEventMouseMotion motionEvent)
			{
				Vector2 displacement = motionEvent.Relative;

				// Apply camera translation
				_camera.Translate(new Vector3(
					-displacement.X * _mouseMoveSensitivity * distanceToOrigin,
					displacement.Y * _mouseMoveSensitivity * distanceToOrigin,
					0));
			}

			if (!Input.IsMouseButtonPressed(MouseButton.Left))
			{
				Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
				Input.WarpMouse(_mousePos);
				Input.MouseMode = Input.MouseModeEnum.Visible;
				_mouseMoveFlag = false;
			}
		}

		// Rotation: Hold Middle Mouse Button
		if (!_mouseMoveFlag && Input.IsMouseButtonPressed(MouseButton.Left))
		{
			if (!_mouseRotateFlag)
			{
				Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
				_mousePos = GetViewport().GetMousePosition();
				GD.Print(_mousePos);
				Input.MouseMode = Input.MouseModeEnum.Captured;
				_mouseRotateFlag = true;
			}
		}

		if (_mouseRotateFlag)
		{
			if (@event is InputEventMouseMotion motionEvent)
			{
				Vector2 displacement = motionEvent.Relative;

				if (Mathf.Abs(_node.Rotation.X - (1e-6f)) < Mathf.Pi / 2 || _node.Rotation.X * displacement.Y > 0)
				{
					_node.RotateX(Mathf.DegToRad(-displacement.Y * _mouseRotationSensitivity * distanceToOrigin));
				}
				RotateY(Mathf.DegToRad(-displacement.X * _mouseRotationSensitivity * distanceToOrigin));
			}

			if (!Input.IsMouseButtonPressed(MouseButton.Left))
			{
				Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
				Input.WarpMouse(_mousePos);
				Input.MouseMode = Input.MouseModeEnum.Visible;
				_mouseRotateFlag = false;
			}
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				_camera.Translate(new Vector3(0, 0, -_mouseZoomSensitivity * distanceToOrigin));
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				_camera.Translate(new Vector3(0, 0, _mouseZoomSensitivity * distanceToOrigin));
			}
		}
	}
}
