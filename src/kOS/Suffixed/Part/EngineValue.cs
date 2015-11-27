using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Part;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;
using kOS.Safe.Exceptions;
using kOS.Suffixed.PartModuleField;


namespace kOS.Suffixed.Part
{
    public class EngineValue : PartValue
    {
        private IModuleEngine engine 
        {get {
                if ((!MultiMode)||(MMengine.runningPrimary)) { return engine1; }
                else { return engine2; }
            }
         }

        private readonly IModuleEngine engine1;
        private readonly IModuleEngine engine2; 
        private readonly MultiModeEngine MMengine; //multimodeengine module (null if not multimode)
        private readonly bool MultiMode;
        private readonly GimbalFields gimbal;
        private readonly bool HasGimbal;


        public EngineValue(global::Part part, IModuleEngine engine, SharedObjects sharedObj)
            : base(part, sharedObj)
        {
            MMengine = null;
            engine1 = engine;
            MultiMode = false;
            ModuleGimbal gimbalModule = findGimbal();
            if (gimbalModule != null) { HasGimbal = true; gimbal = new GimbalFields(gimbalModule, sharedObj); }
            else { HasGimbal = false; };
            EngineInitializeSuffixes(this);

        }

        public EngineValue(global::Part part, MultiModeEngine engine, SharedObjects sharedObj)
    : base(part, sharedObj)
        {
            MMengine = engine;

            foreach (PartModule module in this.Part.Modules)
            {
                var engines = module as ModuleEngines;
                if ((engines != null) && (engines.engineID == MMengine.primaryEngineID))
                {
                    engine1 = new ModuleEngineAdapter(engines);
                }
                if ((engines != null) && (engines.engineID == MMengine.secondaryEngineID))
                {
                    engine2 = new ModuleEngineAdapter(engines);
                }
                var enginesFX = module as ModuleEnginesFX;
                if ((enginesFX != null) && (enginesFX.engineID == MMengine.primaryEngineID))
                {
                    engine1 = new ModuleEngineAdapter(enginesFX);
                }
                if ((enginesFX != null) && (enginesFX.engineID == MMengine.secondaryEngineID))
                {
                    engine2 = new ModuleEngineAdapter(enginesFX);
                }

            } 
            // throw exception if not found
            if (engine1 == null) { throw new KOSException("Engine module error " + MMengine.primaryEngineID); }
            if (engine2 == null) { throw new KOSException("Engine module error " + MMengine.secondaryEngineID); }
            MultiMode = true;

            ModuleGimbal gimbalModule = findGimbal();
            if (gimbalModule != null) { HasGimbal = true; gimbal = new GimbalFields(gimbalModule, sharedObj); }
            else { HasGimbal = false; };

            EngineInitializeSuffixes(this);
        }


        private ModuleGimbal findGimbal()
        {
            foreach (PartModule module in Part.Modules)
            {
                var gimbalModule = module as ModuleGimbal;
                if (gimbalModule != null)
                {
                    return gimbalModule;
                }
            }
            return null;
        }

