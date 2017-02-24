// SONARBackEnd.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <iostream>
#include <fstream>

using namespace cv;
using namespace std;

#define AspectRatio 0.75
#define fovDeg 58.5

#define CASE_RETURN(err) case (err): return "##err"
const char* al_err_str(ALenum err) {
	switch (err) {
		CASE_RETURN(AL_NO_ERROR);
		CASE_RETURN(AL_INVALID_NAME);
		CASE_RETURN(AL_INVALID_ENUM);
		CASE_RETURN(AL_INVALID_VALUE);
		CASE_RETURN(AL_INVALID_OPERATION);
		CASE_RETURN(AL_OUT_OF_MEMORY);
	}
	return "unknown";
}
#undef CASE_RETURN

#define __al_check_error(file,line) \
    do { \
        ALenum err = alGetError(); \
        for(; err!=AL_NO_ERROR; err=alGetError()) { \
            std::cerr << "AL Error " << al_err_str(err) << " at " << file << ":" << line << std::endl; \
        } \
    }while(0)

#define al_check_error() \
    __al_check_error(__FILE__, __LINE__)


void init_al() {
	ALCdevice *dev = NULL;
	ALCcontext *ctx = NULL;

	const char *defname = alcGetString(NULL, ALC_DEFAULT_DEVICE_SPECIFIER);
	std::cout << "Default device: " << defname << std::endl;

	dev = alcOpenDevice(defname);
	ctx = alcCreateContext(dev, NULL);
	alcMakeContextCurrent(ctx);
}

void exit_al() {
	ALCdevice *dev = NULL;
	ALCcontext *ctx = NULL;
	ctx = alcGetCurrentContext();
	dev = alcGetContextsDevice(ctx);

	alcMakeContextCurrent(NULL);
	alcDestroyContext(ctx);
	alcCloseDevice(dev);
}

void CreateXY(Mat* Xmat, Mat* Ymat) {
	float thetaY;
	float thetaX;
	float PhiZX;
	float PhiZY;
	float a1;
	float a2;
	float PhiZ;
	float Xpos;
	float Ypos;

	for (int y = 0; y < 640; y++) {
		for (int x = 0; x < 480; x++) {
			thetaY = (-fovDeg / 2) + (fovDeg*y / 640);
			thetaX = (-fovDeg / (2 / AspectRatio)) + (fovDeg*x / (640 / AspectRatio));

			PhiZX = (90 - abs(thetaX)) * (180 / 3.1415926535);
			PhiZY = (90 - abs(thetaY)) * (180 / 3.1415926535);

			a1 = pow(1.0F / tan(PhiZX), 2);
			a2 = pow(1.0F / tan(PhiZY), 2);

			PhiZ = atan(1 / sqrt(a1 + a2));

			Xpos = cos(PhiZ) * cos(atan2(thetaY, (thetaX + 0.0001)));
			Ypos = cos(PhiZ) * sin(atan2(thetaY, (thetaX + 0.0001)));

			Xmat->at<float>(Point(y, x)) = Xpos;
			Ymat->at<float>(Point(y, x)) = Ypos;
		}
	}
}

