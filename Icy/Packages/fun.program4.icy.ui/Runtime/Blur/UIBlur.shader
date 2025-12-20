/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *	 http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


 Shader "UI/UIBlur"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DownSampleValue("Down Sample Value", Float) = 1.0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		ZWrite Off
		ZTest Always
		Blend Off

		// Down Sample Pass
		Pass
		{
			Name "Down Sample"

			HLSLPROGRAM
			#pragma vertex vert_DownSmpl
			#pragma fragment frag_DownSmpl
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct VertexOutput_DownSmpl
			{
				float4 pos : SV_POSITION;
				float2 uv20 : TEXCOORD0;
				float2 uv21 : TEXCOORD1;
				float2 uv22 : TEXCOORD2;
				float2 uv23 : TEXCOORD3;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			float _DownSampleValue;

			VertexOutput_DownSmpl vert_DownSmpl(VertexInput v)
			{
				VertexOutput_DownSmpl o;
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				float2 offset = _MainTex_TexelSize.xy * 0.5 * _DownSampleValue;
				o.uv20 = v.texcoord + offset;
				o.uv21 = v.texcoord + float2(-offset.x, -offset.y);
				o.uv22 = v.texcoord + float2(offset.x, -offset.y);
				o.uv23 = v.texcoord + float2(-offset.x, offset.y);
				return o;
			}

			float4 frag_DownSmpl(VertexOutput_DownSmpl i) : SV_Target
			{
				float4 color = float4(0, 0, 0, 0);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv20);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv21);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv22);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23);
				return color / 4.0;
			}
			ENDHLSL
		}

		// Blur Vertical Pass
		Pass
		{
			Name "Blur Vertical"

			HLSLPROGRAM
			#pragma vertex vert_BlurVertical
			#pragma fragment frag_Blur
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct VertexOutput_Blur
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float2 offset : TEXCOORD1;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			float _DownSampleValue;

			static const float4 GaussWeight[7] =
			{
				float4(0.0205, 0.0205, 0.0205, 0),
				float4(0.0855, 0.0855, 0.0855, 0),
				float4(0.232, 0.232, 0.232, 0),
				float4(0.324, 0.324, 0.324, 1),
				float4(0.232, 0.232, 0.232, 0),
				float4(0.0855, 0.0855, 0.0855, 0),
				float4(0.0205, 0.0205, 0.0205, 0)
			};

			VertexOutput_Blur vert_BlurVertical(VertexInput v)
			{
				VertexOutput_Blur o;
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.uv = float4(v.texcoord.xy, 1, 1);
				o.offset = _MainTex_TexelSize.xy * float2(0.0, 1.0) * _DownSampleValue;
				return o;
			}

			float4 frag_Blur(VertexOutput_Blur i) : SV_Target
			{
				float2 uv = i.uv.xy;
				float2 offsetWidth = i.offset;
				float2 uvWithOffset = uv - offsetWidth * 3.0;

				float4 color = float4(0, 0, 0, 0);
				for (int j = 0; j < 7; j++)
				{
					float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvWithOffset);
					color += texCol * GaussWeight[j];
					uvWithOffset += offsetWidth;
				}

				color.a = 1;
				return color;
			}
			ENDHLSL
		}

		// Blur Horizontal Pass
		Pass
		{
			Name "Blur Horizontal"

			HLSLPROGRAM
			#pragma vertex vert_BlurHorizontal
			#pragma fragment frag_Blur
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct VertexOutput_Blur
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float2 offset : TEXCOORD1;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			float _DownSampleValue;

			static const float4 GaussWeight[7] =
			{
				float4(0.0205, 0.0205, 0.0205, 0),
				float4(0.0855, 0.0855, 0.0855, 0),
				float4(0.232, 0.232, 0.232, 0),
				float4(0.324, 0.324, 0.324, 1),
				float4(0.232, 0.232, 0.232, 0),
				float4(0.0855, 0.0855, 0.0855, 0),
				float4(0.0205, 0.0205, 0.0205, 0)
			};

			VertexOutput_Blur vert_BlurHorizontal(VertexInput v)
			{
				VertexOutput_Blur o;
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.uv = float4(v.texcoord.xy, 1, 1);
				o.offset = _MainTex_TexelSize.xy * float2(1.0, 0.0) * _DownSampleValue;
				return o;
			}

			float4 frag_Blur(VertexOutput_Blur i) : SV_Target
			{
				float2 uv = i.uv.xy;
				float2 offsetWidth = i.offset;
				float2 uvWithOffset = uv - offsetWidth * 3.0;

				float4 color = float4(0, 0, 0, 0);
				for (int j = 0; j < 7; j++)
				{
					float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvWithOffset);
					color += texCol * GaussWeight[j];
					uvWithOffset += offsetWidth;
				}

				color.a = 1;
				return color;
			}
			ENDHLSL
		}
	}
}

