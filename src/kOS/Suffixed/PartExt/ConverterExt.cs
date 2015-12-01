using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using kOS.Suffixed.PartModuleField;
using System.Collections.Generic;
using System;

namespace kOS.Suffixed.PartExt
{


    class ConverterExt : PartExtender
    {
        public readonly ListValue<ConverterFields> Converters;

        public ConverterExt(global::Part part, SharedObjects sharedObj)
            : base(sharedObj)
        {
            Converters = new ListValue<ConverterFields>();

            foreach (PartModule module in part.Modules)
            {
                var ConverterModule = module as ModuleResourceConverter;
                if (ConverterModule != null)
                {
                    Converters.Add(new ConverterFields(ConverterModule, sharedObj));
                }
            }
           
        }

        public ConverterExt(IEnumerable<global::Part> parts, SharedObjects sharedObj) //can be used for the entire ship!
            : base(sharedObj)
        {
            Converters = new ListValue<ConverterFields>();
            foreach (global::Part part in parts)
            {
                foreach (PartModule module in part.Modules)
                {
                    var ConverterModule = module as ModuleResourceConverter;
                    if (ConverterModule != null)
                    {
                        Converters.Add(new ConverterFields(ConverterModule, sharedObj));
                    }
                }
            }
            ExtenderInitializeSuffixes(this);
        }



