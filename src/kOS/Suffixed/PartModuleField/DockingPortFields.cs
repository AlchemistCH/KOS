using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;
using kOS.Suffixed.Part;

namespace kOS.Suffixed.PartModuleField
{
    public class DockingPortFields : PartModuleFields
    {
        public readonly ModuleDockingNode module;

        public DockingPortFields(ModuleDockingNode module, SharedObjects sharedObj)
            : base(module, sharedObj)
        {
            this.module = module;
            DockingInitializeSuffixes();
        }

        private void DockingInitializeSuffixes()
        {
            AddSuffix("ACQUIRERANGE", new Suffix<float>(() => module.acquireRange));
            AddSuffix("ACQUIREFORCE", new Suffix<float>(() => module.acquireForce));
            AddSuffix("ACQUIRETORQUE", new Suffix<float>(() => module.acquireTorque));
            AddSuffix("REENGAGEDISTANCE", new Suffix<float>(() => module.minDistanceToReEngage));
            AddSuffix("DOCKEDSHIPNAME", new Suffix<string>(() => DockedShipName()));
            AddSuffix(new[] { "STATE" , "PORTSTATE"}, new Suffix<string>(() => module.state));
            AddSuffix("TARGETABLE", new Suffix<bool>(() => true));
            AddSuffix("UNDOCK", new NoArgsSuffix(() => module.Undock()));
            AddSuffix("TARGET", new NoArgsSuffix(() => module.SetAsTarget()));
            AddSuffix("PORTFACING", new NoArgsSuffix<Direction>(GetPortFacing,
                                                               "The direction facing outward from the docking port.  This " +
                                                               "can differ from :FACING in the case of sideways-facing " +
                                                               "docking ports like the inline docking port."));
            AddSuffix("NODEPOSITION", new Suffix<Vector>(GetNodePosition, "The position of the docking node itself rather than the part's center of mass"));
            AddSuffix("NODETYPE", new Suffix<string>(() => module.nodeType, "The type of the docking node"));
            AddSuffix("GENDEREDPORT", new Suffix<bool>(() => module.gendered, "Is the docking port gendered? (probe-drogue"));
            AddSuffix("FEMALEPORT", new Suffix<bool>(() => module.genderFemale, "Is the docking port female (drogue)?"));
            AddSuffix("CROSSFEED", new SetSuffix<bool>(() => module.crossfeed, (value) => setCrossfeed(value) ));
            //animated port cover stuff...
            AddSuffix("PORTDISABLED", new Suffix<bool>(() => module.IsDisabled));//Disabled by another module?
            AddSuffix("PORTCOVER", new Suffix<bool>(() => (module.deployAnimator != null), "Does it have deployable cover?"));
            AddSuffix("OPENPORT", new NoArgsSuffix(() => ToggleCover(true)));
            AddSuffix("CLOSEPORT", new NoArgsSuffix(() => ToggleCover(false)));
            AddSuffix("TOGGLEPORTCOVER", new NoArgsSuffix(() => ToggleCover()));
            AddSuffix("PORTCLOSED", new Suffix<bool>(() =>IsClosed, "True if cover closed or closing."));
        }

        //   public override ITargetable Target
        //   {
        //       get { return module; }
        //   }

        public string DockedShipName()
        {
            return module.vesselInfo != null ? module.vesselInfo.name : string.Empty;
        }



        public Direction GetPortFacing()
        {
            // module.nodeTransform describes the transform representing the facing of
            // the docking node as opposed to the facing of the part itself.  In the
            // case of a docking port facing out the side of the part (the in-line
            // docking node for example) they can differ.

            return new Direction(module.nodeTransform.rotation);
        }

        public Vector GetNodePosition()
        {
            // like with GetPortFacing above, the position of the docking node itself difers
            // from the position of the part's center of mass.  This returns the possition
            // of the node where the two docking ports will join together, which will help
            // with docking operations

            return new Vector(module.nodeTransform.position - shared.Vessel.findWorldCenterOfMass());
        }

        public void setCrossfeed(bool allow)
        {
            if (allow) { module.EnableXFeed(); } else { module.DisableXFeed(); }
        }

        public void ToggleCover(bool open)
        {
            if (module.deployAnimator != null)
            { if (open== module.deployAnimator.animSwitch) { module.deployAnimator.Toggle(); } else { module.deployAnimator.Toggle(); } }
        }

        public void ToggleCover()
        {
            if (module.deployAnimator != null){ module.deployAnimator.Toggle(); }
        }
        public bool IsClosed
        {
            get
            {
                if (module.deployAnimator != null) { return module.deployAnimator.animSwitch; } else { return false; }
            }
        }

    }
}
