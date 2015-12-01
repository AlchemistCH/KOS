using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;
// using kOS.Suffixed.Part;

namespace kOS.Suffixed.PartExt
{
    public class  PartExtender : Structure
    {
        protected SharedObjects Shared { get; private set; }


        public PartExtender(SharedObjects sharedObj)
        {
            Shared = sharedObj;
        }

        public virtual void ExtenderInitializeSuffixes(Structure addTo) //suffix initialization (specify the structure to add to)
        {

        }

        public virtual Lexicon<string, double> ResInput //nominal resource consumption
        {
            get
            {
                return new Lexicon<string, double>();
            }
        }
        public virtual Lexicon<string, double> ResOutput //nominal resource production
        {
            get
            {
                return new Lexicon<string, double>();
            }
        }
        public virtual Lexicon<string, double> ResConsume //current resource consumption
        {
            get
            {
                return new Lexicon<string, double>();
            }
        }
        public virtual Lexicon<string, double> ResProduce //current resource production
        {
            get
            {
                return new Lexicon<string, double>();
            }
        }


    }
}
