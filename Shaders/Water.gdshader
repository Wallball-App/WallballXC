/*CREDIT: StayAtHomeDev (YouTube)*/

shader_type spatial;

uniform vec4 primary_color : source_color;
uniform float roughness = 0.2;

uniform sampler2D normalmap_a : hint_normal;
uniform sampler2D normalmap_b : hint_normal;
uniform float a_strength = 0.3;
uniform float b_strength = 0.4;

uniform vec2 direction = vec2(0.1, 0.1);
uniform float movement_strength = 0.5;
uniform float uv_scale = 1.0;

float fresnel(float amount, vec3 normal, vec3 view) {
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0)), amount);
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float basic_fresnel = fresnel(3.0, NORMAL, VIEW);
	
	vec2 uv = UV * uv_scale;
	vec2 uv_movement = direction * TIME * movement_strength;
	
	vec3 normalmap = texture(normalmap_a, uv + uv_movement).rgb * a_strength;
	normalmap += texture(normalmap_b, uv - uv_movement).rgb * b_strength;
	
	ALBEDO = primary_color.rgb + (0.2 * basic_fresnel);
	ROUGHNESS = roughness * (1.0 - basic_fresnel);
	NORMAL_MAP = normalmap;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
