shader_type spatial;

uniform float timeScaleFactor = .04;
uniform sampler2D noise_texture: repeat_enable, filter_linear;

void fragment() {
	vec2 fragCoord = UV;

	vec4 texColor = texture(noise_texture, vec2(UV.x, UV.y + TIME * timeScaleFactor));

	ALBEDO = vec3(0, 0, texColor.z);
}