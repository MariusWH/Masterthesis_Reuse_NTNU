using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.MethodOne
{ 

    public class MethodOne : GH_Component
    {

        public MethodOne()
          : base("MethodOne", "MethodOne",
              "Description",
              "Master", "MethodOne")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("GeometryLines", "GeometryLines", "GeometryLines", GH_ParamAccess.list, new Line());
            pManager.AddPointParameter("Supports", "Supports", "Supports", GH_ParamAccess.list, new Point3d());
            pManager.AddGenericParameter("ReusableStock", "ReusableStock", "ReusableStock", GH_ParamAccess.list);
            pManager.AddTextParameter("NewElements", "NewElements", "NewElements", GH_ParamAccess.list, "ALL");

            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Distribution Direction", "", "", GH_ParamAccess.item);
            pManager.AddLineParameter("LoadLines", "LoadLines", "LoadLines", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddNumberParameter("N", "N", "N", GH_ParamAccess.list);
            pManager.AddColourParameter("Util", "Util", "Util", GH_ParamAccess.list);
            pManager.AddBrepParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.list);

            pManager.AddGenericParameter("ReplacementSuggestions", "", "", GH_ParamAccess.tree);

        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            List<OLDReusableElement> iReusableElements = new List<OLDReusableElement>();
            List<string> iNewElementsCatalog = new List<string>();

            DA.GetDataList(0, iGeometryLines);
            DA.GetDataList(1, iSupports);
            DA.GetDataList(2, iReusableElements);
            DA.GetDataList(3, iNewElementsCatalog);

            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(4, ref iLineLoadValue);
            DA.GetData(5, ref iLineLoadDirection);
            DA.GetData(6, ref iLineLoadDistribution);
            DA.GetDataList(7, iLinesToLoad);



            

            // CODE
            List<double> loadList = new List<double>();
            List<Vector3d> loadVecs = new List<Vector3d> { new Vector3d(0, 0, 0) };


            TrussModel2D truss2D = new TrussModel2D(iGeometryLines, iProfiles, iSupports);
            truss2D.ApplyNodalLoads(iLoad, iLoadVecs);
            truss2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss2D.Solve();
            truss2D.Retracking();
            truss2D.GetResultVisuals();
            truss2D.GetLoadVisuals();


            TrussModel2D trussModel2D = new TrussModel2D(iGeometryLines, areas, iSupports, loadList, loadVecs, e);
            trussModel2D.Assemble();
            trussModel2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            trussModel2D.Solve();
            trussModel2D.Retracking();
            trussModel2D.GetResultVisuals();
            trussModel2D.GetLoadVisuals();

            /*

            // Check possible reuse elements for each element in geometry
            List<List<OLDReusableElement>> reusablesSuggestionTree = new List<List<OLDReusableElement>>();
            

            int elementCounter = 0;
            foreach( OLDInPlaceElement elementInStructure in trussModel2D.ElementsInStructure )
            {                
                List<OLDReusableElement> reusablesSuggestionList = new List<OLDReusableElement>();
                foreach(OLDReusableElement reusableElement in iReusableElements)
                {   
                    double lengthOfElement = elementInStructure.StartPoint.DistanceTo(elementInStructure.EndPoint);
                    if ( reusableElement.CheckUtilization(trussModel2D.N_out[elementCounter]) < 1 && reusableElement.ReusableLength > lengthOfElement )
                        reusablesSuggestionList.Add(reusableElement);
                }
                reusablesSuggestionTree.Add(reusablesSuggestionList);
                elementCounter++;
            }

            Grasshopper.DataTree<OLDReusableElement> dataTree = new Grasshopper.DataTree<OLDReusableElement>();

            int outerCount = 0;
            foreach ( List<OLDReusableElement> list in reusablesSuggestionTree )
            {
                int innerCount = 0;
                foreach (OLDReusableElement reusableElement in list)
                {
                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(new int[] {outerCount});
                    dataTree.Insert(reusableElement, path, innerCount);
                    innerCount++;
                }
                outerCount++;
            }
                
                    
            
            



            // OUTPUTS
            DA.SetData("Info", trussModel2D.PrintInfo());
            DA.SetDataList("N", trussModel2D.N_out);
            DA.SetDataList("Util", trussModel2D.BrepColors);
            DA.SetDataList("Geometry", trussModel2D.BrepVisuals);

            DA.SetDataTree(4, dataTree);


            */

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("DAA23F40-0F0C-4E67-ADD5-2AE99E9AFC20"); }
        }
    }
}