int main()
{
	void* PointerToBuf = OpenDepthBufMapFileToRead();
	printf("%X \n", ReadDepthMapBufFile(PointerToBuf));
	//while (CheckDMBFlag(PointerToBuf)) {
	//}
	
	/*streampos size;
	char * memblock;
	void* PointerToBuf = 0;

	ifstream file("F:/seniorproject/UnityProjects/Testing/SonarGraphicsTestProj/infoFile.txt", ios::in | ios::binary | ios::ate);
	if (file.is_open())
	{
		size = file.tellg();
		memblock = new char[size];
		file.seekg(0, ios::beg);
		file.read(memblock, size);
		file.close();

		long PointerInt = *((long*)memblock);

		PointerToBuf = (void*)PointerInt;

		printf("opened file\n");

		printf("%d", PointerToBuf);
	}
	else {
		printf("failed to open file\n");
	}*/
	
	Mat Xmat = Mat(480, 640, CV_32FC1);
	Mat Ymat = Mat(480, 640, CV_32FC1);

	CreateXY(&Xmat, &Ymat);

	int* Tacos = new int[640 * 480];
	memcpy(Tacos, ReadDepthMapBufFile(PointerToBuf), 640 * 480 * 4);
	Mat image = Mat(480, 640, CV_16UC2, Tacos);
	Mat planes[2];
	split(image, planes);

	if (!image.data)                              // Check for invalid input
	{
		cout << "Could not open or find the image" << std::endl;
		return -1;
	}

	//OpenAL stuff----------------------------------
	unsigned short pointdist;
	float pointdistnorm;
	float angle;
	float zdist;
	float xdist;

	init_al();

	ALuint buf[4];
	alGenBuffers(4, buf);
	al_check_error();

	/* Fill buffer with Sine-Wave */
	float freq = 180.f;
	float seconds = 0.5; //was 0.25
	float sintime = 2; // was 1
	unsigned sample_rate = 22050;
	size_t buf_size = seconds * sample_rate;

	short *samples;
	samples = new short[buf_size];
	for (int i = 0; i<buf_size; ++i) {
		samples[i] = 0;
	}

	for (int q = 0; q < 4; q++){
		for (int i = 0; i<(buf_size / sintime); ++i) {
			samples[i] = (16380 * ((buf_size / sintime) - i) * sintime * sin((2.f*float(3.141592)*freq) / sample_rate * i)) / buf_size;
		}

		freq = freq*2;
		/* Download buffer to OpenAL */
		alBufferData(buf[q], AL_FORMAT_MONO16, samples, buf_size * 2, sample_rate);
		al_check_error();
	}

	int xpts = 16;
	int ypts = 32;

	int num_sources = xpts;

	//Generate 16 sources
	ALuint *srclist;
	srclist = new ALuint[num_sources];
	int *srcTimeCounters;
	int *srcRemTime;
	srcTimeCounters = new int[num_sources];
	srcRemTime = new int[num_sources];
	alGenSources(num_sources, srclist);
	int y;
	int x;
	int xPix;
	int yPix;
	int sourceMatCoords[16][2];
	float sourcePos[16][2];

	for (int i = 0; i < num_sources; ++i) {
		//Vertical position
		x = i % 4;
		xPix = 60 + (x * 120);
		
		//Horizontal position
		y = (i - x) / 4;
		yPix = 80 + (y * 160);

		sourcePos[i][0] = y - 1.5;
		sourcePos[i][1] = 1;

		sourceMatCoords[i][0] = xPix;
		sourceMatCoords[i][1] = yPix;

		alSourcei(srclist[i], AL_BUFFER, buf[x]);
		alSourcef(srclist[i], AL_REFERENCE_DISTANCE, 1.0f);
		alSourcei(srclist[i], AL_SOURCE_RELATIVE, AL_TRUE);
		alSourcei(srclist[i], AL_LOOPING, AL_TRUE);
		alSourcei(srclist[i], AL_SAMPLE_OFFSET, (buf_size*i) / (num_sources));
		alSource3f(srclist[i], AL_POSITION, sourcePos[i][0], x-1.5, sourcePos[i][1]);
		alSourcePlay(srclist[i]);

		srcTimeCounters[i] = 0;
		srcRemTime[i] = 0;
	}
	//-------------------------------Back to openCV stuff

	namedWindow("Display window", WINDOW_AUTOSIZE);// Create a window for display.
	imshow("Display window", planes[0]);                   // Show our image inside it.

	while (waitKey(100) < 0) {

		//while (CheckDMBFlag(PointerToBuf) != 0) { waitKey(10); }

		memcpy(Tacos, ReadDepthMapBufFile(PointerToBuf), 640 * 480 * 4);
		printf("%X \n", Tacos[0]);
		split(image, planes);
		
		//OpenAL Stuff-----------------------------------
		for (int i = 0; i < num_sources; ++i) {
			x = i % 4;
			pointdist = planes[1].at<ushort>(Point(sourceMatCoords[i][1], sourceMatCoords[i][0]));
			
			pointdistnorm = float(pointdist) / 65535;
			rectangle(planes[0], Point(sourceMatCoords[i][1], sourceMatCoords[i][0]), Point(sourceMatCoords[i][1] + 3, sourceMatCoords[i][0] + 3), Scalar(255));

			//printf("%2.2f ", pointdistnorm);
			
			srcTimeCounters[i] = pointdistnorm * 10;
			srcRemTime[i] --;
			if (srcRemTime[i] <= 0) {
				alSourcePlay(srclist[i]);
				srcRemTime[i] = srcTimeCounters[i];
			}
			alSourcef(srclist[i], AL_GAIN, exp(6.908*(1-pointdistnorm))/2000);
		}
		//End openAL stuff-------------------------------------------

		imshow("Display window", planes[0]);
	}
	UnmapDepthBufFile(PointerToBuf);

	//OpenAL stuff-----------------
	alDeleteSources(num_sources, srclist);
	exit_al();
	al_check_error();
	//Stop OpenALL stuff-------------------

	delete Tacos;
	return 0;
}
