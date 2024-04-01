shader_type spatial;
render_mode unshaded;

uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;
uniform sampler2D uncoveredWorldPositions : filter_linear_mipmap_anisotropic;
uniform sampler2D fogTexture;
uniform float fog_speed = 5.0;
uniform float repeat_interval = 10.0;
uniform vec2 offset;

varying mat4 CAMERA;

void vertex() {
	CAMERA = INV_VIEW_MATRIX;
	POSITION = vec4(VERTEX, 1.0);
}

vec2 world_to_uv(vec3 world_position) {
    // Calculate the UV coordinates with repeating pattern
    float u = mod(world_position.x, repeat_interval) / repeat_interval;
    float v = mod(world_position.z, repeat_interval) / repeat_interval;

    return vec2(u, v);
}

void fragment() {
	float depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;
	vec4 world = CAMERA * inverse(PROJECTION_MATRIX) * vec4(ndc, 1.0);
	vec3 world_position = world.xyz / world.w;
	
	ivec2 texelCoord = ivec2(world_position.xz - offset);
	
	ivec2 texSize = textureSize(uncoveredWorldPositions, 0);
	
	if(texelCoord.x < 0 || texelCoord.y < 0 || texelCoord.x > texSize.x || texelCoord.y > texSize.y)
	{
		ALPHA = 1.0;
	}
	else
	{	
		ALPHA = 1.0 - texelFetch(uncoveredWorldPositions, texelCoord, 0).r;
	}
	vec2 timeOffset = vec2(0.0, TIME * fog_speed);
	ALBEDO.rgb = texture(fogTexture, world_to_uv(world_position) + timeOffset).rgb;
}

