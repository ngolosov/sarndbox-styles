/***********************************************************************
SurfaceAddContourLines - Enhanced with realistic hillshade and Tanaka contouring
***********************************************************************/

#extension GL_ARB_texture_rectangle : enable

uniform sampler2DRect pixelCornerElevationSampler;
uniform float contourLineFactor;

// HILLSHADE SETTINGS
const float HILLSHADE_INTENSITY = 0.6;     // Hillshade strength
const float HILLSHADE_AZIMUTH = 315.0;     // Light from northwest
const float HILLSHADE_ELEVATION = 45.0;    // Natural sun angle
const float HILLSHADE_EXAGGERATION = 2.0;  // Terrain exaggeration
const float PIXEL_SCALE = 1.0;             // Gradient sensitivity

// TANAKA CONTOURING SETTINGS
const float TANAKA_INTENSITY = 0.8;        // Strength of Tanaka effect (0.0-1.0)
const float TANAKA_SHADOW_DARKNESS = 0.0;  // How dark shadow-side contours get (0.0 = black)
const float TANAKA_LIGHT_BRIGHTNESS = 0.6; // How light illuminated-side contours get (1.0 = invisible)
const float TANAKA_THRESHOLD = 0.1;        // Sensitivity threshold for determining lit vs shadow

// Function to calculate surface normal from elevation data
vec3 calculateSurfaceNormal(vec2 coord) {
    // Sample elevation values in a 3x3 neighborhood
    float tl = texture2DRect(pixelCornerElevationSampler, coord + vec2(-1.0, -1.0)).r;
    float tm = texture2DRect(pixelCornerElevationSampler, coord + vec2( 0.0, -1.0)).r;
    float tr = texture2DRect(pixelCornerElevationSampler, coord + vec2( 1.0, -1.0)).r;
    float ml = texture2DRect(pixelCornerElevationSampler, coord + vec2(-1.0,  0.0)).r;
    float mr = texture2DRect(pixelCornerElevationSampler, coord + vec2( 1.0,  0.0)).r;
    float bl = texture2DRect(pixelCornerElevationSampler, coord + vec2(-1.0,  1.0)).r;
    float bm = texture2DRect(pixelCornerElevationSampler, coord + vec2( 0.0,  1.0)).r;
    float br = texture2DRect(pixelCornerElevationSampler, coord + vec2( 1.0,  1.0)).r;
    
    // Apply elevation exaggeration
    tl *= HILLSHADE_EXAGGERATION; tm *= HILLSHADE_EXAGGERATION; tr *= HILLSHADE_EXAGGERATION;
    ml *= HILLSHADE_EXAGGERATION; mr *= HILLSHADE_EXAGGERATION;
    bl *= HILLSHADE_EXAGGERATION; bm *= HILLSHADE_EXAGGERATION; br *= HILLSHADE_EXAGGERATION;
    
    // Calculate gradients using Sobel operator
    float gx = (tr + 2.0*mr + br) - (tl + 2.0*ml + bl);
    float gy = (bl + 2.0*bm + br) - (tl + 2.0*tm + tr);
    
    // Normalize gradients
    gx *= PIXEL_SCALE / 8.0;
    gy *= PIXEL_SCALE / 8.0;
    
    // Calculate surface normal (pointing up)
    vec3 normal = normalize(vec3(-gx, -gy, 1.0));
    return normal;
}

// Function to calculate local gradient for Tanaka effect
vec2 calculateLocalGradient(vec2 coord) {
    // Simple gradient calculation for contour orientation
    float left  = texture2DRect(pixelCornerElevationSampler, coord + vec2(-1.0,  0.0)).r;
    float right = texture2DRect(pixelCornerElevationSampler, coord + vec2( 1.0,  0.0)).r;
    float down  = texture2DRect(pixelCornerElevationSampler, coord + vec2( 0.0, -1.0)).r;
    float up    = texture2DRect(pixelCornerElevationSampler, coord + vec2( 0.0,  1.0)).r;
    
    float gx = (right - left) * 0.5;
    float gy = (up - down) * 0.5;
    
    return vec2(gx, gy);
}

