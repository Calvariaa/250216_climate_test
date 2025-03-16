/* MIT License

Copyright (c) 2025 Asymptotic Manifolds

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

#define O false
#define X true

// charset is `0123456789. `
const bool[] _AM_NUM_BITMAP_10 = {
X, X, X,  X, X, O,  X, X, X,  X, X, X,  X, O, X,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  O, O, O,  O, O, O,  O, O, O,
X, O, X,  O, X, O,  O, O, X,  O, O, X,  X, O, X,  X, O, O,  X, O, O,  O, O, X,  X, O, X,  X, O, X,  O, O, O,  O, O, O,  O, O, O,
X, O, X,  O, X, O,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  O, X, O,  X, X, X,  X, X, X,  O, O, O,  O, O, O,  X, X, X,
X, O, X,  O, X, O,  X, O, O,  O, O, X,  O, O, X,  O, O, X,  X, O, X,  X, O, O,  X, O, X,  O, O, X,  O, O, O,  O, O, O,  O, O, O,
X, X, X,  X, X, X,  X, X, X,  X, X, X,  O, O, X,  X, X, X,  X, X, X,  X, O, O,  X, X, X,  O, O, X,  O, X, O,  O, O, O,  O, O, O
};

// charset is `0123456789ABCDEF. `
const bool[] _AM_NUM_BITMAP_16 = {
X, X, X,  X, X, O,  X, X, X,  X, X, X,  X, O, X,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  O, X, O,  X, X, O,  X, X, X,  X, X, O,  X, X, X,  X, X, X,  O, O, O,  O, O, O,  O, O, O,
X, O, X,  O, X, O,  O, O, X,  O, O, X,  X, O, X,  X, O, O,  X, O, O,  O, O, X,  X, O, X,  X, O, X,  X, O, X,  X, O, X,  X, O, O,  X, O, X,  X, O, O,  X, O, O,  O, O, O,  O, O, O,  O, O, O,
X, O, X,  O, X, O,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  X, X, X,  O, X, O,  X, X, X,  X, X, X,  X, X, X,  X, X, O,  X, O, O,  X, O, X,  X, X, X,  X, X, X,  O, O, O,  O, O, O,  X, X, X,
X, O, X,  O, X, O,  X, O, O,  O, O, X,  O, O, X,  O, O, X,  X, O, X,  X, O, O,  X, O, X,  O, O, X,  X, O, X,  X, O, X,  X, O, O,  X, O, X,  X, O, O,  X, O, O,  O, O, O,  O, O, O,  O, O, O,
X, X, X,  X, X, X,  X, X, X,  X, X, X,  O, O, X,  X, X, X,  X, X, X,  X, O, O,  X, X, X,  O, O, X,  X, O, X,  X, X, O,  X, X, X,  X, X, O,  X, X, X,  X, O, O,  O, X, O,  O, O, O,  O, O, O
};

#undef O
#undef X

const int _AM_NUM_CHAR_WIDTH = 3;
const int _AM_NUM_CHAR_HEIGHT = 5;
const int _AM_NUM_CHAR_EXTRA_COUNT = 3; // `.`, ` ` and `-`
const int _AM_NUM_CHAR_MARGIN = 1;


float _am_ndbg_remap(float x, float from_a, float from_b, float to_a, float to_b) {
	x = x - from_a;
	x = x / (from_b - from_a);
	return to_a + x * (to_b - to_a);
}

bool _am_ndbg_sample_font(vec2 pos, int digit, int base) {
	vec2 effective_margin = vec2(
		1.0 / (float(_AM_NUM_CHAR_WIDTH) + 1.0 / float(_AM_NUM_CHAR_MARGIN)),
		1.0 / (float(_AM_NUM_CHAR_HEIGHT) + 1.0 / float(_AM_NUM_CHAR_MARGIN))
	);
	
	pos.x = _am_ndbg_remap(pos.x, effective_margin.x, 1.0-effective_margin.y, 0.0, 1.0);
	pos.y = _am_ndbg_remap(pos.y, effective_margin.y, 1.0-effective_margin.y, 0.0, 1.0);
	
	if (pos.x < 0.0 || pos.x > 1.0 || pos.y < 0.0 || pos.y > 1.0) {
		return false;
	} else {
		int row = int(pos.y * float(_AM_NUM_CHAR_HEIGHT));
		int col = int(pos.x * float(_AM_NUM_CHAR_WIDTH)) + digit * _AM_NUM_CHAR_WIDTH;
		
		int index = col + row * (_AM_NUM_CHAR_WIDTH * (base + _AM_NUM_CHAR_EXTRA_COUNT));
		if (base == 10) {
			return _AM_NUM_BITMAP_10[index];
		} else {
			return _AM_NUM_BITMAP_16[index];
		}
	}
}

bool am_ndbg_display_float(float number, vec2 uv, int leading_digits, int fractional_digits, int base) {
	bool was_negative = number < 0.0;
	number = abs(number);
	
	bool show_fractions = fractional_digits > 0;
	
	int fractional_offset = show_fractions ? fractional_digits + 1 : 0;
	int total_leading_digits = leading_digits + fractional_offset;
	
	int digit_position = int(floor(float(total_leading_digits) * (1.0-uv.x))) - fractional_offset;
	
	bool at_fractional_separator = digit_position == -1;
	float this_digit;
	vec2 position_within_digit = mod(uv, vec2(1.0/float(total_leading_digits),1.0)) * vec2(float(total_leading_digits), 1.0);
	if (at_fractional_separator) {
		this_digit = float(base); // the fractional separator
	} else if (was_negative && digit_position == leading_digits - 1){
		this_digit = float(base) + 2.0;
	} else {
		if (digit_position <= -2) { // because the fractional separator "loses" a digit otherwise
			digit_position += 1;
		}
		
		// the "column" values, e.g. 1, 10, 1000, 0.1 etc.
		float this_column_value = pow(float(base), float(digit_position));
		float next_column_value = float(base) * this_column_value;
		float prev_column_value = this_column_value / float(base);
		
		if (float(number) < this_column_value && digit_position > 0) {
			this_digit = float(base) + 1.0; // space character
		} else {
			this_digit = floor(mod(float(number), next_column_value) / this_column_value);
		}
	}
	
	return _am_ndbg_sample_font(position_within_digit, int(this_digit), base);
}
