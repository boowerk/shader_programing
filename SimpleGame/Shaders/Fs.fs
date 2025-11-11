#version 330

layout(location=0) out vec4 FragColor;

in vec2 v_UV;

const float c_PI = 3.141592;

void main()
{

    int repeat = 4;
    float vx = 2 * c_PI * v_UV.x * repeat;  // 0 ~ 2PI
    float vy = 2 * c_PI * v_UV.y * repeat;  // 0 ~ 2PI
    // float greyscale = pow(abs(sin(vy) + sin(vx)), 16);
    float greyscale = 1 - pow(abs(sin(vx)), 0.1);

    vec4 newColor = vec4(greyscale);
    FragColor = newColor;
}
