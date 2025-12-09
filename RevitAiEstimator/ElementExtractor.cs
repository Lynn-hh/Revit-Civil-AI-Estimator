using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitAiEstimator
{
    public class ElementData
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string MaterialName { get; set; }
        public double MetricQuantity { get; set; } // e.g., Volume in m3 or Area in m2
        public string Unit { get; set; }
    }

    public class ElementExtractor
    {
        private Document _doc;

        public ElementExtractor(Document doc)
        {
            _doc = doc;
        }

        public List<ElementData> GetCivilElements()
        {
            List<ElementData> dataList = new List<ElementData>();

            // Categories often used in Civil: Topography, Structural Foundations, Floors (Slab), Walls (Retaining)
            BuiltInCategory[] categories = new BuiltInCategory[]
            {
                BuiltInCategory.OST_Topography,
                BuiltInCategory.OST_Toposolid, // For Revit 2024+
                BuiltInCategory.OST_StructuralFoundation,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Walls
            };

            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(categories);
            FilteredElementCollector collector = new FilteredElementCollector(_doc).WherePasses(filter).WhereElementIsNotElementType();

            foreach (Element e in collector)
            {
                ElementData data = new ElementData
                {
                    Id = e.Id.IntegerValue,
                    Category = e.Category.Name,
                    FamilyName = e.Name
                };

                // Try get Type Name
                ElementId typeId = e.GetTypeId();
                if (typeId != ElementId.InvalidElementId)
                {
                    ElementType type = _doc.GetElement(typeId) as ElementType;
                    data.TypeName = type.Name;
                }

                // Extract Material (Simplified: Get first material)
                ICollection<ElementId> matIds = e.GetMaterialIds(false);
                if (matIds.Count > 0)
                {
                    Material mat = _doc.GetElement(matIds.First()) as Material;
                    data.MaterialName = mat.Name;
                }
                else
                {
                    data.MaterialName = "Unspecified";
                }

                // Extract Geometry (Volume or Area)
                Parameter volParam = e.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                if (volParam != null && volParam.HasValue)
                {
                    // Revit internal units are cubic feet. Convert to m3.
                    // 1 ft3 = 0.0283168 m3
                    data.MetricQuantity = volParam.AsDouble() * 0.0283168; 
                    data.Unit = "m3";
                }
                else
                {
                    Parameter areaParam = e.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                    if (areaParam != null && areaParam.HasValue)
                    {
                        // 1 ft2 = 0.092903 m2
                        data.MetricQuantity = areaParam.AsDouble() * 0.092903;
                        data.Unit = "m2";
                    }
                }

                dataList.Add(data);
            }

            return dataList;
        }
    }
}
