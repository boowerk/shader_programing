#version 330

layout(location=0) out vec4 FragColor;
layout(location=1) out vec4 FragColor1;

in vec2 v_UV;
uniform sampler2D u_Texture;

in vec4 v_Color;
in vec2 v_Tex;

void main()
{
    vec4 result = texture(u_Texture, v_UV) * v_Color * 2;
	// vec4 result = texture(u_Texture, v_UV) * 2;
	float brightness = dot(result.rgb, vec3(0.2126, 0.7152, 0.0722));

	FragColor = result;
	if(brightness > 1)	
		FragColor1 = result;
	else
		FragColor1 = vec4(0); 
}
