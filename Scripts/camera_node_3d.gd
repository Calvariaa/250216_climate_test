extends Node3D
var camera: Camera3D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	camera = $Camera3D
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass

func _input(event: InputEvent) -> void:
	_update_camera(event)


func _unhandled_input(_event: InputEvent) -> void:
	pass


var mouse_pos: Vector2
var mouse_move_flag: bool = false
var mouse_rotate_flag: bool = false
var mouse_move_sensitivity: float = 0.001
var mouse_rotation_sensitivity: float = 0.02
var mouse_zoom_sensitivity: float = 0.5
func _update_camera(event: InputEvent) -> void:
	# 平移：按住shift和鼠标中键
	if not mouse_rotate_flag and Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE) and Input.is_key_pressed(KEY_SHIFT):
		if not mouse_move_flag:
			Input.mouse_mode = Input.MOUSE_MODE_CONFINED_HIDDEN
			mouse_pos = get_viewport().get_mouse_position()
			print(mouse_pos)
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
			mouse_move_flag = true

	if mouse_move_flag:
		if event is InputEventMouseMotion:
			var displacement = event.relative

			# Apply camera translation
			camera.translate(Vector3(-displacement.x * mouse_move_sensitivity, displacement.y * mouse_move_sensitivity, 0))
		if not Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE):
			Input.mouse_mode = Input.MOUSE_MODE_CONFINED_HIDDEN
			Input.warp_mouse(mouse_pos)
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
			mouse_move_flag = false

	# 旋转：按住鼠标中键
	if not mouse_move_flag and Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE):
		if not mouse_rotate_flag:
			Input.mouse_mode = Input.MOUSE_MODE_CONFINED_HIDDEN
			mouse_pos = get_viewport().get_mouse_position()
			print(mouse_pos)
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
			mouse_rotate_flag = true

	if mouse_rotate_flag:
		if event is InputEventMouseMotion:
			var displacement = event.relative

			# rotate(Vector3(1, 0, 0), deg_to_rad(displacement.y * mouse_rotation_sensitivity))
			# rotate(Vector3(0, 1, 0), deg_to_rad(-displacement.x * mouse_rotation_sensitivity))
			rotate_x(deg_to_rad(-displacement.y * mouse_rotation_sensitivity));
			rotate_y(deg_to_rad(-displacement.x * mouse_rotation_sensitivity));
		if not Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE):
			Input.mouse_mode = Input.MOUSE_MODE_CONFINED_HIDDEN
			Input.warp_mouse(mouse_pos)
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
			mouse_rotate_flag = false

	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_WHEEL_UP:
			camera.translate(Vector3(0, 0, -mouse_zoom_sensitivity))
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			camera.translate(Vector3(0, 0, mouse_zoom_sensitivity))
