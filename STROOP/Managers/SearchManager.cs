﻿using STROOP.Controls;
using STROOP.Forms;
using STROOP.Models;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace STROOP.Managers
{
    public class SearchManager : DataManager
    {
        private enum ValueRelationship
        {
            EqualTo,
            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,

            Changed,
            DidNotChange,
            Increased,
            Decreased,
            IncreasedBy,
            DecreasedBy,

            BetweenExclusive,
            BetweenInclusive,

            EverythingPasses,
        };

        private readonly Dictionary<uint, object> _dictionary;
        private Type _memoryType;

        private readonly ComboBox _comboBoxSearchMemoryType;
        private readonly ComboBox _comboBoxValueRelationship;
        private readonly BetterTextbox _textBoxSearchValue;
        private readonly Button _buttonSearchFirstScan;
        private readonly Button _buttonSearchNextScan;
        private readonly Label _labelSearchNumResults;
        private readonly Button _buttonSearchAddSelectedAsVars;
        private readonly Button _buttonSearchAddAllAsVars;
        private readonly DataGridView _dataGridViewSearch;

        public SearchManager(TabPage tabControl, WatchVariableFlowLayoutPanel watchVariablePanel)
            : base(null, watchVariablePanel)
        {
            _dictionary = new Dictionary<uint, object>();
            _memoryType = typeof(byte);

            SplitContainer splitContainerSearch = tabControl.Controls["splitContainerSearch"] as SplitContainer;
            SplitContainer splitContainerSearchOptions = splitContainerSearch.Panel1.Controls["splitContainerSearchOptions"] as SplitContainer;

            _comboBoxSearchMemoryType = splitContainerSearchOptions.Panel1.Controls["comboBoxSearchMemoryType"] as ComboBox;
            _comboBoxSearchMemoryType.DataSource = TypeUtilities.InGameTypeList;

            _comboBoxValueRelationship = splitContainerSearchOptions.Panel1.Controls["comboBoxValueRelationship"] as ComboBox;
            _comboBoxValueRelationship.DataSource = Enum.GetValues(typeof(ValueRelationship));

            _textBoxSearchValue = splitContainerSearchOptions.Panel1.Controls["textBoxSearchValue"] as BetterTextbox;

            _buttonSearchFirstScan = splitContainerSearchOptions.Panel1.Controls["buttonSearchFirstScan"] as Button;
            _buttonSearchFirstScan.Click += (sender, e) => DoFirstScan();

            _buttonSearchNextScan = splitContainerSearchOptions.Panel1.Controls["buttonSearchNextScan"] as Button;
            _buttonSearchNextScan.Click += (sender, e) => DoNextScan();

            _labelSearchNumResults = splitContainerSearchOptions.Panel1.Controls["labelSearchNumResults"] as Label;

            _buttonSearchAddSelectedAsVars = splitContainerSearchOptions.Panel1.Controls["buttonSearchAddSelectedAsVars"] as Button;
            _buttonSearchAddSelectedAsVars.Click += (sender, e) => AddTableRowsAsVars(ControlUtilities.GetTableSelectedRows(_dataGridViewSearch));

            _buttonSearchAddAllAsVars = splitContainerSearchOptions.Panel1.Controls["buttonSearchAddAllAsVars"] as Button;
            _buttonSearchAddAllAsVars.Click += (sender, e) => AddTableRowsAsVars(ControlUtilities.GetTableAllRows(_dataGridViewSearch));

            _dataGridViewSearch = splitContainerSearchOptions.Panel2.Controls["dataGridViewSearch"] as DataGridView;
        }

        private void AddTableRowsAsVars(List<DataGridViewRow> rows)
        {
            List<WatchVariableControl> controls = new List<WatchVariableControl>();
            foreach (DataGridViewRow row in rows)
            {
                uint? addressNullable = ParsingUtilities.ParseHexNullable(row.Cells[0].Value);
                if (!addressNullable.HasValue) continue;
                uint address = addressNullable.Value;

                string typeString = TypeUtilities.TypeToString[_memoryType];
                WatchVariable watchVar = new WatchVariable(
                    memoryTypeName: typeString,
                    specialType: null,
                    baseAddressType: BaseAddressTypeEnum.Relative,
                    offsetUS: address,
                    offsetJP: address,
                    offsetSH: address,
                    offsetDefault: null,
                    mask: null,
                    shift: null);
                WatchVariableControlPrecursor precursor = new WatchVariableControlPrecursor(
                    name: typeString + " " + HexUtilities.FormatValue(address),
                    watchVar: watchVar,
                    subclass: WatchVariableSubclass.Number,
                    backgroundColor: null,
                    displayType: null,
                    roundingLimit: null,
                    useHex: null,
                    invertBool: null,
                    isYaw: null,
                    coordinate: null,
                    groupList: new List<VariableGroup>() { VariableGroup.Custom });
                WatchVariableControl control = precursor.CreateWatchVariableControl();
                controls.Add(control);
            }
            AddVariables(controls);
        }

        private void DoFirstScan()
        {
            string memoryTypeString = (string)_comboBoxSearchMemoryType.SelectedItem;
            _memoryType = TypeUtilities.StringToType[memoryTypeString];
            int memoryTypeSize = TypeUtilities.TypeSize[_memoryType];

            object searchValue = ParsingUtilities.ParseValueNullable(_textBoxSearchValue.Text, _memoryType);

            _dictionary.Clear();
            for (uint address = 0x80000000; address < 0x80000000 + Config.RamSize - memoryTypeSize; address += (uint)memoryTypeSize)
            {
                object memoryValue = Config.Stream.GetValue(_memoryType, address);
                if (Equals(memoryValue, searchValue))
                {
                    _dictionary[address] = memoryValue;
                }
            }

            UpdateControlsBasedOnDictionary();
        }

        private void DoNextScan()
        {
            (object searchValue1, object searchValue2) = ParseSearchValue(_textBoxSearchValue.Text, _memoryType);

            List<KeyValuePair<uint, object>> pairs = _dictionary.ToList();
            _dictionary.Clear();
            foreach (KeyValuePair<uint, object> pair in pairs)
            {
                uint address = pair.Key;
                object oldMemoryValue = pair.Value;
                object memoryValue = Config.Stream.GetValue(_memoryType, address);
                if (ValueQualifies(memoryValue, oldMemoryValue, searchValue1, searchValue2, _memoryType))
                {
                    _dictionary[address] = memoryValue;
                }
            }

            UpdateControlsBasedOnDictionary();
        }

        private (object searchValue1, object searchValue2) ParseSearchValue(string text, Type type)
        {
            List<string> stringValues = ParsingUtilities.ParseStringList(text);
            string stringValue1 = stringValues.Count >= 1 ? stringValues[0] : null;
            string stringValue2 = stringValues.Count >= 2 ? stringValues[1] : null;
            return (ParsingUtilities.ParseValueNullable(stringValue1, type),
                ParsingUtilities.ParseValueNullable(stringValue2, type));
        }

        private void UpdateControlsBasedOnDictionary()
        {
            _labelSearchNumResults.Text = _dictionary.Count.ToString();

            _dataGridViewSearch.Rows.Clear();
            _dictionary.Keys.ToList().ForEach(key =>
            {
                _dataGridViewSearch.Rows.Add(HexUtilities.FormatValue(key), _dictionary[key]);
            });
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            base.Update(updateView);
        }

        private bool ValueQualifies(object memoryObject, object oldMemoryObject, object searchObject1, object searchObject2, Type type)
        {
            if (type == typeof(byte))
            {
                byte? memoryValue = ParsingUtilities.ParseByteNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                byte? oldMemoryValue = ParsingUtilities.ParseByteNullable(oldMemoryObject);
                byte? searchValue1 = ParsingUtilities.ParseByteNullable(searchObject1);
                byte? searchValue2 = ParsingUtilities.ParseByteNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return Equals(memoryObject, searchObject1);
        }
    }
}
