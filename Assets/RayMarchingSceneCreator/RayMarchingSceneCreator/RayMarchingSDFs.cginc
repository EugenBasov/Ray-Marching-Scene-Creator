#ifndef RAYMARCH_SDF_INCLUDED
#define RAYMARCH_SDF_INCLUDED

#include "RayMarchingH.cginc"

#define SPHERE 0
#define SPHERE_CUT 1
#define SPHERE_CUT_HOLLOW 2
#define SPHERE_DEATH_STAR 3
#define CUBE 4
#define CUBE_ROUND 5
#define CUBE_FRAME 6
#define TORUS 7
#define TORUS_CAPPED 8
#define LINK 9
#define CONE 10
#define CONE_CAPPED_VERTICAL 11
#define CONE_CAPPED_2DOTS 12
#define CONE_ROUND_VERTICAL 13
#define CONE_ROUND_2DOTS 14
#define CONE_INFINITE 15
#define PLANE 16
#define PRISM 17
#define PRISM_HEX 18
#define CAPSULE_VERTICAL 19
#define CAPSULE_2DOTS 20
#define CYLINDER_VERTICAL 21
#define CYLINDER_2DOTS 22
#define CYLINDER_ROUND 23
#define CYLINDER_INFINITE 24
#define SOLID_ANGLE 25
#define VESICA 26
#define RHOMBUS 27
#define OCTAHEDRON 28
#define PYRAMID 29
#define TRIANGLE 30
#define QUAD 31

float smin(float a, float b, float k)
{
    k *= 1.0 / (1.0 - sqrt(0.5));
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - k * 0.5 * (1.0 + h - sqrt(1.0 - h * (h - 2.0)));
}

float smax(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) + k * h * (1.0 - h);
}

float sphere(float3 p, float3 r)
{
    float k0 = length(p / r);
    float k1 = length(p / (r * r));
    return k0 * (k0 - 1.0) / k1;
}

float sphereCut(float3 p, float r, float h)
{
  // sampling independent computations (only depend on shape)
    float w = sqrt(r * r - h * h);

  // sampling dependant computations
    float2 q = float2(length(p.xz), p.y);
    float s = max((h - r) * q.x * q.x + w * w * (h + r - 2.0 * q.y), h * q.x - w * q.y);
    return (s < 0.0) ? length(q) - r :
         (q.x < w) ? h - q.y :
                   length(q - float2(w, h));
}

float sphereCutHollow(float3 p, float r, float h, float t)
{
  // sampling independent computations (only depend on shape)
    float w = sqrt(r * r - h * h);
  
  // sampling dependant computations
    float2 q = float2(length(p.xz), p.y);
    return ((h * q.x < w * q.y) ? length(q - float2(w, h)) :
                          abs(length(q) - r)) - t;
}

float deathStar(float3 p2, float ra, float rb, float d)
{
  // sampling independent computations (only depend on shape)
    float a = (ra * ra - rb * rb + d * d) / (2.0 * d);
    float b = sqrt(max(ra * ra - a * a, 0.0));
	
  // sampling dependant computations
    float2 p = float2(p2.x, length(p2.yz));
    if (p.x * b - p.y * a > d * max(b - p.y, 0.0))
        return length(p - float2(a, b));
    else
        return max((length(p) - ra),
               -(length(p - float2(d, 0.0)) - rb));
}

float box(float3 p, float3 r)
{
    float3 q = abs(p) - r;
    return length(max(q, 0)) + min(max(q.x, max(q.y, q.z)), 0);
}

float boxRound(float3 p, float3 b, float r)
{
    float3 q = abs(p) - b + r;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
}

