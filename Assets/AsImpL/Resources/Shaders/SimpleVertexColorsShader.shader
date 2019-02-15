// Simple Vertex Colors shader, used when only vertex colors are available

Shader "Unlit/Simple Vertex Colors Shader" {
Properties {
  [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0 //"Off"
}

Category {
	Tags { "Queue"="Geometry" "RenderType"="Opaque" }
	Lighting Off ZWrite On Fog { Color (0,0,0,0) }
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Cull[_Cull]
		Pass {
		}
	}
}
}
