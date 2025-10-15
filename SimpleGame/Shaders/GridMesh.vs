#version 330

in vec3 a_Position;

out vec4 v_Color;

uniform float u_Time;

const float c_PI = 3.141592;

const vec2  C[5] = vec2[5](
    vec2(-0.6,  0.4),
    vec2(-0.2, -0.3),
    vec2( 0.0,  0.0),
    vec2( 0.4,  0.3),
    vec2( 0.7, -0.2)
);

const float R[5] = float[5](0.55, 0.50, 0.65, 0.45, 0.60);
const float F[5] = float[5](8.0, 10.0, 6.0, 9.0, 7.0);
const float S[5] = float[5](12.0, 9.0, 10.0, 11.0, 8.0);
const float A[5] = float[5](1.0, 0.8, 1.0, 0.7, 0.9);

void flag()
{
	vec4 newPosition = vec4(a_Position, 1);


	float value = (a_Position.x + 0.5) * 2 * c_PI;	// 0 ~ 2PI
	float value1 = a_Position.x + 0.5;

	float dx	= 0;
	float dy	= value1 * 0.3 * sin(value + u_Time * 20);

	newPosition.y *= (1 - value1);
	newPosition.xy += vec2(dx, dy);

	float brightness = clamp((dy + 0.3) / 0.6, 0.0, 1.0);

	gl_Position = newPosition;
	v_Color = vec4(brightness);
}

void wave()
{
	vec4 newPosition = vec4(a_Position, 1);

	vec2 pos = vec2(a_Position.xy);
	vec2 center = vec2(0, 0);
	
	float d = distance(pos, center);
	float y = 2 * clamp(0.5 - d, 0, 1);
	
	float newColor = y * sin(d * 4 * c_PI * 10 - u_Time * 30);

	gl_Position = newPosition;
	v_Color = vec4(newColor);
}


void nesting_wave()
{
	vec4 newPosition = vec4(a_Position, 1);

	vec2 pos = a_Position.xy;
	vec2 center = vec2(0, 0);
	
	float accum = 0.0;
	float totalAmp = 0.0;
	float newColor = 0.0;
	
	for (int i = 0; i < 5; ++i) 
	{
		float d = distance(pos, C[i]);

		float r = max(R[i], 1e-4);
		float t = clamp(d / r, 0, 1);
		float atten = smoothstep(1.0, 0.0, t);

		float phase = d * (F[i] * 5.0 * c_PI) - u_Time * S[i];
		float contrib = A[i] * atten * sin(phase);

		accum += contrib;
        totalAmp += A[i] * atten;
	}
	
    if (totalAmp > 1e-4)
        accum /= totalAmp;

	newPosition.y += accum * 0.05;

	gl_Position = newPosition;
	v_Color = vec4(accum * 0.5 + 0.5);
}

void main()
{
	// flag();
	// wave();
	nesting_wave();
}
