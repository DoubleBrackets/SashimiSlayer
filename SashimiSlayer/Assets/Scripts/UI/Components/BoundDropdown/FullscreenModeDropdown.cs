using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Components.BoundDropdown
{
    public class FullscreenModeDropdown : BoundDropdown<FullScreenMode>
    {
        protected override IList<string> GetDropdownOptions()
        {
            IEnumerable<FullScreenMode> allModes = Enum.GetValues(typeof(FullScreenMode)).Cast<FullScreenMode>();
            return allModes.Select(x => x.ToString()).ToList();
        }

        protected override FullScreenMode IndexToType(int index)
        {
            return (FullScreenMode)index;
        }

        protected override int TypeToIndex(FullScreenMode value)
        {
            return (int)value;
        }
    }
}