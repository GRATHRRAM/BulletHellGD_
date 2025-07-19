shader_type canvas_item;

uniform vec4 outline_color = vec4(0.0, 0.0, 0.0, 1.0);

uniform float outline_thickness = 5.0;
uniform float glow_size = 16.0;
uniform float glow_strength = 0.4;
uniform float noise_scale = 20.0;
uniform float noise_strength = 1.5;

float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

// Simple pseudo-random noise based on UV
float noise(vec2 uv) {
    vec2 i = floor(uv);
    vec2 f = fract(uv);
    float a = rand(i);
    float b = rand(i + vec2(1.0, 0.0));
    float c = rand(i + vec2(0.0, 1.0));
    float d = rand(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(a, b, u.x) +
           (c - a) * u.y * (1.0 - u.x) +
           (d - b) * u.x * u.y;
}

void fragment() {
    vec2 uv = UV;

    // Distort UVs for uneven edge look
    float distortion = noise(uv * noise_scale + TIME) * noise_strength;
    vec2 edge_uv = min(uv, vec2(1.0) - uv) + distortion * 0.01;
    float edge_dist = min(edge_uv.x, edge_uv.y) * float(textureSize(TEXTURE, 0).x);

    // Outline
    float outline = smoothstep(outline_thickness, outline_thickness - 1.0, edge_dist);

    // Irregular glow
    float glow_noise = noise(uv * 10.0 + TIME * 0.5);
    float glow = smoothstep(glow_size, glow_size - 2.0, edge_dist + glow_noise * 4.0);
    float pulse = 0.6 + 0.4 * sin(TIME * 3.0 + uv.x * 10.0);

    // Gradient fill
    vec4 fill = COLOR;
    fill.rgb *= 0.9 + 0.1 * uv.y;

    // Combine
    vec4 color = mix(fill, outline_color, outline);
    color += outline_color * glow * pulse * glow_strength;

    COLOR = color;
}