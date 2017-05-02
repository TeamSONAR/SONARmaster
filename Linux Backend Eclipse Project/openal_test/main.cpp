// SONARBackEnd.cpp : Defines the entry point for the console application.
//



//#include "opencv2/core/core.hpp"
//#include "opencv2/highgui/highgui.hpp"
//#include "opencv2/imgproc/imgproc.hpp"
#include <librealsense/rs.hpp>
#include <opencv2/opencv.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>

#include <iostream>
#include <fstream>
#include "AL/al.h"
#include "AL/alc.h"
#include "math.h"

using namespace rs;
using namespace std;

#define verticalsources 16
//#define AspectRatio 0.75
//#define fovDeg 58.5
#define PI 3.1415926535

#define CASE_RETURN(err) case (err): return "##err"

int xSize = 320;
int ySize = 240;

//User parameters for audio stuff
float freq = 200.f;
int horizontal_steps = 20;
float freqInc = 1.15f;
int stepDelay = 35;

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

void readUserParamFile() {

	ifstream readFile;

	readFile.open("UserParameters.txt");

	/*if(!readFile.good()){
		while(!readFile.good()){
			char buff[100];
			std::string current_working_dir(buff);
			cout << "loc is: " << current_working_dir << endl;
			cout << "Enter file location" << endl;
			string loc;
			cin >> loc;
			readFile.open(loc);
		}
	}*/

	if (readFile.good())
	{
		readFile >> freq;
		readFile >> freqInc;
		readFile >> horizontal_steps;
		readFile >> stepDelay;
	}

	printf("freq is %3.2f, freq increment is %2.2f\n horizontal steps is %d, step delay is %d ms\n", freq, freqInc, horizontal_steps, stepDelay);
	readFile.close();
}

int const INPUT_WIDTH      = 320;
int const INPUT_HEIGHT     = 240;
int const FRAMERATE        = 0;

context      _rs_ctx;
device&      _rs_camera = *_rs_ctx.get_device( 0 );
intrinsics   _depth_intrin;
intrinsics  _color_intrin;
bool         _loop = true;


// Initialize the application state. Upon success will return the static app_state vars address

bool initialize_streaming( )
{
       bool success = false;
       if( _rs_ctx.get_device_count( ) > 0 )
       {
             //_rs_camera.enable_stream( rs::stream::color, INPUT_WIDTH, INPUT_HEIGHT, rs::format::rgb8, FRAMERATE );
             _rs_camera.enable_stream( rs::stream::depth, INPUT_WIDTH, INPUT_HEIGHT, rs::format::z16, FRAMERATE );
             _rs_camera.start( );

             success = true;
       }
       return success;
}