        public void EngineInitializeSuffixes(Structure st)
        {
            st.AddSuffix("ACTIVATE", new NoArgsSuffix(() => engine.Activate()));
            st.AddSuffix("SHUTDOWN", new NoArgsSuffix(() => engine.Shutdown()));
            st.AddSuffix("THRUSTLIMIT", new ClampSetSuffix<float>(() => engine.ThrustPercentage, value => engine.ThrustPercentage = value, 0, 100, 0.5f));
            st.AddSuffix("MAXTHRUST", new Suffix<float>(() => engine.MaxThrust));
            st.AddSuffix("THRUST", new Suffix<float>(() => engine.FinalThrust));
            st.AddSuffix("FUELFLOW", new Suffix<float>(() => engine.FuelFlow));
            st.AddSuffix("ISP", new Suffix<float>(() => engine.SpecificImpulse));
            st.AddSuffix(new[] { "VISP", "VACUUMISP" }, new Suffix<float>(() => engine.VacuumSpecificImpluse));
            st.AddSuffix(new[] { "SLISP", "SEALEVELISP" }, new Suffix<float>(() => engine.SeaLevelSpecificImpulse));
            st.AddSuffix("FLAMEOUT", new Suffix<bool>(() => engine.Flameout));
            st.AddSuffix("IGNITION", new Suffix<bool>(() => engine.Ignition));
            st.AddSuffix("ALLOWRESTART", new Suffix<bool>(() => engine.AllowRestart));
            st.AddSuffix("ALLOWSHUTDOWN", new Suffix<bool>(() => engine.AllowShutdown));
            st.AddSuffix("THROTTLELOCK", new Suffix<bool>(() => engine.ThrottleLock));
            st.AddSuffix("ISPAT", new OneArgsSuffix<float, double>(GetIspAtAtm));
            st.AddSuffix("MAXTHRUSTAT", new OneArgsSuffix<float, double>(GetMaxThrustAtAtm));
            st.AddSuffix("AVAILABLETHRUST", new Suffix<float>(() => engine.AvailableThrust));
            st.AddSuffix("AVAILABLETHRUSTAT", new OneArgsSuffix<float, double>(GetAvailableThrustAtAtm));
            //MultiMode features
            st.AddSuffix("MULTIMODE", new Suffix<bool>(() => MultiMode));
            st.AddSuffix("MODES", new Suffix<ListValue>(GetAllModes, "A List of all modes of this engine"));
            if (MultiMode)
            {
                st.AddSuffix("MODE", new Suffix<string>(() => MMengine.mode));
                st.AddSuffix("TOGGLEMODE", new NoArgsSuffix(() => ToggleMode() ));
                st.AddSuffix("PRIMARYMODE", new SetSuffix<bool>(() => MMengine.runningPrimary, value =>     ToggleSetMode(value)));
                st.AddSuffix("AUTOSWITCH", new SetSuffix<bool>(() => MMengine.autoSwitch, value => SetAutoswitch(value)));  
            }
            //gimbal interface
            st.AddSuffix("HASGIMBAL", new Suffix<bool>(() => HasGimbal));
            if (HasGimbal)
            {
                st.AddSuffix("GIMBAL", new Suffix<GimbalFields>(() => gimbal));
            }
        }

        public static ListValue PartsToList(IEnumerable<global::Part> parts, SharedObjects sharedObj)
        {
            var toReturn = new ListValue();
            foreach (var part in parts)
            {
                bool ismultimode = false;
                foreach (PartModule module in part.Modules)
                {
                    var enginesMM = module as MultiModeEngine;

                    if (enginesMM != null)
                    {
                        toReturn.Add(new EngineValue(part, enginesMM, sharedObj));
                        ismultimode = true;
                    }
                }
                if (!ismultimode) {
                    foreach (PartModule module in part.Modules)
                    {
                        var engines = module as ModuleEngines;
                        if (engines != null)
                        {
                            toReturn.Add(new EngineValue(part, new ModuleEngineAdapter(engines), sharedObj));
                        }
                        else
                        {
                            var enginesFX = module as ModuleEnginesFX;
                            if (engines != null)
                            {
                                toReturn.Add(new EngineValue(part, new ModuleEngineAdapter(enginesFX), sharedObj));
                            }
                        }
                    }
                }
            }
            return toReturn;
        }

        public float GetIspAtAtm(double atmPressure)
        {

            return engine.IspAtAtm(atmPressure);
        }

        public float GetMaxThrustAtAtm(double atmPressure)
        {
            return engine.MaxThrustAtAtm(atmPressure);
        }

        public float GetAvailableThrustAtAtm(double atmPressure)
        {
            return engine.AvailableThrustAtAtm(atmPressure);
        }

        public ListValue GetAllModes()
        {
            var toReturn = new ListValue();
            if (MultiMode) { toReturn.Add(MMengine.primaryEngineID); toReturn.Add(MMengine.secondaryEngineID); }
            else { toReturn.Add("Single mode"); }

            return toReturn;
        }

        public void ToggleMode()
        {
            MMengine.Invoke("ModeEvent", 0);
        }

        public void ToggleSetMode(bool prim)
        {
            if (prim != MMengine.runningPrimary) { ToggleMode(); }
        }

        public void SetAutoswitch(bool auto)
        {
            if (auto) { MMengine.Invoke("EnableAutoSwitch", 0); }
            else { MMengine.Invoke("DisableAutoSwitch", 0); }
        }

    }
}
