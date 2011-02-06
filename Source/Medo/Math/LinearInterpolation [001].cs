﻿//Josip Medved <jmedved@jmedved.com>  http://www.jmedved.com  http://blog.jmedved.com

//2010-04-24: Initial version.


using System;
using System.Collections.Generic;

namespace Medo.Math {

    /// <summary>
    /// Returns adjusted value based on given calibration points.
    /// </summary>
    public class LinearInterpolation {

        private SortedDictionary<double, double> _referencePoints = new SortedDictionary<double, double>();

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public LinearInterpolation() {  }

        /// <summary>
        /// Adds new calibration point.
        /// </summary>
        /// <param name="value">Reference value.</param>
        /// <param name="adjustment">Adjustment at reference point.</param>
        public void Add(double value, double adjustment) {
            _referencePoints.Add(value, adjustment);
        }

        /// <summary>
        /// Returns value adjusted with linear aproximation between two nearest calibration points.
        /// </summary>
        /// <param name="value">Value to adjust.</param>
        public double GetAdjustedValue(double value) {
            KeyValuePair<double, double>? itemBelow = null;
            KeyValuePair<double, double>? itemAbove = null;
            
            foreach (var item in _referencePoints) {
                if (item.Key == value) { //just sent it as output
                    return value + item.Value;
                } else if (item.Key < value) { //store for future reference - it may be more than one.
                    itemBelow = item;
                } else if (item.Key > value) { //first above limit
                    itemAbove = item;
                    break;
                }
            }

            if (itemBelow.HasValue && itemAbove.HasValue) { //both reference points
                var range = itemAbove.Value.Key - itemBelow.Value.Key;
                var point = value - itemBelow.Value.Key;
                var percentageAbove = point / range;
                var percentageBelow = 1 - percentageAbove;
                return value  + itemBelow.Value.Value * percentageBelow + itemAbove.Value.Value * percentageAbove;
            } else if (itemBelow.HasValue) { //just lower reference point
                return value + itemBelow.Value.Value;
            } else if (itemAbove.HasValue) { //just upper reference point
                return value + itemAbove.Value.Value;
            } else { //no reference point
                return value;
            }
        }

        /// <summary>
        /// Gets value adjusted with linear aproximation between two nearest calibration points.
        /// </summary>
        /// <param name="value">Value to adjust.</param>
        public double this[double value] {
            get {
                return this.GetAdjustedValue(value);
            }
        }

    }

}