#version 330

layout(location=0) out vec4 FragColor;

uniform sampler2D u_TexID;
uniform float u_Time;

in vec2 v_Tex;

vec4 Lens()
{	
    vec2 uv = v_Tex;
    float dist = length(uv - vec2(0.5));
    float lensEffect = smoothstep(0.2, 0.4, dist);

    return mix(texture(u_TexID, uv), vec4(1.0, 1.0, 1.0, 1.0), lensEffect);
}

vec4 FishEye()
{
    vec2 uv = v_Tex;
    
    // 0.5, 0.5를 중심으로 잡기 위해 좌표 범위 변경 (0~1 -> -0.5~0.5)
    vec2 centeredUV = uv - vec2(0.5);
    
    // 중심으로부터의 거리
    float dist = length(centeredUV);
    
    // 왜곡 강도 (양수면 볼록, 음수면 오목)
    float distortion = 0.5; 
    
    // 거리에 비례하여 좌표를 바깥쪽으로 밈 (Barrel Distortion 공식)
    // (1.0 + dist * dist * 강도)를 곱해줌
    if (dist < 0.5)
    {
        centeredUV *= (1.0 + distortion * dist * dist);
    }
    
    // 다시 0~1 범위로 복구
    uv = centeredUV + vec2(0.5);
    
    // 텍스처 범위 밖은 검은색 처리 (선택사항)
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
        return vec4(0.0, 0.0, 0.0, 1.0);
        
    return texture(u_TexID, uv);
}

vec4 GravityLens()
{
    vec2 uv = v_Tex;
    vec2 center = vec2(0.5, 0.5);
    
    // 중심으로부터의 벡터와 거리
    vec2 dir = uv - center;
    float dist = length(dir);
    
    // 사건의 지평선 (검은 원) 크기
    float radius = 0.1;
    
    // 렌즈가 빛을 휘게 하는 힘
    float strength = 0.05; 
    
    // 중심에 가까울수록 UV를 심하게 왜곡
    // 거리가 멀어질수록 영향력 감소
    vec2 distortedUV = uv - normalize(dir) * (strength / dist);
    
    // 중심부(블랙홀)는 검게 칠함
    if (dist < radius) {
        return vec4(0.0, 0.0, 0.0, 1.0); 
    }
    
    return texture(u_TexID, distortedUV);
}

vec4 MagnifyingGlass()
{
    vec2 uv = v_Tex;
    vec2 center = vec2(0.5, 0.5);
    float dist = distance(uv, center);
    
    float radius = 0.25; // 돋보기 크기
    float zoom = 2.0;    // 확대 배율
    
    if (dist < radius)
    {
        // 돋보기 내부: UV 좌표를 중심으로 당겨서 확대 효과 연출
        uv = (uv - center) / zoom + center;
        return texture(u_TexID, uv);
    }
    else
    {
        // 돋보기 외부: 원래 화면 그대로
        return texture(u_TexID, uv);
    }
}

vec4 ConcaveLens()
{
    vec2 uv = v_Tex;
    vec2 center = vec2(0.5, 0.5);
    
    // -0.5 ~ 0.5 범위로 변환
    vec2 relUV = uv - center;
    float dist = length(relUV);
    
    // 왜곡 강도: 음수(-)를 쓰면 오목 렌즈가 됩니다.
    // 값이 작을수록(-0.5 -> -1.0) 더 심하게 쪼그라듭니다.
    float strength = -0.5;
    float radius = 0.3;

    if (dist < radius)
    {
        // 렌즈 안쪽: 중심부로 갈수록 텍스처를 '밀어냄' (작아 보이게)
        // 어안 렌즈 공식과 같지만 strength가 음수라서 반대로 작동
        float factor = 1.0 + strength * (dist / radius) * (dist / radius);
        
        // 줌 아웃 효과를 위해 factor로 나눔 (혹은 역수를 곱함)
        vec2 distortedUV = relUV * (1.0 / factor);
        
        // 다시 0~1 범위로 복구
        return texture(u_TexID, distortedUV + center);
    }
    
    return texture(u_TexID, uv);
}

