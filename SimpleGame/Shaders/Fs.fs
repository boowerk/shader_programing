#version 330
layout(location=0) out vec4 FragColor;

in vec2 v_UV;
uniform float u_Time; // 초 단위 시간

// ------------------------ 유틸 ------------------------
float hash11(float n) {
    return fract(sin(n) * 43758.5453123);
}
vec2 hash21(float n) {
    float x = hash11(n);
    float y = hash11(n + 17.0);
    return vec2(x, y);
}

// ------------------------ 파라미터 ------------------------
// 초당 빗방울 개수, 최근 N개만 합성
const float DROP_RATE  = 2.0;
const int   RECENT_N   = 14;

// 파동 물리 상수
const float WAVELEN    = 0.12;   // 파장(화면좌표 기준)
const float SPEED      = 0.35;   // 파동 전파 속도
const float AMP        = 0.045;  // 각 드롭의 기본 진폭
const float RAD_DAMP   = 2.2;    // 거리 감쇠 계수(클수록 빨리 사라짐)
const float AGE_DAMP   = 1.3;    // 시간 감쇠 계수(클수록 빨리 사라짐)

// 하이라이트/재질
const float SPEC_POWER = 64.0;
const float METALLIC   = 0.02;   // 금속감(거의 0, 물 느낌)

// ------------------------ 파동(높이) ------------------------
float rippleFromDrop(vec2 p, vec2 center, float age) {
    if (age <= 0.0) return 0.0;

    float r = length(p - center) + 1e-5;
    float k = 6.28318530718 / WAVELEN;      // 2π/λ
    float w = k * SPEED;                    // 각속도
    float phase = k * r - w * age;

    // 반지름/시간에 따른 감쇠 + 얇은 파면만 보이도록 샤프닝
    float env  = exp(-RAD_DAMP * r) * exp(-AGE_DAMP * age);
    float s    = sin(phase);
    float ring = s * smoothstep(0.0, 0.004, abs(s)); // 얇은 링 강조

    return AMP * env * ring;
}

// 최근 N개의 드롭을 합성
float waterHeight(vec2 p, float time) {
    // 최근 셀 인덱스
    float base = floor(time * DROP_RATE);
    float height = 0.0;

    // 균등 간격 + 각 드롭 위치는 해시로 난수
    for (int i = 0; i < RECENT_N; ++i) {
        float idx = base - float(i);

        // 임의 위치(화면 [-1,1]^2 내)
        vec2 rnd = hash21(idx) * 2.0 - 1.0;

        // 살짝의 셀 내 지연(충돌 시점 분산)
        float jitter = 0.35 * hash11(idx + 123.45);
        float hitTime = (idx + jitter) / DROP_RATE;

        float age = time - hitTime;
        height += rippleFromDrop(p, rnd, age);
    }
    return height;
}

// ------------------------ 색/조명 ------------------------
vec3 waterShading(vec2 p, float h) {
    // 화면 공간 미분으로 노멀 근사
    float hx = dFdx(h);
    float hy = dFdy(h);
    vec3 N = normalize(vec3(-hx, -hy, 1.0));

    vec3 V = vec3(0.0, 0.0, 1.0);                // 카메라 방향(정면)
    vec3 L = normalize(vec3(0.5, 0.6, 1.0));     // 광원 방향

    float NdL = max(dot(N, L), 0.0);
    vec3  H   = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), SPEC_POWER);

    // 수면 베이스 컬러(심도에 따라 살짝 변화)
    float depthTone = 0.5 + 0.5 * tanh(6.0 * h);
    vec3 baseCol = mix(vec3(0.06, 0.12, 0.18), vec3(0.02, 0.05, 0.08), depthTone);

    // 프레넬 비슷한 효과로 가장자리 강조
    float fres = pow(1.0 - max(dot(N, V), 0.0), 3.0);

    vec3 diffuse  = baseCol * (0.35 + 0.65 * NdL);
    vec3 specular = mix(vec3(0.04), baseCol, METALLIC) * spec * (0.6 + 0.4 * fres);

    // 약간의 환경색(하늘 반사 느낌)
    vec3 env = vec3(0.10, 0.16, 0.22) * (0.25 + 0.75 * fres);

    return diffuse + specular + env;
}

void main() {
    // 좌표: v_UV(0..1) → [-1,1]
    vec2 p = v_UV * 2.0 - 1.0;

    // 장면 안정화를 위한 미세 드리프트(정지화면 밴딩 방지)
    float t = u_Time;
    p += 0.002 * vec2(sin(t * 0.73), cos(t * 0.51));

    // 물 높이(field) 합성
    float h = waterHeight(p, t);

    // 색/조명
    vec3 col = waterShading(p, h);

    // 가장자리 비네팅
    float vign = smoothstep(1.4, 0.2, length(p));
    col *= mix(0.4, 1.0, vign);

    FragColor = vec4(col, 1.0);
}
