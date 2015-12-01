using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;
using kOS.Suffixed.Part;
using kOS.Suffixed.PartModuleField;
using kOS.Safe.Exceptions;


namespace kOS.Suffixed.PartExt
{
    public class DockingPortExt : PartExtender
    {
        public readonly ListValue<DockingPortFields> modules;
        private int moduleID;
        public DockingPortFields module
        {
            get
            {
                if (modules.Count != 0) { return modules[moduleID]; }
                else throw new KOSException("Docking module not fond");
            }
        }

        public DockingPortExt(global::Part part, SharedObjects sharedObj)
            : base(sharedObj)
        {
            modules = new ListValue<DockingPortFields>();
            foreach (PartModule pmodule in part.Modules)
            {
                var DockingModule = pmodule as ModuleDockingNode;
                if (DockingModule != null)
                {
                    modules.Add(new DockingPortFields(DockingModule, sharedObj));
                }

            }
            if (modules.Count > 0) { moduleID = 0; } else { moduleID = -1; }
        }

        public DockingPortExt(IEnumerable<global::Part> parts, SharedObjects sharedObj) //can be used for the entire ship!
            : base(sharedObj)
        {
            modules = new ListValue<DockingPortFields>();
            foreach (global::Part part in parts)
            {
                foreach (PartModule pmodule in part.Modules)
                {
                    var DockingModule = pmodule as ModuleDockingNode;
                    if (DockingModule != null)
                    {
                        modules.Add(new DockingPortFields(DockingModule, sharedObj));
                    }

                }
            }
            if (modules.Count > 0) { moduleID = 0; } else { moduleID = -1; }
            ExtenderInitializeSuffixes(this);
        }


        public override void ExtenderInitializeSuffixes(Structure addTo)
        {
            //direct access
            addTo.AddSuffix(new[] { "PORTS", "DOCKINGPORTS" }, new Suffix<ListValue<DockingPortFields>>(() => modules));
            addTo.AddSuffix(new[] { "PORTCOUNT", "DOCKINGPORTCOUNT" }, new Suffix<int>(() => modules.Count));

            //multiple ports case - switching active
            addTo.AddSuffix(new[] { "PORTID", "DOCKINGPORTID" }, new SetSuffix<int>(() => moduleID, value => choosePort(value))); 
            addTo.AddSuffix(new[] { "HASPORT", "HASDOCKINGPORT" }, new TwoArgsSuffix<bool, string, bool>((nodeType, f) => hasPort(nodeType, f)));
            addTo.AddSuffix(new[] { "SETPORT", "SETDOCKINGPORT" }, new TwoArgsSuffix<string, bool>((nodeType, f) => setPort(nodeType, f)));

            //for currently selected port
            addTo.AddSuffix("ACQUIRERANGE", new Suffix<float>(() => module.module.acquireRange));
            addTo.AddSuffix("ACQUIREFORCE", new Suffix<float>(() => module.module.acquireForce));
            addTo.AddSuffix("ACQUIRETORQUE", new Suffix<float>(() => module.module.acquireTorque));
            addTo.AddSuffix("REENGAGEDISTANCE", new Suffix<float>(() => module.module.minDistanceToReEngage));
            addTo.AddSuffix("DOCKEDSHIPNAME", new Suffix<string>(() =>module.DockedShipName()));
            addTo.AddSuffix("STATE", new Suffix<string>(() => module.module.state));
            addTo.AddSuffix("TARGETABLE", new Suffix<bool>(() => true));
            addTo.AddSuffix("UNDOCK", new NoArgsSuffix(() => module.module.Undock()));
            addTo.AddSuffix("TARGET", new NoArgsSuffix(() => module.module.SetAsTarget()));
            addTo.AddSuffix("PORTFACING", new NoArgsSuffix<Direction>(module.GetPortFacing,
                                                               "The direction facing outward from the docking port.  This " +
                                                               "can differ from :FACING in the case of sideways-facing " +
                                                               "docking ports like the inline docking port."));
            addTo.AddSuffix("NODEPOSITION", new Suffix<Vector>(module.GetNodePosition, "The position of the docking node itself rather than the part's center of mass"));
            addTo.AddSuffix("NODETYPE", new Suffix<string>(() => module.module.nodeType, "The type of the docking node"));
            addTo.AddSuffix("GENDEREDPORT", new Suffix<bool>(() => module.module.gendered, "Is the docking port gendered? (probe-drogue"));
            addTo.AddSuffix("FEMALEPORT", new Suffix<bool>(() => module.module.genderFemale, "Is the docking port female (drogue)?"));
            addTo.AddSuffix("CROSSFEED", new SetSuffix<bool>(() => module.module.crossfeed, (value) => module.setCrossfeed(value)));
            //animated port cover stuff...
            addTo.AddSuffix("PORTDISABLED", new Suffix<bool>(() => module.module.IsDisabled)); //Disabled by another module? 
            addTo.AddSuffix("PORTCOVER", new Suffix<bool>(() => (module.module.deployAnimator != null), "Does it have deployable cover?"));
            addTo.AddSuffix("OPENPORT", new NoArgsSuffix(() => module.ToggleCover(true)));
            addTo.AddSuffix("CLOSEPORT", new NoArgsSuffix(() => module.ToggleCover(false)));
            addTo.AddSuffix("TOGGLEPORTCOVER", new NoArgsSuffix(() => module.ToggleCover()));
            addTo.AddSuffix("PORTCLOSED", new Suffix<bool>(() => module.IsClosed, "True if cover closed or closing."));
        }
    
    



        public void choosePort(int newID)
        {
            if ((newID >= 0) && (newID < modules.Count)) { moduleID = newID; }
            else throw new KOSException("Docking module count is "+ modules.Count.ToString());
        }

        public bool hasPort(string nodeType, bool f)
        {
            foreach (DockingPortFields port in modules) { if ((port.module.nodeType == nodeType) && ((!port.module.gendered) || (port.module.genderFemale == f))) { return true; } }
            return false;
        }

        public int getPortID(string nodeType, bool f)
        {
            for (int i= 0; i< modules.Count; i++)
            { if ((modules[i].module.nodeType == nodeType) && ((!modules[i].module.gendered) || (modules[i].module.genderFemale == f))) { return i; } }
            return -1;
        }


        public void setPort(string nodeType, bool f)
        {
            int i = getPortID(nodeType, f);
            if (i!=-1) { choosePort(i); }
            else throw new KOSException("Docking module not found: " + nodeType);
        }

        public static ListValue PartsToList(IEnumerable<global::Part> parts, SharedObjects sharedObj)
        {
            var toReturn = new ListValue();
            foreach (var part in parts)
            {
                bool portfound = false;
                foreach (PartModule module in part.Modules)
                {
                    if (!portfound)
                    {
                        var dockingNode = module as ModuleDockingNode;
                        if (dockingNode != null)
                        {
                            portfound = true;
                        }
                    }
                }
                if (portfound) { toReturn.Add(new PartValueExt(part, sharedObj)); }
            }
            return toReturn;
        }

   
        
    }
}
