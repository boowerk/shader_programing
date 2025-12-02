#version 330

layout(location=0) out vec4 FragColor;
layout(location=1) out vec4 FragColor1;

in vec2 v_UV;

uniform sampler2D u_RGBTexture;
uniform sampler2D u_DigitTexture;
uniform sampler2D u_NumTexture;

uniform float u_Time;       // 시간(초 단위)

const float c_PI = 3.14159265;

vec4 Test()
{
    vec2 newUV = v_UV;
    float dx = 0.1f * cos(v_UV.x * 2 * c_PI + u_Time);
    float dy = 0.1f * sin(v_UV.x * 2 * c_PI + u_Time);
    newUV += vec2(dx, dy);
    vec4 sampledColor = texture(u_RGBTexture, newUV);
    return sampledColor;
}

vec4 Circless()
{
    vec2 newUV = v_UV;
    vec2 center = vec2(0.5, 0.5);
    vec4 newColor = vec4(0);
    float d = distance(newUV, center);
    
    float value = sin(d * 8 * c_PI - u_Time);
    newColor = vec4(value);

    return newColor;

}

vec4 Flag()
{
    vec2 newUV = vec2(v_UV.x, 1 - v_UV.y - 0.5);  // 0-1, left bottom (0,0)
    vec4 newColor = vec4(0);

    // float width = 0.2f - v_UV.x * 0.2;
    float width = 0.2f * (1 - newUV.x);
    float sinValue = v_UV.x * 0.2 * sin(newUV.x * 2 * c_PI - u_Time * 10);

    if(newUV.y < sinValue + width && newUV.y > sinValue - width && width > 0)
    {
        newColor = vec4(1);
    }
    else{
        // discard;
    }
    return newColor;
}

// 좌표 꼬아보기
// 수식을 사용하여 if문 없이 발표자료 1을 결과를 만들어보자
vec4 Q1()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)
    float x = newUV.x;  // 0-1
    float y = 1 - abs((v_UV.y - 0.5) * 2);  // -0.5 - 0.5 => -1~1 => 1~0~1 => 0~1~0
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}


vec4 Q2()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)
    float x = fract(newUV.x * 3);  // 0-1 (3)
    float y = (2 - floor(newUV.x * 3))/3 + newUV.y / 3; // 
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}

vec4 Q3()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)
    float x = fract(newUV.x * 3);  // 0-1 (3)
    float y = (floor(newUV.x * 3))/3 + newUV.y / 3; // 
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}

vec4 Q4()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)
    float value = floor(newUV.y * 2) / 2 + 0.5;   // 0, 1 => 0.5 1
    float x = fract(newUV.x * 2 + value);  // 0-1 (3)
    float y = newUV.y * 2; // 
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}

vec4 Brick_Horizontal()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)

    // uniform으로 뺄 수 있음
    float rCount = 3;          // 가로, 세로 블록 개수
    float sAmount = 0.2;        // sAmount 만큼 shift

    float x = fract(newUV.x * rCount) 
            + floor(newUV.y * rCount + 1) * sAmount; 
    float y = fract(newUV.y * rCount);  // 0-1
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}

vec4 Q5()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)
    float x = fract(newUV.x * 2);  // 0-1 
    float y = fract(newUV.y * 2) + floor(newUV.x*2) * 0.5;  // 0-1
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}

vec4 Brick_Vertical()
{
    vec2 newUV = vec2(v_UV.x, v_UV.y);  // 0-1, left top (0,0)
    float x = fract(newUV.x * 2) ;  // 0-1 
    float y = fract(newUV.y * 2)+ floor(newUV.x*2) * 0.5;  // offset : 0, 0.5
    vec4 newColor = texture(u_RGBTexture, vec2(x,y));

    return newColor;
}

