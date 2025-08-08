using System;
using System.Collections.Generic;

namespace Libs
{
    [Serializable]
    public class SpinAwardCashTask:BaseTask
    {
        public int cashTargetNum = 0;
        public int cashCollectNum = 0;

        public SpinAwardCashTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
           
            
        }

        ~SpinAwardCashTask()
        {
           
        }

        void UpdateCash()
        {
  
        }
        
        void UpdateSpinCount()
        {
            HasCollectNum++;
        }
    }
}