using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Core;

using App;
using App.SubSystems;
using Utils;

namespace Plugins
{
	public class ConfigurationParseResult
	{
		public Dictionary<string,object> RawConfiguration()
		{
			return rawConfiguration;
		}

		public void SetRawConfiguration(Dictionary<string,object> rawDict)
		{
			rawConfiguration = rawDict;
		}

		public ApplicationConfig ApplicationConfig ()
		{
			return appConfig;
		}

		public void  SetApplicationConfig (ApplicationConfig ac)
		{
			appConfig = ac;
		}

		public void SetOnLineEarningConfig(Dictionary<string,object> config)
		{
			OnLineEarningMgr.Instance.ParseConfig(config);
			_infiniteModel = OnLineEarningMgr.Instance.GetPopPlanConfig();
		}
		
		public InfiniteModel PopPlanConfig()
		{
			return _infiniteModel;
		}
		
		public List<SlotMachineConfig> SlotMachineConfigs ()
		{
			return slotMachineConfigs;
		}

		public bool IsValidSlots(string machineName)
		{
			return slotMachineConfigs.Find((config) => { return config.Name().Equals(machineName); }) != null;
		}
		
		public void SetSlotMachineConfigs (List<SlotMachineConfig> slConfigs)
		{
			slotMachineConfigs = slConfigs;
		}
		public void SetAllSlotMachineConfigs (List<SlotMachineConfig> slConfigs)
        {
	        if(allSlotMachineConfig==null) allSlotMachineConfig = new List<SlotMachineConfig>();
	        allSlotMachineConfig = slConfigs;
        }
		public List<SlotMachineConfig> AllSlotMachineConfigs ()
		{
			if (allSlotMachineConfig == null||allSlotMachineConfig.Count==0)
			{
				return new List<SlotMachineConfig>();
			}

			return allSlotMachineConfig;
		}
		public LineTable GetLineTable(string name){
			LineTable lineTable = null;
			if (lineTables != null) {
				foreach (string key in lineTables.Keys) {
					LineTable lt = lineTables[key];
					if (lt.Name ().Equals (name)) {
						lineTable = lt;
						break;
					}
				}
				if (lineTable == null) {
					throw new ArgumentException ("can't find line table:" + name);
				}
			}
			return lineTable;
		}

		public Dictionary<string,LineTable> LineTables ()
		{
			return lineTables;
		}
		//LineTables
		public void SetLineTables (Dictionary<string,LineTable> ltables)
		{
			lineTables = ltables;
		}
		
		public Dictionary<string, object> AutoQuestConfigs {
            get {
                return autoQuestConfigs;
            }

            set {
                autoQuestConfigs = value;
            }
        }


		
		
        private ApplicationConfig appConfig;
        private InfiniteModel _infiniteModel;

		private List<SlotMachineConfig> slotMachineConfigs;

		private List<SlotMachineConfig> allSlotMachineConfig;
		
		private Dictionary<string,LineTable> lineTables;
		private int[] slotRequiredLevelDeltas;

        private Dictionary<string, object> autoQuestConfigs;
		private Dictionary<string,object> rawConfiguration;
    }
}