float boxFrame(float3 p, float3 b, float r, float e)
{
    if (e == 1.0)
    {
        float3 q = abs(p) - b + r;
        return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
    }
    else
    {
        e = e / 2.0;
        float rb = max(max(b.x, b.y), b.z) * r;
        p = abs(p) - b + rb;
        float3 q = abs(p + e * b) - e * b;
        return min(
        min(
        length(max(float3(p.x, q.y, q.z), 0.0)) + min(max(p.x, max(q.y, q.z)), 0.0),
        length(max(float3(q.x, p.y, q.z), 0.0)) + min(max(q.x, max(p.y, q.z)), 0.0)
        ),
        length(max(float3(q.x, q.y, p.z), 0.0)) + min(max(q.x, max(q.y, p.z)), 0.0)
        ) - rb;
    }
}

float torus(float3 p, float2 t)
{
    float2 q = float2(length(p.xz) - t.x, p.y);
    return length(q) - t.y;
}

float torusCapped(float3 p, float2 sc, float ra, float rb)
{
    p.x = abs(p.x);
    float k = (sc.y * p.x > sc.x * p.y) ? dot(p.xy, sc) : length(p.xy);
    return sqrt(dot(p, p) + ra * ra - 2.0 * ra * k) - rb;
}

float link(float3 p, float le, float r1, float r2)
{
    float3 q = float3(p.x, max(abs(p.y) - le, 0.0), p.z);
    return length(float2(length(q.xy) - r1, q.z)) - r2;
}

float cone(float3 p, float2 q)
{
    float2 w = float2(length(p.xz), p.y);
    float2 a = w - q * clamp(dot(w, q) / dot(q, q), 0.0, 1.0);
    float2 b = w - q * float2(clamp(w.x / q.x, 0.0, 1.0), 1.0);
    float k = sign(q.y);
    float d = min(dot(a, a), dot(b, b));
    float s = max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
    return sqrt(d) * sign(s);
}

float coneInfinite(float3 p, float2 c)
{
    // c is the sin/cos of the angle
    float2 q = float2(length(p.xz), -p.y);
    float d = length(q - c * max(dot(q, c), 0.0));
    return d * ((q.x * c.y - q.y * c.x < 0.0) ? -1.0 : 1.0);
}

float coneCappedVertical(float3 p, float r1, float r2, float h)
{
    float2 q = float2(length(p.xz), p.y);
    float2 k1 = float2(r2, h);
    float2 k2 = float2(r2 - r1, 2.0 * h);
    float2 ca = float2(q.x - min(q.x, (q.y < 0.0) ? r1 : r2), abs(q.y) - h);
    float2 cb = q - k1 + k2 * clamp(dot(k1 - q, k2) / dot(k2, k2), 0.0, 1.0);
    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    return s * sqrt(min(dot(ca, ca), dot(cb, cb)));
}

float coneCapped2Dots(float3 p, float3 a, float3 b, float ra, float rb)
{
    float rba = rb - ra;
    float baba = dot(b - a, b - a);
    float papa = dot(p - a, p - a);
    float paba = dot(p - a, b - a) / baba;
    float x = sqrt(papa - paba * paba * baba);
    float cax = max(0.0, x - ((paba < 0.5) ? ra : rb));
    float cay = abs(paba - 0.5) - 0.5;
    float k = rba * rba + baba;
    float f = clamp((rba * (x - ra) + paba * baba) / k, 0.0, 1.0);
    float cbx = x - ra - f * rba;
    float cby = paba - f;
    float s = (cbx < 0.0 && cay < 0.0) ? -1.0 : 1.0;
    return s * sqrt(min(cax * cax + cay * cay * baba,
                     cbx * cbx + cby * cby * baba));
}

float coneRoundVertical(float3 p, float r1, float r2, float h)
{
  // sampling independent computations (only depend on shape)
    float b = (r1 - r2) / h;
    float a = sqrt(1.0 - b * b);

  // sampling dependant computations
    float2 q = float2(length(p.xz), p.y);
    float k = dot(q, float2(-b, a));
    if (k < 0.0)
        return length(q) - r1;
    if (k > a * h)
        return length(q - float2(0.0, h)) - r2;
    return dot(q, float2(a, b)) - r1;
}

