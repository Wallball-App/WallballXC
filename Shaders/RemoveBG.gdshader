shader_type canvas_item;

uniform vec4 remove_color : source_color = vec4(1.0);
uniform vec4 replace : source_color = vec4(0.0);
uniform float tolerance = 0.01f;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 current = texture(TEXTURE, UV);
	float difference = distance(current, remove_color);
	
	if(difference <= tolerance) {
		COLOR = replace;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
