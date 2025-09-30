#include "stdafx.h"
#include "Renderer.h"

Renderer::Renderer(int windowSizeX, int windowSizeY)
{
	Initialize(windowSizeX, windowSizeY);
}


Renderer::~Renderer()
{
}

void Renderer::Initialize(int windowSizeX, int windowSizeY)
{
	//Set window size
	m_WindowSizeX = windowSizeX;
	m_WindowSizeY = windowSizeY;

	// Compile all shader programs
	CompileAllShaderPrograms();

	//Create VBOs
	CreateVertexBufferObjects();

	GenerateParticles(10000);

	if (m_SolidRectShader > 0 && m_VBORect > 0)
	{
		m_Initialized = true;
	}
}

void Renderer::CompileAllShaderPrograms()
{
	//Load shaders
	m_SolidRectShader = CompileShaders(
		"./Shaders/SolidRect.vs",
		"./Shaders/SolidRect.fs");

	m_TestShader = CompileShaders(
		"./Shaders/Test.vs",
		"./Shaders/Test.fs");


	m_ParticleShader = CompileShaders(
		"./Shaders/particle.vs",
		"./Shaders/particle.fs");

}

void Renderer::DeleteAllShaderPrograms()
{
	glDeleteShader(m_SolidRectShader);
	glDeleteShader(m_TestShader);
	glDeleteShader(m_ParticleShader);
}


bool Renderer::IsInitialized()
{
	return m_Initialized;
}

void Renderer::ReloadAllShaderPrograms()
{
	DeleteAllShaderPrograms();
	CompileAllShaderPrograms();
}

void Renderer::CreateVertexBufferObjects()
{
	float rect[]
		=
	{
		-1.f / m_WindowSizeX, -1.f / m_WindowSizeY, 0.f, -1.f / m_WindowSizeX, 1.f / m_WindowSizeY, 0.f, 1.f / m_WindowSizeX, 1.f / m_WindowSizeY, 0.f, //Triangle1
		-1.f / m_WindowSizeX, -1.f / m_WindowSizeY, 0.f,  1.f / m_WindowSizeX, 1.f / m_WindowSizeY, 0.f, 1.f / m_WindowSizeX, -1.f / m_WindowSizeY, 0.f, //Triangle2
	};

	glGenBuffers(1, &m_VBORect);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBORect);
	glBufferData(GL_ARRAY_BUFFER, sizeof(rect), rect, GL_STATIC_DRAW);
	
	// lecture2


	float temp = 0.5f;
	float size = 0.1f;
	float testPos[]
		=
	{
		(0.f - temp)* size, (0.f - temp)* size, 0.f, 0.5,
		(1.f - temp)* size, (0.f - temp)* size, 0.f, 0.5,
		(1.f - temp)* size, (1.f - temp)* size, 0.f, 0.5,
		(0.f - temp)* size, (0.f - temp)* size, 0.f, 0.5,
		(1.f - temp)* size, (1.f - temp)* size, 1.f, 0.5,
		(0.f - temp)* size, (1.f - temp)* size, 0.f, 0.5,	// Quard1

		(0.f - temp)* size, (0.f - temp)* size, 0.f, 1.f,
		(1.f - temp)* size, (0.f - temp)* size, 0.f, 1.f,
		(1.f - temp)* size, (1.f - temp)* size, 0.f, 1.f,
		(0.f - temp)* size, (0.f - temp)* size, 0.f, 1.f,
		(1.f - temp)* size, (1.f - temp)* size, 1.f, 1.f,
		(0.f - temp)* size, (1.f - temp)* size, 0.f, 1.f,	// Quard2
	};

	glGenBuffers(1, &m_VBOtestPos);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOtestPos);
	glBufferData(GL_ARRAY_BUFFER, sizeof(testPos), testPos, GL_STATIC_DRAW);


	float testColor[]
		=
	{
		1.f, 0.f, 0.f, 1.f,
		0.f, 1.f, 0.f, 1.f,
		0.f, 0.f, 1.f, 1.f,	//Triangle1
		1.f, 0.f, 0.f, 1.f,
		0.f, 1.f, 0.f, 1.f,
		0.f, 0.f, 1.f, 1.f,	//Triangle2

		1.f, 0.f, 0.f, 1.f,
		0.f, 1.f, 0.f, 1.f,
		0.f, 0.f, 1.f, 1.f,	//Triangle1
		1.f, 0.f, 0.f, 1.f,
		0.f, 1.f, 0.f, 1.f,
		0.f, 0.f, 1.f, 1.f	//Triangle2
	};

	glGenBuffers(1, &m_VBOtestColor);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOtestColor);
	glBufferData(GL_ARRAY_BUFFER, sizeof(testColor), testColor, GL_STATIC_DRAW);
}

