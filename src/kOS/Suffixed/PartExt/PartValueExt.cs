using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;

namespace kOS.Suffixed.Part
{

    public class PartValueExt : PartValue
    {
        public readonly EngineValue engine;
        public readonly bool isEngine;



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
                // dump suffixes :)
                engine.EngineInitializeSuffixes(this);

            }
        }

      //  private void ExtInitializeSuffixes()
      //  {
      //  }


 //Add docking port check...
 //       public override ITargetable Target
 //       {
 //           get { return module; }
 //       }

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
