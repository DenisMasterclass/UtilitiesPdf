using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Utils.Repositories.Enums
{
    public static class TipoPtExtensions
    {
        public static string GetLabel(this TipoPt tipoPt)
        {
            var field = typeof(TipoPt).GetField(tipoPt.ToString());
            var display = field?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? tipoPt.ToString();
        }
    }
}
