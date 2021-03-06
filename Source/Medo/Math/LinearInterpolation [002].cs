/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2011-03-13: Changed Add method.
//            Line is approximated between two points when calculating adjustments below or above.
//            Not compatible with version 001!
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
        public LinearInterpolation() { }

        /// <summary>
        /// Adds new calibration point.
        /// </summary>
        /// <param name="referenceValue">Reference value.</param>
        /// <param name="measuredValue">Value as measured by target device.</param>
        public void Add(double referenceValue, double measuredValue) {
            _referencePoints.Add(referenceValue, measuredValue);
        }

        /// <summary>
        /// Returns value adjusted with linear aproximation between two nearest calibration points.
        /// </summary>
        /// <param name="value">Value to adjust.</param>
        public double GetAdjustedValue(double value) {
            KeyValuePair<double, double>? itemBelowF = null;
            KeyValuePair<double, double>? itemBelowN = null;
            KeyValuePair<double, double>? itemAboveN = null;
            KeyValuePair<double, double>? itemAboveF = null;

            foreach (var item in _referencePoints) {
                if (item.Value == value) { //just sent it as output
                    return item.Key;
                } else if (item.Value < value) { //store for future reference - it may be more than one.
                    itemBelowF = itemBelowN;
                    itemBelowN = item;
                } else if (item.Value > value) { //first above limit
                    itemAboveF = itemAboveN;
                    itemAboveN = item;
                    if (itemAboveF.HasValue) { break; }
                }
            }

            if (itemBelowN.HasValue && itemAboveN.HasValue) { //both reference points
                var range = itemAboveN.Value.Value - itemBelowN.Value.Value;
                var point = value - itemBelowN.Value.Value;
                var percentageAbove = point / range;
                var percentageBelow = 1 - percentageAbove;
                return value + (itemBelowN.Value.Key - itemBelowN.Value.Value) * percentageBelow + (itemAboveN.Value.Key - itemAboveN.Value.Value) * percentageAbove;
            } else if (itemBelowN.HasValue) { //just lower reference point
                if (itemBelowF.HasValue) {
                    double m = (itemBelowF.Value.Key - itemBelowN.Value.Key) / ((itemBelowF.Value.Value - itemBelowN.Value.Value));
                    double b = itemBelowN.Value.Key - m * itemBelowN.Value.Value;
                    return m * value + b;
                } else {
                    return value + (itemBelowN.Value.Key - itemBelowN.Value.Value); //just offset
                }
            } else if (itemAboveN.HasValue) { //just upper reference point
                if (itemAboveF.HasValue) {
                    double m = (itemAboveF.Value.Key - itemAboveN.Value.Key) / ((itemAboveF.Value.Value - itemAboveN.Value.Value));
                    double b = itemAboveN.Value.Key - m * itemAboveN.Value.Value;
                    return m * value + b;
                } else {
                    return value + (itemAboveN.Value.Key - itemAboveN.Value.Value); //just offset
                }
            } else { //no reference point
                return value;
            }
        }

    }

}
