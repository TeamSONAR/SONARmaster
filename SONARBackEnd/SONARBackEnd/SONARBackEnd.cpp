// SONARBackEnd.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


#include "opencv2/core/core.hpp"
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include <iostream>
#include <fstream>
#include<windows.h>


using namespace cv;
using namespace std;

#define verticalsources 16

#define AspectRatio 0.75
#define fovDeg 58.5

#define CASE_RETURN(err) case (err): return "##err"

int xSize = 640;
int ySize = 480;

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

	for (int y = 0; y < xSize; y++) {
		for (int x = 0; x < ySize; x++) {
			thetaY = (-fovDeg / 2) + (fovDeg*y / xSize);
			thetaX = (-fovDeg / (2 / AspectRatio)) + (fovDeg*x / (xSize / AspectRatio));

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

void initDim() {
	/*ofstream test;
	test.open("blah.txt");
	test << "HELLO";
	test.close();
	*/
	int x; int y;
	ifstream readFile;
	
	//readFile.open("C:\\SONARmaster\\SonarGraphicsTestProj\\dimensions.txt");
	readFile.open("..\\..\\..\\SonarGraphicsTestProj\\dimensions.txt");
	if (!readFile.good())
	{
		readFile.close();
		readFile.open("..\\..\\SonarGraphicsTestProj\\dimensions.txt");
	}
	if (!readFile.good())
	{
		cerr << "Bad file access";
		exit(-1);
	}
	//while (readFile.good())
		//cout << (char)readFile.get();
	readFile >> x;
	readFile >> y;
	xSize = x; ySize = y;
	printf("x size is: %d, y size is: %d\n", x, y);
	readFile.close();

}

int main()
{
	initDim();
	void* PointerToBuf = OpenDepthBufMapFileToRead(xSize,ySize);
	printf("%X \n", ReadDepthMapBufFile(PointerToBuf));
	
	Mat Xmat = Mat(ySize, xSize, CV_32FC1);
	Mat Ymat = Mat(ySize, xSize, CV_32FC1);

	CreateXY(&Xmat, &Ymat);

	int* Tacos = new int[xSize * ySize];
	memcpy(Tacos, ReadDepthMapBufFile(PointerToBuf), xSize * ySize * 4);
	Mat image = Mat(ySize, xSize, CV_16UC2, Tacos);
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

	ALuint buf[verticalsources];
	alGenBuffers(verticalsources, buf);
	al_check_error();

	/* Fill buffer with Sine-Wave */
	float freq = 120.f;

	float seconds = 1;
	float amplitude = 0.1f;
	float rolloff = 0.77f;

	unsigned sample_rate = 22050;
	size_t buf_size = seconds * sample_rate;

	short *samples;
	samples = new short[buf_size];

	//Set up the frequencied buffers
	for (int q = 0; q < verticalsources; q++){
		for (int i = 0; i<buf_size; ++i) {
			samples[i] = 16380 * sin((2.f*float(3.141592)*freq) / sample_rate * i) * amplitude;
		}

		amplitude = amplitude * rolloff;

		freq = freq*1.3f;
		/* Download buffer to OpenAL */
		alBufferData(buf[q], AL_FORMAT_MONO16, samples, buf_size * 2, sample_rate);
		al_check_error();
	}
	
	//Generate 16 sources
	//int num_sources = 16;
	ALuint *srclist;
	srclist = new ALuint[verticalsources];

	alGenSources(verticalsources, srclist);
	int y;
	int x;
	int xPix;
	int yPix;
	int sourceMatCoords[verticalsources];
	float sourcePos[verticalsources];
	int horizontal_steps = 20;
	int64 tick = 0;
	float secondselapsed;
	int64 tick2 = 0;
	float secondselapsed2  = 0;

	for (int i = 0; i < verticalsources; ++i) {
		//Vertical position
		x = i;
		xPix = (ySize/(verticalsources *2))+(ySize/ verticalsources)*x;
		
		//Horizontal position
		//y = (i - x) / 4;
		//yPix = 80 + (y * 160);

		sourcePos[i] = -2;//y - 1.5;
		//sourcePos[i][1] = 0;

		sourceMatCoords[i] = xPix;
		//sourceMatCoords[i][1] = yPix;

		alSourcei(srclist[i], AL_BUFFER, buf[i]);
		alSourcef(srclist[i], AL_REFERENCE_DISTANCE, 1.0f);
		alSourcei(srclist[i], AL_SOURCE_RELATIVE, AL_TRUE);
		alSourcei(srclist[i], AL_LOOPING, AL_TRUE);
		alSource3f(srclist[i], AL_POSITION, -2, 0, 1);
		alSourcePlay(srclist[i]);

	}


	//-------------------------------Back to openCV stuff

	namedWindow("Display window", WINDOW_AUTOSIZE);// Create a window for display.
	imshow("Display window", planes[0]);                   // Show our image inside it.

	int horizpos = 0;
	int keyCode = 255;
	//while (cvWaitKey(1) < 0) {
	while(keyCode == 255 || keyCode < 0){
		keyCode = waitKey(45);
		//printf("key code is: %d\n", keyCode);
		if (horizpos == horizontal_steps) { printf("new thing \n"); horizpos = 0; }

		if (horizpos == 0) {
			memcpy(Tacos, ReadDepthMapBufFile(PointerToBuf), xSize * ySize * 4);
			split(image, planes);
			Mat flipped;
			flip(planes[0], flipped, 0);
			imshow("Display window", flipped);

			secondselapsed = (getTickCount() - tick) / getTickFrequency();
			printf("%2.4f \n", secondselapsed);
			printf("%2.6f \n", secondselapsed2);
			secondselapsed2 = 0;
			tick = getTickCount();
		}
		
		//OpenAL Stuff-----------------------------------
		tick2 = getTickCount();
		for (int i = 0; i < verticalsources; ++i) {
			//x = i % 4;
			//y = (i - x) / 4;
			//pointdist = planes[1].at<ushort>(Point(sourceMatCoords[i][1], sourceMatCoords[i][0]));
			
			//printf("defing roi \n");
			//defines roi
			cv::Rect roi((xSize/horizontal_steps)*horizpos, (ySize/ verticalsources)*i, (xSize/horizontal_steps), (ySize/ verticalsources));

			//copies input image in roi
			cv::Mat image_roi = planes[1](roi);

			//printf("compingmean \n");
			//computes mean over roi
			cv::Scalar avgPixelIntensity = cv::mean(image_roi);

			//prints out only .val[0] since image was grayscale
			int pointdist = avgPixelIntensity.val[0];
			
			pointdistnorm = float(pointdist) / 255;
			rectangle(planes[0], Point(horizpos*(xSize / horizontal_steps), sourceMatCoords[i]), Point(horizpos*(xSize / horizontal_steps) + 3, sourceMatCoords[i] + 3), Scalar(255));

			alSource3f(srclist[i], AL_POSITION, -1.9+4*(float(horizpos)/float(horizontal_steps)), 0, 1);
			alSourcef(srclist[i], AL_GAIN, exp( 6.908*(1-pointdistnorm) )/1000); //should be 6.908 for normal rolloff
		}
		//End openAL stuff-------------------------------------------
		//printf("\n");

		horizpos++;

		secondselapsed2 += (getTickCount() - tick2) / getTickFrequency();
	}
	UnmapDepthBufFile(PointerToBuf);

	//OpenAL stuff-----------------
	alDeleteSources(verticalsources, srclist);
	exit_al();
	al_check_error();
	//Stop OpenAL stuff-------------------

	delete Tacos;
	return 0;
}
