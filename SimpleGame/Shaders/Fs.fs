#version 330

layout(location=0) out vec4 FragColor;

in vec2 v_UV;

uniform sampler2D u_RGBTexture;

uniform float u_Time;       // 시간(초 단위)

const float c_PI = 3.14159265;

void Test()
{
    vec2 newUV = v_UV;
    float dx = 0.1f * cos(v_UV.x * 2 * c_PI + u_Time);
    float dy = 0.1f * sin(v_UV.x * 2 * c_PI + u_Time);
    newUV += vec2(dx, dy);
    vec4 sampledColor = texture(u_RGBTexture, newUV);
    FragColor = sampledColor;
}

void Circless()
{
    vec2 newUV = v_UV;
    vec2 center = vec2(0.5, 0.5);
    float d = distance(newUV, center);
    vec4 newColor = vec4(d);

    float value = sin(d * 8 * c_PI - u_Time);
    newColor = vec4(value);
   
    FragColor = newColor;

}

void Flag()
{
    vec2 newUV = vec2(v_UV.x, (1 - v_UV.y) - 0.5 );
    float sinValue = v_UV.x * 0.2 * sin(v_UV.x * 2 * c_PI - u_Time * 10);
    vec4 newColor = vec4(0);
    
    float width = 0.2 * (1 - newUV.x);

    if (sinValue + width > newUV.y && sinValue - width < newUV.y)
    {
        newColor = vec4(1);
    }
    else
    {
        discard;
    }

    FragColor = newColor;
}

void Q1()
{
    float newX      = v_UV.x;
    float newY      = 1 - abs((v_UV.y * 2) - 1);

    FragColor       = texture(u_RGBTexture, vec2(newX, newY));
}

void Q2()
{
    float newX      = fract(v_UV.x * 3);
    float newY      = (2 - floor(v_UV.x * 3)) / 3 + v_UV.y / 3;

    FragColor       = texture(u_RGBTexture, vec2(newX, newY));
}

void main()
{
    // Test();
    // Circless();
    // Flag();
    // Q1();
    Q2();
}