vec4 RainDrops()
{
    vec2 uv = v_Tex;
    
    // 화면을 4x4 격자로 나눔
    float scale = 4.0; 
    vec2 gridUV = fract(uv * scale); // 0~1 반복되는 좌표 생성
    vec2 cellID = floor(uv * scale); // 각 격자의 ID (랜덤 시드용)
    
    // 각 격자의 중심 (0.5, 0.5)
    vec2 center = vec2(0.5, 0.5);
    
    // 격자마다 물방울 위치를 조금씩 랜덤하게 바꿈 (자연스러움 추가)
    // sin 함수를 이용해 ID별로 다른 오프셋을 줌
    center.x += sin(cellID.y * 12.34) * 0.3;
    center.y += cos(cellID.x * 56.78) * 0.3;
    
    float dist = distance(gridUV, center);
    float radius = 0.25; // 격자 내에서의 물방울 크기
    
    if (dist < radius)
    {
        // 1. 굴절
        float t = dist / radius;
        vec2 dir = normalize(gridUV - center);
        float z = sqrt(1.0 - t * t);
        
        // 전체 UV 기준으로 오프셋을 줘야 배경이 자연스럽게 이어짐
        vec2 refraction = dir * (1.0 - z) * 0.02;
        
        // 2. 반사광
        vec2 lightDir = normalize(vec2(-1.0, 1.0));
        vec3 surfaceNormal = normalize(vec3(dir * t, z));
        vec3 lightVec = normalize(vec3(lightDir, 0.5));
        
        float spec = pow(max(0.0, dot(surfaceNormal, lightVec)), 40.0);
        
        return texture(u_TexID, uv + refraction) + vec4(spec);
    }
    
    return texture(u_TexID, uv);
}

// [유틸리티] 난수 생성 함수 (입력값이 같으면 항상 같은 랜덤값을 뱉음)
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// 물방울 레이어 하나를 계산하는 함수
vec4 DropLayer(vec2 uv, float scale) {
    uv *= scale;
    
    vec2 cellID = floor(uv);  // 격자 ID
    vec2 cellUV = fract(uv) - 0.5; // 격자 내 좌표 (-0.5 ~ 0.5)
    
    // 격자마다 랜덤한 값을 뽑음 (0.0 ~ 1.0)
    float randVal = hash21(cellID);
    
    // 1. 물방울 존재 여부 결정 (밀도 조절)
    // 랜덤값이 0.3보다 작으면 물방울 없음 (빈 공간)
    if (randVal < 0.3) return vec4(0.0); 
    
    // 2. 물방울 위치 랜덤 오프셋
    // 정가운데가 아니라 격자 안에서 위치를 조금씩 옮김
    float offsetX = (hash21(cellID + vec2(1.0, 0.0)) - 0.5) * 0.6;
    float offsetY = (hash21(cellID + vec2(0.0, 1.0)) - 0.5) * 0.6;
    vec2 dropPos = vec2(offsetX, offsetY);
    
    // 3. 물방울 크기 랜덤
    float size = 0.3 + (randVal * 0.2); // 0.3 ~ 0.5 크기
    
    // 거리 계산
    float dist = length(cellUV - dropPos);
    
    // 물방울 영역 밖이면 리턴
    if (dist > size) return vec4(0.0);
    
    // 4. 렌더링 (굴절 및 하이라이트)
    // 물방울의 높이(z)와 기울기 계산
    float t = dist / size;
    float z = sqrt(1.0 - t * t);
    vec2 dir = normalize(cellUV - dropPos);
    
    // 굴절 벡터 (Refraction)
    vec2 refraction = dir * (1.0 - z) * 0.03; 
    
    // 반사광 (Specular) - 조명 위치 고정
    vec2 lightDir = normalize(vec2(-1.0, 1.0));
    float spec = max(0.0, dot(vec3(dir * t, z), normalize(vec3(lightDir, 1.0))));
    spec = pow(spec, 40.0); // 반짝임 강도
    
    // 결과 반환 (xy: 굴절 오프셋, z: 하이라이트, w: 마스크)
    return vec4(refraction, spec, 1.0);
}

