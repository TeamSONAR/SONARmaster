// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DepthToColorShader" {
	SubShader{
		Tags{ "RenderType" = "Opaque" }

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		sampler2D _CameraDepthTexture;

	struct v2f {
		float4 pos : SV_POSITION;
		float4 scrPos:TEXCOORD1;
		//float depth : TEXCOORD0;
	};

	struct fragOut
	{
		half4 color : SV_Target;
		//float depth : SV_Depth;
	};

	//Vertex Shader
	v2f vert(appdata_base v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.scrPos = ComputeScreenPos(o.pos);
		//UNITY_TRANSFER_DEPTH(o.depth);
		return o;
	}

	//Fragment Shader
	fragOut frag(v2f i) {
		fragOut o;

		float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);

		o.color.g = depthValue;// fmod(depthValue * 256, 1);
		o.color.b = depthValue;//floor(depthValue * 256)/256;

		o.color.r = depthValue;// 0;
		o.color.a = 0;
		return o;
	}

	ENDCG
	}
	}
		FallBack "Diffuse"
}