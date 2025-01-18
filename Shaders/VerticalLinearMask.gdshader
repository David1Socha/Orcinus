shader_type canvas_item;
uniform float percentVisible: hint_range(0, 1) = 1;

void fragment() {
	bool isVisible = (UV.y + percentVisible) > 1.;
	vec4 newColor = texture(TEXTURE, UV).rgba;
	if (!isVisible) {
		newColor.a = 0.0;
	}
	
	COLOR = newColor;
}