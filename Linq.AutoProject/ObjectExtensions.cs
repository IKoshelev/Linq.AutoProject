using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linq.AutoProject
{
    internal static class ObjectExtensions
    {
        public static TTarget As<TTarget>(this object source) 
            where TTarget:class
        {
            TTarget target = source as TTarget;
            return target;
        }
    }
}
