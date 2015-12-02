using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;

namespace kOS.Suffixed.Part
{
    public class PartValueFactory 
    {
        public static ListValue Construct(IEnumerable<global::Part> parts, SharedObjects shared)
        {
            var partList = parts.Select(part => Construct(part, shared)).ToList();
            return ListValue.CreateList(partList);
        } 

        public static ListValue<PartValue> ConstructGeneric(IEnumerable<global::Part> parts, SharedObjects shared)
        {
            var partList = parts.Select(part => Construct(part, shared)).ToList();
            return ListValue<PartValue>.CreateList(partList);
        } 

        public static PartValue Construct(global::Part part, SharedObjects shared)
        {
            //heh 
            return new PartValueExt(part, shared);


        }
    }
}