        public override void ExtenderInitializeSuffixes(Structure addTo)
        {


            addTo.AddSuffix(new[] { "CONVERTERMODULES", "CONVMODS" }, new Suffix<ListValue<ConverterFields>>(() => Converters));
            addTo.AddSuffix(new[] { "HASCONVERTER", "HASCONV" }, new OneArgsSuffix<bool, string>(HasConverterModule));
            addTo.AddSuffix(new[] { "GETCONVERTER", "GETCONV" }, new OneArgsSuffix<ConverterFields, string>(GetConverterModule));

            addTo.AddSuffix(new[] { "CONVERTERCOUNT", "CONVCOUNT" }, new Suffix<int>(() => Converters.Count));
            // all suffixes

            addTo.AddSuffix(new[] { "CONVERTERNAMES", "CONVNAMES" }, new Suffix<ListValue>(() =>
            {
                var toReturn = new ListValue();
                foreach (ConverterFields conv in Converters) { toReturn.Add(conv.Converter.ConverterName); }
                return toReturn;
            }));

            addTo.AddSuffix(new[] { "CONVERTERNAME", "CONVNAME" }, new Suffix<string>(() => 
            {
                string toReturn = "";
                foreach (ConverterFields conv in Converters) { toReturn=StringAdd(toReturn, conv.Converter.ConverterName, ", "); }
                return toReturn;

            }));

            addTo.AddSuffix("START", new NoArgsSuffix(() =>
            {
                foreach (ConverterFields conv in Converters)
                {
                    conv.Converter.StartResourceConverter();
                }
            }));
            addTo.AddSuffix("STOP", new NoArgsSuffix(() =>
            {
                foreach (ConverterFields conv in Converters)
                {
                    conv.Converter.StopResourceConverter();
                }
            }));
            addTo.AddSuffix("STATUSLIST", new Suffix<ListValue>(() =>
            {
                var toReturn = new ListValue();
                foreach (ConverterFields conv in Converters) { toReturn.Add(conv.Converter.status); }
                return toReturn;
            }));
            addTo.AddSuffix("STATUSFULL", new Suffix<ListValue>(() =>
            {
                var toReturn = new ListValue();
                foreach (ConverterFields conv in Converters) { toReturn.Add(conv.Converter.ConverterName+": "+conv.Converter.status); }
                return toReturn;
            }));

            addTo.AddSuffix("STATUS", new Suffix<string>(() => 
            {
                if (Converters.Count == 1) { return Converters[0].Converter.status; }
                else
                {
                    string toReturn = "";
                    foreach (ConverterFields conv in Converters) { toReturn= LineAdd(toReturn, (conv.Converter.ConverterName + ": " + conv.Converter.status)); }
                    return toReturn;
                }
            }));

            addTo.AddSuffix("ISACTIVATED", new SetSuffix<bool>(() => 
            {
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.IsActivated) { return true; }
                }
                return false;
            }, value => {
                foreach (ConverterFields conv in Converters)
                {
                    conv.toggle(value);
                }
            }));
            addTo.AddSuffix("ISRUNNING", new Suffix<bool>(() => 
            {
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.status.EndsWith("% load")) { return true; }
                }
                return false;
            }));
            addTo.AddSuffix("ACTIVATED", new Suffix<int>(() =>
            {
                int toReturn = 0;
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.IsActivated) { toReturn++; }
                }
                return toReturn;
            })); //count of 
            addTo.AddSuffix("RUNNING", new Suffix<int>(() =>
            {
                int toReturn = 0;
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.status.EndsWith("% load")) { toReturn++; }
                }
                return toReturn;
            }));


            addTo.AddSuffix("ALWAYSACTIVE", new Suffix<bool>(() => {
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.AlwaysActive) { return true; }
                }
                return false;
            }));
            addTo.AddSuffix("GENERATESHEAT", new Suffix<bool>(() => {
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.GeneratesHeat) { return true; }
                }
                return false;
            }));
            addTo.AddSuffix(new[] { "CORETEMPERATURE", "CORETEMP" }, new Suffix<double>(() => {
                if (Converters.Count == 0) { return 0; } 
                return Converters[0].Converter.GetCoreTemperature();
            })); //same for all (from another module)
            AddSuffix(new[] { "GOALTEMPERATURE", "GOALTEMP" }, new Suffix<double>(() => {
                if (Converters.Count == 0) { return 0; }
                return Converters[0].Converter.GetGoalTemperature(); 
            })); //same for all (from another module)

            addTo.AddSuffix("FILLAMOUNT", new Suffix<float>(() => 
            {
                if (Converters.Count == 0) { return 0; }
                float sum = 0; float act = 0; int Nact = 0;
                foreach (ConverterFields conv in Converters)
                {
                    float t = conv.Converter.FillAmount;
                    sum = sum + t;
                    if (conv.Converter.IsActivated) { act = act + t; Nact++; }
                }
                if (Nact > 0) { return act / Nact; } else { return sum / Converters.Count; }
            }));

            addTo.AddSuffix("TAKEAMOUNT", new Suffix<float>(() => 
            {
                if (Converters.Count == 0) { return 0; }
                float sum = 0; float act = 0; int Nact = 0;
                foreach (ConverterFields conv in Converters)
                {
                    float t = conv.Converter.TakeAmount;
                    sum = sum + t;
                    if (conv.Converter.IsActivated) { act = act + t; Nact++; }
                }
                if (Nact > 0) { return act / Nact; } else { return sum / Converters.Count; }
            }));

            addTo.AddSuffix(new[] { "THERMALEFFICIENCY", "THERMEFF" }, new Suffix<float>(() => 
            {
                if (Converters.Count == 0) { return 0; }
                float sum = 0; float act = 0; int Nact = 0;
                foreach (ConverterFields conv in Converters)
                {
                    float te = conv.ThermEff();
                    sum = sum + te;
                    if (conv.Converter.IsActivated) { act = act + te; Nact++; }
                }
                if (Nact > 0) { return act / Nact; } else { return sum / Converters.Count; }
            }));

            addTo.AddSuffix("GETINFO", new Suffix<string>(() => 
            {
                string toReturn = "";
                foreach (ConverterFields conv in Converters) { toReturn = LineAdd(toReturn, conv.writeInfo()); }
                return toReturn;
            }));


            addTo.AddSuffix(new[] { "CONVERTERLOAD", "CONVLOAD" }, new Suffix<float>(() =>
            {
                float sum = 0; int Nact = 0;
                foreach (ConverterFields conv in Converters)
                {
                    if (conv.Converter.IsActivated) { sum = sum + conv.conversionLoad(); Nact++; }
                }
                if (Nact == 0) { return 0; } else { return sum / Nact; }
            })); //actual converter load (average for active converters)


            addTo.AddSuffix("INPUT", new Suffix<Lexicon<string, double>>(() => ResInput)); //maximal input rate
            addTo.AddSuffix("OUTPUT", new Suffix<Lexicon<string, double>>(() => ResOutput)); //maximal output rate

            addTo.AddSuffix("CONSUME", new Suffix<Lexicon<string, double>>(() => ResConsume)); //actual consumption rate
            addTo.AddSuffix("PRODUCE", new Suffix<Lexicon<string, double>>(() => ResProduce)); //actual production rate

        }

        public bool HasConverterModule(string convName)
        {
            foreach (ConverterFields module in Converters)
            {
                if (string.Equals(module.Converter.ConverterName, convName, StringComparison.OrdinalIgnoreCase))  { return true; }
            }
            return false;
        }

        public ConverterFields GetConverterModule(string convName)
        {
            foreach (ConverterFields module in Converters)
            {
                if (string.Equals(module.Converter.ConverterName, convName, StringComparison.OrdinalIgnoreCase)) { return module; } 
            }
            throw new KOSException("Resource Converter Module not found: " + convName); //if not found
        }

        private void MergeResLex(Lexicon<string, double> Lex, Lexicon<string, double> toAdd)
        {
            foreach (string res in toAdd.Keys)
            {
                if (Lex.ContainsKey(res)) { Lex[res] = Lex[res] + toAdd[res]; }
                else { Lex.Add(res, toAdd[res]); }
            }
        }

        private string StringAdd(string s, string toAdd, string Sep)
        {
            if (s == "") { return toAdd; }
            else { return  (s + Sep + toAdd); }
        }
        private string LineAdd(string s, string toAdd)
        {
            return StringAdd(s, toAdd, "\n");
        }


        public override Lexicon<string, double> ResInput //nominal resource consumption
        {
            get
            {
                var toReturn = new Lexicon<string, double>();
                foreach (ConverterFields conv in Converters)
                {
                    MergeResLex(toReturn, conv.InputLex());
                }
                return toReturn;
            }
        }
        public override Lexicon<string, double> ResOutput //nominal resource production
        {
            get
            {
                var toReturn = new Lexicon<string, double>();
                foreach (ConverterFields conv in Converters)
                {
                    MergeResLex(toReturn, conv.OutputLex());
                }
                return toReturn;
            }
        }
        public override Lexicon<string, double> ResConsume //current resource consumption
        {
            get
            {
                var toReturn = new Lexicon<string, double>();
                foreach (ConverterFields conv in Converters)
                {
                    MergeResLex(toReturn, conv.ConsumeLex());
                }
                return toReturn;
            }
        }
        public override Lexicon<string, double> ResProduce //current resource production
        {
            get
            {
                var toReturn = new Lexicon<string, double>();
                foreach (ConverterFields conv in Converters)
                {
                    MergeResLex(toReturn, conv.ProduceLex());
                }
                return toReturn;
            }
        }
    }


}
