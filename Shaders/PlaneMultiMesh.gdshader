shader_type spatial;

uniform uint cell_length;
uniform sampler2D temperature_texture;
uniform float temperature_test = 0.0f;

uvec2 id_to_uv(uint id) {
    uint face_size = cell_length;
    uint face_id = id / (face_size * face_size);
    uint local_id = id % (face_size * face_size);
    return uvec2(
        face_id * face_size + (local_id % face_size),
        local_id / face_size
    );
}

vec4 hsv2rgb(vec4 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return vec4(c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y), c.w);
}

void vertex() {
    // Called for every vertex the material is visible on.

    uint id = uint(INSTANCE_CUSTOM.x);

    // Convert the ID to UV coordinates
    uvec2 uv = id_to_uv(id);

    // Normalize UV coordinates to [0, 1] range
    vec2 normalized_uv = vec2(uv) / float(cell_length * 6u);

    // Sample the temperature from the texture
    float temperature = texture(temperature_texture, normalized_uv).r;
    // float temperature = float(id % (255u));
    // temperature_test = temperature;

    // Clamp the temperature to the desired range
    float clampedTemp = clamp(temperature, -120.0, 120.0);

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
    ALBEDO = COLOR.rgb;
}