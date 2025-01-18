shader_type canvas_item;

uniform float max_alpha: hint_range(0., 1.);

void fragment() {
	vec4 col = texture(TEXTURE, UV);
	if (col.a > max_alpha) {
		col.a = max_alpha;
	}
	
	COLOR = col;
}