void Renderer::AddShader(GLuint ShaderProgram, const char* pShaderText, GLenum ShaderType)
{
	//쉐이더 오브젝트 생성
	GLuint ShaderObj = glCreateShader(ShaderType);

	if (ShaderObj == 0) {
		fprintf(stderr, "Error creating shader type %d\n", ShaderType);
	}

	const GLchar* p[1];
	p[0] = pShaderText;
	GLint Lengths[1];

	size_t slen = strlen(pShaderText);
	if (slen > INT_MAX) {
		// Handle error
	}
	GLint len = (GLint)slen;

	Lengths[0] = len;
	//쉐이더 코드를 쉐이더 오브젝트에 할당
	glShaderSource(ShaderObj, 1, p, Lengths);

	//할당된 쉐이더 코드를 컴파일
	glCompileShader(ShaderObj);

	GLint success;
	// ShaderObj 가 성공적으로 컴파일 되었는지 확인
	glGetShaderiv(ShaderObj, GL_COMPILE_STATUS, &success);
	if (!success) {
		GLchar InfoLog[1024];

		//OpenGL 의 shader log 데이터를 가져옴
		glGetShaderInfoLog(ShaderObj, 1024, NULL, InfoLog);
		fprintf(stderr, "Error compiling shader type %d: '%s'\n", ShaderType, InfoLog);
		printf("%s \n", pShaderText);
	}

	// ShaderProgram 에 attach!!
	glAttachShader(ShaderProgram, ShaderObj);
}

bool Renderer::ReadFile(char* filename, std::string *target)
{
	std::ifstream file(filename);
	if (file.fail())
	{
		std::cout << filename << " file loading failed.. \n";
		file.close();
		return false;
	}
	std::string line;
	while (getline(file, line)) {
		target->append(line.c_str());
		target->append("\n");
	}
	return true;
}

GLuint Renderer::CompileShaders(char* filenameVS, char* filenameFS)
{
	GLuint ShaderProgram = glCreateProgram(); //빈 쉐이더 프로그램 생성

	if (ShaderProgram == 0) { //쉐이더 프로그램이 만들어졌는지 확인
		fprintf(stderr, "Error creating shader program\n");
	}

	std::string vs, fs;

	//shader.vs 가 vs 안으로 로딩됨
	if (!ReadFile(filenameVS, &vs)) {
		printf("Error compiling vertex shader\n");
		return -1;
	};

	//shader.fs 가 fs 안으로 로딩됨
	if (!ReadFile(filenameFS, &fs)) {
		printf("Error compiling fragment shader\n");
		return -1;
	};

	// ShaderProgram 에 vs.c_str() 버텍스 쉐이더를 컴파일한 결과를 attach함
	AddShader(ShaderProgram, vs.c_str(), GL_VERTEX_SHADER);

	// ShaderProgram 에 fs.c_str() 프레그먼트 쉐이더를 컴파일한 결과를 attach함
	AddShader(ShaderProgram, fs.c_str(), GL_FRAGMENT_SHADER);

	GLint Success = 0;
	GLchar ErrorLog[1024] = { 0 };

	//Attach 완료된 shaderProgram 을 링킹함
	glLinkProgram(ShaderProgram);

	//링크가 성공했는지 확인
	glGetProgramiv(ShaderProgram, GL_LINK_STATUS, &Success);

	if (Success == 0) {
		// shader program 로그를 받아옴
		glGetProgramInfoLog(ShaderProgram, sizeof(ErrorLog), NULL, ErrorLog);
		std::cout << filenameVS << ", " << filenameFS << " Error linking shader program\n" << ErrorLog;
		return -1;
	}

	glValidateProgram(ShaderProgram);
	glGetProgramiv(ShaderProgram, GL_VALIDATE_STATUS, &Success);
	if (!Success) {
		glGetProgramInfoLog(ShaderProgram, sizeof(ErrorLog), NULL, ErrorLog);
		std::cout << filenameVS << ", " << filenameFS << " Error validating shader program\n" << ErrorLog;
		return -1;
	}

	glUseProgram(ShaderProgram);
	std::cout << filenameVS << ", " << filenameFS << " Shader compiling is done.";

	return ShaderProgram;
}

void Renderer::DrawSolidRect(float x, float y, float z, float size, float r, float g, float b, float a)
{
	float newX, newY;

	GetGLPosition(x, y, &newX, &newY);

	//Program select
	glUseProgram(m_SolidRectShader);

	glUniform4f(glGetUniformLocation(m_SolidRectShader, "u_Trans"), newX, newY, 0, size);
	glUniform4f(glGetUniformLocation(m_SolidRectShader, "u_Color"), r, g, b, a);

	int attribPosition = glGetAttribLocation(m_SolidRectShader, "a_Position");
	glEnableVertexAttribArray(attribPosition);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBORect);
	glVertexAttribPointer(attribPosition, 3, GL_FLOAT, GL_FALSE, sizeof(float) * 3, 0);

	glDrawArrays(GL_TRIANGLES, 0, 6);

	glDisableVertexAttribArray(attribPosition);

	glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

