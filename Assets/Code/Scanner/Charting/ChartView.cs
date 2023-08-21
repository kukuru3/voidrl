using TMPro;
using UnityEngine;

namespace Scanner.Charting {
    class ChartView : MonoBehaviour {
        [SerializeField] TMP_Text labelx;
        [SerializeField] TMP_Text labely;
        [SerializeField] ChartPlotter plotter;

        public ChartData model;

        private void Start() {
            if (model != null) {
                labelx.text = model.xAxis.caption;
                labely.text = model.yAxis.caption;
                plotter?.Refresh(model);
            }
        }
    }
}
