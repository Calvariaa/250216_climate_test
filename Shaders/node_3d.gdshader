shader_type spatial;

uniform float radius = 1.0;

void vertex() {
	// Called for every vertex the material is visible on.
	vec3 toward = normalize(VERTEX);
	vec3 pos;
		pos.x = toward.x * sqrt(1.0 - (pow(toward.y, 2) / 2.0) - (pow(toward.z, 2) / 2.0) + ((pow(toward.y, 2) + pow(toward.z, 2)) / 3.0));
		pos.y = toward.y * sqrt(1.0 - (pow(toward.x, 2) / 2.0) - (pow(toward.z, 2) / 2.0) + ((pow(toward.x, 2) + pow(toward.z, 2)) / 3.0));
		pos.z = toward.z * sqrt(1.0 - (pow(toward.x, 2) / 2.0) - (pow(toward.y, 2) / 2.0) + ((pow(toward.x, 2) + pow(toward.y, 2)) / 3.0));
	VERTEX = normalize(pos) * radius;
	//VERTEX = toward * radius;
	NORMAL = normalize(VERTEX);
}

void fragment() {
	// Called for every pixel the material is visible on.
	ALBEDO = vec3(1.0, 0.6, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
