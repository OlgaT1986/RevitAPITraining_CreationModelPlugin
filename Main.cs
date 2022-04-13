using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPITraining_CreationModelPlugin
{
    [Transaction(TransactionMode.Manual)] 

    public class Main : IExternalCommand   
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document; // ссылка на документ

            Level level1, level2;
            TakeLevels(doc, out level1, out level2);
            CreateWalls(doc, level1, level2);
            return Result.Succeeded;
        }

        private static void CreateWalls(Document doc, Level level1, Level level2) 
        {
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters); // ширина
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters); // глубина
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>(); // массив точек
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>(); // массив создаем стены

            Transaction ts = new Transaction(doc, "Построение стен"); // внутри транзакции помещаем цикл-будет создавать стены
            ts.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]); // отрезок
                Wall wall = Wall.Create(doc, line, level1.Id, false); // стена
                walls.Add(wall); // массив стен
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id); // привязываем стену к уровню 2 
            }
            ts.Commit();
        }

        private static void TakeLevels(Document doc, out Level level1, out Level level2)
            {
                List<Level> listLevel = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level)) // отфильтровали уровни
                    .OfType<Level>()
                    .ToList();

                level1 = listLevel
                    .Where(x => x.Name.Equals("Уровень 1"))
                    .FirstOrDefault();
                level2 = listLevel
                    .Where(x => x.Name.Equals("Уровень 2"))
                    .FirstOrDefault();
            }
        }
    }