using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsochronDrafter
{
    public class DraftState
    {
        public string alias;
        public List<string> cardPool;
        public List<List<string>> boosters;

        public DraftState(string alias)
        {
            this.alias = alias;
            cardPool = new List<string>();
            boosters = new List<List<string>>();
        }

        public void AddBooster(List<string> booster)
        {
            boosters.Add(booster);
        }
    }
}
