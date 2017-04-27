
Shader "Custom/GridSelected"
{
	Properties
	{
		_LineScale("LineScale", Float) = 0.01
		_LinesPerMeter("LinesPerMeter", Float) = 5
	}

		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 200

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		// These values map from the properties block at the beginning of the shader file.
		// They can be set at run time using renderer.material.SetFloat()
		float _LineScale;
	float _LinesPerMeter;

	// This is the data structure that the vertex program provides to the fragment program.
	struct VertToFrag
	{
		float4 viewPos : SV_POSITION;
		float3 normal : NORMAL;
		float4 worldPos: TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};


	// This is the vertex program.
	VertToFrag vert(appdata_base v)
	{
		UNITY_SETUP_INSTANCE_ID(v);
		VertToFrag o;

		// Calculate where the vertex is in view space.
		o.viewPos = mul(UNITY_MATRIX_MVP, v.vertex);

		// Calculate the normal in WorldSpace.
		o.normal = UnityObjectToWorldNormal(v.normal);

		// Calculate where the object is in world space.
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);

		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		return o;
	}

	fixed4 frag(VertToFrag i) : SV_Target
	{
		// Check where this pixel is in world space.
		// wpmod is documented on the internet, it's basically a 
		// floating point mod function.
		float4 wpmodip;
	float4 wpmod = modf(i.worldPos * _LinesPerMeter, wpmodip);

	// Initialize to draw black with full alpha. This way we will occlude holograms even when
	// we are drawing black.
	fixed4 ret = float4(0,0,0,0);

	/*
	// Normals need to be renormalized in the fragment shader to overcome
	// interpolation.
	float3 normal = normalize(i.normal);
	*/

	if (abs(wpmod.y) < _LineScale* _LinesPerMeter || abs(wpmod.x) < _LineScale* _LinesPerMeter)
	{
		ret = float4(0.3, 1, 0.3, 1);
		ret.a = 1;
	}

	return ret;
	}
		ENDCG
	}
	}
}