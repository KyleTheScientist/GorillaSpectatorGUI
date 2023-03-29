using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using GorillaNetworking;
using Player = GorillaLocomotion.Player;
using OVR.OpenVR;

namespace SpectatorGUI
{
    class Tracker : MonoBehaviour
    {
        public float updateFrequency = 0.1f;
        public float window = 60.0f;
        private List<Entry> entries = new List<Entry>();
        private float lastUpdateTime = 0.0f;
        public Func<Player, float> collector;
        public float Count
        {
            get { return entries.Count; }
        }

        void FixedUpdate()
        {
            if (!(collector is null) && Time.time - lastUpdateTime >= updateFrequency)
            {
                entries.Add(new Entry()
                {
                    timestamp = Time.time,
                    value = collector.Invoke(this.GetComponent<GorillaLocomotion.Player>())
                });
                lastUpdateTime = Time.time;
            }

            // Remove any old values
            entries.RemoveAll(x => Time.time - entries[0].timestamp > window);
        }

        public float Average()
        {
            if (entries.Count == 0) { return 0; }

            float sum = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                sum += entries[i].value;
            }
            return sum / entries.Count;
        }

        public void AddEntry(float value)
        {
            entries.Add(new Entry()
            {
                timestamp = Time.time,
                value = value
            });
        }

        public struct Entry
        {
            public float value;
            public float timestamp;
        }
    }
}