// GLSL Fragment Shader Function
vec4 Brick_Horizontal_AI()
{
    vec2 uv = v_UV.xy; // 0-1, left top (0,0)

    // Uniform으로 뺄 수 있는 변수들
    float rCount = 1.0;        // 가로, 세로 블록 개수 (Brick Count)
    float sAmount = 0.5;       // 행(Row)당 가로 이동량 (Shift Amount)
    float brickRatio = 0.95;   // 벽돌 영역의 비율 (0.95 = 5%가 줄눈)
    vec4 mortarColor = vec4(0.3, 0.3, 0.3, 1.0); // 줄눈 색상 (회색)
    float mortarDepth = 0.1;   // 줄눈의 상대적인 깊이감 (입체감 조절)

    // 광원 방향 (정규화된 벡터)
    vec3 lightDir = normalize(vec3(0.5, 0.5, 1.0)); // Z는 표면을 향하는 방향

    // 꿀렁이는 효과를 위한 Time 변수들
    float waveSpeed = 2.0;     // 꿀렁이는 속도
    float waveAmplitude = 0.05; // 꿀렁이는 진폭 (최대 이동량)
    float waveFrequency = 5.0; // 꿀렁이는 파동의 빈도 (한 화면에 몇 개의 파동)

    // u_Time을 사용하여 UV 좌표를 왜곡
    // X축 방향으로 파동을 만들고, Y축에 따라 파동의 위상을 다르게 함
    vec2 distortedUV = uv;
    distortedUV.x += sin(uv.y * waveFrequency + u_Time * waveSpeed) * waveAmplitude;
    // Y축 방향으로도 꿀렁임을 줄 수 있음
    distortedUV.y += cos(uv.x * waveFrequency * 0.5 + u_Time * waveSpeed * 0.7) * waveAmplitude * 0.5;

    // 이제부터는 왜곡된 UV 좌표(distortedUV)를 사용합니다.
    
    // 1. 벽돌 인덱스 및 패턴 좌표 계산
    float rowIndex = floor(distortedUV.y * rCount);
    float y = fract(distortedUV.y * rCount);
    float x = fract(distortedUV.x * rCount + rowIndex * sAmount);

    // 2. 현재 픽셀이 '벽돌' 영역인지 '줄눈' 영역인지 판단
    float inBrickX = step(x, brickRatio);
    float inBrickY = step(y, brickRatio);
    float isBrick = inBrickX * inBrickY; 

    // 3. 벽돌 색상 및 줄눈 색상 결정
    vec2 brickUV = vec2(x / brickRatio, y / brickRatio);
    vec4 baseColor = texture(u_RGBTexture, brickUV);
    baseColor = mix(mortarColor, baseColor, isBrick);

    // 4. 가상의 노멀 벡터(Virtual Normal Vector) 생성 (이전과 동일)
    vec3 normal;
    if (isBrick > 0.5) 
    {
        float borderSize = 0.05;
        float borderX = min(x, brickRatio - x);
        float borderY = min(y, brickRatio - y);

        if (borderX < borderSize || borderY < borderSize) {
            float nx = (borderX < borderSize) ? mix(0.0, -1.0, 1.0 - borderX / borderSize) : 0.0;
            float ny = (borderY < borderSize) ? mix(0.0, -1.0, 1.0 - borderY / borderSize) : 0.0;
            normal = normalize(vec3(nx, ny, 1.0));
        } else {
            normal = vec3(0.0, 0.0, 1.0);
        }
    }
    else
    {
        float nz = 1.0 - mortarDepth;
        float borderX = abs(x - brickRatio); 
        float borderY = abs(y - brickRatio); 

        if (borderX < (1.0 - brickRatio) * 0.5 || borderY < (1.0 - brickRatio) * 0.5) {
             float edgeInfluence = 0.5;
             float edgeX = 1.0 - abs(x - (brickRatio + (1.0 - brickRatio) * 0.5)) / ((1.0 - brickRatio) * 0.5);
             float edgeY = 1.0 - abs(y - (brickRatio + (1.0 - brickRatio) * 0.5)) / ((1.0 - brickRatio) * 0.5);
             nz = mix(1.0 - mortarDepth, -0.5, max(edgeX, edgeY) * edgeInfluence);
             normal = normalize(vec3(0.0, 0.0, nz));
        } else {
            normal = vec3(0.0, 0.0, nz);
        }
    }
    normal = normalize(normal);

    // 5. 조명 계산 (Lambertian 모델) (이전과 동일)
    float diffuse = max(dot(normal, lightDir), 0.0);
    float ambient = 0.3;
    vec4 finalColor = baseColor * (diffuse + ambient);

    return finalColor;
}

vec4 Digit()
{
    return texture(u_DigitTexture, v_UV);
}

