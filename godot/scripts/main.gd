extends Node3D

const ROAD_RADIUS := 34.0
const ROAD_WIDTH := 12.0
const SEGMENTS := 96

var car: CharacterBody3D
var camera: Camera3D
var speed := 0.0
var steering := 0.0
var drift_angle := 0.0
var throttle_touch := false
var brake_touch := false
var left_touch := false
var right_touch := false
var handbrake_touch := false
var hud_speed: Label
var hud_status: Label

func _ready() -> void:
	_build_world()
	_build_car()
	_build_camera()
	_build_hud()

func _build_world() -> void:
	var env := WorldEnvironment.new()
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color(0.008, 0.012, 0.03)
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(0.42, 0.48, 0.7)
	environment.ambient_light_energy = 0.55
	environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.environment = environment
	add_child(env)

	var ground := MeshInstance3D.new()
	var ground_mesh := PlaneMesh.new()
	ground_mesh.size = Vector2(180, 180)
	ground.mesh = ground_mesh
	ground.position.y = -0.18
	var ground_mat := StandardMaterial3D.new()
	ground_mat.albedo_color = Color(0.025, 0.035, 0.045)
	ground_mat.roughness = 1.0
	ground.material_override = ground_mat
	add_child(ground)

	# Circular night drift course, assembled from road segments.
	for i in range(SEGMENTS):
		var a := TAU * float(i) / SEGMENTS
		var b := TAU * float(i + 1) / SEGMENTS
		var mid := (a + b) * 0.5
		var length := ROAD_RADIUS * TAU / SEGMENTS + 0.22
		var road := MeshInstance3D.new()
		var box := BoxMesh.new()
		box.size = Vector3(ROAD_WIDTH, 0.22, length)
		road.mesh = box
		road.position = Vector3(cos(mid) * ROAD_RADIUS, -0.04, sin(mid) * ROAD_RADIUS)
		road.rotation.y = -mid
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(0.055, 0.065, 0.078)
		mat.roughness = 0.95
		road.material_override = mat
		add_child(road)

		# Outer and inner neon safety markers.
		for side in [-1.0, 1.0]:
			if i % 3 == 0:
				var post := MeshInstance3D.new()
				var pole := CylinderMesh.new()
				pole.top_radius = 0.12
				pole.bottom_radius = 0.16
				pole.height = 1.2
				post.mesh = pole
				var radius: float = ROAD_RADIUS + float(side) * (ROAD_WIDTH * 0.5 + 1.0)
				post.position = Vector3(cos(mid) * radius, 0.5, sin(mid) * radius)
				var pm := StandardMaterial3D.new()
				pm.albedo_color = Color(0.05, 0.7, 1.0) if side > 0 else Color(1.0, 0.18, 0.48)
				pm.emission_enabled = true
				pm.emission = pm.albedo_color
				pm.emission_energy_multiplier = 1.8
				post.material_override = pm
				add_child(post)

	# Distant city silhouettes.
	for i in range(36):
		var angle := TAU * float(i) / 36.0
		var building := MeshInstance3D.new()
		var block := BoxMesh.new()
		var h := randf_range(5.0, 18.0)
		block.size = Vector3(randf_range(2.0, 5.0), h, randf_range(2.0, 5.0))
		building.mesh = block
		building.position = Vector3(cos(angle) * randf_range(53.0, 68.0), h * 0.5 - 0.1, sin(angle) * randf_range(53.0, 68.0))
		var bm := StandardMaterial3D.new()
		bm.albedo_color = Color(0.018, 0.025, 0.055)
		building.material_override = bm
		add_child(building)

	var moon := DirectionalLight3D.new()
	moon.rotation_degrees = Vector3(-55, -25, 0)
	moon.light_energy = 0.65
	moon.light_color = Color(0.48, 0.62, 1.0)
	add_child(moon)

func _build_car() -> void:
	car = CharacterBody3D.new()
	car.position = Vector3(ROAD_RADIUS, 0.65, 0)
	add_child(car)

	var body := MeshInstance3D.new()
	var body_mesh := BoxMesh.new()
	body_mesh.size = Vector3(2.05, 0.55, 4.6)
	body.mesh = body_mesh
	body.position.y = 0.48
	var white := StandardMaterial3D.new()
	white.albedo_color = Color(0.88, 0.91, 0.94)
	white.metallic = 0.62
	white.roughness = 0.24
	body.material_override = white
	car.add_child(body)

	var cabin := MeshInstance3D.new()
	var cabin_mesh := BoxMesh.new()
	cabin_mesh.size = Vector3(1.68, 0.62, 2.15)
	cabin.mesh = cabin_mesh
	cabin.position = Vector3(0, 1.02, -0.18)
	var glass := StandardMaterial3D.new()
	glass.albedo_color = Color(0.025, 0.07, 0.12, 0.9)
	glass.metallic = 0.35
	glass.roughness = 0.12
	cabin.material_override = glass
	car.add_child(cabin)

	# Blue wheels and low side skirts.
	for x in [-1.08, 1.08]:
		for z in [-1.48, 1.48]:
			var wheel := MeshInstance3D.new()
			var cyl := CylinderMesh.new()
			cyl.top_radius = 0.42
			cyl.bottom_radius = 0.42
			cyl.height = 0.22
			wheel.mesh = cyl
			wheel.rotation.z = PI * 0.5
			wheel.position = Vector3(x, 0.22, z)
			var blue := StandardMaterial3D.new()
			blue.albedo_color = Color(0.03, 0.22, 0.95)
			blue.metallic = 0.75
			blue.roughness = 0.22
			wheel.material_override = blue
			car.add_child(wheel)

	var spoiler := MeshInstance3D.new()
	var spoiler_mesh := BoxMesh.new()
	spoiler_mesh.size = Vector3(2.2, 0.12, 0.32)
	spoiler.mesh = spoiler_mesh
	spoiler.position = Vector3(0, 0.95, 2.0)
	spoiler.material_override = white
	car.add_child(spoiler)

