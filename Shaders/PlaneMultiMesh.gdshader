shader_type spatial;

#include "debug_number.gdshaderinc"

uniform uint cell_length;
uniform sampler2D temperature_texture : filter_nearest;
//uniform float temperature_test = 0.0f;

uniform float radius = 1;
uniform float line_width = 1; // 控制线段的宽度

uvec2 id_to_uv(uint id) {
    // 32 * 32 * 6 = 6144

    // 3238

    // 32
    uint face_size = cell_length;

    // 3238 / (1024) = 3
    uint face_id = id / (face_size * face_size);

    // 3238 % (1024) = 166
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
    uvec2 texture_uv = id_to_uv(id);

    // Normalize UV coordinates to [0, 1] range
    // vec2 normalized_uv = vec2(uv) / float(cell_length * 6u);
    vec2 normalized_uv = vec2(texture_uv) / vec2(float(cell_length) * 6.f, float(cell_length));

    // Sample the temperature from the texture
    float temperature = texture(temperature_texture, normalized_uv).r;
    // float temperature = texture(temperature_texture, UV).r;
    //  float temperature = float(id % (120u));
    // temperature_test = temperature;

    // Clamp the temperature to the desired range
    //float clampedTemp = clamp(temperature, -120.0, 120.0);
    float clampedTemp = temperature;

    float hue;

    if (clampedTemp > 0.0) {
        hue = (65.0 - clampedTemp * 13.0 / 24.0) / 360.0;
    } else {
        hue = (65.0 - clampedTemp * 47.0 / 24.0) / 360.0;
    }

    vec4 color = hsv2rgb(vec4(hue, 0.64, 1.0, 1.0));
    COLOR = color;
    // COLOR = texture(temperature_texture, UV).rgba

    // Set Sphere VERTEX position
	vec3 absolute_direction = NODE_POSITION_WORLD;
    vec2 position_uv = UV;
    //float t = position_uv.x * TAU;
    //float p = position_uv.y * PI;
//
    //float x = radius * sin(p) * cos(t);
    //float y = radius * cos(p);
    //float z = radius * sin(p) * sin(t);
//
    //VERTEX = vec3(x, y, z);
    //NORMAL = normalize(VERTEX);
}

void fragment() {
    ALBEDO = COLOR.rgb;
    //ALBEDO = texture(temperature_texture, UV).rgb;
}