// 1. 사실적인 렌즈 색수차 (Radial)
// 화면 가장자리로 갈수록 색이 많이 분리됩니다.
vec4 ChromaticAberration_Lens(vec2 uv, float strength)
{
    // 화면 중심에서 현재 픽셀까지의 거리와 방향 계산
    vec2 center = vec2(0.5, 0.5);
    vec2 distVec = uv - center;
    
    // 거리에 비례해서 강도 조절 (중심은 선명하고 가장자리만 번짐)
    // 큐빅(3제곱)을 써서 가장자리 효과를 극대화
    float distFactor = dot(distVec, distVec) * strength; 
    
    // R, G, B 채널을 서로 다른 위치에서 가져옴
    // Red: 바깥쪽으로 밀기
    float r = texture(u_TexID, uv + distVec * distFactor).r;
    
    // Green: 정위치 (기준)
    float g = texture(u_TexID, uv).g;
    
    // Blue: 안쪽으로 당기기 (Red와 반대 방향)
    float b = texture(u_TexID, uv - distVec * distFactor).b;
    
    return vec4(r, g, b, 1.0);
}

vec4 Retro_CRT(vec2 uv)
{
    // 1. 약한 색수차 (Chromatic Aberration) - 살짝 번짐
    float shift = 0.005; // 번짐 강도
    float r = texture(u_TexID, uv + vec2(shift, 0.0)).r;
    float g = texture(u_TexID, uv).g;
    float b = texture(u_TexID, uv - vec2(shift, 0.0)).b;
    vec3 color = vec3(r, g, b);
    
    // 2. 스캔라인 (Scanline) - 가로줄 무늬
    // uv.y에 큰 수를 곱해서 사인파를 만듦
    // 해상도가 높으면 800.0을 더 높이세요.
    float scanline = sin(uv.y * 800.0) * 0.04; 
    color -= scanline; // 줄 부분만 살짝 어둡게 뺌
    
    // 3. 비네팅 (Vignette) - 가장자리 어둡게
    float dist = distance(uv, vec2(0.5));
    // 중심에서 멀어질수록 어두워짐 (1.0 - 거리 * 강도)
    float vignette = 1.0 - dist * 0.6;
    vignette = clamp(vignette, 0.0, 1.0); // 0~1 사이로 자름
    
    color *= vignette;
    
    // 4. CRT 특유의 밝기 뻥튀기 (선택사항)
    color *= 1.1; 
    
    return vec4(color, 1.0);
}

vec4 Retro_Film(vec2 uv)
{
    vec4 texColor = texture(u_TexID, uv);
    vec3 color = texColor.rgb;
    
    // 1. 그레이스케일(흑백) 변환
    // 인간의 눈은 녹색에 민감하므로 가중치가 다름
    float gray = dot(color, vec3(0.299, 0.587, 0.114));
    
    // 2. 세피아 톤 입히기 (누런 느낌)
    // 흑백 값에 특정 색상 벡터를 곱함
    vec3 sepiaColor = vec3(gray) * vec3(1.2, 1.0, 0.8);
    
    // 원본과 세피아를 섞음 (0.8 정도로 세피아 느낌 강하게)
    color = mix(color, sepiaColor, 0.8);
    
    // 3. 필름 노이즈 (Grain)
    // 정지된 노이즈 (사진 질감)
    float noise = hash21(uv * 100.0); 
    // 노이즈를 살짝 섞어줌 (overlay 느낌)
    color += (noise - 0.5) * 0.15;
    
    // 4. 비네팅 (가장자리 태운 느낌)
    float dist = distance(uv, vec2(0.5));
    float vignette = 1.0 - dist * 0.8;
    color *= vignette;
    
    return vec4(color, 1.0);
}

vec4 Pixelization()
{
    float resol = (sin(u_Time * 10)+1) * 100;
    float tx = floor(v_Tex.x * resol)/resol; // 0~1
    float ty = floor(v_Tex.y * resol)/resol; // 0~1

    return texture(u_TexID, vec2(tx, ty));
}

void main()
{
	// FragColor = texture(u_TexID, vec2(v_Tex.x, 1-v_Tex.y));
	// FragColor = Lens();
    // FragColor = FishEye();
    // FragColor = GravityLens();
    // FragColor = MagnifyingGlass();
    // FragColor = ConcaveLens();
    // FragColor = RainDrops();
    // FragColor = ChromaticAberration_Lens(v_Tex, 0.5);
    // FragColor = Retro_CRT(v_Tex);
    // FragColor = Retro_Film(v_Tex);
    FragColor = Pixelization();
}