float coneRound2Dots(float3 p, float3 a, float3 b, float r1, float r2)
{
  // sampling independent computations (only depend on shape)
    float3 ba = b - a;
    float l2 = dot(ba, ba);
    float rr = r1 - r2;
    float a2 = l2 - rr * rr;
    float il2 = 1.0 / l2;
    
  // sampling dependant computations
    float3 pa = p - a;
    float y = dot(pa, ba);
    float z = y - l2;
    float x2 = dot(pa * l2 - ba * y, pa * l2 - ba * y);
    float y2 = y * y * l2;
    float z2 = z * z * l2;

  // single square root!
    float k = sign(rr) * rr * rr * x2;
    if (sign(z) * a2 * z2 > k)
        return sqrt(x2 + z2) * il2 - r2;
    if (sign(y) * a2 * y2 < k)
        return sqrt(x2 + y2) * il2 - r1;
    return (sqrt(x2 * a2 * il2) + y * rr) * il2 - r1;
}

float plane(float3 p)
{
    return dot(p, float3(0, 1, 0));
}

float prism(float3 p, float2 h)
{
    float3 q = abs(p);
    return max(q.z - h.y, max(q.x * 0.866025 + p.y * 0.5, -p.y) - h.x * 0.5);
}

float prismHex(float3 p, float2 h)
{
    const float3 k = float3(-0.8660254, 0.5, 0.57735);
    p = abs(p);
    p.xy -= 2.0 * min(dot(k.xy, p.xy), 0.0) * k.xy;
    float2 d = float2(
       length(p.xy - float2(clamp(p.x, -k.z * h.x, k.z * h.x), h.x)) * sign(p.y - h.x),
       p.z - h.y);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float capsuleVertical(float3 p, float h, float r)
{
    p.y -= clamp(p.y, 0.0, h);
    return length(p) - r;
}

float capsule2Dots(float3 p, float3 a, float3 b, float r)
{
    float3 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}

float cylinderInfinite(float3 p, float3 c)
{
    return length(p.xz - c.xy) - c.z;
}

float cylinderVertical(float3 p, float r, float h)
{
    float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float cylinder2Dots(float3 p, float3 a, float3 b, float r)
{
    float3 ba = b - a;
    float3 pa = p - a;
    float baba = dot(ba, ba);
    float paba = dot(pa, ba);
    float x = length(pa * baba - ba * paba) - r * baba;
    float y = abs(paba - baba * 0.5) - baba * 0.5;
    float x2 = x * x;
    float y2 = y * y * baba;
    float d = (max(x, y) < 0.0) ? -min(x2, y2) : (((x > 0.0) ? x2 : 0.0) + ((y > 0.0) ? y2 : 0.0));
    return sign(d) * sqrt(abs(d)) / baba;
}

float cylinderRounded(float3 p, float ra, float rb, float h)
{
    rb = ra * rb;
    float2 d = float2(length(p.xz) - ra + rb, abs(p.y) - h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rb;
}

float solidAngle(float3 p, float2 c, float ra)
{
  // c is the sin/cos of the angle
    float2 q = float2(length(p.xz), p.y);
    float l = length(q) - ra;
    float m = length(q - c * clamp(dot(q, c), 0.0, ra));
    return max(l, m * sign(c.y * q.x - c.x * q.y));
}

float vesicaSegment(in float3 p, in float3 a, in float3 b, in float w)
{
    float3 c = (a + b) * 0.5;
    float l = length(b - a);
    float3 v = (b - a) / l;
    float y = dot(p - c, v);
    float2 q = float2(length(p - c - y * v), abs(y));
    
    float r = 0.5 * l;
    float d = 0.5 * (r * r - w * w) / w;
    float3 h = (r * q.x < d * (q.y - r)) ? float3(0.0, r, 0.0) : float3(-d, 0.0, d + w);
 
    return length(q - h.xy) - h.z;
}

float rhombus(float3 p, float la, float lb, float h, float ra)
{
    p = abs(p);
    float2 b = float2(la, lb);
    float f = clamp((ndot(b, b - 2.0 * p.xz)) / dot(b, b), -1.0, 1.0);
    float2 q = float2(length(p.xz - 0.5 * b * float2(1.0 - f, 1.0 + f)) * sign(p.x * b.y + p.z * b.x - b.x * b.y) - ra, p.y - h);
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0));
}

float octahedron(float3 p, float s)
{
    p = abs(p);
    float m = p.x + p.y + p.z - s;
    float3 q;
    if (3.0 * p.x < m)
        q = p.xyz;
    else if (3.0 * p.y < m)
        q = p.yzx;
    else if (3.0 * p.z < m)
        q = p.zxy;
    else
        return m * 0.57735027;
    
    float k = clamp(0.5 * (q.z - q.y + s), 0.0, s);
    return length(float3(q.x, q.y - s + k, q.z - k));
}

float pyramid(float3 p, float h)
{
    float m2 = h * h + 0.25;
    
    p.xz = abs(p.xz);
    p.xz = (p.z > p.x) ? p.zx : p.xz;
    p.xz -= 0.5;

    float3 q = float3(p.z, h * p.y - 0.5 * p.x, h * p.x + 0.5 * p.y);
   
    float s = max(-q.x, 0.0);
    float t = clamp((q.y - 0.5 * p.z) / (m2 + 0.25), 0.0, 1.0);
    
    float a = m2 * (q.x + s) * (q.x + s) + q.y * q.y;
    float b = m2 * (q.x + 0.5 * t) * (q.x + 0.5 * t) + (q.y - m2 * t) * (q.y - m2 * t);
    
    float d2 = min(q.y, -q.x * m2 - q.y * 0.5) > 0.0 ? 0.0 : min(a, b);
    
    return sqrt((d2 + q.z * q.z) / m2) * sign(max(q.z, -p.y));
}

float triangl(float3 p, float3 a, float3 b, float3 c)
{
    float3 ba = b - a;
    float3 pa = p - a;
    float3 cb = c - b;
    float3 pb = p - b;
    float3 ac = a - c;
    float3 pc = p - c;
    float3 nor = cross(ba, ac);

    return sqrt(
    (sign(dot(cross(ba, nor), pa)) +
     sign(dot(cross(cb, nor), pb)) +
     sign(dot(cross(ac, nor), pc)) < 2.0)
     ?
     min(min(
     dot2(ba * clamp(dot(ba, pa) / dot2(ba), 0.0, 1.0) - pa),
     dot2(cb * clamp(dot(cb, pb) / dot2(cb), 0.0, 1.0) - pb)),
     dot2(ac * clamp(dot(ac, pc) / dot2(ac), 0.0, 1.0) - pc))
     :
     dot(nor, pa) * dot(nor, pa) / dot2(nor));
}

float quad(float3 p, float3 a, float3 b, float3 c, float3 d)
{
    float3 ba = b - a;
    float3 pa = p - a;
    float3 cb = c - b;
    float3 pb = p - b;
    float3 dc = d - c;
    float3 pc = p - c;
    float3 ad = a - d;
    float3 pd = p - d;
    float3 nor = cross(ba, ad);

    return sqrt(
    (sign(dot(cross(ba, nor), pa)) +
     sign(dot(cross(cb, nor), pb)) +
     sign(dot(cross(dc, nor), pc)) +
     sign(dot(cross(ad, nor), pd)) < 3.0)
     ?
     min(min(min(
     dot2(ba * clamp(dot(ba, pa) / dot2(ba), 0.0, 1.0) - pa),
     dot2(cb * clamp(dot(cb, pb) / dot2(cb), 0.0, 1.0) - pb)),
     dot2(dc * clamp(dot(dc, pc) / dot2(dc), 0.0, 1.0) - pc)),
     dot2(ad * clamp(dot(ad, pd) / dot2(ad), 0.0, 1.0) - pd))
     :
     dot(nor, pa) * dot(nor, pa) / dot2(nor));
}

#endif