func _build_camera() -> void:
	camera = Camera3D.new()
	camera.current = true
	camera.fov = 72
	add_child(camera)
	camera.position = car.global_position + Vector3(0, 5.4, 9.5)
	camera.look_at(car.global_position + Vector3(0, 0.6, 0), Vector3.UP)

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var title := Label.new()
	title.text = "MARK II  //  NIGHT DRIFT"
	title.position = Vector2(24, 18)
	title.add_theme_font_size_override("font_size", 25)
	title.add_theme_color_override("font_color", Color(0.3, 0.82, 1.0))
	layer.add_child(title)

	hud_speed = Label.new()
	hud_speed.position = Vector2(24, 58)
	hud_speed.add_theme_font_size_override("font_size", 22)
	layer.add_child(hud_speed)

	hud_status = Label.new()
	hud_status.position = Vector2(24, 90)
	hud_status.add_theme_font_size_override("font_size", 17)
	hud_status.add_theme_color_override("font_color", Color(1.0, 0.35, 0.68))
	layer.add_child(hud_status)

	_add_hold_button(layer, "◀", Vector2(24, -116), Vector2(82, 82), "left")
	_add_hold_button(layer, "▶", Vector2(118, -116), Vector2(82, 82), "right")
	_add_hold_button(layer, "BRAKE", Vector2(-210, -112), Vector2(88, 76), "brake")
	_add_hold_button(layer, "GAS", Vector2(-112, -112), Vector2(88, 76), "gas")
	_add_hold_button(layer, "HANDBRAKE", Vector2(-160, -205), Vector2(136, 62), "handbrake")

func _add_hold_button(layer: CanvasLayer, caption: String, offset: Vector2, size: Vector2, action: String) -> void:
	var button := Button.new()
	button.text = caption
	button.size = size
	button.anchor_left = 0.0 if offset.x >= 0 else 1.0
	button.anchor_right = 0.0 if offset.x >= 0 else 1.0
	button.anchor_top = 1.0
	button.anchor_bottom = 1.0
	button.position = offset
	button.add_theme_font_size_override("font_size", 18)
	button.modulate = Color(0.75, 0.88, 1.0, 0.88)
	button.button_down.connect(func(): _set_touch(action, true))
	button.button_up.connect(func(): _set_touch(action, false))
	layer.add_child(button)

func _set_touch(action: String, value: bool) -> void:
	match action:
		"left": left_touch = value
		"right": right_touch = value
		"gas": throttle_touch = value
		"brake": brake_touch = value
		"handbrake": handbrake_touch = value

func _physics_process(delta: float) -> void:
	var gas := Input.is_action_pressed("throttle") or throttle_touch
	var brake := Input.is_action_pressed("brake") or brake_touch
	var left := Input.is_action_pressed("steer_left") or left_touch
	var right := Input.is_action_pressed("steer_right") or right_touch
	var handbrake := Input.is_action_pressed("handbrake") or handbrake_touch

	if gas:
		speed = move_toward(speed, 27.0, 18.0 * delta)
	elif brake:
		speed = move_toward(speed, -7.0, 22.0 * delta)
	else:
		speed = move_toward(speed, 0.0, 3.2 * delta)

	var steer_input := float(right) - float(left)
	steering = move_toward(steering, steer_input, 4.0 * delta)
	var grip := 0.72 if handbrake else 2.7
	drift_angle = move_toward(drift_angle, steering * (0.72 if handbrake else 0.28), (1.0 + grip) * delta)
	car.rotation.y -= steering * clampf(absf(speed) / 12.0, 0.0, 1.4) * delta
	var forward := -car.global_transform.basis.z
	var sideways := car.global_transform.basis.x
	var velocity_vector := forward * speed + sideways * drift_angle * absf(speed) * 0.72
	car.velocity.x = velocity_vector.x
	car.velocity.z = velocity_vector.z
	car.move_and_slide()

	var target := car.global_position + Vector3(0, 5.2, 0) - forward * 8.8
	camera.global_position = camera.global_position.lerp(target, 4.0 * delta)
	camera.look_at(car.global_position + forward * 2.0 + Vector3(0, 0.8, 0), Vector3.UP)

	hud_speed.text = "SPEED  %03d km/h" % int(absf(speed) * 4.2)
	hud_status.text = "DRIFT  //  HANDBRAKE" if handbrake else "DRIFT  //  READY"