int main()
{
	readUserParamFile();

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
			samples[i] = 16380 * sin((2.f*float(PI)*freq) / sample_rate * i) * amplitude;
		}

		amplitude = amplitude * rolloff;

		freq = freq*freqInc;
		/* Download buffer to OpenAL */
		alBufferData(buf[q], AL_FORMAT_MONO16, samples, buf_size * 2, sample_rate);
		al_check_error();
	}

	//Generate 16 sources
	ALuint *srclist;
	srclist = new ALuint[verticalsources];

	alGenSources(verticalsources, srclist);
	int y;
	int x;
	int xPix;
	int yPix;
	int sourceMatCoords[verticalsources];
	float sourcePos[verticalsources];

	for (int i = 0; i < verticalsources; ++i) {
		//Vertical position
		x = i;
		xPix = (ySize/(verticalsources *2))+(ySize/ verticalsources)*x;

		sourcePos[i] = -2;

		sourceMatCoords[i] = xPix;

		alSourcei(srclist[i], AL_BUFFER, buf[i]);
		alSourcef(srclist[i], AL_REFERENCE_DISTANCE, 1.0f);
		alSourcei(srclist[i], AL_SOURCE_RELATIVE, AL_TRUE);
		alSourcei(srclist[i], AL_LOOPING, AL_TRUE);
		alSource3f(srclist[i], AL_POSITION, -2, 0, 1);
		alSourcePlay(srclist[i]);

	}

	al_check_error();

	//RealSense stuff

	if( !initialize_streaming( ) )
	   {
			 std::cout << "Unable to locate a camera" << std::endl;
			 rs::log_to_console( rs::log_severity::fatal );
			 return EXIT_FAILURE;
	   }

	//-------------------------------Back to openCV stuff
	cv::Mat depth8u;
	cv::Mat flipped;

	cv::namedWindow("Display window", cv::WINDOW_AUTOSIZE);// Create a window for display.

	int64_t delay;
	int64_t starttick;

	int horizpos = 0;
	bool cPressed = false;
	int keyCode = 255;
	// 255: 'no key' press, < 0: 'no key registered', 99: 'c' pressed
	while(keyCode == 255 || keyCode < 0 || keyCode == 99){
		starttick = cv::getTickCount();
		keyCode = cv::waitKey(1);
		// Pressed c, hide/show window
		if (keyCode == 99)
		{
			cPressed = !cPressed;
			printf("cpressed is: %d\n", cPressed);
		}
		//printf("key code is: %d\n", keyCode);
		if (horizpos == horizontal_steps) {
			//printf("new thing \n");
			horizpos = 0;
		}

		if (horizpos == 0) {
			if( _rs_camera.is_streaming( ) )
			                    _rs_camera.wait_for_frames( );

			//show new image
			_depth_intrin       = _rs_camera.get_stream_intrinsics( rs::stream::depth );

		   // Create depth image
		   cv::Mat depth16( _depth_intrin.height,
									  _depth_intrin.width,
									  CV_16U,
									  (uchar *)_rs_camera.get_frame_data( rs::stream::depth ) );

		   depth8u = depth16;

		   depth8u.convertTo( depth8u, CV_8UC1, 255.0/5000 );

		   cv::bitwise_not(depth8u, depth8u);
		   cv::threshold( depth8u, depth8u, 254, 255, 4 );
		   cv::bitwise_not(depth8u, depth8u);

		   cv::flip(depth8u, flipped, 0);

		   imshow("Display window", depth8u);

		   //printf("showing stuff\n");
		}

		//OpenAL Stuff-----------------------------------
		for (int i = 0; i < verticalsources; ++i) {
			//defines roi
			cv::Rect roi((xSize/horizontal_steps)*horizpos, (ySize/ verticalsources)*i, (xSize/horizontal_steps), (ySize/ verticalsources));

			//copies input image in roi
			cv::Mat image_roi = flipped(roi);

			//computes mean over roi
			cv::Scalar avgPixelIntensity = cv::mean(image_roi);

			//prints out only .val[0] since image was grayscale
			int pointdist = avgPixelIntensity.val[0];

			pointdistnorm = float(pointdist) / 255;
			//rectangle(planes[0], Point(horizpos*(xSize / horizontal_steps), sourceMatCoords[i]), Point(horizpos*(xSize / horizontal_steps) + 3, sourceMatCoords[i] + 3), Scalar(255));

			angle = PI * (float(horizpos) / float(horizontal_steps-1));

			xdist = -cos(angle);
			zdist = sin(angle);

			alSource3f(srclist[i], AL_POSITION, xdist, 0, zdist);
			alSourcef(srclist[i], AL_GAIN, exp( 6.908*(1-pointdistnorm) )/1000); //should be 6.908 for normal rolloff
		}
		//End openAL stuff-------------------------------------------
		delay = ((cv::getTickCount() - starttick) * 1000) / cv::getTickFrequency();
		while (delay < stepDelay) { delay = ((cv::getTickCount() - starttick) * 1000) / cv::getTickFrequency(); }
		//printf("%d delay", delay);

		horizpos++;
	}

	//Realsense stuff
	cv::destroyAllWindows( );
	_rs_camera.stop( );

	//OpenAL stuff-----------------
	alDeleteSources(verticalsources, srclist);
	al_check_error();
	exit_al();
	//Stop OpenAL stuff-------------------

	return 0;
}
