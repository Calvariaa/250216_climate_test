shader_type spatial;
#include "debug_number.gdshaderinc"

uniform vec4 text_colour : source_color = vec4(0.8, 0.85, 0.7, 1.0);
uniform vec4 background_colour : source_color;


void vertex() {
	int leading_digits = 9; // digits before the decimal separator
	int fractional_digits = 6; // digits after the decimal separator
	float[] numbers = {
		PI,
		TIME,
		1.0/200.0, // screen width
		1.0/200.0 // screen height
	};
	 
	int number_index = int(floor(UV.y * float(numbers.length())));
	
	float number = numbers[number_index];
	COLOR = am_ndbg_display_float(
		number,
		vec2(UV.x, mod(UV.y, 1.0 / float(numbers.length()))*float(numbers.length())),
		leading_digits, fractional_digits, 10
	) ? text_colour : background_colour;
}

void fragment() {
    ALBEDO = COLOR.rgb;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
