﻿using STROOP.Controls;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STROOP.Managers
{
    public class TasManager : DataManager
    {
        public TasManager(List<WatchVariableControlPrecursor> variables, TabPage tabControl, WatchVariableFlowLayoutPanel watchVariablePanel)
            : base(variables, watchVariablePanel)
        {
            SplitContainer splitContainerTas = tabControl.Controls["splitContainerTas"] as SplitContainer;

        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            base.Update(updateView);
        }
    }
}
