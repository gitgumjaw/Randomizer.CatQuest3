
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class HintSystemSource
    {
        public string Key { get; }

        public string Label { get; set; }

        public List<string> Triggers { get; }

        public HintSystemSource(string key)
        {
            Key = key;
            Label = null;

            Triggers = new List<string>();
        }

        public void AddTrigger(string trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger))
                return;

            if (!Triggers.Contains(trigger))
                Triggers.Add(trigger);
        }
    }
}