void Renderer::DrawTest()
{
	m_time += 0.00016f;

	//Program select
	glUseProgram(m_TestShader);

	int uTimeLoc = glGetUniformLocation(m_TestShader,
		"u_Time");
	glUniform1f(uTimeLoc, m_time);

	int aPosLoc = glGetAttribLocation(m_TestShader, "a_Position");
	int aRadiusLoc = glGetAttribLocation(m_TestShader, "a_Radius");
	int aColorLoc = glGetAttribLocation(m_TestShader, "a_Color");

	glEnableVertexAttribArray(aPosLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOtestPos);
	glVertexAttribPointer(aPosLoc, 3, GL_FLOAT, GL_FALSE, sizeof(float) * 4, 0);

	glEnableVertexAttribArray(aRadiusLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOtestPos);
	glVertexAttribPointer(aRadiusLoc, 1, GL_FLOAT, GL_FALSE, sizeof(float) * 4, (GLvoid*)(sizeof(float)*3));

	glEnableVertexAttribArray(aColorLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOtestColor);
	glVertexAttribPointer(aColorLoc, 4, GL_FLOAT, GL_FALSE, sizeof(float) * 4, 0);


	glDrawArrays(GL_TRIANGLES, 0, 12);

	glDisableVertexAttribArray(aPosLoc);
	glDisableVertexAttribArray(aRadiusLoc);
	glDisableVertexAttribArray(aColorLoc);

	glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

void Renderer::DrawParticle()
{
	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

	m_time += 0.00016f;

	//Program select
	GLuint shader = m_ParticleShader;
	glUseProgram(shader);

	int uTimeLoc = glGetUniformLocation(shader,
		"u_Time");
	glUniform1f(uTimeLoc, m_time);

	int aPosLoc = glGetAttribLocation(shader, "a_Position");
	int aRadiusLoc = glGetAttribLocation(shader, "a_Radius");
	int aColorLoc = glGetAttribLocation(shader, "a_Color");
	int aSTimeLoc = glGetAttribLocation(shader, "a_STime");
	int aVelLoc = glGetAttribLocation(shader, "a_Vel");
	int aLifeTimeLoc = glGetAttribLocation(shader, "a_LifeTime");

	int stride = 13;

	glEnableVertexAttribArray(aPosLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glVertexAttribPointer(aPosLoc, 3, GL_FLOAT, GL_FALSE, sizeof(float) * stride, 0);

	glEnableVertexAttribArray(aRadiusLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glVertexAttribPointer(aRadiusLoc, 1, GL_FLOAT, GL_FALSE, sizeof(float) * stride, (GLvoid*)(sizeof(float) * 3));

	glEnableVertexAttribArray(aColorLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glVertexAttribPointer(aColorLoc, 4, GL_FLOAT, GL_FALSE, sizeof(float) * stride, (GLvoid*)(sizeof(float) * 4));

	glEnableVertexAttribArray(aSTimeLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glVertexAttribPointer(aSTimeLoc, 1, GL_FLOAT, GL_FALSE, sizeof(float) * stride, (GLvoid*)(sizeof(float) * 8));

	glEnableVertexAttribArray(aVelLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glVertexAttribPointer(aVelLoc, 3, GL_FLOAT, GL_FALSE, sizeof(float) * stride, (GLvoid*)(sizeof(float) * 9));

	glEnableVertexAttribArray(aLifeTimeLoc);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glVertexAttribPointer(aLifeTimeLoc, 1, GL_FLOAT, GL_FALSE, sizeof(float) * stride, (GLvoid*)(sizeof(float) * 12));

	

	glDrawArrays(GL_TRIANGLES, 0, m_VBOParticleVertexCount);	// m_VBOParticleVertexCount

	glDisableVertexAttribArray(aPosLoc);
	glDisableVertexAttribArray(aRadiusLoc);
	glDisableVertexAttribArray(aColorLoc);

	glBindFramebuffer(GL_FRAMEBUFFER, 0);

	glDisable(GL_BLEND);
}

void Renderer::GetGLPosition(float x, float y, float *newX, float *newY)
{
	*newX = x * 2.f / m_WindowSizeX;
	*newY = y * 2.f / m_WindowSizeY;
}

void Renderer::GenerateParticles(int numParticle)
{
	int floatCountPervertex = 3 + 1 + 4 + 1 + 3 + 1; // x, y, z, value, r, g, b, a, sTime, vx, vy, vz, lifeTime
	int verticesCountPerParticle = 6;
	int floatCountPerParticle = floatCountPervertex * verticesCountPerParticle;
	int totalVerticesCount = numParticle * verticesCountPerParticle;
	int totalFloatCount = floatCountPervertex * totalVerticesCount;

	float* vertices = new float[totalFloatCount];

	for (int i = 0; i < numParticle; ++i) {
		float x, y, z, value, r, g, b, a, lifeTime;
		/*x = (float)rand() / (float)RAND_MAX * 2.f - 1.f;
		y = (float)rand() / (float)RAND_MAX * 2.f - 1.f;*/
		x = 0.f;
		y = 0.f;
		z = 0.f;
		value = (float)rand() / (float)RAND_MAX;
		r =  (float)rand() / (float)RAND_MAX;
		g =  (float)rand() / (float)RAND_MAX;
		b =  (float)rand() / (float)RAND_MAX;
		a =  (float)rand() / (float)RAND_MAX;
		float size;
		size = ((float)rand() / (float)RAND_MAX) * 0.007f;
		float sTime = ((float)rand() / (float)RAND_MAX) * 2.f;
		float vx, vy, vz;
		vx = ((float)rand() / (float)RAND_MAX * 2.f - 1.f) * 1.0f;
		vy = (((float)rand() / (float)RAND_MAX * 2.f - 1.f) + 1.5f) * 0.8f;
		vz = 0.f;
		lifeTime = (float)rand() / (float)RAND_MAX * 2.f;

		int index = i * floatCountPervertex * verticesCountPerParticle;	

		vertices[index++] = x - size;		// v1
		vertices[index++] = y - size;
		vertices[index++] = z;
		vertices[index++] = value;
		vertices[index++] = r;
		vertices[index++] = g;
		vertices[index++] = b;
		vertices[index++] = a;
		vertices[index++] = sTime;
		vertices[index++] = vx;		
		vertices[index++] = vy;
		vertices[index++] = vz;
		vertices[index++] = lifeTime;

		vertices[index++] = x + size;		// v2
		vertices[index++] = y - size;
		vertices[index++] = z;
		vertices[index++] = value;
		vertices[index++] = r;
		vertices[index++] = g;
		vertices[index++] = b;
		vertices[index++] = a;
		vertices[index++] = sTime;
		vertices[index++] = vx;
		vertices[index++] = vy;
		vertices[index++] = vz;
		vertices[index++] = lifeTime;

		vertices[index++] = x + size;		// v3
		vertices[index++] = y + size;
		vertices[index++] = z;
		vertices[index++] = value;
		vertices[index++] = r;
		vertices[index++] = g;
		vertices[index++] = b;
		vertices[index++] = a;
		vertices[index++] = sTime;
		vertices[index++] = vx;
		vertices[index++] = vy;
		vertices[index++] = vz;
		vertices[index++] = lifeTime;

		vertices[index++] = x - size;		// v4
		vertices[index++] = y + size;
		vertices[index++] = z;
		vertices[index++] = value;
		vertices[index++] = r;
		vertices[index++] = g;
		vertices[index++] = b;
		vertices[index++] = a;
		vertices[index++] = sTime;
		vertices[index++] = vx;
		vertices[index++] = vy;
		vertices[index++] = vz;
		vertices[index++] = lifeTime;

		vertices[index++] = x - size;		// v5
		vertices[index++] = y - size;
		vertices[index++] = z;
		vertices[index++] = value;
		vertices[index++] = r;
		vertices[index++] = g;
		vertices[index++] = b;
		vertices[index++] = a;
		vertices[index++] = sTime;
		vertices[index++] = vx;
		vertices[index++] = vy;
		vertices[index++] = vz;
		vertices[index++] = lifeTime;

		vertices[index++] = x + size;		// v6
		vertices[index++] = y + size;
		vertices[index++] = z;
		vertices[index++] = value;
		vertices[index++] = r;
		vertices[index++] = g;
		vertices[index++] = b;
		vertices[index++] = a;
		vertices[index++] = sTime;
		vertices[index++] = vx;
		vertices[index++] = vy;
		vertices[index++] = vz;
		vertices[index++] = lifeTime;
	}

	glGenBuffers(1, &m_VBOParticle);
	glBindBuffer(GL_ARRAY_BUFFER, m_VBOParticle);
	glBufferData(GL_ARRAY_BUFFER, sizeof(float) * totalFloatCount, vertices, GL_STATIC_DRAW);

	delete [] vertices;

	m_VBOParticleVertexCount = totalVerticesCount;
}

