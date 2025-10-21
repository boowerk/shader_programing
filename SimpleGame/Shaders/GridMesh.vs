#version 330
#define MAX_POINTS 100

in vec3 a_Position;

out vec4 v_Color;

uniform float u_Time;
uniform vec4 u_Points[MAX_POINTS];

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

float hash11(float n) {
    return fract(sin(n * 12.9898) * 43758.5453);
}
vec2 hash21(float n) {
    return vec2(hash11(n), hash11(n + 17.13));
}

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

		float phase = d * (F[i] * 5.0 * c_PI) - u_Time * S[i] * 3;
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

void RainDrop()
{
	vec4 newPosition = vec4(a_Position, 1);
	float dX = 0;
	float dY = 0;

	vec2 pos = vec2(a_Position.xy);
	float newColor = 0;
	
	for(int i  = 0; i< MAX_POINTS; i++)
	{
		float sTime = u_Points[i].z;
		float lTime = u_Points[i].w;
		float newTime = u_Time - u_Points[i].z;
		if(newTime > 0) 
		{
			float baseTime = fract(newTime / lTime);
			float oneMinus = 1 - baseTime;
			float t = baseTime * lTime;
			float range = baseTime * lTime / 5;
			vec2 center = u_Points[i].xy;
			float d = distance(pos, center);
			float y = 10 * clamp(range - d, 0, 1);
			newColor += oneMinus * y * sin(d * 4 * c_PI * 10 - t * 30);
		}
	}
	

	newPosition += vec4(dX, dY, 0, 0);
	gl_Position = newPosition;

	v_Color = vec4(newColor);
}


void nesting_wave_with_raindrop()
{
    vec4 newPosition = vec4(a_Position, 1.0);
    vec2 pos = a_Position.xy;

    float accum = 0.0;
    float totalAmp = 0.0;

 
    for (int i = 0; i < 5; ++i)
    {
        float d = distance(pos, C[i]);
        float r = max(R[i], 1e-4);
        float t = clamp(d / r, 0.0, 1.0);
        float atten = smoothstep(1.0, 0.0, t);

        float phase = d * (F[i] * 5.0 * c_PI) - u_Time * S[i] * 3.0;
        float contrib = A[i] * atten * sin(phase);

        accum += contrib;
        totalAmp += A[i] * atten;
    }

   
    for (int i = 0; i < MAX_POINTS; ++i)
    {
        float startTime = u_Points[i].z;
        float lifeTime  = u_Points[i].w;
        float tElapsed  = u_Time - startTime;

        if (tElapsed > 0.0 && lifeTime > 1e-6)
        {
            float cycT      = fract(tElapsed / lifeTime);     
            float oneMinus  = 1.0 - cycT;                      
            float phaseTime = cycT * lifeTime;

            vec2 center = u_Points[i].xy;
            float d      = distance(pos, center);

            float ringR   = phaseTime / 5.0;                   
            float local   = clamp(ringR - d, 0.0, 1.0);       
            float atten   = local;

            float phase = d * (4.0 * c_PI * 10.0) - phaseTime * 30.0;

            float amp   = oneMinus * 10.0 * 0.2;

            float contrib = amp * atten * sin(phase);

            accum    += contrib;
            totalAmp += amp * atten;
        }
    }

    if (totalAmp > 1e-4) accum /= totalAmp;

    newPosition.y += accum * 0.025;

    gl_Position = newPosition;
	v_Color = vec4(accum * 0.5 + 0.5, 0.55 - accum * 0.25, 0.65, 1.0);

}


void main()
{
	// flag();
	// wave();	
	// nesting_wave();
	// RainDrop();
	nesting_wave_with_raindrop();
}
