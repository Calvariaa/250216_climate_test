shader_type spatial;

// uniform uint cell_length;
uniform float[256*256*6] temperature_data;

vec4 hsv2rgb(vec4 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return vec4(c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y), c.w);
}

void vertex() {
    // Called for every vertex the material is visible on.

    // float clampedTemp = clamp(INSTANCE_CUSTOM.x , -120.0, 120.0);
    float clampedTemp = clamp(temperature_data[uint(INSTANCE_CUSTOM.x)] , -120.0, 120.0);
    //float clampedTemp = clamp(float(int(INSTANCE_CUSTOM.x) % 120) , -120.0, 120.0);
    float hue;

    if (clampedTemp > 0.0) {
        hue = (65.0 - clampedTemp * 13.0 / 24.0) / 360.0;
    } else {
        hue = (65.0 - clampedTemp * 47.0 / 24.0) / 360.0;
    }

    vec4 color = hsv2rgb(vec4(hue, 0.64, 1.0, 1.0));
    COLOR = color;
}

void fragment() {
    //static int i = 0;
    //ALBEDO = color;
	 ALBEDO = COLOR.rgb;

    // uint id=vertex_instance_id;
}  

//void light() {
//    // Called for every pixel for every light affecting the material.
//    // Uncomment to replace the default light processing function with this one.
//}