// Function to convert azimuth and elevation to 3D direction vector
vec3 sphericalToCartesian(float azimuth, float elevation) {
    float azimuthRad = radians(azimuth);
    float elevationRad = radians(elevation);
    
    return vec3(
        sin(azimuthRad) * cos(elevationRad),
        cos(azimuthRad) * cos(elevationRad),
        sin(elevationRad)
    );
}

// Realistic hillshade calculation
float calculateHillshade(vec3 normal, vec3 lightDir) {
    // Calculate the dot product between surface normal and light direction
    float cosTheta = dot(normal, lightDir);
    
    // Add ambient lighting to avoid pure black shadows
    float ambient = 0.2;
    
    // Combine directional and ambient lighting
    float shading = max(0.0, cosTheta) + ambient;
    
    // Apply gentle contrast enhancement
    shading = pow(shading, 1.1);
    
    // Keep in natural range
    return clamp(shading, 0.3, 1.2);
}

// Function to calculate Tanaka contour intensity
float calculateTanakaIntensity(vec2 coord, vec3 lightDir) {
    // Get local gradient (slope direction)
    vec2 gradient = calculateLocalGradient(coord);
    
    // Convert light direction to 2D (ignore z component)
    vec2 lightDir2D = normalize(lightDir.xy);
    
    // Calculate how much the slope faces the light
    float lightAlignment = dot(normalize(gradient), lightDir2D);
    
    // Determine if this contour segment is on lit or shadow side
    if(abs(lightAlignment) < TANAKA_THRESHOLD) {
        // Nearly perpendicular to light - use medium intensity
        return 0.3;
    } else if(lightAlignment > 0.0) {
        // Slope faces away from light (shadow side) - darker contours
        float shadowFactor = lightAlignment * TANAKA_INTENSITY;
        return mix(0.3, TANAKA_SHADOW_DARKNESS, shadowFactor);
    } else {
        // Slope faces toward light (illuminated side) - lighter contours
        float lightFactor = -lightAlignment * TANAKA_INTENSITY;
        return mix(0.3, TANAKA_LIGHT_BRIGHTNESS, lightFactor);
    }
}

void addContourLines(in vec2 fragCoord, inout vec4 baseColor) {
    /* Calculate the elevation of each pixel corner: */
    float corner0 = texture2DRect(pixelCornerElevationSampler, vec2(fragCoord.x, fragCoord.y)).r;
    float corner1 = texture2DRect(pixelCornerElevationSampler, vec2(fragCoord.x+1.0, fragCoord.y)).r;
    float corner2 = texture2DRect(pixelCornerElevationSampler, vec2(fragCoord.x, fragCoord.y+1.0)).r;
    float corner3 = texture2DRect(pixelCornerElevationSampler, vec2(fragCoord.x+1.0, fragCoord.y+1.0)).r;
    
    /* Calculate the elevation range of the pixel's area: */
    float elMin = min(min(corner0, corner1), min(corner2, corner3));
    float elMax = max(max(corner0, corner1), max(corner2, corner3));
    
    // Calculate light direction (same for both hillshade and Tanaka)
    vec3 lightDir = sphericalToCartesian(HILLSHADE_AZIMUTH, HILLSHADE_ELEVATION);
    
    /* Check if the pixel's area crosses at least one contour line: */
    if(floor(elMin*contourLineFactor) != floor(elMax*contourLineFactor)) {
        /* This is a contour line - apply Tanaka effect */
        
        // Calculate Tanaka intensity for this contour segment
        float tanakaIntensity = calculateTanakaIntensity(fragCoord, lightDir);
        
        // Apply Tanaka contouring (varying intensity based on illumination)
        baseColor = vec4(tanakaIntensity, tanakaIntensity, tanakaIntensity, 1.0);
        
    } else {
        /* Apply hillshade effect to non-contour areas */
        
        // Calculate surface normal at this fragment
        vec3 surfaceNormal = calculateSurfaceNormal(fragCoord);
        
        // Calculate hillshade value
        float hillshade = calculateHillshade(surfaceNormal, lightDir);
        
        // Apply hillshade by modulating the existing color
        baseColor.rgb *= mix(1.0, hillshade, HILLSHADE_INTENSITY);
    }
}
