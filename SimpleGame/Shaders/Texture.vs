#version 330

in vec3 a_Pos;
in vec2 a_Tex;

out vec2 v_Tex;

uniform vec2 u_Trans;
uniform vec2 u_Scale;

void main()
{
	vec4 newPosition = vec4(a_Pos, 1);
	newPosition.xy *= u_Scale;
	newPosition.xy += u_Trans;
	gl_Position = newPosition;

	v_Tex = a_Tex;
}
