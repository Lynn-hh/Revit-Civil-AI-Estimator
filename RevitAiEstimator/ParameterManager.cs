using System;
using Autodesk.Revit.DB;

namespace RevitAiEstimator
{
    public static class ParameterManager
    {
        public static void SetupParameters(Document doc, Autodesk.Revit.ApplicationServices.Application app)
        {
            // 1. Check if parameters already exist (simple check)
            if (ParameterExists(doc, "AI_CostCode")) return;

            // 2. Manage Shared Parameter File
            string sharedParamsPath = app.SharedParametersFilename;
            if (string.IsNullOrEmpty(sharedParamsPath) || !System.IO.File.Exists(sharedParamsPath))
            {
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RevitAISharedParams.txt");
                if (!System.IO.File.Exists(tempPath))
                {
                    System.IO.File.WriteAllText(tempPath, ""); // Create empty file
                }
                app.SharedParametersFilename = tempPath;
                sharedParamsPath = tempPath;
            }

            DefinitionFile defFile = app.OpenSharedParameterFile();
            if (defFile == null) return;

            // 3. Get or Create Group
            DefinitionGroup group = defFile.Groups.get_Item("AI_Estimator");
            if (group == null) group = defFile.Groups.Create("AI_Estimator");

            // 4. Create Definitions
            Definition defCode = GetOrCreateDef(group, "AI_CostCode", SpecTypeId.String.Text);
            Definition defDesc = GetOrCreateDef(group, "AI_Description", SpecTypeId.String.Text);
            Definition defRate = GetOrCreateDef(group, "AI_Rate", SpecTypeId.Number);

            // 5. Bind to Categories
            CategorySet catSet = app.Create.NewCategorySet();
            InsertCategory(doc, catSet, BuiltInCategory.OST_Walls);
            InsertCategory(doc, catSet, BuiltInCategory.OST_Floors);
            InsertCategory(doc, catSet, BuiltInCategory.OST_StructuralFoundation);
            InsertCategory(doc, catSet, BuiltInCategory.OST_Topography);
            InsertCategory(doc, catSet, BuiltInCategory.OST_Toposolid); // 2024+

            using (Transaction t = new Transaction(doc, "Create AI Parameters"))
            {
                t.Start();
                BindParam(doc, defCode, catSet);
                BindParam(doc, defDesc, catSet);
                BindParam(doc, defRate, catSet);
                t.Commit();
            }
        }

        private static bool ParameterExists(Document doc, string name)
        {
            BindingMap map = doc.ParameterBindings;
            DefinitionBindingMapIterator it = map.ForwardIterator();
            while (it.MoveNext())
            {
                if (it.Key.Name == name) return true;
            }
            return false;
        }

        private static Definition GetOrCreateDef(DefinitionGroup group, string name, ForgeTypeId typeId)
        {
            Definition def = group.Definitions.get_Item(name);
            if (def == null)
            {
                ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(name, typeId);
                def = group.Definitions.Create(options);
            }
            return def;
        }

        private static void InsertCategory(Document doc, CategorySet set, BuiltInCategory bic)
        {
            try
            {
                Category cat = doc.Settings.Categories.get_Item(bic);
                if (cat != null) set.Insert(cat);
            }
            catch { }
        }

        private static void BindParam(Document doc, Definition def, CategorySet cats)
        {
            InstanceBinding binding = doc.Application.Create.NewInstanceBinding(cats);
            // Revit 2024+ API Change: BuiltInParameterGroup is replaced by GroupTypeId
            doc.ParameterBindings.Insert(def, binding, GroupTypeId.Data);
        }

        public static void SetElementValues(Element elem, EstimationResult result)
        {
            Parameter codeParam = elem.LookupParameter("AI_CostCode");
            Parameter descParam = elem.LookupParameter("AI_Description");
            Parameter rateParam = elem.LookupParameter("AI_Rate");

            if (codeParam != null && !codeParam.IsReadOnly) codeParam.Set(result.CostCode);
            if (descParam != null && !descParam.IsReadOnly) descParam.Set(result.Description);
            if (rateParam != null && !rateParam.IsReadOnly) rateParam.Set(result.UnitRate);
        }
    }
}
