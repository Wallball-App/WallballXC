shader_type spatial;

uniform vec4 Color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float Rows = 5.0;
uniform float Columns = 5.0;
uniform float Overlap = 0.1;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float plankX = fract(UV.y * Rows);
	float sl = plankX;
	ALBEDO = Color.rgb * (1.0 - (sl * Overlap));
	
	float plankY = fract(UV.x * Columns);
	float sl2 = plankY;
	ALBEDO *= Color.rgb * (1.0 - (sl2 * Overlap));

	vec3 normal = vec3(0.0, Overlap, 1.0);
	NORMAL = normalize(normal);
	
	vec3 normal2 = vec3(Overlap, 0.0, 1.0);
	NORMAL += normalize(normal2);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
