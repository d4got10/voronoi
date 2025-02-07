#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Output fragment color
out vec4 finalColor;

#define MAX_POINT_COUNT 16

// Custom variables
uniform int count = 0;
uniform int xCoords[MAX_POINT_COUNT];
uniform int yCoords[MAX_POINT_COUNT];
uniform int color[MAX_POINT_COUNT];
uniform int width;
uniform int height;

void main()
{
    vec2 fragPos = fragTexCoord;
    ivec2 ipos = ivec2(floor(fragPos.x * width), floor(fragPos.y * height));  // Get the integer coords

    int min_dist = 4000000;
    int result = 0;
    for(int i = 0; i < count; i++) 
    {
        int dx = xCoords[i] - ipos.x;
        int dy = yCoords[i] - ipos.y;
        int dist = dx * dx;
        if (dist == min_dist) {
            result = 0;
        } else if(dist < min_dist) {
            result = color[i];
            min_dist = dist;
        }

        if(abs(dx) < 10 && abs(dy) < 10) {
            result = 0;
        }
    }

    finalColor = vec4(vec3(result / 255.0), 1.0);
}