// Shader "UI/UIBlur"
// {
// 	Properties
// 	{
// 		_MainTex("Base (RGB)", 2D) = "white" {}
// 	}

// 	SubShader
// 	{
// 		ZWrite Off
// 		Blend Off

// 		Pass
// 		{
// 			ZTest Off
// 			Cull Off

// 			CGPROGRAM

// 			#pragma vertex vert_DownSmpl
// 			#pragma fragment frag_DownSmpl

// 			ENDCG
// 		}

// 		Pass
// 		{
// 			ZTest Always
// 			Cull Off

// 			CGPROGRAM

// 			#pragma vertex vert_BlurVertical
// 			#pragma fragment frag_Blur

// 			ENDCG
// 		}

// 		Pass
// 		{
// 			ZTest Always
// 			Cull Off

// 			CGPROGRAM

// 			#pragma vertex vert_BlurHorizontal
// 			#pragma fragment frag_Blur

// 			ENDCG
// 		}
// 	}

// 	CGINCLUDE

// 	#include "UnityCG.cginc"

// 	sampler2D _MainTex;
// 	uniform half4 _MainTex_TexelSize;
// 	uniform half _DownSampleValue;

// 	struct VertexInput
// 	{
// 		float4 vertex : POSITION;
// 		half2 texcoord : TEXCOORD0;
// 	};

// 	struct VertexOutput_DownSmpl
// 	{
// 		float4 pos : SV_POSITION;
// 		half2 uv20 : TEXCOORD0;
// 		half2 uv21 : TEXCOORD1;
// 		half2 uv22 : TEXCOORD2;
// 		half2 uv23 : TEXCOORD3;
// 	};

// 	static const half4 GaussWeight[7] =
// 	{
// 		half4(0.0205,0.0205,0.0205,0),
// 		half4(0.0855,0.0855,0.0855,0),
// 		half4(0.232,0.232,0.232,0),
// 		half4(0.324,0.324,0.324,1),
// 		half4(0.232,0.232,0.232,0),
// 		half4(0.0855,0.0855,0.0855,0),
// 		half4(0.0205,0.0205,0.0205,0)
// 	};

// 	VertexOutput_DownSmpl vert_DownSmpl(VertexInput v)
// 	{
// 		VertexOutput_DownSmpl o;
// 		o.pos = UnityObjectToClipPos(v.vertex);
// 		o.uv20 = v.texcoord + _MainTex_TexelSize.xy * half2(0.5h, 0.5h);;
// 		o.uv21 = v.texcoord + _MainTex_TexelSize.xy * half2(-0.5h, -0.5h);
// 		o.uv22 = v.texcoord + _MainTex_TexelSize.xy * half2(0.5h, -0.5h);
// 		o.uv23 = v.texcoord + _MainTex_TexelSize.xy * half2(-0.5h, 0.5h);
// 		return o;
// 	}

// 	fixed4 frag_DownSmpl(VertexOutput_DownSmpl i) : SV_Target
// 	{
// 		fixed4 color = (0,0,0,0);
// 		color += tex2D(_MainTex, i.uv20);
// 		color += tex2D(_MainTex, i.uv21);
// 		color += tex2D(_MainTex, i.uv22);
// 		color += tex2D(_MainTex, i.uv23);
// 		return color / 4;
// 	}

// 	struct VertexOutput_Blur
// 	{
// 		float4 pos : SV_POSITION;
// 		half4 uv : TEXCOORD0;
// 		half2 offset : TEXCOORD1;
// 	};

// 	VertexOutput_Blur vert_BlurHorizontal(VertexInput v)
// 	{
// 		VertexOutput_Blur o;
// 		o.pos = UnityObjectToClipPos(v.vertex);
// 		o.uv = half4(v.texcoord.xy, 1, 1);
// 		o.offset = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _DownSampleValue;
// 		return o;
// 	}

// 	VertexOutput_Blur vert_BlurVertical(VertexInput v)
// 	{
// 		VertexOutput_Blur o;
// 		o.pos = UnityObjectToClipPos(v.vertex);
// 		o.uv = half4(v.texcoord.xy, 1, 1);
// 		o.offset = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _DownSampleValue;
// 		return o;
// 	}

// 	half4 frag_Blur(VertexOutput_Blur i) : SV_Target
// 	{
// 		half2 uv = i.uv.xy;

// 		half2 OffsetWidth = i.offset;
// 		half2 uv_withOffset = uv - OffsetWidth * 3.0;

// 		half4 color = 0;
// 		for (int j = 0; j< 7; j++)
// 		{
// 			half4 texCol = tex2D(_MainTex, uv_withOffset);
// 			color += texCol * GaussWeight[j];
// 			uv_withOffset += OffsetWidth;
// 		}

// 		color.a = 1;
// 		return color;
// 	}

// 	ENDCG

// 	FallBack Off
// }
