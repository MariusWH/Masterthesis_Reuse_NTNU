using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Reflection;

namespace MasterthesisGHA
{
    public class MaterialBankComponent : GH_Component
    {
        public MaterialBankComponent()
          : base("Reusable Stock of Elements", "MaterialBank",
              "Description",
              "Master", "MethodOne")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {            
            pManager.AddTextParameter("Section", "SectionName", "SectionName", GH_ParamAccess.list, "IPE200");
            pManager.AddIntegerParameter("Amount", "Amount", "Amount", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Length", "Length", "Length", GH_ParamAccess.list, 1000);
            pManager.AddTextParameter("CommandInput", "Command", "Input as: Amount x SectionName x Length (ex.: 10xIPE200x1000)",
                GH_ParamAccess.list, "0xIPE200x1000");
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "StockVisuals", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "StockColour", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<string> profiles = new List<string>();
            List<int> quantities = new List<int>();
            List<double> lengths = new List<double>();
            List<string> inputCommands = new List<string>();

            DA.GetDataList(0, profiles);
            DA.GetDataList(1, quantities);
            DA.GetDataList(2, lengths);
            DA.GetDataList(3, inputCommands);



            // CODE
            MaterialBank materialBank = new MaterialBank(profiles, quantities, lengths);
            MaterialBank materialBank1 = new MaterialBank(inputCommands);
            


            // OUTPUTS
            List<System.Drawing.Color> colors = new List<System.Drawing.Color>();

            DA.SetData(0, materialBank);
            DA.SetData(1, materialBank.GetMaterialBankInfo());
            DA.SetDataList(2, materialBank.VisualizeMaterialBank(0, out colors));
            DA.SetDataList(3, colors);

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MaterialBank;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("EC30A7CE-C83A-4A4D-AA49-3D5B7AFF7A00"); }
        }
    }
}