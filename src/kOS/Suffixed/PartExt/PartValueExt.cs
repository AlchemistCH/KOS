using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;
using kOS.Suffixed.PartExt;

namespace kOS.Suffixed.Part
{

    public class PartValueExt : PartValue
    {
        public readonly EngineValue engine;
        public readonly bool isEngine;

        public readonly DockingPortExt dockPort;
        public readonly bool hasDockingPort;

        public readonly ConverterExt Converter;
        public readonly bool hasConverter;


        public PartValueExt(global::Part part, SharedObjects sharedObj)
            : base(part, sharedObj)
        {
            // Engine initialization
            isEngine = false;
            foreach (PartModule module in part.Modules) //Check for mutimode engine (let's not run into usual engine modules first)
            {
                if (!isEngine)
                {
                    MultiModeEngine mmEng = module as MultiModeEngine;
                    if (mmEng != null)
                    { engine = new EngineValue(part, mmEng, sharedObj); isEngine = true; }
                }
            }
            foreach (PartModule module in part.Modules)
            {
                if (!isEngine)
                {
                    ModuleEngines mEng = module as ModuleEngines;
                    if (mEng != null)
                    { engine = new EngineValue(part, new ModuleEngineAdapter(mEng), sharedObj); isEngine = true; }
                    else{
                        ModuleEnginesFX mEngFX = module as ModuleEnginesFX;
                        if (mEngFX != null)
                        { engine = new EngineValue(part, new ModuleEngineAdapter(mEngFX), sharedObj); isEngine = true; }
                    }
                }
            }
            //engine check
            AddSuffix("ISENGINE", new Suffix<bool>(() => isEngine));

            if (isEngine)
            {
                //secondary access point
                AddSuffix("ENGINE", new Suffix<EngineValue>(() => engine));
                // dump suffixes 
                engine.EngineInitializeSuffixes(this);
            }

            hasDockingPort = false;

            foreach (PartModule module in part.Modules) 
            {
                if (!hasDockingPort)
                {
                    ModuleDockingNode dNode = module as ModuleDockingNode;
                    if (dNode != null)
                    { hasDockingPort = true; }
                }
            }
            AddSuffix("HASDOCKING", new Suffix<bool>(() => hasDockingPort));
            if (hasDockingPort)
            {
                dockPort = new DockingPortExt(part, Shared);
                dockPort.ExtenderInitializeSuffixes(this);
            }

            foreach (PartModule module in part.Modules) 
            {
                if (!hasConverter)
                {
                    ModuleResourceConverter dNode = module as ModuleResourceConverter;
                    if (dNode != null)
                    { hasConverter = true; }
                }
            }
            AddSuffix("HASCONVERTER", new Suffix<bool>(() => hasConverter));
            if (hasConverter)
            {
                Converter = new ConverterExt(part, Shared);
                Converter.ExtenderInitializeSuffixes(this);
            }

        }

      //  private void ExtInitializeSuffixes()
      //  {
      //  }


 //For docking port...
       public override ITargetable Target
        {
            get
            {
                if (hasDockingPort) { return dockPort.module.module; }
                else { return base.Target; }
            }
        }

        public static ListValue PartsToList(IEnumerable<global::Part> parts, SharedObjects sharedObj)
        {
            var toReturn = new ListValue();
            foreach (var part in parts)
            {
                toReturn.Add(new PartValueExt(part, sharedObj));
            }
            return toReturn;
        }




    }
}
