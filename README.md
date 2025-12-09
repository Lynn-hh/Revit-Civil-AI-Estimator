# Revit Civil AI Estimator

An AI-powered Revit Add-in that automates the Quantity Takeoff and Cost Estimation process for Civil Engineering elements (Walls, Floors, Topography, etc.).

It connects Revit 2025 to OpenAI (GPT-4) to automatically classify elements, assign cost codes (CESMM4/SMM7), and estimate unit rates based on project context.

![Revit AI Estimator Screenshot](https://via.placeholder.com/800x400?text=Revit+AI+Estimator+Screenshot)

## Features
- **Smart Estimation**: Automatically extracts geometry and material data from selected elements.
- **Context-Aware AI**: Allows user to input project context (e.g., "Texas, 2024 Pricing") to adjust estimates.
- **Auto-Parameter Creation**: Automatically creates Shared Parameters (`AI_CostCode`, `AI_Description`, `AI_Rate`) in your Revit project.
- **Async Processing**: Runs API calls in the background to prevent Revit UI freezing.

## Prerequisites
- **Autodesk Revit 2025** (Project targets .NET 8).
- **Visual Studio 2022** (with .NET Desktop Development workload).
- **OpenAI API Key** (You need a paid account to access GPT-4 models).

## Installation & Usage

Since this project requires an OpenAI API Key, you must build it yourself.

1. **Clone this repository**.
2. **Open in Visual Studio 2022**.
3. **Add your API Key**:
   - Open the file `AiService.cs`.
   - Find the line `private const string ApiKey = ...`
   - Paste your own OpenAI API key (starts with `sk-...`).
4. **Build the Solution**:
   - Click **Build** -> **Build Solution**.
5. **Install to Revit**:
   - Go to the output folder `bin\Debug\net8.0-windows\`.
   - Open `RevitAiEstimator.addin` and check the path.
   - Copy `RevitAiEstimator.addin` to `%ProgramData%\Autodesk\Revit\Addins\2025\`.
6. **Run Revit**:
   - The "AI Estimator" tab will appear.

## Usage
1. Open Revit 2025 and load a model.
2. Go to the **"AI Estimator"** tab in the Ribbon.
3. Click **"Smart Estimate"**.
4. In the dialog box, enter your **Project Context**.
   - Example 1: *"Austin, Texas. Use local 2024 labor rates. Soil: Limestone."*
   - Example 2: *"Texas Project. Output in US dollars (USD)."*
5. Click **Start**. The plugin will process elements and populate the properties.
6. Check the **Properties Palette** of any wall/floor to see the results.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
