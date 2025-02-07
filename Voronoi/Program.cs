// See https://aka.ms/new-console-template for more information

using Raylib_cs;

const int screenWidth = 800;
const int screenHeight = 450;

Raylib.InitWindow(screenWidth, screenHeight, "Voronoi");
var imBlank = Raylib.GenImageColor(screenWidth, screenHeight, Color.Blank);
var texture = Raylib.LoadTextureFromImage(imBlank);  // Load blank texture to fill on shader

var points = new List<Point>();

const int pointCount = 6;
var colorQueue = new Queue<int>();
for (int i = 0; i < 16; i++)
{
    colorQueue.Enqueue(128 / 16 * ((2 * i) % 16 + 1) + 64);
}

for (int i = 0; i < pointCount; i++)
{
    points.Add(new Point(Random.Shared.Next(screenWidth), Random.Shared.Next(screenHeight), colorQueue.Dequeue(), i));
}

var selectedShaderIndex = 0;
var shaders = new (string Name, Shader Shader)[]
{
    ("max(|dx|, |dy|)", LoadShader(pointCount, screenWidth, screenHeight, "voronoi.fs")),
    ("dx^2 + dy^2", LoadShader(pointCount, screenWidth, screenHeight, "voronoi_normal.fs")),
    ("|dx| + |dy|", LoadShader(pointCount, screenWidth, screenHeight, "voronoi_linear.fs")),
    ("sqrt(|dx|) + sqrt(|dy|)", LoadShader(pointCount, screenWidth, screenHeight, "voronoi_sqrt.fs")),
    ("|dx|", LoadShader(pointCount, screenWidth, screenHeight, "voronoi_horizontal.fs")),
    ("|dy|", LoadShader(pointCount, screenWidth, screenHeight, "voronoi_vertical.fs")),
};

int? selectedPoint = null; 

Raylib.UnloadImage(imBlank);
Raylib.SetTargetFPS(165);
while (!Raylib.WindowShouldClose())
{
    for (int i = 0; i < shaders.Length; i++)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One + i))
        {
            selectedShaderIndex = i;
        }
    }
    var shader = shaders[selectedShaderIndex].Shader;
    
    var mousePosition = Raylib.GetMousePosition();
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        var closestPoint = points
            .Where(p => Math.Abs(p.X - mousePosition.X) < 10 && Math.Abs(p.Y - mousePosition.Y) < 10)
            .OrderBy(p => Math.Abs(p.X - mousePosition.X) + Math.Abs(p.Y - mousePosition.Y))
            .FirstOrDefault();

        selectedPoint = closestPoint?.Index;
    }

    if (Raylib.IsMouseButtonReleased(MouseButton.Left))
    {
        selectedPoint = null;
    }

    if (Raylib.IsMouseButtonPressed(MouseButton.Right))
    {
        var closestPoint = points
            .Where(p => Math.Abs(p.X - mousePosition.X) < 10 && Math.Abs(p.Y - mousePosition.Y) < 10)
            .OrderBy(p => Math.Abs(p.X - mousePosition.X) + Math.Abs(p.Y - mousePosition.Y))
            .FirstOrDefault();

        if (closestPoint is not null)
        {
            points.Remove(closestPoint);
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i] with
                {
                    Index = i
                };
            }
            colorQueue.Enqueue(closestPoint.Color);
        }
        else if(points.Count < 16)
        {
            points.Add(new Point((int)mousePosition.X, (int)mousePosition.Y, colorQueue.Dequeue(), points.Count));
        }
    }

    if (selectedPoint is not null)
    {
        points[selectedPoint.Value] = points[selectedPoint.Value] with
        {
            X = (int)mousePosition.X,
            Y = (int)mousePosition.Y,
        };
    }
    
    for (int i = 0; i < points.Count; i++)
    {
        var point = points[i];
    
        int xCoords = Raylib.GetShaderLocation(shader, $"xCoords[{i}]");
        Raylib.SetShaderValue(shader, xCoords, point.X, ShaderUniformDataType.Int);

        int yCoords = Raylib.GetShaderLocation(shader, $"yCoords[{i}]");
        Raylib.SetShaderValue(shader, yCoords, point.Y, ShaderUniformDataType.Int);

        int color = Raylib.GetShaderLocation(shader, $"color[{i}]");
        Raylib.SetShaderValue(shader, color, point.Color, ShaderUniformDataType.Int);
    }
    
    int count = Raylib.GetShaderLocation(shader, "count");
    Raylib.SetShaderValue(shader, count, points.Count, ShaderUniformDataType.Int);
    
    Raylib.BeginDrawing();
    
    Raylib.ClearBackground(Color.White);
    
    Raylib.BeginShaderMode(shader);    // Enable our custom shader for next shapes/textures drawings
    Raylib.DrawTexture(texture, 0, 0, Color.White);  // Drawing BLANK texture, all magic happens on shader
    Raylib.EndShaderMode();            // Disable our custom shader, return to default shader

    for (int i = 0; i < shaders.Length; i++)
    {
        Raylib.DrawText($"{i + 1} - {shaders[i].Name} {(selectedShaderIndex == i ? "[SELECTED]" : "")}", 10, i * 20 + 10, 20, Color.Maroon);
    }
    
    Raylib.EndDrawing();
}

Shader LoadShader(int count, int width, int height, string name)
{
    var shader = Raylib.LoadShader(string.Empty, Path.Combine("Resources", "Shaders", "Glsl", name));
    
    int countLocation = Raylib.GetShaderLocation(shader, "count");
    Raylib.SetShaderValue(shader, countLocation, count, ShaderUniformDataType.Int);

    int widthLocation = Raylib.GetShaderLocation(shader, "width");
    Raylib.SetShaderValue(shader, widthLocation, width, ShaderUniformDataType.Int);

    int heightLocation = Raylib.GetShaderLocation(shader, "height");
    Raylib.SetShaderValue(shader, heightLocation, height, ShaderUniformDataType.Int);
    return shader;
}

record Point(int X, int Y, int Color, int Index);