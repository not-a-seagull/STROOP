﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM64_Diagnostic.Structs;
using System.Windows.Forms;
using SM64_Diagnostic.Utilities;
using SM64_Diagnostic.Controls;

namespace SM64_Diagnostic.Managers
{
    public class HudManager : DataManager
    {
        Control _tabControl;

        public HudManager(List<WatchVariable> hudData, Control tabControl, NoTearFlowLayoutPanel noTearFlowLayoutPanelHud)
            : base(hudData, noTearFlowLayoutPanelHud)
        {
            _tabControl = tabControl;

            SplitContainer splitContainerHud = tabControl.Controls["splitContainerHud"] as SplitContainer;

            (splitContainerHud.Panel1.Controls["buttonFillHp"] as Button).Click += (sender, e) => ButtonUtilities.RefillHp();
            (splitContainerHud.Panel1.Controls["buttonDie"] as Button).Click += (sender, e) => ButtonUtilities.Die();
            (splitContainerHud.Panel1.Controls["buttonStandardHud"] as Button).Click += (sender, e) => ButtonUtilities.StandardHud();
            (splitContainerHud.Panel1.Controls["button99Coins"] as Button).Click += (sender, e) => ButtonUtilities.Coins99();
        }
    }
}
