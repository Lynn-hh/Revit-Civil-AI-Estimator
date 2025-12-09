using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitAiEstimator
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Prepare Parameters (ensure output fields exist)
                ParameterManager.SetupParameters(doc, uiapp.Application);

                // 2. Extract Data from selection or whole model
                var extractor = new ElementExtractor(doc);
                List<ElementData> civilElements = extractor.GetCivilElements();

                if (civilElements.Count == 0)
                {
                    TaskDialog.Show("Info", "No relevant civil elements found.");
                    return Result.Succeeded;
                }

                // 3. NEW: Ask User for Project Context
                string projectContext = "";
                using (var form = new EstimationOptionsForm())
                {
                    // If user cancels, stop the command
                    if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return Result.Cancelled;
                    }
                    projectContext = form.ProjectContext;
                }

                // 4. Process with AI
                AiService aiService = new AiService();
                int processedCount = 0;

                using (Transaction t = new Transaction(doc, "AI Estimation"))
                {
                    t.Start();

                    foreach (var elemData in civilElements)
                    {
                        // Call AI to get classification and cost (with Context)
                        EstimationResult result = aiService.GetEstimation(elemData, projectContext);

                        // Write back to Revit
                        // Note: ElementId constructor using long/int might vary by version, 
                        // but 2025 supports long. Assuming elemData.Id is compatible.
                        Element elem = doc.GetElement(new ElementId(elemData.Id));
                        
                        if (elem != null)
                        {
                            ParameterManager.SetElementValues(elem, result);
                            processedCount++;
                        }
                    }

                    t.Commit();
                }

                TaskDialog.Show("Success", $"AI Estimation Complete.\nProcessed {processedCount} elements.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
