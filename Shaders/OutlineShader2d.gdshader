shader_type canvas_item;

uniform float stroke = 1.0;
uniform vec4 outlineColor : hint_color;
uniform float alphaCutoff = .1;

void fragment(){
    vec2 size = stroke * 1. / vec2(textureSize(TEXTURE, 0).xy);
    float highestAlphaAmongNeighbors = .0;
    for(float x = -1.; x <= 1.; x += 1.){
        for(float y = -1.; y <= 1.0; y += 1.){
            vec2 uv = UV + normalize(vec2(x, y)) * size;
            highestAlphaAmongNeighbors = max(highestAlphaAmongNeighbors, texture(TEXTURE, uv).a);
        }
    }
    vec4 origin = texture(TEXTURE, UV);
    COLOR.rgb = mix(outlineColor.rgb, origin.rgb, origin.a);
	if (highestAlphaAmongNeighbors > alphaCutoff) {
		COLOR.a = highestAlphaAmongNeighbors;
	}
	else {
		COLOR.a = 0.;
	}
}
