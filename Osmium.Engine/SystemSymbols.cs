using System;
using System.Runtime.CompilerServices;
using System.Text;
using Osmium.Engine.Implementations;
using Osmium.Engine.Patterns;
using static Osmium.Engine.SymbolAttributes;

namespace Osmium.Engine
{
    partial class SystemSymbols
    {
        private void Init()
        {
            And.DownCode = ElementaryImplementations.And;
            And.Attributes = Flat | HoldAll | OneIdentity;
            Append.DownCode = ElementaryImplementations.Append;
            Apply.DownCode = ElementaryImplementations.Apply;
            Set.DownCode = SetImplementations.Set;
            Set.Attributes = HoldFirst | Protected | SequenceHold;
            SetDelayed.DownCode = SetImplementations.SetDelayed;
            SetDelayed.Attributes = HoldAll | Protected | SequenceHold;
        }
    }
}