vec4 Digit_Num()
{
    int digit = int(u_Time * 10)%10;

    int titleIndex = (digit + 9) % 10;

    float offX = float(titleIndex % 5) / 5;
    float offY = float(titleIndex / 5) / 2;

    float tx = v_UV.x / 5 + offX;
    float ty = v_UV.y / 2 + offY;
    return texture(u_DigitTexture, vec2(tx, ty));
}

vec4 Digit_Num_AI()
{
    // 1. 하드코딩된 입력 숫자 설정
    int inputNumber = int(u_Time * 100); // 여기서 원하는 5자리 숫자를 설정하세요!
    
    // 텍스처 레이아웃 설정 (5x2 배열 가정)
    float columns = 5.0; // 화면을 5개로 분할 (자릿수)
    float rows = 2.0;    // 텍스처 내 숫자 행 (0-4 / 5-9)

    // 1. 화면을 5개의 자릿수 영역으로 분할하고 현재 영역을 파악
    // 현재 픽셀이 속한 자릿수의 인덱스 (0: 만의 자리, 4: 일의 자리)
    int digitPos = int(floor(v_UV.x * columns)); 

    // 현재 픽셀의 해당 자릿수 내에서의 로컬 UV (0.0 ~ 1.0)
    float localX = fract(v_UV.x * columns);
    float localY = v_UV.y;

    // 2. 입력된 숫자(inputNumber)에서 해당 자릿수의 값 추출
    int digitValue;

    // 자릿수 위치(digitPos)에 따라 if/else if로 값을 분리 (하드코딩된 로직)
    if (digitPos == 0) {
        // 만의 자리: (98765 / 10000) % 10 = 9
        digitValue = (inputNumber / 10000) % 10;
    } 
    else if (digitPos == 1) {
        // 천의 자리: (98765 / 1000) % 10 = 8
        digitValue = (inputNumber / 1000) % 10;
    } 
    else if (digitPos == 2) {
        // 백의 자리: (98765 / 100) % 10 = 7
        digitValue = (inputNumber / 100) % 10;
    } 
    else if (digitPos == 3) {
        // 십의 자리: (98765 / 10) % 10 = 6
        digitValue = (inputNumber / 10) % 10;
    } 
    else { // digitPos == 4
        // 일의 자리: 98765 % 10 = 5
        digitValue = inputNumber % 10;
    }


    // 3. 텍스처 좌표 계산: digitValue를 기반으로 u_DigitTexture 내의 오프셋을 계산
    
    // **[사용자 요청 반영: 텍스처 레이아웃 12345 / 67890]**
    // 텍스처 인덱스 매핑:
    // 1 -> 0, 2 -> 1, ..., 9 -> 8
    // 0 -> 9 (두 번째 줄의 마지막)
    int textureIndex;
    
    if (digitValue == 0) {
        // 숫자 0은 텍스처의 9번 인덱스에 위치합니다. (2번째 줄 5번째 칸)
        textureIndex = 9;
    } 
    else {
        // 숫자 1~9는 해당 값에서 1을 뺀 인덱스에 위치합니다.
        textureIndex = digitValue - 1; 
    }

    // X 오프셋: 인덱스를 5로 나눈 나머지 (0, 1, 2, 3, 4)
    float offX = float(textureIndex % 5) / columns; 
    
    // Y 오프셋: 인덱스를 5로 나눈 몫 (0 또는 1)
    float offY = floor(float(textureIndex) / 5.0) / rows; // 5.0은 columns 값과 동일

    // 4. 최종 텍스처 좌표 (tx, ty) 계산
    // localX (0~1)을 전체 텍스처 기준으로 스케일링한 후 오프셋을 더함
    float tx = localX / columns + offX;
    float ty = localY / rows + offY;
    
    // 5. 색상 샘플링 및 출력
    return texture(u_DigitTexture, vec2(tx, ty));
}


void main()
{
    // Test();
    // Circless();
    // Flag();
    // Q1();
    // Q2();
    // Q3();
    // Q4();
    // Brick_Horizontal();
    // Q5();
    // Brick_Vertical();
    // Brick_Horizontal_AI();
    // Digit();
    // Digit_Num();

    FragColor   = Circless();  // 0_0
    FragColor1  = Flag();       // 0_1
}